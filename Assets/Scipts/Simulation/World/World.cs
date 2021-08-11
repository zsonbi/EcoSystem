using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

/// <summary>
/// The world where the simulation is going on
/// </summary>
public class World : MonoBehaviour
{
    [Header("Animals to spawn")]
    public GameObject[] Animals;

    [Header("How many animals to spawn")]
    public int[] AnimalCount;

    [Header("Does animals have stats above their heads")]
    public bool ShowStatBars = false;

    private WorldGenerator generatedWorld; //The world which was generated at the start

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

        SpawnAnimals();
    }

    //---------------------------------------------------------------
    //Spawn the animals
    private void SpawnAnimals()
    {
        for (int i = 0; i < Animals.Length; i++)
        {
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
                        break;
                    }
                } while (true);
            }
        }
    }

    //------------------------------------------------------------------
    /// <summary>
    /// Kill the being and remove it from the grid
    /// </summary>
    /// <param name="beingToKill">The being which is to be killed</param>
    public void Kill(LivingBeings beingToKill)
    {
        LivingBeingsLayer[beingToKill.XCoordOnGrid, beingToKill.YCoordOnGrid].Remove(beingToKill);
        Destroy(beingToKill.gameObject);
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
        this.LivingBeingsLayer[xIndex, yIndex].Add(livingBeing);
    }

    //------------------------------------------------------------------
    /// <summary>
    /// Move the animal to the next cell
    /// </summary>
    /// <param name="originalCoord">It's original position</param>
    /// <param name="newCoord">It's new position</param>
    /// <param name="animal">The animal to move</param>
    public void Move(Coord originalCoord, Coord newCoord, Animal animal)
    {
        this.LivingBeingsLayer[originalCoord.IntX, originalCoord.IntY].Remove(animal);
        this.LivingBeingsLayer[newCoord.IntX, newCoord.IntY].Add(animal);
    }

    //---------------------------------------------------------------------
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

    //---------------------------------------------------------------------
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

    //--------------------------------------------------------------------
    //Gets the closest livingBeing to the animal
    private Coord GetClosestLivingBeing(Animal seekingAnimal, ref LivingBeings targetBeing, ref TargetType targetType)
    {
        Species specie = (Species.Plant + (byte)targetType);
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

                if (generatedWorld.livingLayer[i, j].Find(x => x.Specie == specie) != null)
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
            targetBeing = generatedWorld.livingLayer[nearestCoord.IntX, nearestCoord.IntY].Find(x => x.Specie == specie);
            return nearestCoord;
        }
        else
        {
            targetType = TargetType.Explore;
            return Explore(seekingAnimal);
        }
    }

    //---------------------------------------------------------------------
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
                return new Coord(xIndex * TileSize, yIndex * TileSize);
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
    /// <returns></returns>
    public Stack<Coord> CreatePath(Coord current, Coord target)
    {
        PathMaker pathMaker = new PathMaker(current, target, 20, this);

        return pathMaker.CreatePath();
    }

    public Coord GetBestMoveTarget(Coord current, Coord target)
    {
        float leastDist = float.MaxValue;
        Coord moveTarget = null;
        float tempDist;
        int currentXIndex = (int)Mathf.Round(current.x);
        int currentYIndex = (int)Mathf.Round(current.y);

        if (currentXIndex - 1f >= 0 && moveLayer[currentXIndex - 1, currentYIndex] == 1)
        {
            leastDist = Coord.CalcDistance(target.x, target.y, currentXIndex - 1f, currentYIndex);
            moveTarget = new Coord(currentXIndex - 1f, currentYIndex);
        }
        if (currentYIndex - 1f >= 0 && moveLayer[currentXIndex, currentYIndex - 1] == 1)
        {
            tempDist = Coord.CalcDistance(target.x, target.y, currentXIndex, currentYIndex - 1.0f);
            if (tempDist < leastDist)
            {
                moveTarget = new Coord(currentXIndex, currentYIndex - 1.0f);
            }
        }
        if (currentXIndex + 1 < generatedWorld.xSize && moveLayer[currentXIndex + 1, currentYIndex] == 1)
        {
            tempDist = Coord.CalcDistance(target.x, target.y, currentXIndex + 1.0f, currentYIndex);
            if (tempDist < leastDist)
            {
                moveTarget = new Coord(currentXIndex + 1.0f, currentYIndex);
            }
        }
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

                List<LivingBeings> sameSpecies = generatedWorld.livingLayer[i, j].FindAll(x => x.Specie == theOneAskingOut.Specie && ((Animal)x).Gender != theOneAskingOut.Gender);
                for (int listIndex = 0; listIndex < sameSpecies.Count; listIndex++)
                {
                    if (((Animal)sameSpecies[listIndex]).GetAskedOut())
                    {
                        theOneWhichAccepted = sameSpecies[listIndex];
                        return true;
                    }
                }
            }
        }

        return false;
    }

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