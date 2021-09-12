using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void StartSimulation()
    {
        SceneManager.LoadScene("Simulation", LoadSceneMode.Single);
    }

    public void Exit()
    {
        Application.Quit();
    }
}