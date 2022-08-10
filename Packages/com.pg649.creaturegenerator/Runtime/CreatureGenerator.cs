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

    public static GameObject LSystem(CreatureGeneratorSettings settings, LSystemSettings lSystemSettings, int seed)
    {
        var lSystem = lSystemSettings.BuildLSystem(seed);

        List<Tuple<Vector3, Vector3>> segments = lSystem.segments;
        GameObject root = SkeletonGenerator.Generate(lSystem, settings.DebugSettings.AttachPrimitiveMesh, settings.SkeletonSettings.ConnectHips);
        // TODO: Readd orientation cube

        if (settings.MeshSettings.GenerateMetaballMesh)
        {
            Segment[] segments_ = new Segment[segments.Count];
            for (int i = 0; i < segments.Count; i++)
            {
                segments_[i] = new Segment(segments[i].Item1, segments[i].Item2, .025f);
            }
            Metaball m = Metaball.BuildFromSegments(segments_, useCapsules: false);
            MeshGenerator mg = root.AddComponent<MeshGenerator>();
            mg.ApplySettings(settings.MeshSettings, settings.DebugSettings);
            mg.material.color = Color.white;

            mg.Generate(m);
        }

        return root;
    }

    private static GameObject Parametric(
        ParametricGenerator.Mode mode,
        CreatureGeneratorSettings settings,
        ParametricCreatureSettings creatureSettings, int seed = 0)
    {
        var g = new ParametricGenerator(creatureSettings);
        var skeletonDef = g.BuildCreature(mode, seed);
        var root = SkeletonAssembler.Assemble(skeletonDef, settings.SkeletonSettings, settings.DebugSettings);

        if (settings.MeshSettings.GenerateMetaballMesh)
        {
            var meshGen = root.AddComponent<MeshGenerator>();
            meshGen.ApplySettings(settings.MeshSettings, settings.DebugSettings);
            meshGen.Generate(Metaball.BuildFromSkeleton(root.GetComponent<Skeleton>()));
        }

        Physics.autoSimulation = !settings.DebugSettings.DisablePhysics;

        return root;
    }
}