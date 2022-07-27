using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarchingCubesProject;

public class GenerateParametric : MonoBehaviour
{
    [Tooltip("Generate skeleton")]
    public bool generate_skeleton = true;
    [Tooltip("Generate metaball mesh")]
    public bool metaball_mesh = true;
    [Tooltip("Disable Physics after generation to better see generated creature.")]
    public bool disablePhysics = false;

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

            if (disablePhysics)
            {
                Physics.autoSimulation = false;
            }

            if (metaball_mesh)
            {
                MeshGenerator mg = GetComponent<MeshGenerator>();
                mg.material = new Material(Shader.Find("MadCake/Material/Standard hacked for DQ skinning"));
                mg.material.color = Color.white;
                meshGen.Generate(Metaball.BuildFromSkeleton(rootGo.GetComponent<Skeleton>()));
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
