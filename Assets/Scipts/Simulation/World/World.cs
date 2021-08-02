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

    private WorldGenerator generatedWorld;

    public byte[,] moveLayer { get => generatedWorld.layers[0]; }

    public int YSize { get => generatedWorld.zSize; }

    public int XSize { get => generatedWorld.xSize; }

    public float TileSize { get => generatedWorld.TileScale; }
    public List<LivingBeings>[,] LivingBeingsLayer { get => generatedWorld.livingLayer; }
    public TileType[,] TileLayer { get => generatedWorld.tileLayer; }

    private void Start()
    {
        generatedWorld = this.GetComponent<WorldGenerator>();

        SpawnAnimals();
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
        LivingBeingsLayer[beingToKill.XCoordOnGrid, beingToKill.YCoordOnGrid].Remove(beingToKill);
        Destroy(beingToKill.gameObject);
    }

    public void RemoveFromLivingLayer(int xIndex, int yIndex, LivingBeings livingBeing)
    {
        this.LivingBeingsLayer[xIndex, yIndex].Remove(livingBeing);
    }

    public void AddToLivingLayer(int xIndex, int yIndex, LivingBeings livingBeing)
    {
        this.LivingBeingsLayer[xIndex, yIndex].Add(livingBeing);
    }

    public Coord CreateNewTarget(TargetType targetType, Animal seekingAnimal, ref LivingBeings targetBeing)
    {
        switch (targetType)
        {
            case TargetType.Explore:
                return Explore(seekingAnimal);
                break;

            case TargetType.Water:
                return GetClosestTile(seekingAnimal, TileType.Water);
                break;

            default:

                Species specie = (Species.Plant + (byte)targetType);
                return GetClosestLivingBeing(seekingAnimal, specie, ref targetBeing);
                break;
        }
    }

    private Coord GetClosestTile(Animal seekingAnimal, TileType tileType)
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
            return Explore(seekingAnimal);
    }

    private Coord GetClosestLivingBeing(Animal seekingAnimal, Species specie, ref LivingBeings targetBeing)
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
    }
}