using System;
using System.Collections.Generic;
using System.Text;
using LSystem;
using MarchingCubesProject;
using UnityEngine;

public class CreatureGenerator
{
    public static GameObject ParametricBiped(CreatureGeneratorSettings settings, ParametricCreatureSettings creatureSettings,
        int? seed)
    {
        var gen = new BipedGenerator();
        var def = gen.BuildCreature(creatureSettings, seed);
        return Parametric(settings, def);
    }

    public static GameObject ParametricQuadruped(CreatureGeneratorSettings settings,
        ParametricCreatureSettings creatureSettings, int seed = 0)
    {
        var gen = new QuadrupedGenerator();
        var def = gen.BuildCreature(creatureSettings, seed);
        return Parametric(settings, def);
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

    private static GameObject Parametric(CreatureGeneratorSettings settings, SkeletonDefinition skeletonDef)
    {
        GameObject go = new GameObject("creature");

        var skeleton = SkeletonAssembler.Assemble(skeletonDef, settings.SkeletonSettings, settings.DebugSettings);
        SkeletonLinter.Lint(skeleton, settings.SkeletonLinterSettings);
        skeleton.transform.parent = go.transform;

        if (settings.MeshSettings.GenerateMetaballMesh)
        {
            var meshGen = go.AddComponent<MeshGenerator>();
            meshGen.ApplySettings(settings.MeshSettings, settings.DebugSettings);
            meshGen.Generate(Metaball.BuildFromSkeleton(skeleton));
        }

        Physics.autoSimulation = !settings.DebugSettings.DisablePhysics;

        if (settings.DebugSettings.LogAdditionalInfo)
        {
            LogInfo(skeleton);
        }

        return go;
    }

    private static void LogInfo(Skeleton skeleton)
    {
        var mass = 0.0f;
        var rbs = 0;
        foreach (var (_, _, rb, _) in skeleton.Iterator())
        {
            mass += rb.mass;
            rbs++;
        }
        Debug.Log("===== Creature Stats =====\n");
        Debug.Log("Mass:\n");
        Debug.Log("\tTotal Mass: " + mass + "\n");
        Debug.Log("\tAverage Bone Mass: " + (mass / (float)rbs) + "\n");
        Debug.Log("Skeleton:\n");
        Debug.Log("\tTotal Observations: " + skeleton.SettingsInstance.Observations().Count + "\n");
        StringBuilder observation = new();
        observation.Append("\tObservations: ");
        foreach (var f in skeleton.SettingsInstance.Observations())
        {
            observation.Append(f + ", ");
        }
        Debug.Log(observation + "\n");
    }
}