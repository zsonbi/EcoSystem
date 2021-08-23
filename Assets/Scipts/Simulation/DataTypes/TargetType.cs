/// <summary>
/// Type of the target of the animal
/// </summary>
public enum TargetType : byte
{
    Explore = 255,
    Water = 254,
    NONE = 253,
    Plant = 0,
    Chicken = 1,
    Bunny = 2,
    Fox = 3,

    Mate,
    Food,
}