using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MetaballShape {
    Ball,
    Cone,
    Capsule,
}


[CreateAssetMenu(fileName = "Spec", menuName = "Details/PrefabDetail", order = 1)]
public class PrefabDetail : ScriptableObject
{
    [Header("Shared Settings")]
    public RaycastSettings RaycastSettings;

    public Vector3 Scale = Vector3.one;

    public BoneCategory BoneFilter;

    [Header("Metaball Settings")]
    public bool PlaceMetaball;
    public MetaballShape MetaballShape;
    public MetaballFunction MetaballFunction;
    public bool Inverted;
    public float Thickness;
    public float TipThickness;

    [Header("Prefab Settings")]
    public GameObject Prefab;



    public void applyMetaball(GameObject bone, Metaball meta) {
        if (!PlaceMetaball) return;

        Transform t = bone.transform;
        Collider c = bone.GetComponent<Collider>();
        Bone b = bone.GetComponent<Bone>();

        foreach (Vector3 pos in RaycastSettings.GetPositions(t, c)) {
            Ball ball = null;
            switch(MetaballShape) {
                case MetaballShape.Ball: ball = new Ball(Thickness, pos, MetaballFunction, Inverted); break;
                case MetaballShape.Capsule: ball = new Capsule(new Segment(b.segment.Item1, b.segment.Item2, Thickness), MetaballFunction); break;
                case MetaballShape.Cone: ball = new Cone(new Segment(b.segment.Item1, b.segment.Item2, Thickness), TipThickness, MetaballFunction); break;
            }
            meta.AddBall(ball);
        }
    }

    public void applyDetail(GameObject target) {
        Transform t = target.transform;
        Collider c = target.GetComponent<Collider>();

        foreach (Vector3 pos in RaycastSettings.GetPositions(t, c)) {
            GameObject detail = Instantiate(Prefab);
            detail.transform.parent = t;
            detail.transform.localScale = Vector3.Scale(detail.transform.localScale, Scale);
            detail.transform.position = pos;
        }
    }
}
