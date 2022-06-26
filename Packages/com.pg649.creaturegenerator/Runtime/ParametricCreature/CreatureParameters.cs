using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureParameters : MonoBehaviour
{
    [Range(1,2)]
    public int minLegPairs = 1;
    [Range(1, 2)]
    public int maxLegPairs = 2;
    public float minLegThickness = 0.1f;
    public float maxLegThickness = 0.5f;
    public float minLegSize = 2f;
    public float maxLegSize = 6f;

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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Returns whether or not the parameters are valid values
    public bool isValid()
    {
        return true;
    }
}
