using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        LSystemEditor ed = gameObject.GetComponent<LSystemEditor>();
        SkeletonGenerator.Generate(ed.Evaluate());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
