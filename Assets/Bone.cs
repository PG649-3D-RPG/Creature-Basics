using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bone : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnDrawGizmosSelected()
    {
        var joints = gameObject.GetComponents<ConfigurableJoint>();
            
        foreach (var item in joints)
        {
            Gizmos.color = Color.yellow;
            var connectedPos = item.connectedBody.gameObject.transform.position;
            Gizmos.DrawLine(transform.position, connectedPos);
            Gizmos.DrawCube(connectedPos, new Vector3(0.1f, 0.1f, 0.1f));
        }
    }
}
