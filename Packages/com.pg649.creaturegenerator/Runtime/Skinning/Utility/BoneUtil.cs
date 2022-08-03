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

    public static BoneWeight[] CalcBoneWeightsTheStupidWay(Mesh mesh, Transform[] bones, Transform meshTransform) {
		Vector3[] vertices = mesh.vertices;

        BoneWeight[] weights = new BoneWeight[vertices.Length];
        for (int i = 0; i < vertices.Length; i++) {
            float distMin = float.PositiveInfinity;
            int jMin = 0;
            for (int j = 0; j < bones.Length; j++) {
                Vector3 bonePos = bones[j].position;
                Vector3 vertexPos = meshTransform.TransformPoint(vertices[i]);

                float dist = Vector3.Distance(bonePos, vertexPos);
                if (dist < distMin) {
                    distMin = dist;
                    jMin = j;
                }
            }
            //Debug.Log(jMin + "     " + distMin);
            weights[i].boneIndex0 = jMin;
            weights[i].weight0 = 1;
        }

        return weights;
    }

}
