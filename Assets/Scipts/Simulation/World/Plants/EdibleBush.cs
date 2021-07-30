using UnityEngine;

/// <summary>
/// Vegetarian animals will go crazy over this
/// </summary>
public class EdibleBush : LivingBeings
{
    private static float defaultTimeToRegrow = 10f; //The number of seconds it takes for the bus to regrow
    private float timeTillRegrow = 0f; //The current progress of the regrowth
    private bool GotEaten; //Bool to store if it has been already eaten

    //------------------------------------------------------------
    //Runs before the first Update
    private void Start()
    {
        this.Specie = Species.Plant;
    }

    //--------------------------------------------------------------
    //Runs every frame
    private void Update()
    {
        if (!GotEaten)
            return;
        timeTillRegrow += Time.deltaTime;
        if (timeTillRegrow <= defaultTimeToRegrow)
            Regrow();
    }

    //----------------------------------------------------------------------------------
    /// <summary>
    /// Eat the plant
    /// </summary>
    /// <returns>true if it was eaten successfully false if it is fails miserably</returns>
    public bool GetEaten()
    {
        if (GotEaten)
        {
            return false;
        }

        this.GotEaten = true;
        timeTillRegrow = 0f;
        return true;
    }

    //----------------------------------------------------------------------------------
    //Regrow the plant
    private void Regrow()
    {
        this.GotEaten = false;
    }
}