using System.Collections.Generic;
using Common.Unity.Drawing;
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

        private List<GameObject> meshes = new List<GameObject>();

        private NormalRenderer normalRenderer;

        private IRigSolver rigSolver;

        void Start() 
        {
        }

        void Update() 
        {
        }

        public void ApplySettings(MeshSettings settings, DebugSettings debugSettings)
        {
            mode = settings.Mode;
            rigSolver = IRigSolver.Get(settings.RigSolver);
            material = settings.Material;
            smoothNormals = settings.SmoothNormals;
            drawNormals = debugSettings.DrawNormals;
            enableDQSkinner = settings.enableDQSkinner;
            gridResolution = settings.GridResolution;
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

            Bounds metaballBounds = metaball.GetBounds();
            float maxSizeComp = Mathf.Max(metaballBounds.size.x, metaballBounds.size.y, metaballBounds.size.z);
            float voxelSize = maxSizeComp / (gridResolution - 1.0f);
            maxSizeComp += 3.0f * voxelSize;

            Bounds voxelBounds = new Bounds(metaballBounds.center - new Vector3(voxelSize, voxelSize, voxelSize), new Vector3(maxSizeComp, maxSizeComp, maxSizeComp));

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

            var voxels = new VoxelArray(gridResolution, gridResolution, gridResolution);

            //Fill voxels with values.
            for (int x = 0; x < gridResolution; x++)
            {
                for (int y = 0; y < gridResolution; y++)
                {
                    for (int z = 0; z < gridResolution; z++)
                    {
                        float u = (x / (gridResolution - 1.0f)) * voxelBounds.size.x + voxelBounds.min.x;
                        float v = (y / (gridResolution - 1.0f)) * voxelBounds.size.y + voxelBounds.min.y;
                        float w = (z / (gridResolution - 1.0f)) * voxelBounds.size.z + voxelBounds.min.z;
                        
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
                    Vector3 p = verts[i];

                    float u = p.x / (gridResolution - 1.0f);
                    float v = p.y / (gridResolution - 1.0f);
                    float w = p.z / (gridResolution - 1.0f);

                    Vector3 n = voxels.GetNormal(u, v, w);

                    normals.Add(n);
                }
            }

            var position = this.transform.localPosition + (voxelBounds.min );
            Matrix4x4 mat = Matrix4x4.Translate(position) * Matrix4x4.Scale(voxelBounds.size / gridResolution);

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

            mesh.boneWeights = rigSolver.CalcBoneWeights(mesh, bones, transform);

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
