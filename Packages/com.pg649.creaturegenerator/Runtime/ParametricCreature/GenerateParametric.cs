using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarchingCubesProject;

public class GenerateParametric : MonoBehaviour
{
    [Tooltip("Generate skeleton")]
    public bool generate_skeleton = true;
    [Tooltip("Generate primitive mesh")]
    public bool primitive_mesh = false;
    [Tooltip("Generate metaball mesh")]
    public bool metaball_mesh = true;

    // Start is called before the first frame update
    void Start()
    {
        CreatureParameters p = GetComponent<CreatureParameters>();
        ParametricCreature c = new ParametricCreature(p);
        c.buildCreature();

        if (generate_skeleton)
        {
            GameObject boneTree = SkeletonGenerator.Generate(c, primitive_mesh);
            boneTree.transform.parent = gameObject.transform;
        }

        if (metaball_mesh)
        {
            MeshGenerator meshGen = GetComponent<MeshGenerator>();
            meshGen.Generate(c.makeMetaball());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
