public struct Node
{
    public Coord MoveTarget;
    public float DistanceFromGoal;

    public Node(Coord moveTarget, float distance)
    {
        this.MoveTarget = moveTarget;
        this.DistanceFromGoal = distance;
    }
}