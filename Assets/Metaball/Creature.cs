using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarchingCubesProject;

public class Creature : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Segment[] segments = GetComponentsInChildren<Segment>();
        Metaball m = Metaball.BuildFromSegments(segments);

        //generate mesh from the metaball
        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        //attributes such as the size and grid resolution can be set via component or through meshGen.gridResolution
        meshGen.Generate(m);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
