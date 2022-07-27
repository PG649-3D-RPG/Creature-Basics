using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneRotationDebugger : MonoBehaviour
{
    // Start is called before the first frame update
    ConfigurableJoint joint;

    public float xAngle;
    public float yAngle;
    public float zAngle;

    void Start()
    {
        joint = GetComponent<ConfigurableJoint>();
        xAngle = joint.targetRotation.eulerAngles.x;
        yAngle = joint.targetRotation.eulerAngles.y;
        zAngle = joint.targetRotation.eulerAngles.z;
        foreach (var rb in GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = false;
        }
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
    }

    // Update is called once per frame
    void Update()
    {
        joint.targetRotation = Quaternion.Euler(xAngle, yAngle, zAngle);
    }
}
