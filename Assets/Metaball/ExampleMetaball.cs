using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarchingCubesProject;

public class ExampleMetaball : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //Create metaball
        Metaball m = new Metaball();
        m.AddBall(0.7f, new Vector3(0, 0, 0));

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
