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
    public BipedSettings BipedSettings;
    public QuadrupedSettings QuadrupedSettings;
    public LSystemSettings LSystemSettings;
    public JointLimitOverrides JointLimitOverrides;
    public int Seed = 0;
    
    // Start is called before the first frame update
    void Start()
    {

        GameObject creature = null;
        switch (Target)
        {
            case TargetCreature.LSystem:
                creature = CreatureGenerator.LSystem(Settings, LSystemSettings);
                break;
            case TargetCreature.ParametricBiped:
                creature = CreatureGenerator.ParametricBiped(Settings, BipedSettings, Seed, JointLimitOverrides);
                break;
            case TargetCreature.ParametricQuadruped:
                creature = CreatureGenerator.ParametricQuadruped(Settings, QuadrupedSettings, Seed, JointLimitOverrides); 
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
