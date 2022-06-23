using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarchingCubesProject;

public class GenerateParametric : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CreatureParameters p = GetComponent<CreatureParameters>();
        ParametricCreature c = new ParametricCreature(p);
        c.buildCreature();

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.Generate(c.makeMetaball());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
