using System;
using System.Collections.Generic;
using LSystem;
using MarchingCubesProject;
using UnityEngine;

public class SkeletonTest : MonoBehaviour
{
    [Tooltip("Generate skeleton")]
    public bool generateSkeleton = true;
    [Tooltip("Generate primitive mesh")]
    public bool primitiveMesh = false;
    [Tooltip("Generate metaball mesh")]
    public bool metaballMesh = true;

    [Tooltip("Exported prefab filename. Defaults to GameObject's name.")]
    public string creatureExportName = "";
    [Tooltip("Path where the prefab will be saved. Must end with a \"/\". Defaults to \"Packages/com.pg649.creaturegenerator/Runtime/SkeletonFromLSystem/Prefabs/\".")]
    public string creatureExportPath = "";

    // Start is called before the first frame update
    private GameObject orientationCube;
    void Start()
    {
        LSystemEditor ed = gameObject.GetComponent<LSystemEditor>();
        LSystem.LSystem l = ed.BuildLSystem();
        List<Tuple<Vector3, Vector3>> segments = l.segments;
        if (generateSkeleton)
        {
            GameObject boneTree = SkeletonGenerator.Generate(l, primitiveMesh);
            boneTree.transform.parent = gameObject.transform;
            gameObject.transform.Translate(new Vector3(0,0.025f,0));
            
        }
        Transform oc = transform.Find("orientation cube");

        orientationCube = oc == null ? CreateOrientationCube(gameObject) : oc.gameObject;
        
        if(metaballMesh){
            Segment[] segments_ = new Segment[segments.Count];
            for (int i = 0; i < segments.Count; i++)
            {
                segments_[i] = new Segment(segments[i].Item1, segments[i].Item2, .025f);
            }
            Metaball m = Metaball.BuildFromSegments(segments_, useCapsules: false);
            MeshGenerator mg = gameObject.GetComponent<MeshGenerator>();
            mg.material = new Material(Shader.Find("MadCake/Material/Standard hacked for DQ skinning"));
            mg.material.color = Color.white;

            mg.Generate(m);
        }

        Physics.autoSimulation = true;
    }

    // Update is called once per frame
    void Update()
    {
        Transform t = GetComponentsInChildren<Transform>()[1];
        Vector3 pos = t.position;
        pos.y = 0;
        orientationCube.transform.position = pos; 
        orientationCube.transform.rotation = t.rotation;
    }

    GameObject CreateOrientationCube(GameObject parent){
        GameObject orientationCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(orientationCube.GetComponent<Collider>());
        orientationCube.transform.parent = parent.transform;
        orientationCube.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
        orientationCube.name = "orientation cube";
        return orientationCube;
    }
}


