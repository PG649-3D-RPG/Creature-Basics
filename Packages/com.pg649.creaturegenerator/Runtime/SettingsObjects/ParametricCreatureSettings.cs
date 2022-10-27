using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ParametricCreatureSettings", menuName = "PG649-CreatureGenerator/Parametric Creature Settings")]
public class ParametricCreatureSettings : ScriptableObject
{
    [Header("Arms")]
    public NormalDistribution ArmThickness = new(0.3f, 0.2f, 0.0f, float.MaxValue);
    public NormalDistribution ArmLength = new(3.0f, 1.0f, 0.0f, float.MaxValue);
    public UniformIntegerDistribution ArmBones = new(2, 2);
    
    [Header("Feet")]
    public NormalDistribution FeetWidth = new(1.0f, 0.5f, 0.0f, float.MaxValue);
    public NormalDistribution FeetLength = new(1.5f, 0.5f, 0.0f, float.MaxValue);
    
    [Header("Hands")]
    public NormalDistribution HandRadius = new(0.75f, 0.25f, 0.0f, float.MaxValue);
    
    [Header("Head")]
    public NormalDistribution HeadSize = new(0.9f, 0.6f, 0.0f, float.MaxValue);
    
    [Header("Hips")] 
    public NormalDistribution HipThickness = new(1.0f, 0.6f, 0.0f, float.MaxValue);
    public NormalDistribution HipLength = new(1.5f, 0.5f, 0.0f, float.MaxValue);

    [Header("Legs")] 
    public NormalDistribution LegLength = new(3.0f, 2.0f, 0.0f, float.MaxValue);
    public NormalDistribution LegThickness = new(0.3f, 0.2f, 0.0f, float.MaxValue);
    public UniformIntegerDistribution LegBones = new(2, 4);
    
    [Header("Neck")]
    public NormalDistribution NeckThickness = new(0.3f, 0.2f, 0.0f, float.MaxValue);
    public NormalDistribution NeckLength = new(0.8f, 0.2f, 0.0f, float.MaxValue);
    public UniformIntegerDistribution NeckBones = new(1, 4);

    [Header("Shoulders")]
    public NormalDistribution ShoulderThickness = new(0.3f, 0.2f, 0.0f, float.MaxValue);
    public NormalDistribution ShoulderLength = new(0.75f, 0.25f, 0.0f, float.MaxValue);

    [Header("Torso")]
    public NormalDistribution TorsoThickness = new(1.0f, 0.5f, 0.0f, float.MaxValue);
    public NormalDistribution TorsoLength = new(2.0f, 1.0f, 0.0f, float.MaxValue);
    public UniformIntegerDistribution TorsoBones = new(1, 3);

}