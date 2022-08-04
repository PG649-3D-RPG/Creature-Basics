using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ParametricCreatureSettings", menuName = "PG649-CreatureGenerator/Parametric Creature Settings")]
public class ParametricCreatureSettings : ScriptableObject
{
    [Header("Legs")]
    public float minLegThickness = 0.1f;
    public float maxLegThickness = 0.5f;
    [Space(10)]
    public float minLegSize = 2f;
    public float maxLegSize = 6f;

    [Header("Arms")]
    public float minArmThickness = 0.1f;
    public float maxArmThickness = 0.5f;
    [Space(10)]
    public float minArmSize = 2f;
    public float maxArmSize = 6f;

    [Header("Torso")]
    public float minTorsoSize = 4f;
    public float maxTorsoSize = 8f;
    [Space(10)]
    public float minTorsoThickness = 0.4f;
    public float maxTorsoThickness = 1.5f;

    [Header("Neck")]
    public float minNeckSize = 1f;
    public float maxNeckSize = 3f;
    [Space(10)]
    public int minNeckSegments = 1;
    public int maxNeckSegments = 4;

    [Header("Head")]
    public float minHeadSize = 0.3f;
    public float maxHeadSize = 1.5f;

    [Header("Snout")]
    public float minSnoutSize = 1.2f;
    public float maxSnoutSize = 4f;
    [Space(10)]
    public float minSnoutThickness = 0.1f;
    public float maxSnoutThickness = 0.5f;
}
