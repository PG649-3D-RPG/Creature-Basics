using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BoneUtil
{

    public static Transform[] FindBones(GameObject go) {
        List<Transform> bones = new List<Transform>();

        if (go.GetComponent<Bone>() != null) {
            bones.Add(go.transform);
        }

        foreach (Transform child in go.transform) {
            bones.AddRange(FindBones(child.gameObject));
        }

        return bones.ToArray();
    }

}
