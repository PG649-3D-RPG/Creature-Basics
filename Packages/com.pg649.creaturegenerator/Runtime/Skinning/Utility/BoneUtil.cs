using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BoneUtil
{

    public static Transform[] FindBones(GameObject go) {
        List<Transform> bones = new List<Transform>();

        Bone bone = go.GetComponent<Bone>();
        if (bone != null) {
            if (bone.isRoot && bones.Count > 0)
                Debug.LogWarning("bone.isRoot is true, but is not the topmost bone in the skeleton!");

            bones.Add(go.transform);
        }

        foreach (Transform child in go.transform) {
            bones.AddRange(FindBones(child.gameObject));
        }

        return bones.ToArray();
    }

}
