using UnityEngine;

/// <summary>
/// A chicken (scared of kfc)
/// </summary>
public class Predator : Animal
{
    private static float maxSpeed = 2f; //The maximum speed chickens can have
    private static float maxVisionRange = 20f; //The maximum vision chickens can have

    //------------------------------------------------------
    //Runs when the script is loaded
    private void Awake()
    {
        ResetStats();
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
                moveState = MoveState.Hunting;
                return (TargetType)FoodChainTier - 1;

            case TargetType.Water:
                moveState = MoveState.Moving;
                return TargetType.Water;

            case TargetType.Mate:
                moveState = MoveState.Meeting;
                return TargetType.Mate;

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
            case TargetType.Mate:
                if (targetBeing != null && !targetBeing.GotEaten && Gender.Male == Gender)
                {
                    this.Reproduce(targetBeing);
                }
                break;

            case TargetType.Water:
                Drink();
                break;

            default:
                if (currentTarget == (TargetType)FoodChainTier - 1)
                {
                    if (targetBeing != null)
                        Eat();
                }
                break;
        }
        currentTarget = TargetType.NONE;
        //  GetNewTarget();
    }

    //----------------------------------------------------------------------------
    /// <summary>
    /// Called when it's borned
    /// Set it's initial stats
    /// </summary>
    public override void Born()
    {
        base.Speed = Random.Range(0.2f, maxSpeed);
        base.VisionRange = Random.Range(4f, maxVisionRange);
        base.timeToMove = 1f / Speed;
    }

    //--------------------------------------------------------------------------
    /// <summary>
    /// Should be called when the animal is borned because of sexual intercourse
    /// </summary>
    public override void Born(Animal parent1, Animal parent2)
    {
        ResetStats();
        float minSpeed = (parent1.Speed + parent2.Speed) / 2 - mutationRate;
        float maxSpeed = (parent1.Speed + parent2.Speed) / 2 + mutationRate;
        float minVisionRange = (parent1.VisionRange + parent2.VisionRange) / 2 - mutationRate;
        float maxVisionRange = (parent1.VisionRange + parent2.VisionRange) / 2 + mutationRate;

        Speed = Random.Range((minSpeed < 0.2f ? 0.2f : minSpeed), maxSpeed > Predator.maxSpeed ? Predator.maxSpeed : maxSpeed);
        base.VisionRange = Random.Range(minVisionRange < 4f ? 4f : minVisionRange, maxVisionRange > Predator.maxVisionRange ? Predator.maxVisionRange : maxVisionRange);
        base.timeToMove = 1f / Speed;
    }
}