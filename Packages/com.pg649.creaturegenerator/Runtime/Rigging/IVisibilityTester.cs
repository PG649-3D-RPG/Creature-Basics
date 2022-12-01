using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IVisibilityTester {

    bool CanSee(Vector3 v1, Vector3 v2);

}