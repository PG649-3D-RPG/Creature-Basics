using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SkeletonAssemblerSettings : MonoBehaviour
{
    public bool AttachPrimitiveMesh = false;
    public ShadowCastingMode PrimitiveMeshShadows = ShadowCastingMode.Off;
    public float RigidbodyDrag = 0.05f;
    public CollisionDetectionMode CollisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

    public bool DebugDisableBoneGravity = false;
}
