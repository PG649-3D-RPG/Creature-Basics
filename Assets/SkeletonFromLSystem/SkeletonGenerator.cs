using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SkeletonGenerator
{
    static float BoneTreeRadius = 0.025f;
    static float BoneTreeMass = 1.0f;

    class BoneTree{
        public Tuple<Vector3, Vector3> segment;

        public BoneTree parent;

        public List<BoneTree> children;

        private GameObject go;

        public  Vector3 min;

        public BoneTree(Tuple<Vector3, Vector3> segment, BoneTree parent) {
            this.segment = segment;
            this.parent = parent;
            children = new List<BoneTree>();
        }

        public GameObject toGameObject() {
            if (parent != null) {
                go = gameObjectFromBoneTree(segment, parent.go);
            } else {
                go = gameObjectFromBoneTree(segment, null);
            }
            foreach (BoneTree child in children) {
                child.toGameObject();
            }
            //go.transform.position += this.min;
            return go;
        }

        public BoneTree findParent(Tuple<Vector3, Vector3> segment) {
            if (this.segment.Item2 == segment.Item1) {
                return this;
            }
            foreach (BoneTree child in children) {
                var res = child.findParent(segment);
                if (res != null) {
                    return res;
                }
            }
            return null;
        }
    }

    public static GameObject Generate(IList<Tuple<Vector3, Vector3>> segments) {
        BoneTree root = null;
        foreach (var segment in segments) {
            if (root == null) {
                root = new BoneTree(segment, null);
            } else {
                BoneTree parent = root.findParent(segment);
                BoneTree child = new BoneTree(segment, parent);
                parent.children.Add(child);
            }
        }
         //calculate y transform, remove when l system stands above ground by itself
        // float min_f = 0;
        // foreach(var a in segments){
        //     var min_ = a.Item1.y < a.Item2.y ? a.Item1.y : a.Item2.y;
        //     if(min_ < min_f) min_f = min_;
        // }
        // root.min = Vector3.up * -min_f;
        // root.min = new Vector3(0,0,0);

        return root.toGameObject();
    }

    private static GameObject gameObjectFromBoneTree(Tuple<Vector3, Vector3> segment, GameObject parent) {
        Vector3 start = segment.Item1;
        Vector3 end = segment.Item2;
        float length = Vector3.Distance(start, end);

        // TODO(markus): Find better names for BoneTrees
        GameObject result = new GameObject("BoneTree");
        result.transform.rotation = Quaternion.LookRotation(end-start) * Quaternion.FromToRotation(Vector3.forward, Vector3.down);//Quaternion.FromToRotation(start,end);
        result.transform.position = start + ((end-start).normalized*(length/2));  
        //result.transform.localPosition = result.transform.localRotation * Vector3.forward * length * 1.5f; 

        Rigidbody rb = result.AddComponent<Rigidbody>();
        rb.mass = BoneTreeMass;

        ConfigurableJoint joint = result.AddComponent<ConfigurableJoint>();
        //joint.transform.rotation = result.transform.rotation;
        //joint.targetRotation = Quaternion.LookRotation(end-start);

        joint.anchor = new Vector3(0,-length/2,0);

        CapsuleCollider collider = result.AddComponent<CapsuleCollider>();
        collider.height = length;
        collider.radius = BoneTreeRadius;
        //result.AddComponent<Bone>();
        
        if (parent) {
            joint.connectedBody = parent.GetComponent<Rigidbody>();
            joint.connectedAnchor = parent.transform.position;
            result.transform.parent = parent.transform;
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
        }
        else{
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
        }

        // TODO(markus): Scale capsule mesh correctly
        // GameObject meshObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        // meshObject.transform.parent = result.transform;
        // meshObject.transform.position = result.transform.position;
        // meshObject.transform.rotation = result.transform.rotation;

        return result;
    }
}
