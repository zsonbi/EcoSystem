using UnityEngine;

/// <summary>
/// A chicken (scared of kfc)
/// </summary>
public class Chicken : Animal
{
    private static float maxSpeed = 2f; //The maximum speed chickens can have
    private static float maxVisionRange = 15f; //The maximum vision chickens can have

    //------------------------------------------------------
    //Runs before first update
    private void Awake()
    {
        //Set the chickens stats
        base.FoodType.Add(Species.Plant);
        base.Specie = Species.Chicken;
        base.Hunger = 30f;
        base.Thirst = 30f;
        base.Gender = (Random.Range(0, 1) == 1 ? Gender.Male : Gender.Female);
        base.Speed = Random.Range(0.2f, maxSpeed);
        base.VisionRange = Random.Range(4f, maxVisionRange);
        base.timeToMove = 1f / Speed;
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
                return TargetType.Plant;

            case TargetType.Water:
                return TargetType.Water;

            case TargetType.Mate:
                return TargetType.Chicken;

            default:
                return TargetType.Explore;
        }
    }

    //---------------------------------------------------------
    /// <summary>
    /// What it should do when it reached the target
    /// </summary>
    protected override void ReachedTarget()
    {
        switch (currentTarget)
        {
            case TargetType.Chicken:

                break;

            case TargetType.Plant:
                Eat();
                break;

            case TargetType.Water:
                Drink();
                break;

            default:
                break;
        }
        target = null;
    }

    //------------------------------------------------------------
    /// <summary>
    ///  Reproduce (even chickens are better at it than me)
    /// </summary>
    /// <param name="otherOne">the other chicken</param>
    public override void Reproduce(Animal otherOne)
    {
        //TODO simple but unbreakable
        throw new System.NotImplementedException();
    }
}