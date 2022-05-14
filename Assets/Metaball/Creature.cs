using Common.Unity.Drawing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering;

namespace MarchingCubesProject
{
    public class Creature : MonoBehaviour
    {
        public enum MARCHING_MODE { CUBES, TETRAHEDRON };

        public Material material;

        public MARCHING_MODE mode = MARCHING_MODE.CUBES;

        public bool smoothNormals = false;

        public bool drawNormals = false;

        public int gridResolution = 32;

        private List<GameObject> meshes = new List<GameObject>();

        private Vector3 boundingVolume;

        private NormalRenderer normalRenderer;
        // Start is called before the first frame update
        void Start()
        {

            //Set the mode used to create the mesh.
            //Cubes is faster and creates less verts, tetrahedrons is slower and creates more verts but better represents the mesh surface.
            Marching marching = null;
            if (mode == MARCHING_MODE.TETRAHEDRON)
                marching = new MarchingTertrahedron();
            else
                marching = new MarchingCubes();

            //Surface is the value that represents the surface of mesh
            //For example the perlin noise has a range of -1 to 1 so the mid point is where we want the surface to cut through.
            //The target value does not have to be the mid point it can be any value with in the range.
            marching.Surface = 1.0f;

            //The size of voxel array.
            int width = gridResolution;
            int height = gridResolution;
            int depth = gridResolution;

            boundingVolume = GetComponent<BoxCollider>().size;

            var voxels = new VoxelArray(width, height, depth);

            Metaball metaball = BuildMetaball();

            //Fill voxels with values.
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        float u = (x / (width - 1.0f) - 0.5f) * boundingVolume.x;
                        float v = (y / (height - 1.0f) - 0.5f) * boundingVolume.y;
                        float w = (z / (depth - 1.0f) - 0.5f) * boundingVolume.z;
                        
                        voxels[x, y, z] = metaball.Value(u, v, w);
                    }
                }
            }

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> indices = new List<int>();

            //The mesh produced is not optimal. There is one vert for each index.
            //Would need to weld vertices for better quality mesh.
            marching.Generate(voxels.Voxels, verts, indices);
            indices.Reverse();

            //Create the normals from the voxel.

            if (smoothNormals)
            {
                for (int i = 0; i < verts.Count; i++)
                {
                    //Presumes the vertex is in local space where
                    //the min value is 0 and max is width/height/depth.
                    Vector3 p = verts[i];

                    float u = p.x / (width - 1.0f);
                    float v = p.y / (height - 1.0f);
                    float w = p.z / (depth - 1.0f);

                    Vector3 n = voxels.GetNormal(u, v, w);

                    normals.Add(n);
                }

                normalRenderer = new NormalRenderer();
                normalRenderer.DefaultColor = Color.red;
                normalRenderer.Length = 0.25f;
                normalRenderer.Load(verts, normals);
            }

            var position = this.transform.localPosition - boundingVolume / 2;//new Vector3(-width / 2, -height / 2, -depth / 2);

            CreateMesh32(verts, normals, indices, position);
        }

        private Metaball BuildMetaball()
        {
            Metaball metaball = new Metaball();
            Segment[] segments = GetComponentsInChildren<Segment>();

            foreach(Segment segment in segments)
            {
                //TODO Calculate number of balls required (Aufgabe 2.1)
                //useful: segment.thickness, segment.GetLength(), Mathf.CeilToInt()

                //polynomial solution
                int numBalls = Mathf.CeilToInt(segment.GetLength() / (Mathf.Pow(2, 0.5f) * segment.thickness * 2));

                // exponential solution
                //int numBalls = Mathf.CeilToInt(segment.GetLength() / (2.97f * segment.thickness));


                //TODO Place balls along segment (Aufgabe 2.2)
                //useful: metaball.AddBall(), segment.thickness, segment.startPoint, segment.endPoint
                Vector3 fwd = segment.endPoint - segment.startPoint;

                for (float i=0; i<=numBalls; i++)
                {
                    metaball.AddBall(segment.thickness, segment.startPoint + (i / numBalls) * fwd);
                    Debug.Log((segment.startPoint + (i / numBalls) * fwd).ToString("F4"));
                }
            }
            return metaball;
        }

        private void CreateMesh32(List<Vector3> verts, List<Vector3> normals, List<int> indices, Vector3 position)
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetTriangles(indices, 0);

            if (normals.Count > 0)
                mesh.SetNormals(normals);
            else
                mesh.RecalculateNormals();

            mesh.RecalculateBounds();

            GameObject go = new GameObject("Mesh");
            go.transform.parent = transform;
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.GetComponent<Renderer>().material = material;
            go.GetComponent<MeshFilter>().mesh = mesh;
            go.transform.localPosition = position;
            go.transform.localScale = boundingVolume/gridResolution;

            meshes.Add(go);
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void CreateMesh16(List<Vector3> verts, List<Vector3> normals, List<int> indices, Vector3 position)
        {

            int maxVertsPerMesh = 30000; //must be divisible by 3, ie 3 verts == 1 triangle
            int numMeshes = verts.Count / maxVertsPerMesh + 1;

            for (int i = 0; i < numMeshes; i++)
            {
                List<Vector3> splitVerts = new List<Vector3>();
                List<Vector3> splitNormals = new List<Vector3>();
                List<int> splitIndices = new List<int>();

                for (int j = 0; j < maxVertsPerMesh; j++)
                {
                    int idx = i * maxVertsPerMesh + j;

                    if (idx < verts.Count)
                    {
                        splitVerts.Add(verts[idx]);
                        splitIndices.Add(j);

                        if (normals.Count != 0)
                            splitNormals.Add(normals[idx]);
                    }
                }

                if (splitVerts.Count == 0) continue;

                Mesh mesh = new Mesh();
                mesh.indexFormat = IndexFormat.UInt16;
                mesh.SetVertices(splitVerts);
                mesh.SetTriangles(splitIndices, 0);

                if (splitNormals.Count > 0)
                    mesh.SetNormals(splitNormals);
                else
                    mesh.RecalculateNormals();

                mesh.RecalculateBounds();

                GameObject go = new GameObject("Mesh");
                go.transform.parent = transform;
                go.AddComponent<MeshFilter>();
                go.AddComponent<MeshRenderer>();
                go.GetComponent<Renderer>().material = material;
                go.GetComponent<MeshFilter>().mesh = mesh;
                go.transform.localPosition = position;

                meshes.Add(go);
            }
        }

        private void OnRenderObject()
        {
            if (normalRenderer != null && meshes.Count > 0 && drawNormals)
            {
                var m = meshes[0].transform.localToWorldMatrix;

                normalRenderer.LocalToWorld = m;
                normalRenderer.Draw();
            }

        }
    }
}
