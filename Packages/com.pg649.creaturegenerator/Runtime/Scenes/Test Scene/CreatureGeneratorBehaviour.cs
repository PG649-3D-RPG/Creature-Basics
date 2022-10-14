using System.Collections;
using System.Collections.Generic;
using LSystem;
using UnityEngine;

public class CreatureGeneratorBehaviour : MonoBehaviour
{
    public enum TargetCreature
    {
        LSystem,
        ParametricBiped,
        ParametricQuadruped,
    }

    public TargetCreature Target = TargetCreature.ParametricBiped;
    public CreatureGeneratorSettings Settings;
    public ParametricCreatureSettings CreatureSettings;
    public LSystemSettings LSystemSettings;
    public JointLimitOverrides JointLimitOverrides;
    public int Seed = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        Dictionary< (BoneCategory, BoneCategory), JointLimits > limitOverrides = null;
        if (JointLimitOverrides != null)
            limitOverrides = JointLimitOverrides.ToDict();

        GameObject creature = null;
        switch (Target)
        {
            case TargetCreature.LSystem:
                creature = CreatureGenerator.LSystem(Settings, LSystemSettings);
                break;
            case TargetCreature.ParametricBiped:
                creature = CreatureGenerator.ParametricBiped(Settings, CreatureSettings, Seed, limitOverrides);
                break;
            case TargetCreature.ParametricQuadruped:
                creature = CreatureGenerator.ParametricQuadruped(Settings, CreatureSettings, Seed, limitOverrides); 
                break;
            default: break;
        }

        if (creature != null)
        {
            creature.transform.parent = gameObject.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
