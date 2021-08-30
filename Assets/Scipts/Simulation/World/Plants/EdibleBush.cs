using UnityEngine;

/// <summary>
/// Vegetarian animals will go crazy over this
/// </summary>
public class EdibleBush : LivingBeings
{
    private static float defaultTimeToRegrow = 30f; //The number of seconds it takes for the bus to regrow
    private static Vector3 defaultScale = new Vector3(0.5f, 0.5f, 0.5f); //The default scale of the bush
    private float timeTillRegrow = 0f; //The current progress of the regrowth

    //------------------------------------------------------------
    //Runs when the script is loaded
    private void Awake()
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
        if (timeTillRegrow >= defaultTimeToRegrow)
            Regrow();
        this.transform.localScale = defaultScale * (timeTillRegrow / defaultTimeToRegrow);
    }

    //----------------------------------------------------------------------------------
    /// <summary>
    /// Eat the plant
    /// </summary>
    /// <returns>true if it was eaten successfully false if it is fails miserably</returns>
    public override bool GetEaten()
    {
        if (base.GetEaten())
        {
            this.timeTillRegrow = 0f;
            this.transform.localScale = new Vector3(0f, 0f, 0f);
            return true;
        }
        return false;
    }

    //----------------------------------------------------------------------------------
    //Regrow the plant
    private void Regrow()
    {
        this.transform.localScale = defaultScale;
        this.GotEaten = false;
        this.world.AddToLivingLayer(XCoordOnGrid, YCoordOnGrid, this);
    }
}