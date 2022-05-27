using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarchingCubesProject;


public class CreateCreature : MonoBehaviour
{
    // This Function is called up to create the symmetric creature
    void Start()
    {
        SymmetricCreature testCreatue = new SymmetricCreature(new Vector3(5,10,5));
        Metaball m = testCreatue.createMetaballMesh();


        //generate mesh from the metaball
        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        //attributes such as the size and grid resolution can be set via component or through meshGen.gridResolution
        meshGen.size = 25;

        meshGen.Generate(m);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
