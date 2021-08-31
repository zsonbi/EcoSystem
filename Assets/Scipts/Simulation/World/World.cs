using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// The world where the simulation is going on
/// </summary>
public class World : MonoBehaviour
{
    [Header("Does animals have stats above their heads")]
    public bool ShowStatBars = false;

    [Header("Animals to spawn")]
    public GameObject[] Animals;

    [Header("How many animals to spawn")]
    public int[] AnimalCount;

    private WorldGenerator generatedWorld; //The world which was generated at the start
    private Dictionary<Species, Stack<GameObject>> shadowRealm; //This is where the dead animals go (so they can be recycled)
    private Dictionary<Species, GameObject> dictionaryToAnimals; //This is used when the shadowRealm is empty
    private Dictionary<Species, List<LivingBeings>> livingBeingsCategorized; //Categorize the living beings by species
    private Dictionary<Species, Text> statusTextsToSpecies; //The status text connected to the species
    private PathMaker pathMaker; //The pathmaker obj

    /// <summary>
    /// 1 where the area is passable 0 where it is blocked
    /// </summary>
    public byte[,] moveLayer { get => generatedWorld.layers[0]; }

    /// <summary>
    /// Y size of the world
    /// </summary>
    public int YSize { get => generatedWorld.zSize; }

    /// <summary>
    /// X size of the world
    /// </summary>
    public int XSize { get => generatedWorld.xSize; }

    /// <summary>
    /// Size of one Tile
    /// </summary>
    public float TileSize { get => generatedWorld.TileScale; }

    /// <summary>
    /// The beings in each cell
    /// </summary>
    public List<LivingBeings>[,] LivingBeingsLayer { get => generatedWorld.livingLayer; }

    /// <summary>
    /// What type of tile it is on each cell
    /// </summary>
    public TileType[,] TileLayer { get => generatedWorld.tileLayer; }

    //---------------------------------------------------------------
    //Runs before first Update
    private void Start()
    {
        generatedWorld = this.GetComponent<WorldGenerator>();
        pathMaker = new PathMaker(20, this);
        SpawnAnimals();
        CreateStatusText();
    }

    //---------------------------------------------------------------
    //Spawn the animals
    private void SpawnAnimals()
    {
        shadowRealm = new Dictionary<Species, Stack<GameObject>>();
        dictionaryToAnimals = new Dictionary<Species, GameObject>();
        livingBeingsCategorized = new Dictionary<Species, List<LivingBeings>>();

        //Create a categorized dictionary for the livingBeings
        for (byte i = 0; i < System.Enum.GetNames(typeof(Species)).Length; i++)
        {
            livingBeingsCategorized.Add((Species.Plant + i), new List<LivingBeings>());

            //This is where the dead ones will go
            shadowRealm.Add((Species.Plant + i), new Stack<GameObject>());
        }

        //Add the plants to the dictionary which was generated with the world
        livingBeingsCategorized[Species.Plant] = generatedWorld.Plants;

        for (int i = 0; i < Animals.Length; i++)
        {
            //So it is easier to access when spawning new animals
            dictionaryToAnimals.Add(Animals[i].GetComponent<Animal>().Specie, Animals[i]);

            GameObject parentObj = new GameObject(Animals[i].name + "Parent");
            parentObj.transform.parent = this.transform;
            for (int j = 0; j < AnimalCount[i]; j++)
            {
                do
                {
                    int xIndex = UnityEngine.Random.Range(0, XSize);
                    int yIndex = UnityEngine.Random.Range(0, XSize);
                    if (moveLayer[xIndex, yIndex] == 1)
                    {
                        GameObject animal = Instantiate(Animals[i], parentObj.transform);
                        animal.transform.position = new Vector3(xIndex, 0.7f, yIndex);
                        LivingBeings being = animal.GetComponent<LivingBeings>();
                        livingBeingsCategorized[being.Specie].Add(being);
                        break;
                    }
                } while (true);
            }
        }
    }

    /// <summary>
    /// Spawn a new animal
    /// </summary>
    /// <param name="parent1">One of the parents</param>
    /// <param name="parent2">Other one of the parents</param>
    public void SpawnNewAnimal(Animal parent1, Animal parent2)
    {
        GameObject child;
        if (shadowRealm[parent1.Specie].Count > 0)
        {
            child = shadowRealm[parent1.Specie].Pop();
            child.gameObject.SetActive(true);
        }
        else
        {
            child = Instantiate(dictionaryToAnimals[parent1.Specie], parent1.transform.parent);
        }
        child.transform.position = new Vector3(parent1.XPos, parent1.YPos, parent1.ZPos);
        Animal animal = child.GetComponent<Animal>();
        animal.Born(parent1, parent2);
        livingBeingsCategorized[animal.Specie].Add(animal);
        statusTextsToSpecies[animal.Specie].text = animal.Specie.ToString() + " count: " + livingBeingsCategorized[animal.Specie].Count;
    }

    //------------------------------------------------------------------
    /// <summary>
    /// Kill the being and remove it from the grid
    /// </summary>
    /// <param name="beingToKill">The being which is to be killed</param>
    public void Kill(LivingBeings beingToKill)
    {
        livingBeingsCategorized[beingToKill.Specie].Remove(beingToKill);
        LivingBeingsLayer[beingToKill.XCoordOnGrid, beingToKill.YCoordOnGrid].Remove(beingToKill);
        shadowRealm[beingToKill.Specie].Push(beingToKill.gameObject);
        beingToKill.gameObject.SetActive(false);
    }

    //-------------------------------------------------------------------
    /// <summary>
    /// Remove the being from the specified index from the livingLayer
    /// </summary>
    /// <param name="xIndex">The x index</param>
    /// <param name="yIndex">The y index</param>
    /// <param name="livingBeing">The being to remove</param>
    public void RemoveFromLivingLayer(int xIndex, int yIndex, LivingBeings livingBeing)
    {
        statusTextsToSpecies[livingBeing.Specie].text = livingBeing.Specie.ToString() + " count: " + livingBeingsCategorized[livingBeing.Specie].Count;
        livingBeingsCategorized[livingBeing.Specie].Remove(livingBeing);
        this.LivingBeingsLayer[xIndex, yIndex].Remove(livingBeing);
    }

    //-------------------------------------------------------------------
    /// <summary>
    /// Add the being to the specified index from the livingLayer
    /// </summary>
    /// <param name="xIndex">The x index</param>
    /// <param name="yIndex">The y index</param>
    /// <param name="livingBeing">The being to add</param>
    public void AddToLivingLayer(int xIndex, int yIndex, LivingBeings livingBeing)
    {
        statusTextsToSpecies[livingBeing.Specie].text = livingBeing.Specie.ToString() + " count: " + livingBeingsCategorized[livingBeing.Specie].Count;
        livingBeingsCategorized[livingBeing.Specie].Add(livingBeing);
        this.LivingBeingsLayer[xIndex, yIndex].Add(livingBeing);
    }

    //------------------------------------------------------------------
    /// <summary>
    /// Move the animal to the next cell
    /// </summary>
    /// <param name="originalCoord">It's original position</param>
    /// <param name="newCoord">It's new position</param>
    /// <param name="animal">The animal to move</param>
    public void Move(Coord newCoord, Animal animal, ref int xPosInGrid, ref int yPosInGrid)
    {
        this.LivingBeingsLayer[xPosInGrid, yPosInGrid].Remove(animal);
        this.LivingBeingsLayer[newCoord.IntX, newCoord.IntY].Add(animal);
        xPosInGrid = newCoord.IntX;
        yPosInGrid = newCoord.IntY;
    }

    //------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Creates a new target
    /// </summary>
    /// <param name="targetType">What type of target we're looking for</param>
    /// <param name="seekingAnimal">The animal which is looking for a target</param>
    /// <param name="targetBeing">Reference to the being which may be selected as a target </param>
    /// <returns>The position of the target</returns>
    public Coord CreateNewTarget(ref TargetType targetType, Animal seekingAnimal, ref LivingBeings targetBeing)
    {
        switch (targetType)
        {
            case TargetType.Explore:
                return Explore(seekingAnimal);
                break;

            case TargetType.Water:
                return GetClosestTile(seekingAnimal, TileType.Water, ref targetType);
                break;

            default:

                return GetClosestLivingBeing(seekingAnimal, ref targetBeing, ref targetType);
                break;
        }
    }

    //------------------------------------------------------------------------------------------
    //Gets the closest tile to the animal
    private Coord GetClosestTile(Animal seekingAnimal, TileType tileType, ref TargetType targetType)
    {
        byte visionRange = seekingAnimal.RoundedVisionRange;
        int xCoord = seekingAnimal.XCoordOnGrid;
        int yCoord = seekingAnimal.YCoordOnGrid;

        float nearest = float.MaxValue;
        Coord nearestCoord = new Coord();

        for (int i = xCoord - visionRange; i < xCoord + visionRange; i++)
        {
            if (i < 0 || i >= generatedWorld.xSize)
                continue;

            for (int j = yCoord - visionRange; j < yCoord + visionRange; j++)
            {
                if (j < 0 || j >= generatedWorld.zSize)
                    continue;

                if (generatedWorld.tileLayer[i, j] == tileType)
                {
                    float dist = Coord.CalcDistance(i, j, seekingAnimal.XPos, seekingAnimal.ZPos);
                    if (dist < nearest)
                    {
                        nearest = dist;
                        nearestCoord.x = i;
                        nearestCoord.y = j;
                    }
                }
            }
        }

        if (nearest != float.MaxValue)
        {
            return nearestCoord;
        }
        else
        {
            targetType = TargetType.Explore;
            return Explore(seekingAnimal);
        }
    }

    //--------------------------------------------------------------------------------------------
    //Gets the closest livingBeing to the animal
    private Coord GetClosestLivingBeing(Animal seekingAnimal, ref LivingBeings targetBeing, ref TargetType targetType)
    {
        byte visionRange = seekingAnimal.RoundedVisionRange;
        int xCoord = seekingAnimal.XCoordOnGrid;
        int yCoord = seekingAnimal.YCoordOnGrid;
        TargetType localTarget = targetType;
        float nearest = float.MaxValue;
        Coord nearestCoord = new Coord();

        for (int i = xCoord - visionRange; i < xCoord + visionRange; i++)
        {
            if (i < 0 || i >= generatedWorld.xSize)
                continue;

            for (int j = yCoord - visionRange; j < yCoord + visionRange; j++)
            {
                if (j < 0 || j >= generatedWorld.zSize)
                    continue;

                if (generatedWorld.livingLayer[i, j].Find(x => x.FoodChainTier == (FoodChainTier)localTarget) != null)
                {
                    float dist = Coord.CalcDistance(i, j, seekingAnimal.XPos, seekingAnimal.ZPos);
                    if (dist < nearest)
                    {
                        nearest = dist;
                        nearestCoord.x = i;
                        nearestCoord.y = j;
                    }
                }
            }
        }

        if (nearest != float.MaxValue)
        {
            targetBeing = generatedWorld.livingLayer[nearestCoord.IntX, nearestCoord.IntY].Find(x => x.FoodChainTier == (FoodChainTier)localTarget);
            return nearestCoord;
        }
        else
        {
            targetType = TargetType.Explore;
            return Explore(seekingAnimal);
        }
    }

    //--------------------------------------------------------------------------------------------
    //Just explore the map get a random coordinate in viewDistance and set it as the target
    private Coord Explore(Animal seekingAnimal)
    {
        int counter = 0;
        int xPos = seekingAnimal.XCoordOnGrid;
        int yPos = seekingAnimal.YCoordOnGrid;
        int visionRange = seekingAnimal.RoundedVisionRange;
        do
        {
            int xIndex = UnityEngine.Random.Range(xPos - visionRange, xPos + visionRange);
            int yIndex = UnityEngine.Random.Range(yPos - visionRange, yPos + visionRange);
            if (generatedWorld.zSize > yIndex && yIndex >= 0 && generatedWorld.xSize > xIndex && xIndex >= 0 && moveLayer[xIndex, yIndex] == 1)
            {
                return new Coord(xIndex, yIndex);
            }
            if (counter >= 100)
            {
                return new Coord(seekingAnimal.XPos, seekingAnimal.ZPos);
            }
            counter++;
        } while (true);
    }

    //---------------------------------------------------------------------
    /// <summary>
    /// Creates a path to the target
    /// </summary>
    /// <param name="current">Current position</param>
    /// <param name="target">The target's position</param>
    /// <param name="targetType">Reference to the targetType so it can change it to Explore if there was no path to the target</param>
    /// <returns>The path</returns>
    public Stack<Coord> CreatePath(Coord current, Coord target, ref TargetType targetType)
    {
        Stack<Coord> path;
        //If making the path failed set the targetType to explore
        if (!pathMaker.CreatePath(out path, current, target))
        {
            targetType = TargetType.Explore;
        }
        return path;
    }

    //-----------------------------------------------------------------------------------------
    /// <summary>
    /// Gets the nearest cell to the goal
    /// </summary>
    /// <param name="current">The current position</param>
    /// <param name="target">The goal's position</param>
    /// <returns>The next move target</returns>
    public Coord GetClosestMoveTarget(Coord current, Coord target)
    {
        float leastDist = float.MaxValue;
        Coord moveTarget = current;
        float tempDist;
        int currentXIndex = (int)Mathf.Round(current.x);
        int currentYIndex = (int)Mathf.Round(current.y);
        //Left
        if (currentXIndex - 1f >= 0 && moveLayer[currentXIndex - 1, currentYIndex] == 1)
        {
            leastDist = Coord.CalcDistance(target.x, target.y, currentXIndex - 1f, currentYIndex);
            moveTarget = new Coord(currentXIndex - 1f, currentYIndex);
        }
        //Up
        if (currentYIndex - 1f >= 0 && moveLayer[currentXIndex, currentYIndex - 1] == 1)
        {
            tempDist = Coord.CalcDistance(target.x, target.y, currentXIndex, currentYIndex - 1.0f);
            if (tempDist < leastDist)
            {
                moveTarget = new Coord(currentXIndex, currentYIndex - 1.0f);
            }
        }
        //Right
        if (currentXIndex + 1 < generatedWorld.xSize && moveLayer[currentXIndex + 1, currentYIndex] == 1)
        {
            tempDist = Coord.CalcDistance(target.x, target.y, currentXIndex + 1.0f, currentYIndex);
            if (tempDist < leastDist)
            {
                moveTarget = new Coord(currentXIndex + 1.0f, currentYIndex);
            }
        }
        //Bottom
        if (currentYIndex + 1 < generatedWorld.zSize && moveLayer[currentXIndex, currentYIndex + 1] == 1)
        {
            tempDist = Coord.CalcDistance(target.x, target.y, currentXIndex, currentYIndex + 1.0f);
            if (tempDist < leastDist)
            {
                moveTarget = new Coord(currentXIndex, currentYIndex + 1.0f);
            }
        }

        return moveTarget;
    }

    //-----------------------------------------------------------------------------------------
    /// <summary>
    /// Gets the furthest cell to the goal
    /// </summary>
    /// <param name="current">The current position</param>
    /// <param name="target">The goal's position</param>
    /// <returns>The next move target</returns>
    public Coord GetFurthestMoveTarget(Coord current, Coord target)
    {
        float leastDist = float.MinValue;
        Coord moveTarget = current;
        float tempDist;
        int currentXIndex = (int)Mathf.Round(current.x);
        int currentYIndex = (int)Mathf.Round(current.y);
        //Left
        if (currentXIndex - 1f >= 0 && moveLayer[currentXIndex - 1, currentYIndex] == 1)
        {
            leastDist = Coord.CalcDistance(target.x, target.y, currentXIndex - 1f, currentYIndex);
            moveTarget = new Coord(currentXIndex - 1f, currentYIndex);
        }
        //Up
        if (currentYIndex - 1f >= 0 && moveLayer[currentXIndex, currentYIndex - 1] == 1)
        {
            tempDist = Coord.CalcDistance(target.x, target.y, currentXIndex, currentYIndex - 1.0f);
            if (tempDist > leastDist)
            {
                moveTarget = new Coord(currentXIndex, currentYIndex - 1.0f);
            }
        }
        //Right
        if (currentXIndex + 1 < generatedWorld.xSize && moveLayer[currentXIndex + 1, currentYIndex] == 1)
        {
            tempDist = Coord.CalcDistance(target.x, target.y, currentXIndex + 1.0f, currentYIndex);
            if (tempDist > leastDist)
            {
                moveTarget = new Coord(currentXIndex + 1.0f, currentYIndex);
            }
        }
        //Bottom
        if (currentYIndex + 1 < generatedWorld.zSize && moveLayer[currentXIndex, currentYIndex + 1] == 1)
        {
            tempDist = Coord.CalcDistance(target.x, target.y, currentXIndex, currentYIndex + 1.0f);
            if (tempDist > leastDist)
            {
                moveTarget = new Coord(currentXIndex, currentYIndex + 1.0f);
            }
        }

        return moveTarget;
    }

    //--------------------------------------------------------------------------------
    /// <summary>
    /// Asks out the nearby animals so it can reproduce
    /// </summary>
    /// <param name="theOneAskingOut">The animal which wants to reproduce</param>
    /// <param name="range">The range of it's asking out</param>
    /// <param name="theOneWhichAccepted">The animal which accepted it's advances</param>
    /// <returns>true if he/she were successful, false if he/she failed</returns>
    public bool AskOutNearbyAnimals(Animal theOneAskingOut, byte range, ref LivingBeings theOneWhichAccepted)
    {
        int xCoord = theOneAskingOut.XCoordOnGrid;
        int yCoord = theOneAskingOut.YCoordOnGrid;
        for (int i = xCoord - range; i < xCoord + range; i++)
        {
            if (i < 0 || i >= generatedWorld.xSize)
                continue;

            for (int j = yCoord - range; j < yCoord + range; j++)
            {
                if (j < 0 || j >= generatedWorld.zSize)
                    continue;
                //Check if there is an animal which accepts it in the cell
                theOneWhichAccepted = generatedWorld.livingLayer[i, j].Find(x => x.Specie == theOneAskingOut.Specie && ((Animal)x).Gender != theOneAskingOut.Gender && ((Animal)x).GetAskedOut(theOneAskingOut));
                if (theOneWhichAccepted != null)
                {
                    return true;
                }
            }
        }

        return false;
    }

    //--------------------------------------------------------------------
    /// <summary>
    /// Creates the status texts which will display the number of living beings
    /// </summary>
    private void CreateStatusText()
    {
        statusTextsToSpecies = new Dictionary<Species, Text>();
        GameObject parentObj = Camera.main.transform.GetChild(0).transform.GetChild(0).gameObject;

        for (byte i = 0; i < System.Enum.GetNames(typeof(Species)).Length; i++)
        {
            GameObject textObj = new GameObject((Species.Plant + i).ToString() + "CountText");
            textObj.transform.SetParent(parentObj.transform);

            Text text = textObj.AddComponent<Text>();
            RectTransform rectTransform = textObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1f);
            rectTransform.anchorMax = new Vector2(0, 1f);

            rectTransform.anchoredPosition = new Vector3(100, -50 + -20 * i, 0);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 200);
            statusTextsToSpecies.Add((Species.Plant + i), textObj.GetComponent<Text>());
            text.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            text.color = Color.black;
        }
        UpdateStatusTexts();
    }

    //-------------------------------------------------------------------------
    /// <summary>
    /// Updates all of the status texts
    /// </summary>
    private void UpdateStatusTexts()
    {
        for (byte i = 0; i < System.Enum.GetNames(typeof(Species)).Length; i++)
        {
            statusTextsToSpecies[(Species.Plant + i)].text = (Species.Plant + i).ToString() + " count: " + livingBeingsCategorized[(Species.Plant + i)].Count;
        }
    }

    //-------------------------------------------------------------
    //Debug Gizmos
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f);
        for (int i = 0; i < XSize; i++)
        {
            for (int j = 0; j < YSize; j++)
            {
                if (generatedWorld.livingLayer[i, j].Find(x => x.Specie == Species.Plant) != null)
                {
                    Gizmos.color = new Color(0f, 1f, 0f);
                    Gizmos.DrawSphere(new Vector3(i, 1f, j), 0.1f);
                }
                else if (generatedWorld.livingLayer[i, j].Find(x => x.Specie == Species.Chicken) != null)
                {
                    Gizmos.color = new Color(1f, 1f, 1f);
                    Gizmos.DrawSphere(new Vector3(i, 1f, j), 0.1f);
                }
            }
        }
    }
}