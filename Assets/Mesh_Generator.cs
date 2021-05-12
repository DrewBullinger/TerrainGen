using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


[RequireComponent(typeof(MeshFilter))]
public class Mesh_Generator : MonoBehaviour
{
    public GameObject treeObject;
    public Tree_Generator treeScript;
    Vector3 treeLoc;

    Mesh mesh;
    Material landMaterial;
    Material treeMaterial;

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
    public String seedString;

    //This is for diplaying the seed onscreen
    //public String textValue;
    public Text textElement;


    //A good baseline for these variables ^^^ I've found is xSize = 200, zSize = 200, perlinScaleFactor = 20f, perlinZoomFactor = 0.05f

    float islandRadius = 50f;


    void Awake() {
        treeScript = treeObject.GetComponent<Tree_Generator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        int seed = Random.Range(-100000,100000);
        seedOffset = seed;
        seedString = seed.ToString();

        textElement.text = "Seed: " + seedString;

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        //MeshCollider collider = mesh.AddComponent<MeshCollider>();
        //collider.convex = true;

        landMaterial = Resources.Load<Material>("Land_Material");
        GetComponent<MeshRenderer>().material = landMaterial;

        CreateShape();
        UpdateMesh();
        CreateTrees();
        
    }

    //creates trees
    void CreateTrees() {
        treeMaterial = Resources.Load<Material>("Tree_Material");

        int fstDigit;
        if(seedString.Substring(0,1) == "-") {
            fstDigit = int.Parse(seedString.Substring(0,2), NumberStyles.AllowLeadingSign);
        } else {
            fstDigit = int.Parse(seedString.Substring(0,1));
        }

        int treeAmnt = 300 + fstDigit*10;

        for(int i = 0; i < treeAmnt; i++)
        {   
            float treeY = 10;

            int treeX = Random.Range(25,175);
            int treeZ = Random.Range(25,175);
            
            float treeScale = Random.Range(.950f, .975f);
            treeLoc = new Vector3(treeX, treeY, treeZ);

            GameObject tree = treeScript.MakeTree();
            tree.GetComponent<MeshRenderer>().material = treeMaterial;
            tree.transform.localScale -= new Vector3(.96f, treeScale, .96f);
            tree.transform.Translate(treeLoc);

            RaycastHit hit;
            Vector3 down = transform.TransformDirection(Vector3.down);
            if(Physics.Raycast (treeLoc, down, out hit)) {
                var distanceToGround = hit.distance;
                var newY = treeY-distanceToGround;
                treeLoc = new Vector3(treeX, newY, treeZ);
                tree.transform.position = treeLoc;
            } 

            if(treeLoc.y < 1.7f) {
                Destroy (tree);
            }

        }
    
    }

    //creates the mesh
    void CreateShape() {
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

                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //Lower outside of the island
                if (distance > islandRadius)
                {
                    y *= scale(islandRadius, 100f, 1f, .01f, distance);
                } 


                if(x < 15 || x > 185 || z < 15 || z > 185)
                {
                    y *= scale(0, 10f, 0.01f, 0.8f, 10);
                    if(y > 0.8f){
                        y *= 0.25f;
                    }
                }
                if((x < 25 || x > 175 || z < 25 || z > 175) && y >0.9) y *= 0.75f;
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
        
        mesh.RecalculateBounds();
        MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

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
