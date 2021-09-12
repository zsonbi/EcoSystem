using System.Collections.Generic;

/// <summary>
/// Settings set by the user
/// </summary>
public static class Settings
{
    /// <summary>
    /// The size of the world on the x axis
    /// </summary>
    public static byte XSize = 50;

    /// <summary>
    /// The size of the world on the z axis
    /// </summary>
    public static byte ZSize = 50;

    /// <summary>
    /// The number of animals the simulation should spawn of each specie
    /// </summary>
    public static Dictionary<Species, int> NumberOfAnimalsToSpawn = new Dictionary<Species, int>();
}