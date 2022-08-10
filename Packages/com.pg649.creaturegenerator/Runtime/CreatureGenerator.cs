using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LSystem;
using MarchingCubesProject;
using UnityEngine;

public class CreatureGenerator
{
    public static GameObject ParametricBiped(CreatureGeneratorSettings settings, ParametricCreatureSettings creatureSettings,
        int seed = 0)
    {
        return Parametric(ParametricGenerator.Mode.Biped, settings, creatureSettings, seed);
    }

    public static GameObject ParametricQuadruped(CreatureGeneratorSettings settings,
        ParametricCreatureSettings creatureSettings, int seed = 0)
    {
        return Parametric(ParametricGenerator.Mode.Quadruped, settings, creatureSettings, seed);
    }

    public static GameObject LSystem(CreatureGeneratorSettings settings, LSystemSettings lSystemSettings)
    {
        var lSystem = lSystemSettings.BuildLSystem();

        GameObject go = new GameObject("creature");
        
        List<Tuple<Vector3, Vector3>> segments = lSystem.segments;
        GameObject rootBone = SkeletonGenerator.Generate(lSystem, settings.DebugSettings.AttachPrimitiveMesh, settings.SkeletonSettings.ConnectHips);
        rootBone.transform.parent = go.transform;

        // TODO: Readd orientation cube
        
        if (settings.MeshSettings.GenerateMetaballMesh){
            Segment[] segments_ = new Segment[segments.Count];
            for (int i = 0; i < segments.Count; i++)
            {
                segments_[i] = new Segment(segments[i].Item1, segments[i].Item2, .025f);
            }
            Metaball m = Metaball.BuildFromSegments(segments_, useCapsules: false);
            MeshGenerator meshGen = go.AddComponent<MeshGenerator>();
            meshGen.ApplySettings(settings.MeshSettings, settings.DebugSettings);
            meshGen.Generate(m);
        }

        return go;
    }

    private static GameObject Parametric(
        ParametricGenerator.Mode mode,
        CreatureGeneratorSettings settings,
        ParametricCreatureSettings creatureSettings, int seed = 0)
    {
        GameObject go = new GameObject("creature");

        var g = new ParametricGenerator(creatureSettings);
        var skeletonDef = g.BuildCreature(mode, seed);
        var rootBone = SkeletonAssembler.Assemble(skeletonDef, settings.SkeletonSettings, settings.DebugSettings);
        rootBone.transform.parent = go.transform;

        if (settings.MeshSettings.GenerateMetaballMesh)
        {
            var meshGen = go.AddComponent<MeshGenerator>();
            meshGen.ApplySettings(settings.MeshSettings, settings.DebugSettings);
            meshGen.Generate(Metaball.BuildFromSkeleton(rootBone.GetComponent<Skeleton>()));
        }

        Physics.autoSimulation = !settings.DebugSettings.DisablePhysics;

        return go;
    }
}