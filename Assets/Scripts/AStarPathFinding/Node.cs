using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IHeapable<Node>
{
    public bool isWalkable;
    public int extraCost;
    public Vector3 position;

    public int gridX;
    public int gridZ;

    public Node parent;

    int heapIndex;

    public int gCost;
    public int hCost;

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public Node(bool isWalkable, int extraCost, Vector3 position, int gridX, int gridZ)
    {
        this.isWalkable = isWalkable;
        this.extraCost = extraCost;
        this.position = position;
        this.gridX = gridX;
        this.gridZ = gridZ;
    }

    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }

    public int CompareTo(Node node)
    {
        int compare = fCost.CompareTo(node.fCost);
        if (compare == 0)
            compare = hCost.CompareTo(node.hCost);
        return compare;
    }

}
