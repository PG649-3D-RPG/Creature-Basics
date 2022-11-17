using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BoneUtil
{

    public static Bone[] FindBones(GameObject go) {
        List<Bone> bones = new List<Bone>();

        Bone bone = go.GetComponent<Bone>();
        if (bone != null) {
            if (bone.isRoot && bones.Count > 0)
                Debug.LogWarning("bone.isRoot is true, but is not the topmost bone in the skeleton!");

            bones.Add(bone);
        }

        foreach (Transform child in go.transform) {
            bones.AddRange(FindBones(child.gameObject));
        }

        return bones.ToArray();
    }

}
