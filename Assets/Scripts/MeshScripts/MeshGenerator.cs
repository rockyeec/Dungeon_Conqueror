using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    int xSize;
    int zSize;

    //mygrid variables
    MyGrid myGrid;

    private void Start()
    {
        // mygrid initialize
        myGrid = MyGrid.Instance;
        myGrid.Init();
        xSize = myGrid.gridMaxX;
        zSize = myGrid.gridMaxZ;

        //GetComponent<PathFinding>().FindPath();


        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        

        CreateShape();
        UpdateMesh();
    }


    private void CreateShape()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        float startX = transform.position.x - xSize * 0.5f;
        float startZ = transform.position.z - zSize * 0.5f;

        for (int z = 0, i = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {
                float y = Mathf.PerlinNoise(x * 0.08f, z * 0.08f) * 2;
                vertices[i] = new Vector3(x + startX, y, z + startZ);
            }
        }

        triangles = new int[6 * xSize * zSize];
        for (int z = 0, vert = 0, tris = 0;
            z < zSize;
            z++, vert++)
        {
            for (int x = 0;
            x < xSize;
            x++, vert++, tris += 6)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;
            }
        }

    }
    private void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
