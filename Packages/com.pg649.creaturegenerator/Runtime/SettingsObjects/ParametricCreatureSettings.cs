using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ParametricCreatureSettings", menuName = "PG649-CreatureGenerator/Parametric Creature Settings")]
public class ParametricCreatureSettings : ScriptableObject
{
    [Header("Hips")]
    [Tooltip("Hip bones will be at least this thick.")]
    public float minimumHipThickness = 0.4f;
    [Tooltip("Hip bones will be at most this thick.")]
    public float maximumHipThickness = 1.5f;
    [Space(10)]
    [Tooltip("Hip bones will be at least this long.")]
    public float minimumHipLength = 1.0f;
    [Tooltip("Hip bones will be at most this long.")]
    public float maximumHipLength = 2.0f;
    
    [Header("Legs")]
    [Tooltip("Leg bones will be at least this thick.")]
    public float minimumLegThickness = 0.1f;
    [Tooltip("Leg bones will be at most this thick.")]
    public float maximumLegThickness = 0.5f;
    [Space(10)]
    [Tooltip("Leg limbs will be at least this long, excluding hips and feet.")] 
    public float minimumTotalLegLength = 2f;
    [Tooltip("Leg limbs will be at most this long, excluding hips and feet.")] 
    public float maximumTotalLegLength = 6f;
    
    [Header("Feet")]
    [Tooltip("Feet will be at least this wide.")]
    public float minimumFeetWidth = 0.4f;
    [Tooltip("Feet will be at most this wide.")]
    public float maximumFeetWidth = 1.5f;
    [Space(10)]
    [Tooltip("Feet will be at least this long.")]
    public float minimumFeetLength = 1.0f;
    [Tooltip("Feet will be at most this long.")]
    public float maximumFeetLength = 2.0f;

    [Header("Shoulders")] 
    [Tooltip("Shoulder bones will be at least this thick.")]
    public float minimumShoulderThickness = 0.1f;
    [Tooltip("Shoulder bones will be at most this thick.")]
    public float maximumShoulderThickness = 0.5f;
    [Space(10)]
    [Tooltip("Shoulder bones will be at least this long.")]
    public float minimumShoulderLength = 0.5f;
    [Tooltip("Shoulder bones will be at most this long.")]
    public float maximumShoulderLength = 1.0f;

    [Header("Arms")]
    [Tooltip("Arm bones will be at least this thick.")]
    public float minimumArmThickness = 0.1f;
    [Tooltip("Arm bones will be at most this thick.")]
    public float maximumArmThickness = 0.5f;
    [Space(10)]
    [Tooltip("Arm limbs will be at least this long, excluding shoulders and hands.")]
    public float minimumTotalArmLength = 2f;
    [Tooltip("Arm limbs will be at most this long, excluding shoulders and hands.")]
    public float maximumTotalArmLength = 6f;

    [Header("Hands")] 
    [Tooltip("Hand bones will be at least this radius.")]
    public float minimumHandRadius = 0.5f;
    [Tooltip("Hand bones will be at most this radius.")]
    public float maximumHandRadius = 1.0f;

    [Header("Torso")]
    [Tooltip("Torso bones will be at least this thick.")]
    public float minimumTorsoThickness = 0.4f;
    [Tooltip("Torso bones will be at most this thick.")]
    public float maximumTorsoThickness = 1.5f;
    [Space(10)]
    [Tooltip("Complete torso will be at least this long, excluding neck and hips.")]
    public float minimumTotalTorsoLength = 4f;
    [Tooltip("Complete torso will be at most this long, excluding neck and hips.")]
    public float maximumTotalTorsoLength = 8f;

    [Header("Neck")] 
    [Tooltip("Neck bones will be at least this thick.")]
    public float minimumNeckThickness = 0.1f;
    [Tooltip("Neck bones will be at most this thick.")]
    public float maximumNeckThickness = 0.5f;
    [Space(10)]
    [Tooltip("Complete neck will be at least this long.")]
    public float minimumNeckLength = 1f;
    [Tooltip("Complete neck will be at most this long.")]
    public float maximumNeckLength = 3f;
    [Space(10)]
    [Tooltip("Neck will be divided into at least this many bones.")]
    public int minimumNeckBones = 1;
    [Tooltip("Neck will be divided into at most this many bones.")]
    public int maximumNeckBones = 4;

    [Header("Head")]
    [Tooltip("Head will be at least this long and thick.")]
    public float minimumHeadSize = 0.3f;
    [Tooltip("Head will be at most this long and thick.")]
    public float maximumHeadSize = 1.5f;

    [Header("Snout")]
    public float minSnoutSize = 1.2f;
    public float maxSnoutSize = 4f;
    [Space(10)]
    public float minSnoutThickness = 0.1f;
    public float maxSnoutThickness = 0.5f;
}
