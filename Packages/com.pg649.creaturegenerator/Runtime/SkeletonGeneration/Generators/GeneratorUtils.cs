using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GeneratorUtils
{
    public static (BoneDefinition, BoneDefinition) BuildLimb(List<float> lengths, List<float> thicknesses, Func<float, float, int, BoneDefinition> factory)
    {
        Assert.AreEqual(lengths.Count, thicknesses.Count);
        Assert.IsTrue(lengths.Count > 0);

        var root = factory(lengths[0], thicknesses[0], 0);
        var prev = root;

        for (var i = 1; i < thicknesses.Count; i++) {
            var part = factory(lengths[i], thicknesses[i], i);
            prev.LinkChild(part);
            prev = part;
        }
        return (root, prev);
    }
}