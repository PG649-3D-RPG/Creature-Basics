using System;
using MarchingCubesProject;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "CreatureGeneratorSettings", menuName = "PG649-CreatureGenerator/Creature Generator Settings")]
public class CreatureGeneratorSettings : ScriptableObject
{
    public SkeletonSettings SkeletonSettings = new();
    public SkeletonLinterSettings SkeletonLinterSettings = new();
    public MeshSettings MeshSettings = new();
    public DebugSettings DebugSettings = new();

    private void OnEnable()
    {
        // NOTE: Shader.Find is not allowed during deserialization.
        // Instead find the standard shader once the object is loaded.
        MeshSettings.Material = new Material(Shader.Find("Standard"));
    }
}

[Serializable]
public class SkeletonSettings
{
    [Serializable]
    public struct BoneDensity
    {
        public BoneCategory Category;
        public float MinimumDensity;
        public float MaximumDensity;
    }


    [Header("Bones")]
    
    [Tooltip("If set, the hip is a singular bone, instead of multiple. Only applies to the LSystem Generator.")]
    public bool ConnectHips = false;
    
    [Header("Physics")]
    
    [Tooltip("Multiplier for all rigibody masses. More convenient than adjusting densities.")]
    public float MassMultiplier = 1.0f;
    [Tooltip("Minimum Density for bone colliders in kg/m^3. Randomized once and used for all bones, except when an override is set for a bone category.")]
    public float MinimumDensity = 900.0f;
    [Tooltip("Maximum Density for bone colliders in kg/m^3. Randomized once and used for all bones, except when an override is set for a bone category.")]
    public float MaximumDensity = 1100.0f;
    [Tooltip("Overrides for individual bone densities. Add an element to the list, then choose the category of bones to override as well as minimum and maximum possible densities. Density will be randomized once for all bones of that category.")]
    public BoneDensity[] BoneDensityOverrides;
    [Tooltip("Drag of each Rigidbody in the skeleton.")]
    public float RigidbodyDrag = 0.05f;
    [Tooltip("Collision Detection Mode of each Rigidbody in the Skeleton.")]
    public CollisionDetectionMode CollisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    
    [Header("Rendering")]
    
    [Tooltip("Determines how the primitive mesh casts shadows. Primitive mesh can be enabled in the Debug Settings.")]
    public ShadowCastingMode PrimitiveMeshShadows = ShadowCastingMode.Off;
}

[Serializable]
public class SkeletonLinterSettings
{
    [Header("Warnings")]
    [Tooltip("Warn if non parent-child colliders penetrate in default pose.")]
    public bool WarnOnColliderPenetration = true;

    [Header("Autofixes")]
    [Tooltip("Automatically move penetrating colliders out of each other. May produce non symmetrical creatures.")]
    public bool FixColliderPenetrations = false;

    [Tooltip("Fix identical bone positions by moving one bone slightly.")]
    public bool FixIdenticalBonePositions = true;
}

[Serializable]
public class DebugSettings
{
    [Tooltip("If set, will log additional creature info in console")]
    public bool LogAdditionalInfo = false;
    [Tooltip("If set, bones are not affected by gravity.")]
    public bool DisableBoneGravity = false;
    [Tooltip("If set, all bones will be kinematic rigid bodies.")]
    public bool KinematicBones = false;
    [Tooltip("If set, physics simulation is stopped after creature generation. Allows inspecting result in its default pose.")]
    public bool DisablePhysics = false;
    [Tooltip("If set, capsule meshes matching the bone colliders will be added to the skeleton.")]
    public bool AttachPrimitiveMesh = false;
    [Tooltip("If set, surface normals will be visualized as vectors.")]
    public bool DrawNormals = false;
}

[Serializable]
public class MeshSettings
{
    [Tooltip("If set, a single mesh will be generated around the skeleton.")]
    public bool GenerateMetaballMesh = false;
    [Tooltip("Choose whether the marching cubes algorithm should sample space via cubes or tetrahedons. Tetrahedons will better approximate the isosurface, but generate more vertices.")]
    public MeshGenerator.MARCHING_MODE Mode = MeshGenerator.MARCHING_MODE.CUBES;
    [Tooltip("Choose the default material of the generated mesh.")]
    public Material Material;
    [Tooltip("If set, surface normals will be smoothed across triangles. If not set, surface normals will be discontinous along triangle edges.")]
    public bool SmoothNormals = true;
    [Tooltip("If set, enabled the dual quaternion skinner.")]
    public bool enableDQSkinner = false;
    public int GridResolution = 90;
}
