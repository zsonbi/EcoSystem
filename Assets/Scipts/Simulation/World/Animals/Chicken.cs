using UnityEngine;

/// <summary>
/// A chicken (scared of kfc)
/// </summary>
public class Chicken : Animal
{
    private static float maxSpeed = 2f; //The maximum speed chickens can have
    private static float maxVisionRange = 15f; //The maximum vision chickens can have

    //------------------------------------------------------
    //Runs when the script is loaded
    private void Awake()
    {
        //Set the chickens stats
        base.FoodType.Add(Species.Plant);
        base.Specie = Species.Chicken;
        base.Hunger = maxHunger;
        base.Thirst = maxThirst;
        Born();
        SetInitialStatBarMaxValues(Hunger, Thirst, maxHorniness);
    }

    //--------------------------------------------------------------
    /// <summary>
    /// Determine what should it look for in the meantime
    /// </summary>
    /// <returns>The target's type</returns>
    protected override TargetType DecideTargetPriority()
    {
        switch (base.GetMostImportantTargetType())
        {
            case TargetType.Food:
                moveState = MoveState.Moving;
                return TargetType.Plant;

            case TargetType.Water:
                moveState = MoveState.Moving;
                return TargetType.Water;

            case TargetType.Mate:
                moveState = MoveState.Meeting;
                return TargetType.Chicken;

            default:
                moveState = MoveState.Moving;
                return TargetType.Explore;
        }
    }

    //---------------------------------------------------------
    /// <summary>
    /// What it should do when it reached the target
    /// </summary>
    protected override void ReachedTarget()
    {
        //Tell the targetbeing that it's no longer tageting it
        if (targetBeing != null)
        {
            targetBeing.NoLongerBeingTargetedBy(this);
        }

        switch (currentTarget)
        {
            case TargetType.Chicken:
                if (targetBeing != null && !targetBeing.GotEaten && Gender.Female == Gender)
                {
                    this.Reproduce(targetBeing);
                }
                break;

            case TargetType.Plant:
                if (targetBeing != null)
                    Eat();
                break;

            case TargetType.Water:
                Drink();
                break;

            default:
                break;
        }
        //target = null;
        currentTarget = TargetType.NONE;
    }

    public override void Born()
    {
        base.Gender = (Random.Range(0, 2) == 1 ? Gender.Male : Gender.Female);
        base.Speed = Random.Range(0.2f, maxSpeed);
        base.VisionRange = Random.Range(4f, maxVisionRange);
        base.timeToMove = 1f / Speed;
    }

    public override void Born(Animal parent1, Animal parent2)
    {
        throw new System.NotImplementedException();
    }

    //------------------------------------------------------------
    /// <summary>
    ///  Reproduce (even chickens are better at it than me)
    /// </summary>
    /// <param name="otherOne">the other chicken</param>
    public override void Reproduce(LivingBeings otherOne)
    {
        Debug.Log("reproduce");
    }
}