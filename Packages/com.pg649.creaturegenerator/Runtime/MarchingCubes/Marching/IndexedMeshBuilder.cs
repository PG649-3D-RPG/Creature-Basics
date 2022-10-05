using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class IndexedMeshBuilder
{

    public readonly List<Vector3> verts = new List<Vector3>();
    public readonly List<int> indices = new List<int>();

    public void Reset() {
        verts.Clear();
        indices.Clear();
    }

    public void Push(Vector3 pos) {
        int idx = -1;
        for (int i = 0; i < verts.Count; i++) {
            if (verts[i] == pos) {
                idx = i;
                break;
            }
        }

        if (idx < 0) {
            verts.Add(pos);
            indices.Add(verts.Count - 1);
        } else {
            indices.Add(idx);
        }
    }

}