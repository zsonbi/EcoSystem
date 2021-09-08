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
        public float DistanceFromGoal { get; private set; } //Distance from the target
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
        public Node(byte[] indices)
        {
            this.indices = indices;
            this.position = new Coord();
        }

        public Node(byte[] indices, int xPos, int yPos, bool passable)
        {
            this.indices = indices;
            this.passable = passable;

            this.position = new Coord((float)xPos, (float)yPos);
            this.Visited = false;
            this.DistanceFromGoal = float.MaxValue;
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

        public void Reset(int xPos, int yPos, bool passable)
        {
            this.passable = passable;

            this.position.SetCoord((float)xPos, (float)yPos);
            this.Visited = false;
            this.DistanceFromGoal = float.MaxValue;
        }

        public void Reset()
        {
            this.passable = false;
        }

        public void Reset(bool passable)
        {
            this.passable = passable;
            this.Visited = false;
        }
    }

    private const byte maxDepth = 24;
    private Node[,] map; //The grid
    private World world; //Reference to the world
    private Stack<Coord> path; //Path which it will return
    private Node start; //Start of the path
    private Node goal; //The target
    private byte size; //The size of the allocated map
    private Stack<Node> visitedNodes = new Stack<Node>();

    /// <summary>
    /// Creates a new instance of the PathMaker
    /// </summary>
    /// <param name="start">The starting position</param>
    /// <param name="goal">The target's position</param>
    /// <param name="size">The size of the grid (x size = Size *2, y size = Size*2)</param>
    /// <param name="world">The world it is in</param>
    public PathMaker(byte size, World world)
    {
        this.world = world;
        this.size = size;
        map = new Node[world.XSize, world.YSize];
        //Fill in the grid
        for (byte i = 0; i < map.GetLength(0); i++)
        {
            for (byte j = 0; j < map.GetLength(1); j++)
            {
                map[i, j] = new Node(new byte[] { i, j }, i, j, world.moveLayer[i, j] == 1);
            }
        }
    }

    //-------------------------------------------------------------
    /// <summary>
    /// Resets the goal and start back to what it should be and reverses the visited nodes state to not visited
    /// </summary>
    private void ResetBack()
    {
        while (visitedNodes.Count != 0)
        {
            visitedNodes.Pop().Visited = false;
        }

        start.Reset(world.moveLayer[start.indices[0], start.indices[1]] == 1);
        goal.Reset(world.moveLayer[start.indices[0], start.indices[1]] == 1);
    }

    //-----------------------------------------------------------
    //Moves the grid
    //LEGACY CODE
    private void MoveGrid(Coord start, Coord goal)
    {
        int startXCoord = (int)Mathf.Round(start.x) - size;
        int startYCoord = (int)Mathf.Round(start.y) - size;

        //Fill in the grid
        for (byte i = 0; i < map.GetLength(0); i++)
        {
            for (byte j = 0; j < map.GetLength(1); j++)
            {
                int xIndex = i + startXCoord;
                int yIndex = j + startYCoord;
                if (xIndex >= 0 && xIndex < world.XSize && yIndex >= 0 && yIndex < world.YSize)
                    map[i, j].Reset(xIndex, yIndex, world.moveLayer[xIndex, yIndex] == 1);
                else
                    map[i, j].Reset();
            }
        }
        try
        {
            //  Set the goal and the start
            this.goal = map[goal.IntX - startXCoord, goal.IntY - startYCoord];
            this.goal.passable = true; //If it is water make it passable (maybe someday I will make this better)
            this.start = map[start.IntX - startXCoord, start.IntY - startYCoord];
        }
        catch (System.Exception)
        {
            Debug.Log("The buggy coords were: " + goal.ToStringWholeCoords() + start.ToStringWholeCoords() + " startXCoord: " + startXCoord + " startYCoord: " + startYCoord);
        }
    }

    //---------------------------------------------------------------------
    /// <summary>
    /// Creates a new path
    /// </summary>
    /// <param name="moveTargetStack">The stack which will store the coord toward the target</param>
    /// <returns>False if there can't be a path made True if it was a success</returns>
    public bool CreatePath(out Stack<Coord> moveTargetStack, Coord startCoord, Coord goalCoord)
    {
        //  Set the goal and the start
        this.goal = map[goalCoord.IntX, goalCoord.IntY];
        this.goal.passable = true; //If it is water make it passable (maybe someday I will make this better)
        this.start = map[startCoord.IntX, startCoord.IntY];

        path = new Stack<Coord>();
        //Search for the goal
        if (!SearchForGoal(start, 0))
        {
            moveTargetStack = path;
            return false;
        }
        moveTargetStack = path;
        ResetBack();
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
        visitedNodes.Push(current);

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
                path.Push(new Coord(item.position.x, item.position.y));
                return true;
            }
        }
        //If none of the ways were good return false
        return false;
    }
}