using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSystem;
using System;
//using MarchingCubesProject;

public class SkeletonTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        LSystemEditor ed = gameObject.GetComponent<LSystemEditor>();
        LSystem.LSystem l = ed.BuildLSystem();
        List<Tuple<Vector3,Vector3>> segments = l.segments;
        GameObject boneTree = SkeletonGenerator.Generate(l);
        boneTree.transform.parent = gameObject.transform;
        /*Segment[] segments_ = new Segment[segments.Count];
        for (int i = 0; i < segments.Count; i++)
        {
            segments_[i] = new Segment(segments[i].Item1, segments[i].Item2, .1f);
            Debug.Log(segments_[i].startPoint + ", " + segments_[i].endPoint);
        }
        Metaball m = Metaball.BuildFromSegments(segments_);
        MeshGenerator mg = gameObject.GetComponent<MeshGenerator>();
        mg.Generate(m);*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
