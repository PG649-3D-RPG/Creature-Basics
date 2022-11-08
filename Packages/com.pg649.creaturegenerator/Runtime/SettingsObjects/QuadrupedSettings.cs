using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "QuadrupedSettings", menuName = "PG649-CreatureGenerator/Quadruped Settings")]
public class QuadrupedSettings : ParametricCreatureSettings
{
    [Header("Front Legs")]
    public NormalDistribution FrontLegLength = new(3.0f, 2.0f, 0.0f, float.MaxValue);
    public NormalDistribution FrontLegThickness = new(0.3f, 0.2f, 0.0f, float.MaxValue);
    public UniformIntegerDistribution FrontLegBones = new(2, 4);
    [Tooltip("Make front and hind legs rotate in opposite directions")]
    public bool MirrorFrontLegs = true;

    [Header("Hind Legs")]
    public NormalDistribution HindLegLength = new(3.0f, 2.0f, 0.0f, float.MaxValue);
    public NormalDistribution HindLegThickness = new(0.3f, 0.2f, 0.0f, float.MaxValue);
    public UniformIntegerDistribution HindLegBones = new(2, 4);
}