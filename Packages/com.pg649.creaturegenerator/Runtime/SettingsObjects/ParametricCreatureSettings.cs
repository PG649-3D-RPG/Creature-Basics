using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ParametricCreatureSettings", menuName = "PG649-CreatureGenerator/Parametric Creature Settings")]
public class ParametricCreatureSettings : ScriptableObject
{
    public float minLegThickness = 0.1f;
    public float maxLegThickness = 0.5f;
    public float minLegSize = 2f;
    public float maxLegSize = 6f;

    public float minArmThickness = 0.1f;
    public float maxArmThickness = 0.5f;
    public float minArmSize = 2f;
    public float maxArmSize = 6f;

    public float minTorsoSize = 4f;
    public float maxTorsoSize = 8f;
    public float minTorsoThickness = 0.4f;
    public float maxTorsoThickness = 1.5f;

    public float minNeckSize = 1f;
    public float maxNeckSize = 3f;
    public int minNeckSegments = 1;
    public int maxNeckSegments = 4;

    public float minHeadSize = 0.3f;
    public float maxHeadSize = 1.5f;

    public float minSnoutSize = 1.2f;
    public float maxSnoutSize = 4f;
    public float minSnoutThickness = 0.1f;
    public float maxSnoutThickness = 0.5f;
}
