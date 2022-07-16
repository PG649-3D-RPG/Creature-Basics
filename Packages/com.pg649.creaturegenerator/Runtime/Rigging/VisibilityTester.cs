using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibilityTester {

    private SignedDistanceField sdf;

    public VisibilityTester(Mesh mesh, int sdfResolution=128) {
        this.sdf = new SignedDistanceField(mesh, sdfResolution);
    }

    //faster when v2 is farther inside than v1
    public bool CanSee(Vector3 v1, Vector3 v2) {
        float maxVal = 0.002f;
        float distAtV2 = sdf.sampleSdf(v2);
        float left = (v2 - v1).magnitude;
        float leftInc = left / 100.0f;
        Vector3 diff = (v2 - v1) / 100.0f;
        Vector3 cur = v1 + diff;
        while(left >= 0.0f) {
            float curDist = sdf.sampleSdf(cur);
            if(curDist > maxVal)
                return false;
            //if curDist and atV2 are so negative that distance won't reach above maxVal, return true
            if(curDist + distAtV2 + left <= maxVal)
                return true;
            cur += diff;
            left -= leftInc;
        }
        return true;
    }

}