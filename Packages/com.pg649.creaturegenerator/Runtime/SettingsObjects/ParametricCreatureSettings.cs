using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ParametricCreatureSettings", menuName = "PG649-CreatureGenerator/Parametric Creature Settings")]
public class ParametricCreatureSettings : ScriptableObject
{
    [Header("Arms")]
    public NormalDistribution ArmThickness = new(0.3f, 0.2f);
    public NormalDistribution ArmLength = new(3.0f, 1.0f);
    public UniformIntegerDistribution ArmBones = new(2, 2);
    
    [Header("Feet")]
    public NormalDistribution FeetWidth = new(1.0f, 0.5f);
    public NormalDistribution FeetLength = new(1.5f, 0.5f);
    
    [Header("Hands")]
    public NormalDistribution HandRadius = new(0.75f, 0.25f);
    
    [Header("Head")]
    public NormalDistribution HeadSize = new(0.9f, 0.6f);
    
    [Header("Hips")] 
    public NormalDistribution HipThickness = new(1.0f, 0.6f);
    public NormalDistribution HipLength = new(1.5f, 0.5f);

    [Header("Legs")] 
    public NormalDistribution LegLength = new(3.0f, 2.0f);
    public NormalDistribution LegThickness = new(0.3f, 0.2f);
    public UniformIntegerDistribution LegBones = new(2, 4);
    
    [Header("Neck")]
    public NormalDistribution NeckThickness = new(0.3f, 0.2f);
    public NormalDistribution NeckLength = new(0.8f, 0.2f);
    public UniformIntegerDistribution NeckBones = new(1, 4);

    [Header("Shoulders")]
    public NormalDistribution ShoulderThickness = new(0.3f, 0.2f);
    public NormalDistribution ShoulderLength = new(0.75f, 0.25f);

    [Header("Torso")]
    public NormalDistribution TorsoThickness = new(1.0f, 0.5f);
    public NormalDistribution TorsoLength = new(2.0f, 1.0f);
    public UniformIntegerDistribution TorsoBones = new(1, 3);

}
