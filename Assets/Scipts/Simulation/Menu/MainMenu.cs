using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    //This is multiplied with the reciprok of the FoodChainTier to get how many it should spawn of an animal
    private static int animalNumberMultiplier = 1000;

    private void Awake()
    {
        SetupDefaultAnimalSpawn();
    }

    //----------------------------------------------------------
    //Setup the default number of animals to spawn
    private void SetupDefaultAnimalSpawn()
    {
        GameObject[] Animals = Resources.LoadAll<GameObject>("");
        foreach (GameObject item in Animals)
        {
            Animal animal = item.GetComponent<Animal>();
            Settings.NumberOfAnimalsToSpawn.Add(animal.Specie, (int)(1f / (float)animal.FoodChainTier * animalNumberMultiplier));
        }
    }

    public void StartSimulation()
    {
        SceneManager.LoadScene("Simulation", LoadSceneMode.Single);
    }

    public void Exit()
    {
        Application.Quit();
    }
}