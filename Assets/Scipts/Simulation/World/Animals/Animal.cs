using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Every animal's parent class
/// </summary>
public abstract class Animal : LivingBeings
{
    protected static float maxHunger = 30f; //The time it takes for it to starve to death
    protected static float maxThirst = 30f; //The time it takes for it to die of thirst

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
    public float ReproductionUrge { get; protected set; }

    /// <summary>
    /// Round the vision and convert it to byte
    /// </summary>
    public byte RoundedVisionRange { get => (byte)Mathf.Round(this.VisionRange); }

    /// <summary>
    /// The gender of the animal
    /// </summary>
    public Gender Gender { get; protected set; }

    protected Coord target = null;
    protected float timeToMove;
    private float time = 0;
    private Coord moveTarget;
    private Coord basePosition;
    protected TargetType currentTarget;
    private Stack<Coord> path;

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
    public abstract void Reproduce(Animal otherOne);

    /// <summary>
    /// Determine what should it look for in the meantime
    /// </summary>
    /// <returns>The target's type</returns>
    protected abstract TargetType DecideTargetPriority();

    //*****************************************************************************
    //Runs every frame
    private void Update()
    {
        time += Time.deltaTime;

        //If we have a target move towards it
        if (target != null)
        {
            MoveTowardsTarget();
        }
        else
        {
            currentTarget = DecideTargetPriority();
            target = world.CreateNewTarget(currentTarget, this);
            path = world.CreatePath(new Coord(XPos, ZPos), target);
            PopNextPathNode();
            time = 0;
        }

        //After a certain time get a new movetarget
        if (time >= timeToMove)
        {
            time = 0;
            //this.transform.position = new Vector3(Mathf.Round(XPos), this.YPos, Mathf.Round(ZPos));
            //if (Coord.CalcDistance(target.x, target.y, XPos, ZPos) <= world.TileSize)
            //{
            //    moveTarget = target;
            //}
            //else
            //{
            PopNextPathNode();
            //}
        }

        Hunger -= Time.deltaTime;
        Thirst -= Time.deltaTime;
        //if the hunger or thirst reached 0 kill it
        if (Hunger <= 0 || Thirst <= 0)
        {
            // world.Kill(this);
        }

        Debug.Log(currentTarget.ToString());
    }

    private void PopNextPathNode()
    {
        if (path.Count > 0)
            moveTarget = path.Pop();
        else
        {
            ReachedTarget();
            return;
        }
        basePosition = new Coord(XPos, ZPos);
        float angle = Mathf.Rad2Deg * Coord.CalcAngle(basePosition, moveTarget);
        this.transform.eulerAngles = new Vector3(0, angle + (angle % 180 == 0 ? 90f : -90f), 0);
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
            return TargetType.Mate;
        }
        else if (Hunger < Thirst)
        {
            return TargetType.Food;
        }
        else
        {
            return TargetType.Water;
        }
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
        this.Hunger = maxHunger;
    }

    //-------------------------------------------------------------------------------
    /// <summary>
    /// Debug tool
    /// </summary>
    private void OnDrawGizmos()
    {
        if (target == null)
            return;

        Gizmos.DrawSphere(new Vector3(moveTarget.x, YPos, moveTarget.y), 0.1f);
        Gizmos.DrawSphere(new Vector3(target.x, YPos, target.y), 0.1f);
    }
}