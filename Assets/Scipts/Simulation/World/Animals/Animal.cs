using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Every animal's parent class
/// </summary>
public abstract class Animal : LivingBeings
{
    protected static float maxHunger = 45f; //The time it takes for it to starve to death
    protected static float maxThirst = 45f; //The time it takes for it to die of thirst
    protected static float maxHorniness = 60f;

    /// <summary>
    /// The food types the animal can eat
    /// </summary>
    public List<Species> FoodType { get; protected set; } = new List<Species>();

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
    public Gender Gender;// { get; protected set; }

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
    protected MoveState moveState;

    //**********************************************************************************
    //Abstract methods
    /// <summary>
    /// What it should do when it reached the target
    /// </summary>
    protected abstract void ReachedTarget();

    /// <summary>
    ///  Reproduce (even chickens are better at it than me)
    /// </summary>
    /// <param name="otherOne">the other chicken</param>
    public abstract void Reproduce(LivingBeings otherOne);

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
        time += Time.deltaTime;

        //If we have a target move towards it
        if (currentTarget != TargetType.NONE)
        {
            MoveTowardsTarget();
        }
        else
        {
            GetNewTarget();
        }

        //After a certain time get a new movetarget
        if (time >= timeToMove)
        {
            time = 0;
            GetNextMoveTarget();
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
            Debug.Log("Dead :(");
            Die();
        }
    }

    //----------------------------------------------------------------
    /// <summary>
    /// Get the next moveTarge
    /// </summary>
    private void GetNextMoveTarget()
    {
        switch (moveState)
        {
            case MoveState.Waiting:
                //if (moveTarget.Equals(new Coord(targetBeing.XPos, targetBeing.ZPos)))
                //{
                //    Debug.Log("Completed waiting");
                //    ReachedTarget();
                //}
                //  basePosition = new Coord(XCoordOnGrid, YCoordOnGrid);
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
                //TODO
                break;

            case MoveState.Meeting:
                if (target.Equals(new Coord(XPos, ZPos)))
                {
                    ReachedTarget();
                    (targetBeing as Animal).ReachedByMeetingPartner(this);
                    return;
                }
                moveTarget = world.GetBestMoveTarget(new Coord(XPos, ZPos), new Coord(targetBeing.XPos, targetBeing.ZPos));
                break;

            case MoveState.Hunting:
                if (CheckIfReachedTarget())
                {
                    ReachedTarget();
                    return;
                }
                moveTarget = world.GetBestMoveTarget(new Coord(XPos, ZPos), new Coord(targetBeing.XPos, targetBeing.ZPos));
                break;

            default:
                break;
        }
        world.Move(new Coord(XCoordOnGrid, YCoordOnGrid), this, ref xPosInGrid, ref yPosInGrid);
        basePosition = new Coord(XPos, ZPos);

        float angle = Mathf.Rad2Deg * Coord.CalcAngle(basePosition, moveTarget);
        this.transform.eulerAngles = new Vector3(0, angle + (angle % 180 == 0 ? 90f : -90f), 0);
    }

    public void ReachedByMeetingPartner(Animal meetingPartner)
    {
        if (moveState == MoveState.Waiting)
        {
            ReachedTarget();
            AlertMatingPartners();
        }
    }

    private void GetNewTarget()
    {
        currentTarget = DecideTargetPriority();
        targetBeing = null;

        switch (moveState)
        {
            case MoveState.Moving:
                target = world.CreateNewTarget(ref currentTarget, this, ref targetBeing);
                path = world.CreatePath(new Coord(XPos, ZPos), target, ref currentTarget);
                break;

            case MoveState.Fleeing:
                break;

            case MoveState.Meeting:
                Horniness = 0f;
                if (world.AskOutNearbyAnimals(this, RoundedVisionRange, ref targetBeing))
                {
                    target = ((Animal)targetBeing).moveTarget;

                    try
                    {
                        path = world.CreatePath(new Coord(XPos, ZPos), target, ref currentTarget);
                    }
                    catch (System.Exception)
                    {
                        Debug.Log("It was the meeting");
                        throw;
                    }
                }
                else
                {
                    Debug.Log("Rejected");
                    GetNewTarget();
                    return;
                }
                break;

            case MoveState.Hunting:
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
        Debug.Log(currentTarget.ToString());
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
                Debug.Log("HORNY");
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
        Debug.Log("Lost Target");
        //  this.target = null;
        currentTarget = TargetType.NONE;
        // GetNewTarget();
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

    public bool GetAskedOut(Animal theOneAskingOut)
    {
        if (TargetType.Explore == currentTarget)
        {
            if (Horniness > maxHorniness * 0.5f)
            {
                target = moveTarget;
                moveState = MoveState.Waiting;
                currentTarget = TargetType.Chicken;
                targetBeing = theOneAskingOut;
                Debug.Log("Got Asked out");
                theOneAskingOut.BeingTargetedBy(this);
                this.Horniness = 0f;
                return true;
            }
        }
        return false;
    }

    protected bool CheckIfReachedTarget()
    {
        return Coord.CalcDistance(XPos, ZPos, targetBeing.XPos, targetBeing.ZPos) < 0.2f;
    }

    protected void SetInitialStatBarMaxValues(float maxHunger, float maxThirst, float maxHorniness)
    {
        this.StatBarController.SetMaxHungerValue(maxHunger);
        this.StatBarController.SetMaxThirstValue(maxThirst);
        this.StatBarController.SetMaxHornyValue(maxHorniness);
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