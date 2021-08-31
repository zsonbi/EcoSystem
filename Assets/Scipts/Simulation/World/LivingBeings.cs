using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Everything is a living being (even the plants which will be eaten by the animals)
/// </summary>
public abstract class LivingBeings : MonoBehaviour
{
    /// <summary>
    /// The x position in the world's grid
    /// </summary>
    protected int xPosInGrid;

    /// <summary>
    /// The y postition in the world's grid
    /// </summary>
    protected int yPosInGrid;

    /// <summary>
    /// Controls the statbars
    /// </summary>
    public StatBarController StatBarController;

    /// <summary>
    /// The world where the being is
    /// </summary>
    protected World world;

    /// <summary>
    /// Beings which want to do something to this being
    /// </summary>
    protected List<Animal> beingTargetedBy = new List<Animal>();

    /// <summary>
    /// Where it was so we can move it according to the time ellapsed
    /// </summary>
    public Coord basePosition { get; protected set; }

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
    public int XCoordOnGrid { get => (int)(Mathf.Round(XPos)); }

    /// <summary>
    /// Get the y coord if it is needed for the grid
    /// </summary>
    public int YCoordOnGrid { get => (int)(Mathf.Round(ZPos)); }

    /// <summary>
    /// The specie of the being
    /// </summary>
    public Species Specie;//{ get; protected set; }

    /// <summary>
    /// Bool to store if it has been already eaten
    /// </summary>
    public bool GotEaten { get; protected set; }

    /// <summary>
    /// The level the being is on the food chain
    /// </summary>
    [Header("The level the being is on the food chain")]
    public FoodChainTier FoodChainTier;

    [Header("The animator component")]
    public Animator Animator;

    //-------------------------------------------------------
    //Runs when the script is loaded
    private void Start()
    {
        xPosInGrid = (int)XPos;
        yPosInGrid = (int)ZPos;

        this.world = this.GetComponentInParent<World>();
        basePosition = new Coord(XPos, YPos);
        if (StatBarController != null)
            StatBarController.gameObject.SetActive(world.ShowStatBars);
        if (Animator != null)
        { Animator.enabled = world.Animation; }
    }

    //----------------------------------------------------------------------------------
    /// <summary>
    /// Get eaten
    /// </summary>
    /// <returns>true if it was eaten successfully false if it is fails miserably</returns>
    public virtual bool GetEaten()
    {
        if (GotEaten)
        {
            return false;
        }
        world.RemoveFromLivingLayer(xPosInGrid, yPosInGrid, this);
        this.GotEaten = true;
        ClearBeingTargetedList();
        return true;
    }

    //--------------------------------------------------------------------------------------
    /// <summary>
    /// Kills the being
    /// </summary>
    protected virtual void Die()
    {
        world.RemoveFromLivingLayer(xPosInGrid, yPosInGrid, this);
        ClearBeingTargetedList();
        world.Kill(this);
    }

    //-----------------------------------------------------------------------------------
    /// <summary>
    /// Add an animal to the list
    /// </summary>
    /// <param name="animal">Animal which is targeting this being</param>
    public void BeingTargetedBy(Animal animal)
    {
        beingTargetedBy.Add(animal);
    }

    //-----------------------------------------------------------------------------------
    /// <summary>
    /// Remove an animal from the list
    /// </summary>
    /// <param name="animal">the animal which it should remove from the list</param>
    public void NoLongerBeingTargetedBy(Animal animal)
    {
        beingTargetedBy.Remove(animal);
    }

    //-------------------------------------------------------------------------
    /// <summary>
    /// Clear the list and notify the beings that it is no longer avalible
    /// </summary>
    public void ClearBeingTargetedList()
    {
        //Tell those who wanted this being that it is no longer avalible
        for (int i = 0; i < beingTargetedBy.Count; i++)
        {
            beingTargetedBy[i].LostTarget();
        }
        beingTargetedBy.Clear();
    }

    //--------------------------------------------------------------------
    /// <summary>
    /// Alerts the mating partners if there were any
    /// </summary>
    public void AlertMatingPartners()
    {
        //Tell those who wanted this being that it is no longer avalible
        for (int i = 0; i < beingTargetedBy.Count; i++)
        {
            if (beingTargetedBy[i].Specie.Equals(this.Specie))
            {
                beingTargetedBy[i].LostTarget();
                beingTargetedBy.RemoveAt(i);
                i--;
            }
        }
    }

    //-------------------------------------------------------------------------
    /// <summary>
    /// Changes the visibility of the stat bars
    /// </summary>
    /// <param name="visibility">true - visible, false - hidden</param>
    public void ChangeStatBarVisibility(bool visibility)
    {
        if (StatBarController != null)
        {
            StatBarController.gameObject.SetActive(visibility);
        }
    }

    //--------------------------------------------------------------------------------
    /// <summary>
    /// Changes the animator state
    /// </summary>
    /// <param name="visibility">true - active, false - disabled</param>
    public void ChangeAnimatorState(bool state)
    {
        if (Animator != null)
        {
            Animator.Play("Base Layer.New State", -1, 0);
            Animator.enabled = state;
        }
    }
}