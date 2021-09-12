using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This is for the menu during the simulation
/// </summary>
public class MenuScript : MonoBehaviour
{
    [Header("The world where the animals are")]
    public World World;

    //Is it hidden
    private bool hidden = true;

    //-------------------------------------------------------
    /// <summary>
    /// Hide or show it when the gear is clicked on
    /// </summary>
    public void SettingButtonClicked()
    {
        this.gameObject.SetActive(hidden);
        hidden = !hidden;
    }

    //--------------------------------------------------
    /// <summary>
    /// Enable or disable the animations
    /// </summary>
    /// <param name="value">true - enable, false - disable</param>
    public void ChangeAnimators(bool value)
    {
        World.Animation = value;
        World.UpdateAnimators();
    }

    //----------------------------------------------
    /// <summary>
    /// Enable or disable the status bars
    /// </summary>
    /// <param name="value">true - enable, false - disable</param>
    public void ChangeStatusBars(bool value)
    {
        World.ShowStatBars = value;
        World.UpdateStatBars();
    }

    //------------------------------------------------------
    /// <summary>
    /// Go back to the main menu
    /// </summary>
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}