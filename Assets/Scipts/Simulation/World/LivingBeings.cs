using UnityEngine;

/// <summary>
/// Everything is a living being (even the plants which will be eaten by the animals)
/// </summary>
public abstract class LivingBeings : MonoBehaviour
{
    /// <summary>
    /// The world where the being is
    /// </summary>
    protected World world;

    /// <summary>
    /// The x position
    /// </summary>
    public float XPos { get => this.gameObject.transform.position.x; }

    /// <summary>
    /// The y position (how high it is)
    /// </summary>
    public float YPos { get => this.gameObject.transform.position.y; }

    /// <summary>
    /// The z position (in 2d this is the y coord)
    /// </summary>
    public float ZPos { get => this.gameObject.transform.position.z; }

    /// <summary>
    /// Get the x coord if it is needed for the grid
    /// </summary>
    public int XCoordOnGrid { get => (int)(Mathf.Round(XPos) / world.TileSize); }

    /// <summary>
    /// Get the y coord if it is needed for the grid
    /// </summary>
    public int YCoordOnGrid { get => (int)(Mathf.Round(ZPos) / world.TileSize); }

    /// <summary>
    /// The specie of the being
    /// </summary>
    public Species Specie { get; protected set; }

    /// <summary>
    /// Bool to store if it has been already eaten
    /// </summary>
    public bool GotEaten { get; protected set; }

    //-------------------------------------------------------
    //Runs when the script is loaded
    private void Start()
    {
        this.world = this.GetComponentInParent<World>();
    }

    //----------------------------------------------------------------------------------
    /// <summary>
    /// Eat the plant
    /// </summary>
    /// <returns>true if it was eaten successfully false if it is fails miserably</returns>
    public virtual bool GetEaten()
    {
        if (GotEaten)
        {
            return false;
        }
        world.RemoveFromLivingLayer(XCoordOnGrid, YCoordOnGrid, this);
        this.GotEaten = true;
        return true;
    }
}