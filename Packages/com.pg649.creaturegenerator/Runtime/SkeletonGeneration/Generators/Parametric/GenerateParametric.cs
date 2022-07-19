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
        SkeletonAssemblerSettings s = GetComponent<SkeletonAssemblerSettings>();
        ParametricCreature c = new ParametricCreature(p);
        ParametricGenerator g = new ParametricGenerator(p);

        SkeletonDefinition skeletonDefinition = g.BuildCreature();

        if (generate_skeleton)
        {
            GameObject rootGo = SkeletonAssembler.Assemble(skeletonDefinition, s);


            rootGo.transform.parent = gameObject.transform;
            Physics.autoSimulation = false;

            if (metaball_mesh)
            {
                MeshGenerator meshGen = GetComponent<MeshGenerator>();
                meshGen.Generate(Metaball.BuildFromSkeleton(rootGo.GetComponent<Skeleton>()));
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
