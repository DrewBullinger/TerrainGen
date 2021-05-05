using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


[RequireComponent(typeof(MeshFilter))]
public class Mesh_Generator : MonoBehaviour
{
    public Tree_Generator treeScript;

    Mesh mesh;
    Material landMaterial;

    Vector3[] vertices;
    int[] triangles;

    public int xSize = 200;
    public int zSize = 200;
    //public float perlinScaleFactor = 29f;

    //This zooms out. A smaller number = more spread out geometry
    public float perlinZoomFactor = .04f;

    public int octaves = 4;
    //persistance needs to be between 0 and 1
    public float persistance = 0.5f;
    //lacunarity needs to be one or greater
    public float lacunarity = 2f;


    public int seedOffset;

    //This is for diplaying the seed onscreen
    //public String textValue;
    public Text textElement;


    //A good baseline for these variables ^^^ I've found is xSize = 200, zSize = 200, perlinScaleFactor = 20f, perlinZoomFactor = 0.05f

    float islandRadius = 50f;


    void Awake() {
        treeScript = GetComponent<Tree_Generator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //Seed not working yet
        int seed = Random.Range(-100000,100000);
        seedOffset = seed;

        textElement.text = "Seed: " + seed.ToString();


        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        landMaterial = Resources.Load<Material>("Land_Material");
        GetComponent<MeshRenderer>().material = landMaterial;
        
        CreateShape();
        UpdateMesh();

        ///
            //PUT L-SYSTEM CALLS HERE
                //Have like 10-ish trees within the bounds of the island (island radius from the center --> xSize/2 & zSize/2)

            //can get the y values of the terrain from mesh.vertices
            //mesh.vertices is a 1D array (% by xSize to get the z coordinate)

        //method 1
        // treeScript.MakeTree(50, 0, 50).transform.localScale -= new Vector3(.5f, .5f, .5f);
        // treeScript.MakeTree(25, 0, 75).transform.localScale -= new Vector3(.5f, .5f, .5f);
        // treeScript.MakeTree(125, 0, 160).transform.localScale -= new Vector3(.5f, .5f, .5f);

        //method 2
        // Instantiate(treeScript, new Vector3(0, 0, 0), Quaternion.identity).transform.Translate(new Vector3(50, 0, 50));
        // Instantiate(treeScript, new Vector3(25, 0, 75), Quaternion.identity);
        // Instantiate(treeScript, new Vector3(125, 0, 160), Quaternion.identity);

        ///


    }

    //creates the mesh
    void CreateShape()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        for(int i = 0, z = 0; z <= zSize; z++)
        {
            for(int x = 0; x <= xSize; x++)
            {
                //float y = Mathf.PerlinNoise((x + xOffset) * perlinZoomFactor , (z + zOffset) * perlinZoomFactor) * perlinScaleFactor;
                //float y = Mathf.PerlinNoise((x + seedOffset) * perlinZoomFactor , (z + seedOffset) * perlinZoomFactor) * perlinScaleFactor;

            
               //amplitude = how much that octave affects the height
               float amplitude = 10f;
               //frequency = the scale of the octave
               float frequency = 1f;
               //noiseHeight = y value being worken on / "current Y"
               float noiseHeight = 1f;

               //loop through the octaves
               for(int o = 0; o < octaves; o++){
                   float sampleX = (x + seedOffset) * perlinZoomFactor * frequency;
                   float sampleZ = (z + seedOffset) * perlinZoomFactor * frequency;

                   float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
                   noiseHeight += perlinValue * amplitude;

                   amplitude *= persistance;
                   frequency *= lacunarity;
               }
               float y = noiseHeight;



                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //Wobbly Island Edge

                Vector3 center = new Vector3(xSize / 2, .5f, zSize / 2);
                Vector3 point = new Vector3(x, y, z);
                float distance = Mathf.Abs(Vector3.Distance(center, point));
                float twoPi = Mathf.PI * 2;
                float t = 0;
                float phase = 0;
                //noise max?
                float noiseMax = 0;

                List<Vector3> verticesInIsland = new List<Vector3>();

                //make wobbly circle
                for (float angle = 0; angle < twoPi; angle +=  0.1f)
                {
                    float circleXOffset = scale(Mathf.Cos(angle + phase), -1, 1, 0, noiseMax);
                    float circleYOffset = scale(Mathf.Sin(angle + phase), -1, 1, 0, noiseMax);
                    
                    float noise = Mathf.PerlinNoise(circleXOffset, circleYOffset);
                    float radius = scale(0f, 1f, 50f, 100f, noise);
                    float newX = radius * Mathf.Cos(angle);
                    float newY = radius * Mathf.Sin(angle);
   
                    verticesInIsland.Add(new Vector3(newX, newY, z));

                    Vector3 pointInCircle = new Vector3(newX, newY, z);

                    float distance2 = Mathf.Abs(Vector3.Distance(point, pointInCircle));

                    t += .1f;
                }

                phase += 0.003f;

                if (distance > islandRadius)
                {
                    y *= scale(islandRadius, 100f, 1f, .01f, distance);
                }
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////


                vertices[i] = new Vector3(x, y, z);

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


    public float scale(float OldMin, float OldMax, float NewMin, float NewMax, float OldValue)
    {

        float OldRange = (OldMax - OldMin);
        float NewRange = (NewMax - NewMin);
        float NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;

        return (NewValue);
    }


    //updates the mesh in Unity
    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals(); 
    }




    //draws vertices of the tris
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
