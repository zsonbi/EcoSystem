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

    private List<byte[,]> speciesLayers = new List<byte[,]>();
    private WorldGenerator generatedWorld;
    private Dictionary<TargetType, byte[,]> targetTypeToLayers = new Dictionary<TargetType, byte[,]>();
    private Dictionary<Species, byte[,]> speciesToLayers = new Dictionary<Species, byte[,]>();

    //Layers

    public byte[,] waterLayer { get => generatedWorld.layers[0]; }
    public byte[,] moveLayer { get => generatedWorld.layers[1]; }
    public byte[,] plantLayer { get => generatedWorld.layers[3]; }
    public byte[,] chickenLayer { get => generatedWorld.layers[4]; }

    public int YSize { get => generatedWorld.zSize; }

    public int XSize { get => generatedWorld.xSize; }

    public float TileSize { get => generatedWorld.TileScale; }

    private void Start()
    {
        generatedWorld = this.GetComponent<WorldGenerator>();

        SetupDictionaries();

        SpawnAnimals();
    }

    private void SetupDictionaries()
    {
        //Create the speciesLayers and add them to the dictionary
        for (int i = 0; i <= Animals.Length; i++)
        {
            speciesLayers.Add(new byte[generatedWorld.xSize, generatedWorld.zSize]);
        }
        speciesLayers[0] = plantLayer;
        for (byte i = 1; i <= Animals.Length; i++)
        {
            speciesToLayers.Add(Species.Plant + i, speciesLayers[i]);
        }

        //Manually adding the targetTypes to the dictionary (haven't found a better way :C)
        targetTypeToLayers.Add(TargetType.Water, waterLayer);
        targetTypeToLayers.Add(TargetType.Plant, plantLayer);
        targetTypeToLayers.Add(TargetType.Explore, moveLayer);
        targetTypeToLayers.Add(TargetType.Chicken, speciesToLayers[Species.Chicken]);
    }

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

    public void Kill(LivingBeings beingToKill)
    {
        Destroy(beingToKill);
    }

    public Coord CreateNewTarget(TargetType targetType, Animal seekingAnimal)
    {
        if (TargetType.Explore == targetType)
        {
            return Explore(seekingAnimal);
        }

        byte visionRange = seekingAnimal.RoundedVisionRange;
        int xCoord = seekingAnimal.XCoordOnGrid;
        int yCoord = seekingAnimal.YCoordOnGrid;

        float nearest = float.MaxValue;
        Coord nearestCoord = new Coord();

        for (int i = xCoord - visionRange; i < xCoord + visionRange; i++)
        {
            if (i < 0 || i >= generatedWorld.xSize)
            {
                continue;
            }
            for (int j = yCoord - visionRange; j < yCoord + visionRange; j++)
            {
                if (j < 0 || j >= generatedWorld.zSize)
                {
                    continue;
                }
                if (targetTypeToLayers[targetType][i, j] == 1)
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
        //The targetType is needed to avoid possible StackOverflowException
        if (nearest != float.MaxValue || targetType == TargetType.Explore)
            return nearestCoord;
        else
            return Explore(seekingAnimal);
    }

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

    public Stack<Coord> CreatePath(Coord current, Coord target)
    {
        PathMaker pathMaker = new PathMaker(current, target, 20, this);

        return pathMaker.CreatePath();
        //if (Coord.CalcDistance(current, target) <= 0.5f)
        //{
        //    Stack<Coord> path = new Stack<Coord>();
        //    path.Push(current);
        //    return path;
        //}
        //else
        //{
        //    foreach (var moveTarget in GetPossibleMoveTargets(current, target).OrderBy(x => x.DistanceFromGoal))
        //    {
        //        Stack<Coord> path = CreatePath(moveTarget.MoveTarget, target);
        //        if (path != null)
        //        {
        //            path.Push(current);
        //        }
        //    }
        //    return null;
        //}
    }

    public List<Node> GetPossibleMoveTargets(Coord current, Coord target)
    {
        float dist = float.MaxValue;
        List<Node> moveTargets = new List<Node>();

        if (current.x - 0.5f >= 0 && moveLayer[(int)Mathf.Round(current.x - 0.5f), Convert.ToInt32(current.y)] == 1)
        {
            dist = Coord.CalcDistance(target.x, target.y, current.x - 0.5f, current.y);
            moveTargets.Add(new Node(new Coord(current.x - 0.5f, current.y), dist));
        }
        if (current.y - 0.5f >= 0 && moveLayer[Convert.ToInt32(current.x), (int)Mathf.Round(current.y - 0.5f)] == 1)
        {
            dist = Coord.CalcDistance(target.x, target.y, current.x, current.y - 0.5f);
            //if (temp < least)
            //{ }
            moveTargets.Add(new Node(new Coord(current.x, current.y - 0.5f), dist));
        }
        if (current.x + 1 < generatedWorld.xSize && moveLayer[(int)Mathf.Round(current.x + 0.5f), Convert.ToInt32(current.y)] == 1)
        {
            dist = Coord.CalcDistance(target.x, target.y, current.x + 0.5f, current.y);
            //if (temp < least)
            //{ }
            moveTargets.Add(new Node(new Coord(current.x + 0.5f, current.y), dist));
        }
        if (current.y + 1 < generatedWorld.zSize && moveLayer[Convert.ToInt32(current.x), (int)Mathf.Round(current.y + 0.5f)] == 1)
        {
            dist = Coord.CalcDistance(target.x, target.y, current.x, current.y + 0.5f);
            //if (temp < least)
            //{ }
            moveTargets.Add(new Node(new Coord(current.x, current.y + 0.5f), dist));
        }

        return moveTargets;
    }
}