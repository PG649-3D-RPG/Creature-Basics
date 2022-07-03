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
        ParametricGenerator g = new ParametricGenerator(p);

        SkeletonDefinition skeletonDefinition = g.BuildCreature();

        if (generate_skeleton)
        {
            GameObject rootGo = SkeletonAssembler.Assemble(skeletonDefinition);
            /*BoneDefinition test = new();
            test.Length = 2.0f;
            test.ProximalAxis = Vector3.down;
            test.VentralAxis = Vector3.forward;
            test.Category = BoneCategory.Torso;
            test.AttachmentHint = new();
            test.Thickness = 0.1f;

            BoneDefinition test2 = new();
            test2.Length = 2.0f;
            test2.ProximalAxis = Vector3.down;
            test2.VentralAxis = Vector3.forward;
            test2.Category = BoneCategory.Torso;
            test2.AttachmentHint = new();
            test2.AttachmentHint.VentralDirection = Vector3.back;
            test2.AttachmentHint.Offset = new Vector3(1.0f, 0.0f, 1.0f);
            test2.Thickness = 0.1f;

            test.childBones.Add(test2);
            test2.ParentBone = test;

            //GameObject rootGo = SkeletonAssembler.Assemble(test);*/


            rootGo.transform.parent = gameObject.transform;
            Physics.autoSimulation = false;
        }

        /*if (metaball_mesh)
        {
            MeshGenerator meshGen = GetComponent<MeshGenerator>();
            meshGen.Generate(c.makeMetaball());
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
