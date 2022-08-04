using MarchingCubesProject;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "CreatureGeneratorSettings", menuName = "PG649-CreatureGenerator/Creature Generator Settings")]
public class CreatureGeneratorSettings : ScriptableObject
{
    public SkeletonSettings SkeletonSettings = new();
    public MeshSettings MeshSettings = new();
    public DebugSettings DebugSettings = new();

    private void OnEnable()
    {
        // NOTE: Shader.Find is not allowed during deserialization.
        // Instead find the standard shader once the object is loaded.
        MeshSettings.Material = new Material(Shader.Find("MadCake/Material/Standard hacked for DQ skinning"));
    }
}

[System.Serializable]
public class SkeletonSettings
{
    [Header("Bones")]
    public bool ConnectHips = false;
    [Header("Physics")]
    public float RigidbodyDrag = 0.05f;
    public CollisionDetectionMode CollisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    
    [Header("Rendering")]
    public ShadowCastingMode PrimitiveMeshShadows = ShadowCastingMode.Off;
}

[System.Serializable]
public class DebugSettings
{
    public bool DisableBoneGravity = false;
    
    public bool DisablePhysics = false;

    public bool AttachPrimitiveMesh = false;

    public bool DrawMeshNormals = false;
}

[System.Serializable]
public class MeshSettings
{
    public bool GenerateMetaballMesh = false;
    public MeshGenerator.MARCHING_MODE Mode = MeshGenerator.MARCHING_MODE.CUBES;
    public Material Material;
    public bool SmoothNormals = true;
    public bool DrawNormals = false;
    public bool enableDQSkinner = false;
    public int GridResolution = 90;
    public float Size = 20f;
}
