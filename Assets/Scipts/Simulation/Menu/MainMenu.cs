using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script handles all of the main menu
/// </summary>
public class MainMenu : MonoBehaviour
{
    //This is multiplied with the reciprok of the FoodChainTier to get how many it should spawn of an animal
    private static int animalNumberMultiplier = 200;

    /// <summary>
    /// The number of animals it should spawn of the currently selected animal
    /// </summary>
    public InputField NumberOfAnimalsInput;

    /// <summary>
    /// The Size of the map on the x axis
    /// </summary>
    public InputField XSizeInput;

    /// <summary>
    /// The Size of the map on the z axis
    /// </summary>
    public InputField ZSizeInput;

    /// <summary>
    /// The specie of the currently selected animal
    /// </summary>
    public Text SpecieLabel;

    private int currentIndex = 0; //The index of the current animal
    private GameObject[] Animals; //The avalible animals
    private GameObject currentlySelectedAnimal; //The currently selected animal's gameobject

    //----------------------------------------------------------
    //Runs when the script is loaded
    private void Awake()
    {
        Animals = Resources.LoadAll<GameObject>("");
        //If it hasn't been se set the default spawn rate
        if (Settings.NumberOfAnimalsToSpawn.Count == 0)
        {
            SetupDefaultAnimalSpawn();
            //Cap the fps to 60
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
        }

        //Update the input fields content to the default value
        XSizeInput.text = Settings.XSize.ToString();
        ZSizeInput.text = Settings.ZSize.ToString();
        //Load in an animal model
        LoadAnimalModel();
    }

    //--------------------------------------------------------
    //Runs every frame
    private void Update()
    {
    }

    //----------------------------------------------------------
    //Setup the default number of animals to spawn
    private void SetupDefaultAnimalSpawn()
    {
        foreach (GameObject item in Animals)
        {
            Animal animal = item.GetComponent<Animal>();
            Settings.NumberOfAnimalsToSpawn.Add(animal.Specie, (int)(1f / (float)animal.FoodChainTier * animalNumberMultiplier));
        }
    }

    //---------------------------------------------------------
    //Loads in the animal from the Animals array at the currentIndex
    private void LoadAnimalModel()
    {
        if (currentlySelectedAnimal != null)
        {
            Destroy(currentlySelectedAnimal);
        }

        currentlySelectedAnimal = Instantiate(Animals[currentIndex], Camera.main.transform);
        //So it doesn't start moving around
        currentlySelectedAnimal.GetComponent<Animal>().enabled = false;
        NumberOfAnimalsInput.text = Settings.NumberOfAnimalsToSpawn[currentlySelectedAnimal.GetComponent<Animal>().Specie].ToString();
        currentlySelectedAnimal.transform.position = new Vector3(5f, -3f, 20f);
        currentlySelectedAnimal.transform.eulerAngles = new Vector3(0, 200f, 0);
        currentlySelectedAnimal.transform.localScale *= 5;
        SpecieLabel.text = currentlySelectedAnimal.GetComponent<Animal>().Specie.ToString();
        currentlySelectedAnimal.GetComponent<Animal>().ChangeStatBarVisibility(false);
    }

    //------------------------------------------------------
    /// <summary>
    /// Gets the next animal
    /// </summary>
    public void NextAnimal()
    {
        if (currentIndex == Animals.Length - 1)
            currentIndex = 0;
        else
            currentIndex++;
        LoadAnimalModel();
    }

    //--------------------------------------------------------------
    /// <summary>
    /// Gets the previous animal
    /// </summary>
    public void PreviousAnimal()
    {
        if (currentIndex == 0)
            currentIndex = Animals.Length - 1;
        else
            currentIndex--;
        LoadAnimalModel();
    }

    //----------------------------------------------------------------
    /// <summary>
    /// Updates the loaded animal's spawn number
    /// </summary>
    public void UpdateSpawnNumber()
    {
        Settings.NumberOfAnimalsToSpawn[currentlySelectedAnimal.GetComponent<Animal>().Specie] = System.Convert.ToInt32(NumberOfAnimalsInput.text);
    }

    //--------------------------------------------------------------------------
    /// <summary>
    /// Updates the XSize in the settings
    /// </summary>
    /// <param name="size">The new size</param>
    public void UpdateXSize(string size)
    {
        Settings.XSize = System.Convert.ToByte(size);
    }

    //--------------------------------------------------------------------------
    /// <summary>
    /// Updates the ZSize in the settings
    /// </summary>
    /// <param name="size">The new size</param>
    public void UpdateZSize(string size)
    {
        Settings.ZSize = System.Convert.ToByte(size);
    }

    //-----------------------------------------------------------
    /// <summary>
    /// Loads the simulation scene
    /// </summary>
    public void StartSimulation()
    {
        SceneManager.LoadScene("Simulation", LoadSceneMode.Single);
    }

    //--------------------------------------------------------------
    /// <summary>
    /// Exits the application
    /// </summary>
    public void Exit()
    {
        Application.Quit();
    }
}