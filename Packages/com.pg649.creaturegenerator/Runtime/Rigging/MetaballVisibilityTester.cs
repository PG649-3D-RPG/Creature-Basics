using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaballVisibilityTester : IVisibilityTester {

    private Metaball metaball;
    private float surface;

    public MetaballVisibilityTester (Metaball metaball, float surface) {
        this.metaball = metaball;
        this.surface = surface;
    }

    public bool CanSee(Vector3 v1, Vector3 v2) {
        Vector3 dir = v2 - v1;
        int steps = 100;
        float invSteps = (1.0f / steps);
        for (int i = 10; i < steps; i++) {
            Vector3 pos = v1 + steps * (dir * invSteps);

            float val = metaball.Value(pos.x, pos.y, pos.z);
            if(val < surface)
                return false;
        }
        return true;
    }

}