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

        //Metaball m = new Metaball();
        //m.AddBall(0.7f, new Vector3(0, 0, 0));

        Vector3 startPoint = new Vector3(-1, -3, 0);
        Vector3 endPoint = new Vector3(10, 0, 1.5f);
        Segment[] segments = new Segment[] {new Segment(startPoint, endPoint, 2) };
        Metaball m = Metaball.BuildFromSegments(segments);


        //generate mesh from the metaball
        MeshGenerator meshGen = GetComponent<MeshGenerator>();

        meshGen.Generate(m);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
