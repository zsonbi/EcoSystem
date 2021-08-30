using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Every animal's parent class
/// </summary>
public abstract class Animal : LivingBeings
{
    [Header("The maximum amount of children it can have it gets selected by random")]
    public byte MaxNumberOfChildren = 1;

    protected static float maxHunger = 80f; //The time it takes for it to starve to death
    protected static float maxThirst = 80f; //The time it takes for it to die of thirst
    protected static float maxHorniness = 60f; //The amount of horniness required to have the confidence to ask out nearby animals
    protected static float mutationRate = 0.4f; //The amount the stats can mutate

    /// <summary>
    /// Current hunger level of the animal when it reaches 0 it dies
    /// </summary>
    public float Hunger { get; protected set; }

    /// <summary>
    /// Current thirst level of the animal when it reaches 0 it dies
    /// </summary>
    public float Thirst { get; protected set; }

    /// <summary>
    /// How fast the animal moves
    /// </summary>
    public float Speed { get; protected set; }

    /// <summary>
    /// How far the animal can see
    /// </summary>
    public float VisionRange { get; protected set; }

    /// <summary>
    /// How horny it is
    /// </summary>
    public float Horniness { get; protected set; }

    /// <summary>
    /// Round the vision and convert it to byte
    /// </summary>
    public byte RoundedVisionRange { get => (byte)Mathf.Round(this.VisionRange); }

    /// <summary>
    /// The gender of the animal
    /// </summary>
    public Gender Gender { get; protected set; }

    /// <summary>
    /// The next square where it wants to move
    /// </summary>
    public Coord moveTarget { get; private set; }

    protected Coord target = null; //The end target
    protected float timeToMove; //The time it takes for it to move one square
    private float time = 0; //The time since last square

    protected TargetType currentTarget = TargetType.NONE; //Type of the current target
    private Stack<Coord> path; //The path to the target
    protected LivingBeings targetBeing; //The being which is being targeted
    protected MoveState moveState; //How it should get the next moveTarget

    //**********************************************************************************
    //Abstract methods
    /// <summary>
    /// What it should do when it reached the target
    /// </summary>
    protected abstract void ReachedTarget();

    /// <summary>
    /// Determine what should it look for in the meantime
    /// </summary>
    /// <returns>The target's type</returns>
    protected abstract TargetType DecideTargetPriority();

    public abstract void Born();

    public abstract void Born(Animal parent1, Animal parent2);

    //*****************************************************************************
    //Runs every frame
    private void Update()
    {
        //After a certain time get a new movetarget
        if (time >= timeToMove)
        {
            time = 0;
            GetNextMoveTarget();
        }
        //If we have a target move towards it
        if (currentTarget != TargetType.NONE)
        {
            MoveTowardsTarget();
        }
        //Update the stats
        Hunger -= Time.deltaTime;
        Thirst -= Time.deltaTime;
        this.Horniness += Time.deltaTime;

        //Update the stat bars
        if (world.ShowStatBars)
            StatBarController.UpdateSliders(Hunger, Thirst, Horniness);

        //if the hunger or thirst reached 0 kill it
        if (Hunger <= 0 || Thirst <= 0)
        {
            Die();
        }
        time += Time.deltaTime;
    }

    //----------------------------------------------------------------
    /// <summary>
    /// Get the next moveTarge
    /// </summary>
    private void GetNextMoveTarget()
    {
        if (currentTarget == TargetType.NONE)
        {
            GetNewTarget();
            return;
        }
        //Move it in the LivingBeings grid
        world.Move(new Coord(XCoordOnGrid, YCoordOnGrid), this, ref xPosInGrid, ref yPosInGrid);
        basePosition = new Coord(XCoordOnGrid, YCoordOnGrid);

        switch (moveState)
        {
            case MoveState.Waiting:
                //Cancel the waiting if the animal is about to die
                if (Hunger < maxHunger * 0.3f || Thirst < maxThirst * 0.3f)
                {
                    AlertMatingPartners();
                    LostTarget();
                    return;
                }
                //Making sure so it doesn't spam error :(
                if (moveTarget is null)
                {
                    moveTarget = basePosition;
                }
                break;

            case MoveState.Moving:
                if (path.Count > 0)
                {
                    moveTarget = path.Pop();
                }
                else
                {
                    ReachedTarget();
                    return;
                }
                break;

            case MoveState.Fleeing:
                //Get the furthest cell from the predator
                moveTarget = world.GetFurthestMoveTarget(new Coord(xPosInGrid, yPosInGrid), new Coord(targetBeing.XPos, targetBeing.YPos));
                break;

            case MoveState.Meeting:
                /*
                if (target.Equals(new Coord(xPosInGrid, yPosInGrid)))
                {
                    ReachedTarget();
                    (targetBeing as Animal).ReachedByMeetingPartner(this);
                    return;
                }*/
                if (path.Count > 0)
                {
                    moveTarget = path.Pop();
                }
                else
                {
                    ReachedTarget();
                    (targetBeing as Animal).ReachedByMeetingPartner(this);
                    return;
                }

                break;

            case MoveState.Hunting:
                if (CheckIfReachedTarget())
                {
                    ReachedTarget();
                    return;
                }
                else if (Coord.CalcDistance(new Coord(targetBeing.XPos, targetBeing.YPos), new Coord(XPos, YPos)) >= VisionRange)
                {
                    targetBeing.NoLongerBeingTargetedBy(this);
                    Escape();
                }
                moveTarget = world.GetClosestMoveTarget(new Coord(xPosInGrid, yPosInGrid), new Coord(targetBeing.XCoordOnGrid, targetBeing.YCoordOnGrid));
                break;

            default:
                break;
        }

        float angle = Mathf.Rad2Deg * Coord.CalcAngle(basePosition, moveTarget);
        this.transform.eulerAngles = new Vector3(0, angle + (angle % 180 == 0 ? 90f : -90f), 0);
    }

    //---------------------------------------------------------------
    /// <summary>
    /// Alerts it's partner that it reached him/her
    /// </summary>
    /// <param name="meetingPartner">The partner</param>
    public void ReachedByMeetingPartner(Animal meetingPartner)
    {
        if (moveState == MoveState.Waiting)
        {
            ReachedTarget();
            AlertMatingPartners();
        }
    }

    //--------------------------------------------------------------------------------
    /// <summary>
    /// Gets a new target according to the moveState and the currentTaget
    /// </summary>
    protected void GetNewTarget()
    {
        currentTarget = DecideTargetPriority();
        targetBeing = null;

        switch (moveState)
        {
            case MoveState.Moving:
                target = world.CreateNewTarget(ref currentTarget, this, ref targetBeing);
                path = world.CreatePath(new Coord(xPosInGrid, yPosInGrid), target, ref currentTarget);
                break;

            case MoveState.Meeting:
                Horniness = 0f;
                if (world.AskOutNearbyAnimals(this, RoundedVisionRange, ref targetBeing))
                {
                    target = ((Animal)targetBeing).moveTarget;
                    path = world.CreatePath(new Coord(xPosInGrid, yPosInGrid), target, ref currentTarget);
                }
                else
                {
                    GetNewTarget();
                    return;
                }
                break;

            case MoveState.Hunting:
                target = world.CreateNewTarget(ref currentTarget, this, ref targetBeing);
                if (targetBeing != null)
                    ((Animal)targetBeing).BeingHuntedDown(this);

                break;

            default:
                break;
        }

        if (targetBeing != null)
        {
            targetBeing.BeingTargetedBy(this);
        }
        else
        {
            moveState = MoveState.Moving;
        }
        GetNextMoveTarget();

        time = 0f;
    }

    //----------------------------------------------------------------------
    /// <summary>
    /// Gets what is the most important to it food water etc.
    /// </summary>
    /// <returns></returns>
    protected TargetType GetMostImportantTargetType()
    {
        if (Hunger > maxHunger * 0.6f && Thirst > maxThirst * 0.6f)
        {
            if (true && Horniness >= maxHorniness)
            {
                return TargetType.Mate;
            }
            else
                return TargetType.Explore;
        }
        else if (Hunger <= Thirst)
        {
            return TargetType.Food;
        }
        else
        {
            return TargetType.Water;
        }
    }

    //------------------------------------------------------------------------------
    /// <summary>
    /// Lose it's current target
    /// </summary>
    public void LostTarget()
    {
        currentTarget = TargetType.NONE;
    }

    //---------------------------------------------------------------------------------
    //Moves the animal towards the current movetarget
    private void MoveTowardsTarget()
    {
        this.transform.position = Vector3.Lerp(new Vector3(basePosition.x, base.YPos, basePosition.y), new Vector3(moveTarget.x, base.YPos, moveTarget.y), Speed * time);
    }

    //-------------------------------------------------------------------------------
    /// <summary>
    /// Drink water
    /// </summary>
    protected void Drink()
    {
        this.Thirst = maxThirst;
    }

    //-------------------------------------------------------------------------------
    /// <summary>
    /// Eat
    /// </summary>
    protected void Eat()
    {
        targetBeing.NoLongerBeingTargetedBy(this);

        if (targetBeing.GetEaten())
            this.Hunger = maxHunger;
    }

    //---------------------------------------------------------------------------
    /// <summary>
    /// Get eaten
    /// </summary>
    /// <returns>true if it was a success false if it failed</returns>
    public override bool GetEaten()
    {
        Die();
        return base.GetEaten();
    }

    //----------------------------------------------------------------------------------
    /// <summary>
    /// Kills the animal
    /// </summary>
    protected override void Die()
    {
        if (targetBeing != null)
            targetBeing.NoLongerBeingTargetedBy(this);
        base.Die();
    }

    //---------------------------------------------------------------------------------
    /// <summary>
    /// Gets asked out and if it accepts change the currentTarget to the asking out animal and the moveState to Waiting
    /// </summary>
    /// <param name="theOneAskingOut">The animal which asked it out</param>
    /// <returns>true if accepted, false if rejected</returns>
    public bool GetAskedOut(Animal theOneAskingOut)
    {
        if (TargetType.Explore == currentTarget)
        {
            if (Horniness > maxHorniness * 0.5f)
            {
                target = moveTarget;
                moveState = MoveState.Waiting;
                currentTarget = theOneAskingOut.currentTarget;
                targetBeing = theOneAskingOut;

                theOneAskingOut.BeingTargetedBy(this);
                this.Horniness = 0f;
                return true;
            }
        }
        return false;
    }

    //------------------------------------------------------------------------------------
    /// <summary>
    /// Alerts the animal that it is being hunted down
    /// </summary>
    /// <param name="theOneHuntingItDown">The animal which is trying to hunt it down</param>
    public void BeingHuntedDown(LivingBeings theOneHuntingItDown)
    {
        moveState = MoveState.Fleeing;
        currentTarget = TargetType.Fleeing;
        targetBeing = theOneHuntingItDown;
        AlertMatingPartners();
        if (targetBeing != null)
        {
            targetBeing.NoLongerBeingTargetedBy(this);
        }
    }

    //-----------------------------------------------------------------------------------
    /// <summary>
    /// Called when it escaped from the predator
    /// </summary>
    public void Escape()
    {
        Debug.Log("Escaped");
        LostTarget();
    }

    //-----------------------------------------------------------------
    /// <summary>
    /// A simple distanceCheck to the target
    /// </summary>
    /// <returns></returns>
    protected bool CheckIfReachedTarget()
    {
        return Coord.CalcDistance(XPos, ZPos, targetBeing.XPos, targetBeing.ZPos) < 0.2f;
    }

    //--------------------------------------------------------------------------
    /// <summary>
    /// Set the maxValues for the stat bars
    /// </summary>
    /// <param name="maxHunger">Maximum hunger</param>
    /// <param name="maxThirst">Maximum thirst</param>
    /// <param name="maxHorniness">Maximum horniness</param>
    protected void SetInitialStatBarMaxValues(float maxHunger, float maxThirst, float maxHorniness)
    {
        this.StatBarController.SetMaxHungerValue(maxHunger);
        this.StatBarController.SetMaxThirstValue(maxThirst);
        this.StatBarController.SetMaxHornyValue(maxHorniness);
    }

    //------------------------------------------------------------
    /// <summary>
    ///  Reproduce (even chickens are better at it than me)
    /// </summary>
    /// <param name="otherOne">the other chicken</param>
    protected void Reproduce(LivingBeings otherOne)
    {
        byte childCount = (byte)Random.Range(1, MaxNumberOfChildren + 1);
        for (int i = 0; i < childCount; i++)
        {
            world.SpawnNewAnimal(this, (Animal)otherOne);
        }
    }

    //--------------------------------------------------------------------
    /// <summary>
    /// Resets the animal's stats and reroll it's gender
    /// </summary>
    protected void ResetStats()
    {
        xPosInGrid = (int)XPos;
        yPosInGrid = (int)ZPos;
        Hunger = maxHunger;
        Thirst = maxThirst;
        Horniness = 0f;
        Gender = (Random.Range(0, 2) == 1 ? Gender.Male : Gender.Female);
        currentTarget = TargetType.NONE;
        time = float.MaxValue;
    }

    //-------------------------------------------------------------------------------
    /// <summary>
    /// Debug tool
    /// </summary>
    private void OnDrawGizmos()
    {
        if (target == null)
            return;
        Gizmos.color = new Color(0.3f, 0.3f, 0.3f);
        Gizmos.DrawSphere(new Vector3(moveTarget.x, YPos, moveTarget.y), 0.1f);
        Gizmos.color = new Color(0, 0, 1);
        Gizmos.DrawSphere(new Vector3(target.x, YPos, target.y), 0.1f);
    }
}