using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BipedSettings", menuName = "PG649-CreatureGenerator/Biped Settings")]
public class BipedSettings : ParametricCreatureSettings
{
    [Header("Arms")]
    public NormalDistribution ArmThickness = new(0.3f, 0.2f, 0.0f, float.MaxValue);
    public NormalDistribution ArmLength = new(3.0f, 1.0f, 0.0f, float.MaxValue);
    public UniformIntegerDistribution ArmBones = new(2, 2);

    [Header("Legs")]
    public NormalDistribution LegLength = new(3.0f, 2.0f, 0.0f, float.MaxValue);
    public NormalDistribution LegThickness = new(0.3f, 0.2f, 0.0f, float.MaxValue);
    public UniformIntegerDistribution LegBones = new(2, 4);

    [Header("Feet")]
    public NormalDistribution FeetWidth = new(1.0f, 0.5f, 0.0f, float.MaxValue);
    public NormalDistribution FeetLength = new(1.5f, 0.5f, 0.0f, float.MaxValue);
    
    [Header("Hands")]
    public NormalDistribution HandRadius = new(0.75f, 0.25f, 0.0f, float.MaxValue);

}