using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class SignedDistanceField
{

    private int sdfResolution;
    public Bounds meshBounds;
    private Vector3 minExtents;
    private Vector3 maxExtents;

    private float[] sdfData;

    public SignedDistanceField(Mesh mesh, int sdfResolution)
    {
        // let's start with some safety checks
        if (mesh == null)
        {
            Debug.LogError("The mesh provided to the SDF Generator must not be null.");
        }
        if (sdfResolution % 8 != 0) // check if the sdf resolution is a power of 8
        {
            Debug.LogError("Sdf resolution must be a power of 8.");
        }

        this.sdfResolution = sdfResolution;
        this.meshBounds = mesh.bounds;
        this.minExtents = ComputeMinExtents(meshBounds);
        this.maxExtents = ComputeMaxExtents(meshBounds);

        // create a timer
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        double computeTime;

        // create the resources we need to upload and retrive data from the GPU
        ComputeBuffer outputBuffer = GetOutputBuffer(sdfResolution);
        ComputeBuffer meshTrianglesBuffer = GetMeshTrianglesBuffer(mesh);
        ComputeBuffer meshVerticesBuffer = GetMeshVerticesBuffer(mesh);
        ComputeBuffer meshNormalsBuffer = GetMeshNormalsBuffer(mesh);

        // Instantiate the compute shader from the resources folder
        ComputeShader computeShader = (ComputeShader)Resources.Load<ComputeShader>("SDF_Generator");
        int kernel = computeShader.FindKernel("CSMain");

        // bind the resources to the compute shader
        computeShader.SetInt("SdfResolution", sdfResolution);
        computeShader.SetBuffer(kernel, "MeshTrianglesBuffer", meshTrianglesBuffer);
        computeShader.SetBuffer(kernel, "MeshVerticesBuffer", meshVerticesBuffer);
        computeShader.SetBuffer(kernel, "MeshNormalsBuffer", meshNormalsBuffer);
        computeShader.SetBuffer(kernel, "Output", outputBuffer);
        computeShader.SetVector("MinExtents", minExtents);
        computeShader.SetVector("MaxExtents", maxExtents);

        // dispatch
        int threadGroupSize = sdfResolution / 8;
        stopwatch.Start();
        computeShader.Dispatch(kernel, threadGroupSize, threadGroupSize, threadGroupSize);

        this.sdfData = new float[sdfResolution * sdfResolution * sdfResolution];
        outputBuffer.GetData(sdfData);

        stopwatch.Stop();
        computeTime = stopwatch.Elapsed.TotalSeconds;

        // destroy the resources
        outputBuffer.Release();
        meshTrianglesBuffer.Release();
        meshVerticesBuffer.Release();
        meshNormalsBuffer.Release();

        // print computational duration data
        Debug.Log($"SDF generated in {computeTime} seconds");
    }

    public float sampleSdf(Vector3 v) {
        if (!meshBounds.Contains(v))
        {
            Debug.LogError("Vector v is outside of mesh bounds!");
            return 0;
        }

        float size = (maxExtents - minExtents).x;
        float invSize = 1.0f / size;
        Vector3 coord = Vector3.Scale((v - minExtents), new Vector3(invSize, invSize, invSize)) * sdfResolution;

        int bufferIndex = (int)coord.x + (int)coord.y * sdfResolution + (int)coord.z * sdfResolution * sdfResolution;
        return sdfData[bufferIndex];
    }

        /*
float3 CoordToPosition(uint3 coord)
{
	// add float3(0.5, 0.5, 0.5) in order to center the position to the middle of the voxel
	return Remap((float3)coord + float3(0.5, 0.5, 0.5), 0, SdfResolution.xxx, MinExtents.xyz, MaxExtents.xyz);
}

// Transforms the 3D dimensional voxel ID coordiante into a one-dimensional index for accessing a ComputeBuffer
inline int CoordsToBufferIndex(int x, int y, int z)
{
	return x + y * SdfResolution + z * SdfResolution * SdfResolution;
}
*/

    private static ComputeBuffer GetOutputBuffer(int sdfResolution)
    {
        int cubicSdfResolution = sdfResolution * sdfResolution * sdfResolution;
        ComputeBuffer outputBuffer = new ComputeBuffer(cubicSdfResolution, Marshal.SizeOf(typeof(float)));
        return outputBuffer;
    }

    private static ComputeBuffer GetMeshNormalsBuffer(Mesh mesh)
    {
        ComputeBuffer outputBuffer = new ComputeBuffer(mesh.normals.Length, Marshal.SizeOf(typeof(Vector3)));
        outputBuffer.SetData(mesh.normals);
        return outputBuffer;
    }

    private static ComputeBuffer GetMeshVerticesBuffer(Mesh mesh)
    {
        ComputeBuffer outputBuffer = new ComputeBuffer(mesh.vertices.Length, Marshal.SizeOf(typeof(Vector3)));
        outputBuffer.SetData(mesh.vertices);
        return outputBuffer;
    }

    private static ComputeBuffer GetMeshTrianglesBuffer(Mesh mesh)
    {
        ComputeBuffer outputBuffer = new ComputeBuffer(mesh.triangles.Length, Marshal.SizeOf(typeof(int)));
        outputBuffer.SetData(mesh.triangles);
        return outputBuffer;
    }

    private static Vector3 ComputeMinExtents(Bounds meshBounds)
    {
        float largestSide = MaxComponent(meshBounds.size);
        float padding = largestSide / 20;
        return meshBounds.center - (Vector3.one * (largestSide * 0.5f + padding));
    }

    private static Vector3 ComputeMaxExtents(Bounds meshBounds)
    {
        float largestSide = MaxComponent(meshBounds.size);
        float padding = largestSide / 20;
        return meshBounds.center + (Vector3.one * (largestSide * 0.5f + padding));
    }

    private static float MaxComponent(Vector3 vector)
    {
        return Mathf.Max(vector.x, vector.y, vector.z);
    }

}
