using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    [Header("The world where the animals are")]
    public World World;

    private bool hidden = true;

    public void SettingButtonClicked()
    {
        this.gameObject.SetActive(hidden);
        hidden = !hidden;
    }

    public void ChangeAnimators(bool value)
    {
        World.Animation = value;
        World.UpdateAnimators();
    }

    public void ChangeStatusBars(bool value)
    {
        World.ShowStatBars = value;
        World.UpdateStatBars();
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}