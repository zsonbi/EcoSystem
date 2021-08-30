/// <summary>
/// Type of the target of the animal
/// </summary>
public enum TargetType : byte
{
    Explore = 255,
    Water = 254,
    Fleeing = 253,
    NONE = 252,

    LowestTier = 0,
    BottomTier = 1,
    MiddleTier = 2,
    TopTier = 3,
    HighestTier = 4,

    Mate,
    Food,
}