using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGrid : MonoBehaviour
{
    public static MyGrid Instance;
    private void Awake()
    {
        Instance = this;
    }


    public Vector3 stageSize =  new Vector3(100, 0, 100);
    
    float nodeRadius = 0.5f;
    Node[,] grid;
    float nodeDiameter;
    [HideInInspector] public int gridMaxX;
    [HideInInspector] public int gridMaxZ;

    float startX;
    float startZ;


    public int MaxSize
    {
        get
        {
            return gridMaxZ * gridMaxX;
        }
    }

    public void Init()
    {
        nodeDiameter = 2 * nodeRadius;
        gridMaxX = Mathf.RoundToInt(stageSize.x);
        gridMaxZ = Mathf.RoundToInt(stageSize.z);
        CreateGrid();
    }

    void CreateGrid()
    {
        grid = new Node[gridMaxX, gridMaxZ];
        // get bottom left
        startX = transform.position.x - gridMaxX * 0.5f; 
        startZ = transform.position.z - gridMaxZ * 0.5f;
        for (int x = 0; x < gridMaxX; x++)
        {
            for (int z = 0; z < gridMaxZ; z++)
            {
                Vector3 point =
                    new Vector3
                    (
                        startX + nodeRadius + x * nodeDiameter,
                        -2.5f, // slightly below ground level to avoid triggers
                        startZ + nodeRadius + z * nodeDiameter
                    );
                bool tempWalkable = !Physics.CheckSphere(point, nodeRadius);
                grid[x, z] = new Node(tempWalkable, 0, point, x, z);
            }
        }

        for (int x = 0; x < gridMaxX; x++)
        {
            for (int z = 0; z < gridMaxZ; z++)
            {
                if (grid[x, z].isWalkable)
                    continue;

                List<Node> neighbours = GetNeighbours(grid[x, z]);

                for (int i = 0; i < neighbours.Count; i++)
                {
                    neighbours[i].extraCost = 50;
                }
            }
        }
    }



    public Node GetNode(Vector3 position)
    {
        ClampCoordinate(position);
        int gridX = Mathf.RoundToInt(position.x - startX);
        int gridZ = Mathf.RoundToInt(position.z - startZ);
        if (gridZ >= gridMaxZ)
        {
            gridZ = gridMaxZ - 1;
        }
        if (gridZ < 0)
        {
            gridZ = 0;
        }
        if (gridX >= gridMaxX)
        {
            gridX = gridMaxX - 1;
        }
        if (gridX < 0)
        {
            gridX = 0;
        }
        return grid[gridX, gridZ];
    }

    public Vector3 GetWorldPosition(Node node)
    {
        float x = node.gridX + startX;
        float z = node.gridZ + startZ;
        return new Vector3(x, 0, z);
    }

    public Vector3 ClampCoordinate(Vector3 position)
    {
        Mathf.Clamp(position.x, -gridMaxX / 2 + 1, gridMaxX / 2 - 1);
        Mathf.Clamp(position.z, -gridMaxZ / 2 + 1, gridMaxZ / 2 - 1);
        position.y = 0;
        return position;
    }

    public List<Node> GetNeighbours(Node node)
    {
        //getbottom left
        List<Node> neighbours = new List<Node>();
        int startX = node.gridX - 1;
        int startZ = node.gridZ - 1;
        for (int x = startX; x < startX + 3; x++)
        {
            for (int z = startZ; z < startZ + 3; z++)
            {
                // not within grid
                if (x < 0 || x >= gridMaxX
                    || z < 0 || z >= gridMaxZ)
                    continue;

                // it is you
                if (x == node.gridX
                    && z == node.gridZ)
                    continue;

                // not walkable
                if (!grid[x, z].isWalkable)
                    continue;
                
                neighbours.Add(grid[x, z]);                
            }
        }
        return neighbours;
    }

    /*
    //=========================================================================================
    // temp to be removed
    public List<Node> path = new List<Node>();
    public List<Node> traversed = new List<Node>();
    private void OnDrawGizmos()
    {
        for (int x = 0; x < gridMaxX; x++)
        {
            for (int z = 0; z < gridMaxZ; z++)
            {
                if (grid[x, z].isWalkable)
                {
                    Gizmos.color = Color.white;

                    
                    
                    if (path.Contains(grid[x,z]))
                        Gizmos.color = Color.cyan;

                    if (traversed.Contains(grid[x, z]))
                        Gizmos.color = Color.black;
                }
                else
                {
                    Gizmos.color = Color.red;
                }
                Gizmos.DrawCube(grid[x, z].position, Vector3.one * (nodeDiameter - 0.08f));
            }
        }
    }
    //========================================================================================
    */

}
