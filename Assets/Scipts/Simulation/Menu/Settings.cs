using System.Collections.Generic;

/// <summary>
/// Settings set by the user
/// </summary>
public static class Settings
{
    public static byte XSize = 50;
    public static byte YSize = 50;

    public static Dictionary<Species, int> NumberOfAnimalsToSpawn = new Dictionary<Species, int>();
}