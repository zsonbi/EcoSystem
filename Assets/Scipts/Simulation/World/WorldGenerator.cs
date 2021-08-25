using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Used for generating a new world
/// </summary>
public class WorldGenerator : MonoBehaviour
{
    [Header("Tiles")]
    public GameObject[] Tiles;

    [Header("Props")]
    public GameObject[] Props;

    [Header("Tile scale")]
    public int TileScale = 1;

    [Header("Map z size")]
    public int zSize = 100;

    [Header("Map x size")]
    public int xSize = 100;

    [Header("Offset of perlin noise on X axis")]
    public float XOffset = 0f;

    [Header("Offset of perlin noise on Z axis")]
    public float ZOffset = 0f;

    [Header("Randomize Offset")]
    public bool RandomizeOffset = false;

    //------------------------------------------
    [HideInInspector]
    public List<byte[,]> layers = new List<byte[,]>(); //The layers used for navigation and other stuff

    [HideInInspector]
    public List<LivingBeings> Plants = new List<LivingBeings>();

    public TileType[,] tileLayer;
    public List<LivingBeings>[,] livingLayer;

    //private byte[,] waterLayer { get => layers[0]; } //1 for water tiles
    private byte[,] moveLayer { get => layers[0]; } //1 for passable tiles

    //private byte[,] grassLayer { get => layers[2]; } //1 for grass tiles used for the prop generation
    //private byte[,] plantLayer { get => layers[3]; } //1 where the plants are

    private byte numberOfDifferentObjects;
    private List<List<GameObject>> objectsToCombine;
    private GameObject plantParent;

    /// <summary>
    /// Adds the layers to the layers list
    /// </summary>
    /// <param name="numberOfLayers">the number of layers we want to add</param>
    private void AddLayers(byte numberOfLayers)
    {
        for (int i = 0; i < numberOfLayers; i++)
        {
            layers.Add(new byte[xSize, zSize]);
        }
    }

    // Start is called before the first frame update
    private void Awake()
    {
        //Cap the fps to 60
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        this.tileLayer = new TileType[xSize, zSize];
        this.livingLayer = new List<LivingBeings>[xSize, zSize];
        plantParent = new GameObject("plantParent");
        this.plantParent.transform.parent = this.transform;
        for (int x = 0; x < xSize; x++)
        {
            for (int z = 0; z < zSize; z++)
            {
                livingLayer[x, z] = new List<LivingBeings>();
            }
        }

        AddLayers(6);
        //Randomizes the offset
        if (RandomizeOffset)
        {
            this.XOffset = Random.Range(0, 99999);
            this.ZOffset = Random.Range(0, 99999);
        }

        //Scale up the tiles
        foreach (var item in Tiles)
        {
            item.transform.localScale = new Vector3(TileScale, TileScale, TileScale);
        }

        //Get the number of different objects we will have and make the list
        numberOfDifferentObjects = (byte)(Tiles.Length + Props.Length);
        objectsToCombine = new List<List<GameObject>>();

        for (int i = 0; i < numberOfDifferentObjects; i++)
        {
            objectsToCombine.Add(new List<GameObject>());
        }
        //Create the tiles
        CreateTiles();
        //Create the props
        AddEnviromentObjects();
        //Combine the objects
        for (byte i = 0; i < numberOfDifferentObjects; i++)
        {
            if (objectsToCombine[i].Count != 0)
                CombineMeshes(i);
        }
    }

    //---------------------------------------------------------------------------
    // <summary>
    // Creates the tiles for the world
    private void CreateTiles()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int z = 0; z < zSize; z++)
            {
                GameObject clone;
                TileType tileType = DetermineTileType(x, z);
                switch (tileType)
                {
                    //When it is a water tile
                    case TileType.Water:
                        clone = Instantiate(Tiles[(byte)TileType.Water], this.transform);
                        clone.transform.position = new Vector3(x, 0f, z) * TileScale;
                        objectsToCombine[(byte)TileType.Water].Add(clone);
                        moveLayer[x, z] = 0;
                        break;
                    //When it is a grass tile
                    case TileType.Grass:
                        clone = Instantiate(Tiles[(byte)TileType.Grass], this.transform);
                        clone.transform.position = new Vector3(x, 0.2f, z) * TileScale;
                        objectsToCombine[(byte)TileType.Grass].Add(clone);
                        moveLayer[x, z] = 1;

                        break;
                    //When it is a sand tile
                    case TileType.Sand:
                        clone = Instantiate(Tiles[(byte)TileType.Sand], this.transform);
                        clone.transform.position = new Vector3(x, 0.2f, z) * TileScale;
                        objectsToCombine[(byte)TileType.Sand].Add(clone);
                        moveLayer[x, z] = 1;
                        break;

                    default:
                        break;
                }
                tileLayer[x, z] = tileType;
            }
        }
    }

    //------------------------------------------------------------------
    //Adds the props to the world
    private void AddEnviromentObjects()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int z = 0; z < zSize; z++)
            {
                GameObject clone;
                switch (DetermineEnviromentType(x, z))
                {
                    //When the prop is a tree
                    case EnviromentType.Tree:
                        clone = Instantiate(Props[(byte)EnviromentType.Tree], this.transform);
                        moveLayer[x, z] = 0;
                        objectsToCombine[(byte)EnviromentType.Tree + 3].Add(clone);
                        clone.transform.position = new Vector3(x, 0.6f, z) * TileScale;
                        break;
                    //When the prop is a plant
                    case EnviromentType.Plant:
                        clone = Instantiate(Props[(byte)EnviromentType.Plant], this.transform);
                        //  objectsToCombine[(byte)EnviromentType.Plant + 3].Add(clone);
                        livingLayer[x, z].Add(clone.GetComponent<LivingBeings>());
                        Plants.Add(clone.GetComponent<LivingBeings>());
                        clone.transform.position = new Vector3(x, 0.7f, z) * TileScale;
                        clone.transform.parent = plantParent.transform;
                        break;
                    //When the prop is a rock and you don't even need to be afraid of the malphite ult
                    case EnviromentType.Rock:
                        clone = Instantiate(Props[(byte)EnviromentType.Rock], this.transform);
                        objectsToCombine[(byte)EnviromentType.Rock + 3].Add(clone);
                        moveLayer[x, z] = 0;
                        clone.transform.position = new Vector3(x, 1f, z) * TileScale;
                        break;

                    default:
                        continue;//Useless like me, but better be safe than sorry just ask my dad
                }
            }
        }
    }

    //--------------------------------------------------------------
    /// <summary>
    /// Combines the gameobjects into meshes and cleares out that index of the objectsToCombine
    /// </summary>
    /// <param name="index">the index of the objectsToCombine we want to combine</param>
    private void CombineMeshes(byte index)
    {
        //Creates the parent which will have the combined mesh
        GameObject parent = new GameObject(objectsToCombine[index][0].name.Replace("(Clone)", "") + "Mesh", typeof(MeshFilter), typeof(MeshRenderer));
        MeshFilter meshFilter = parent.GetComponent<MeshFilter>();
        //Assign the material of the first gameobject in the list to the parent
        parent.GetComponent<MeshRenderer>().material = objectsToCombine[index][0].GetComponent<MeshRenderer>().material;
        parent.transform.parent = this.transform;

        meshFilter.mesh = new Mesh();
        //Makes so that really big meshes are supported also
        meshFilter.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        CombineInstance[] combine = new CombineInstance[objectsToCombine[index].Count];
        for (int i = 0; i < objectsToCombine[index].Count; i++)
        {
            MeshFilter objectMeshFilter = objectsToCombine[index][i].GetComponent<MeshFilter>();
            combine[i].mesh = objectMeshFilter.sharedMesh;
            combine[i].transform = objectMeshFilter.transform.localToWorldMatrix;
        }

        //Empties the list and deletes the no longer used gameobjects
        do
        {
            Destroy(objectsToCombine[index][0]);
            objectsToCombine[index].RemoveAt(0);
        } while (objectsToCombine[index].Count > 0);

        meshFilter.mesh.CombineMeshes(combine, true, true);
        meshFilter.gameObject.SetActive(true);
    }

    //-------------------------------------------------------------------
    /// <summary>
    /// Determines the type of the Enviroment at the coord f.e. tree, rock, plant
    /// </summary>
    /// <param name="x">x coord where we want to sample the perlin noise</param>
    /// <param name="z">z coord where we want to sample the perlin noise</param>
    /// <returns>the proper EnviromentType</returns>
    private EnviromentType DetermineEnviromentType(int x, int z)
    {
        float noise = Mathf.PerlinNoise((float)(x * 0.9f) + XOffset, (float)(z * 0.9f) + ZOffset);
        if (noise > 0.9f && tileLayer[x, z] != TileType.Water)
        {
            return EnviromentType.Rock;
        }
        else if (noise > 0.75f && tileLayer[x, z] == TileType.Grass)
        {
            return EnviromentType.Tree;
        }
        else if (noise > 0.60f && tileLayer[x, z] == TileType.Grass)
        {
            return EnviromentType.Plant;
        }
        else
        {
            return EnviromentType.Empty;
        }
    }

    //-------------------------------------------------------------------------
    /// <summary>
    /// Determines the type of the tile at the coord
    /// </summary>
    /// <param name="x">x coord where we want to sample the perlin noise</param>
    /// <param name="z">z coord where we want to sample the perlin noise</param>
    /// <returns>the proper TileType</returns>
    private TileType DetermineTileType(int x, int z)
    {
        float noise = Mathf.PerlinNoise((float)(x / (float)xSize * 5f) + XOffset, (float)(z / (float)zSize * 5f) + ZOffset) * 10f - 3.8f;
        if (noise > 0.45f)
        {
            return TileType.Grass;
        }
        else if (noise > 0.1f)
        {
            return TileType.Sand;
        }
        else
        {
            return TileType.Water;
        }
    }
}