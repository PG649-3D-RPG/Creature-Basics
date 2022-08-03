using Common.Unity.Drawing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering;

namespace MarchingCubesProject
{
    public class MeshGenerator : MonoBehaviour
    {
        public enum MARCHING_MODE { CUBES, TETRAHEDRON };

        public Material material;

        public Metaball metaball;

        public MARCHING_MODE mode = MARCHING_MODE.CUBES;

        public bool smoothNormals = false;

        public bool drawNormals = false;

        public bool enableDQSkinner = false;


        /// <summary>
        /// number of cubes in each direction
        /// </summary>
        public int gridResolution = 32;

        /// <summary>
        /// size of the volume that marching cubes is performed in
        /// </summary>
        public float size = 1f;

        private List<GameObject> meshes = new List<GameObject>();

        private NormalRenderer normalRenderer;

        void Start() 
        {
        }

        void Update() 
        {
        }

        public void ApplySettings(MeshSettings settings)
        {
            mode = settings.Mode;
            material = settings.Material;
            smoothNormals = settings.SmoothNormals;
            drawNormals = settings.DrawNormals;
            enableDQSkinner = settings.enableDQSkinner;
            gridResolution = settings.GridResolution;
            size = settings.Size;
        }

        void Clear() {
            if (normalRenderer != null)
                normalRenderer.Clear();

            foreach (GameObject go in meshes) {
                Destroy(go);
            }
            meshes.Clear();
        }

        void Regenerate() {
            if (this.metaball != null) {
                Clear();
                Generate(this.metaball);
            }
        }

        /// <summary>
        /// Generates a mesh from the given Metaball
        /// within a box of dimensions size*size*size centered at (0,0,0)
        /// </summary>
        /// <param name="metaball"></param>
        public void Generate(Metaball metaball)
        {
            if (normalRenderer == null) {
                normalRenderer = new NormalRenderer();
                normalRenderer.DefaultColor = Color.red;
                normalRenderer.Length = 0.25f;
            }

            this.metaball = metaball;

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

            var voxels = new VoxelArray(width, height, depth);

            //Fill voxels with values.
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        float u = (x / (width - 1.0f) - 0.5f) * size;
                        float v = (y / (height - 1.0f) - 0.5f) * size;
                        float w = (z / (depth - 1.0f) - 0.5f) * size;
                        
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
            }

            var position = this.transform.localPosition - (new Vector3(size, size, size)/ 2);
            Matrix4x4 mat = Matrix4x4.Translate(position) * Matrix4x4.Scale(new Vector3(size, size, size) / gridResolution);

            for (int i = 0; i < verts.Count; i++)
            {
                verts[i] = mat.MultiplyPoint3x4(verts[i]);
            }

            CreateMesh32(verts, normals, indices, position);
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

            Transform[] bones = BoneUtil.FindBones(transform.gameObject);
            Debug.Log("Found " + bones.Length + " Bones in the hierarchy");

            // bind poses must be generated relative to the meshes transform
            List<Matrix4x4> bindPoses = new List<Matrix4x4>();
            foreach (Transform bone in bones) {
                bindPoses.Add(bone.worldToLocalMatrix * go.transform.localToWorldMatrix);
            }
            mesh.bindposes = bindPoses.ToArray();

            mesh.boneWeights = BoneUtil.CalcBoneWeightsTheStupidWay(mesh, bones, transform);

            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.GetComponent<MeshRenderer>().material = material;
            go.AddComponent<SkinnedMeshRenderer>();
            go.GetComponent<SkinnedMeshRenderer>().sharedMesh = mesh;
            go.GetComponent<SkinnedMeshRenderer>().rootBone = bones[0];
            go.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
            go.GetComponent<SkinnedMeshRenderer>().quality = SkinQuality.Bone1;
            go.GetComponent<SkinnedMeshRenderer>().bones = bones;

            if (enableDQSkinner) {
                go.AddComponent<DualQuaternionSkinner>();
                go.GetComponent<DualQuaternionSkinner>().shaderComputeBoneDQ = (ComputeShader) Resources.Load("Compute/ComputeBoneDQ");
                go.GetComponent<DualQuaternionSkinner>().shaderDQBlend = (ComputeShader) Resources.Load("Compute/DQBlend");
                go.GetComponent<DualQuaternionSkinner>().shaderApplyMorph = (ComputeShader) Resources.Load("Compute/ApplyMorph");
                go.GetComponent<DualQuaternionSkinner>().SetViewFrustrumCulling(false);
            }

            meshes.Add(go);
            
            if (normals.Count > 0)
                normalRenderer.Load(verts, normals);
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
