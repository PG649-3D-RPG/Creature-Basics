using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Skeleton : MonoBehaviour
{
    public Dictionary<BoneCategory, List<GameObject>> bonesByCategory;

    public ISettingsInstance SettingsInstance;

    public int nBones;
    public int nAngXMotLimited;
    public int nAngYMotLimited;
    public int nAngZMotLimited;

    public Skeleton()
    {
        bonesByCategory = new();

        foreach (BoneCategory cat in Enum.GetValues(typeof(BoneCategory)))
        {
            bonesByCategory[cat] = new List<GameObject>();
        }

        nBones = 0;
        nAngXMotLimited = 0;
        nAngYMotLimited = 0;
        nAngZMotLimited = 0;
    }

    public IEnumerable<(GameObject, Bone, Rigidbody, ConfigurableJoint)> Iterator()
    {
        Queue<GameObject> todo = new();
        todo.Enqueue(gameObject);
        while (todo.Count > 0)
        {
            GameObject current = todo.Dequeue();

            if (current.GetComponent<Bone>() != null)
            {
                yield return (
                    current,
                    current.GetComponent<Bone>(),
                    current.GetComponent<Rigidbody>(),
                    current.GetComponent<ConfigurableJoint>()
                );
            }

            foreach (Transform child in current.transform)
            {
                todo.Enqueue(child.gameObject);
            }
        }
    }

    /// <summary>
    /// Iterates over pairs of bones.
    /// If a and b are bones, it does generate (a, b), but not (a, a) or (b, a).
    /// </summary>
    /// <returns></returns>
    public IEnumerable<((GameObject, Bone, Rigidbody, ConfigurableJoint), (GameObject, Bone, Rigidbody, ConfigurableJoint))> Pairs()
    {
        foreach (var a in Iterator())
        {
            foreach (var b in Iterator())
            {
                if (a == b) break;

                yield return (a, b);
            }
        }
    }
    
    /// <summary>
    /// Iterates similar to Pairs(), but also discards all tuples where bones are directly linked.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<((GameObject, Bone, Rigidbody, ConfigurableJoint), (GameObject, Bone, Rigidbody, ConfigurableJoint))> UnrelatedPairs()
    {
        foreach (var (a, b) in Pairs())
        {
                if (b.Item1.transform.parent == a.Item1.transform || a.Item1.transform.parent == b.Item1.transform) continue;
                yield return (a, b);
        }
    }

    public GameObject FindLimbRoot(GameObject go)
    {
        Debug.Assert(go.GetComponent<Bone>() != null,
            "GameObject must have a bone component. Make sure the GameObject is part of a skeleton");
        var bone = go.GetComponent<Bone>();
        while (bone.boneIndex != 0)
        {
            bone = bone.gameObject.GetComponentInParent<Bone>();
        }

        return bone.gameObject;
    }
}