using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
public class Mesh_Generator : MonoBehaviour
{

    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    public int xSize = 200;
    public int zSize = 200;
    public float perlinScaleFactor = 20f;

    //This zooms out. A smaller number = more spread out geometry
    public float perlinZoomFactor = .05f;

    //A good baseline for these variables ^^^ I've found is xSize = 200, zSize = 200, perlinScaleFactor = 20f, perlinZoomFactor = 0.05f


    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        
        CreateShape();
        UpdateMesh();
    }

    //creates the mesh
    void CreateShape()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        for(int i = 0, z = 0; z <= zSize; z++)
        {
            for(int x = 0; x <= xSize; x++)
            {

                float y = Mathf.PerlinNoise(x * perlinZoomFactor , z * perlinZoomFactor) * perlinScaleFactor;

                vertices[i] = new Vector3(x,y,z);
                i++;
            }
        }


        int vert = 0;
        int tris = 0;
        
        triangles = new int[xSize * zSize * 6];

        for(int z = 0; z < zSize; z++)
        {
            for(int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

    }

    //updates the mesh in Unity
    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals(); 
    }


    //draws verticies of the tris
    // private void OnDrawGizmos()
    // {
    //     if(vertices == null)
    //         return;

    //     for(int i = 0; i < vertices.Length; i++)
    //     {
    //         Gizmos.DrawSphere(vertices[i], .1f);
    //     }
    // }

}
