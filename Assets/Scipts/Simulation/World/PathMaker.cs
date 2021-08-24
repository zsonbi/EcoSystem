using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// This class can create a path to the specified target
/// </summary>
public class PathMaker
{
    /// <summary>
    /// Each cell in the grid is a node
    /// </summary>
    private class Node
    {
        public float DistanceFromGoal { get; set; } //Distance from the target
        public bool Visited; //Was it already visited before
        public bool passable; //Is it passable
        public Coord position; //It's position
        public byte[] indices = new byte[2]; //The index it is in the grid

        //----------------------------------------------------------------------
        /// <summary>
        /// Creates a new instance of it
        /// </summary>
        /// <param name="xPos">The x position in the world</param>
        /// <param name="yPos">The y position in the world</param>
        /// <param name="passable">Is it passable</param>
        /// <param name="indices">It's position in the grid</param>
        public Node(int xPos, int yPos, bool passable, byte[] indices)
        {
            this.passable = passable;
            this.position = new Coord(xPos, yPos);
            this.Visited = false;
            this.DistanceFromGoal = float.MaxValue;
            this.indices = indices;
        }

        //-------------------------------------------------------
        /// <summary>
        /// Creates an empty Cell
        /// </summary>
        public Node()
        {
            this.passable = false;
        }

        //-------------------------------------------------------
        /// <summary>
        /// Sets the distance
        /// </summary>
        /// <param name="dist">The distance to the target</param>
        public void SetDist(float dist)
        {
            this.DistanceFromGoal = dist;
        }

        //--------------------------------------------------------
        /// <summary>
        /// Sets the visited property
        /// </summary>
        public void Visit()
        {
            this.Visited = true;
        }
    }

    private const byte maxDepth = 15;
    private Node[,] map; //The grid
    private World world; //Reference to the world
    private Stack<Coord> path; //Path which it will return
    private Node start; //Start of the path
    private Node goal; //The target

    /// <summary>
    /// Creates a new instance of the PathMaker
    /// </summary>
    /// <param name="start">The starting position</param>
    /// <param name="goal">The target's position</param>
    /// <param name="Size">The size of the grid (x size = Size *2, y size = Size*2)</param>
    /// <param name="world">The world it is in</param>
    public PathMaker(Coord start, Coord goal, byte Size, World world)
    {
        this.world = world;
        int startXCoord = (int)Mathf.Round(start.x) - Size;
        int startYCoord = (int)Mathf.Round(start.y) - Size;

        map = new Node[Size * 2, Size * 2];
        //Fill in the grid
        for (byte i = 0; i < map.GetLength(0); i++)
        {
            for (byte j = 0; j < map.GetLength(1); j++)
            {
                int xIndex = i + startXCoord;
                int yIndex = j + startYCoord;
                if (xIndex >= 0 && xIndex < world.XSize && yIndex >= 0 && yIndex < world.YSize)
                    map[i, j] = new Node(xIndex, yIndex, world.moveLayer[xIndex, yIndex] == 1, new byte[] { i, j });
                else
                    map[i, j] = new Node();
            }
        }
        //try
        //{
        //Set the goal and the start
        this.goal = map[goal.IntX - startXCoord, goal.IntY - startYCoord];
        this.goal.passable = true; //If it is water make it passable (maybe someday I will make this better)
        this.start = map[start.IntX - startXCoord, start.IntY - startYCoord];
        //}
        //catch (System.Exception)
        //{
        //    Debug.Log("The buggy coords were: " + goal.ToStringWholeCoords() + start.ToStringWholeCoords() + " startXCoord: " + startXCoord + " startYCoord: " + startYCoord);
        //    throw;
        //}
    }

    //---------------------------------------------------------------------
    /// <summary>
    /// Creates a new path
    /// </summary>
    /// <param name="moveTargetStack">The stack which will store the coord toward the target</param>
    /// <returns>False if there can't be a path made True if it was a success</returns>
    public bool CreatePath(out Stack<Coord> moveTargetStack)
    {
        path = new Stack<Coord>();
        //Search for the goal
        if (!SearchForGoal(start, 0))
        {
            moveTargetStack = new Stack<Coord>();
            return false;
        }
        moveTargetStack = path;
        return true;
    }

    //-------------------------------------------------------------
    //Checks if the node is valid for being part of the path
    private bool CheckIfValid(Node node)
    {
        return node.passable && !node.Visited;
    }

    //------------------------------------------------------------
    //Search for the node recursively
    //Returns true if it managed to find the goal, returns false if it didn't manage to find the goal
    private bool SearchForGoal(Node current, byte depth)
    {
        current.Visit(); //Visit the node
        //Check if it is the goal
        if (current.Equals(goal))
        {
            return true;
        }
        //So it doesn't make a path which kills the animal in the process
        if (depth >= maxDepth)
        {
            return false;
        }

        //Get the possible ways it can go
        List<Node> possibleWays = new List<Node>();
        if (current.indices[0] - 1 >= 0 && CheckIfValid(map[current.indices[0] - 1, current.indices[1]]))
        {
            possibleWays.Add(map[current.indices[0] - 1, current.indices[1]]);
        }

        if (current.indices[1] - 1 >= 0 && CheckIfValid(map[current.indices[0], current.indices[1] - 1]))
        {
            possibleWays.Add(map[current.indices[0], current.indices[1] - 1]);
        }

        if (current.indices[0] + 1 < map.GetLength(0) && CheckIfValid(map[current.indices[0] + 1, current.indices[1]]))
        {
            possibleWays.Add(map[current.indices[0] + 1, current.indices[1]]);
        }
        if (current.indices[1] + 1 < map.GetLength(1) && CheckIfValid(map[current.indices[0], current.indices[1] + 1]))
        {
            possibleWays.Add(map[current.indices[0], current.indices[1] + 1]);
        }
        //Calculate their distance from the goal
        for (int i = 0; i < possibleWays.Count; i++)
        {
            possibleWays[i].SetDist(Coord.CalcDistance(goal.position, possibleWays[i].position));
        }

        //Order them by the distance and try them each
        foreach (var item in possibleWays.OrderBy(x => x.DistanceFromGoal))
        {
            if (SearchForGoal(item, (byte)(depth + 1)))
            {
                path.Push(item.position);
                return true;
            }
        }
        //If none of the ways were good return false
        return false;
    }
}