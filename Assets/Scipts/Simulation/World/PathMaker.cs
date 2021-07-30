using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PathMaker
{
    private class Node
    {
        public float DistanceFromGoal { get; set; }
        public bool Visited;
        public bool passable;
        public Coord position;
        public byte[] indices = new byte[2];

        public Node(int xPos, int yPos, bool passable, byte[] indices)
        {
            this.passable = passable;
            this.position = new Coord(xPos, yPos);
            this.Visited = false;
            this.DistanceFromGoal = float.MaxValue;
            this.indices = indices;
        }

        public Node()
        {
            this.passable = false;
        }

        public void SetDist(float dist)
        {
            this.DistanceFromGoal = dist;
        }

        public void Visit()
        {
            this.Visited = true;
        }
    }

    private Node[,] map;
    private World world;
    private Stack<Coord> path;
    private Node start;
    private Node goal;

    public PathMaker(Coord start, Coord goal, byte Size, World world)
    {
        this.world = world;
        int startXCoord = (int)Mathf.Round(start.x) - Size;
        int startYCoord = (int)Mathf.Round(start.y) - Size;

        map = new Node[Size * 2, Size * 2];
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

        this.goal = map[(int)goal.x - startXCoord, (int)goal.y - startYCoord];
        this.goal.passable = true;
        this.start = map[(int)start.x - startXCoord, (int)start.y - startYCoord];
    }

    public Stack<Coord> CreatePath()
    {
        path = new Stack<Coord>();

        if (!SearchForGoal(start))
        {
            Debug.Log("No path found");
        }

        return path;
    }

    private bool CheckIfValid(Node node)
    {
        return node.passable && !node.Visited;
    }

    private bool SearchForGoal(Node current)
    {
        current.Visit();
        if (current.Equals(goal))
        {
            return true;
        }

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
        for (int i = 0; i < possibleWays.Count; i++)
        {
            possibleWays[i].SetDist(Coord.CalcDistance(goal.position, possibleWays[i].position));
        }

        foreach (var item in possibleWays.OrderBy(x => x.DistanceFromGoal))
        {
            if (SearchForGoal(item))
            {
                path.Push(item.position);
                return true;
            }
        }

        return false;
    }
}