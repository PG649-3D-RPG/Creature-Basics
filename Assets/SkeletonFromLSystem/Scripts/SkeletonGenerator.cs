using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using LSystem;


public static class BoneAdd{
    public static readonly Dictionary<char, BoneCategory> literalCategoryMap = new Dictionary<char, BoneCategory>{
        {'A',BoneCategory.Arm},
        {'L',BoneCategory.Leg},
        {'T',BoneCategory.Torso},
        {'C',BoneCategory.Head},
        {'H',BoneCategory.Hand},
        {'V',BoneCategory.Foot},
        {'U',BoneCategory.Shoulder},
        {'G',BoneCategory.Hip},
        {'S',BoneCategory.Other}
    };

        public static readonly Dictionary<BoneCategory, char> categoryLiteralMap = new Dictionary<BoneCategory, char>{
        {BoneCategory.Arm,'A'},
        {BoneCategory.Leg,'L'},
        {BoneCategory.Torso,'T'},
        {BoneCategory.Head,'C'},
        {BoneCategory.Hand,'H'},
        {BoneCategory.Foot,'V'},
        {BoneCategory.Shoulder,'U'},
        {BoneCategory.Hip,'G'},
        {BoneCategory.Other,'S'}
    };

    public static readonly Dictionary<char, string> literalStringMap = new Dictionary<char, string>{
        {'A',"arm"},
        {'L',"leg"},
        {'T',"torso"},
        {'C',"head"},
        {'H',"hand"},
        {'V',"foot"},
        {'U',"shoulder"},
        {'G',"hip"},
        {'S',"other"}
    };
    public static readonly Dictionary<BoneCategory, SoftJointLimit> defaultLowXLimit = new Dictionary<BoneCategory, SoftJointLimit>{
        {BoneCategory.Arm, new SoftJointLimit() {limit = -45}},
        {BoneCategory.Leg, new SoftJointLimit() {limit = 0}},
        {BoneCategory.Torso, new SoftJointLimit() {limit = -15}},
        {BoneCategory.Head, new SoftJointLimit() {limit = -45}},
        {BoneCategory.Hand, new SoftJointLimit() {limit = -45}},
        {BoneCategory.Foot, new SoftJointLimit() {limit = -45}},
        {BoneCategory.Shoulder, new SoftJointLimit() {limit = -5}},
        {BoneCategory.Hip, new SoftJointLimit() {limit = -5}},
        {BoneCategory.Other, new SoftJointLimit() {limit = 0}}
    };

    public static readonly Dictionary<BoneCategory, SoftJointLimit> defaultHighXLimit = new Dictionary<BoneCategory, SoftJointLimit>{
        {BoneCategory.Arm, new SoftJointLimit() {limit = 45}},
        {BoneCategory.Leg, new SoftJointLimit() {limit = 145}},
        {BoneCategory.Torso, new SoftJointLimit() {limit = 15}},
        {BoneCategory.Head, new SoftJointLimit() {limit = 45}},
        {BoneCategory.Hand, new SoftJointLimit() {limit = 45}},
        {BoneCategory.Foot, new SoftJointLimit() {limit = 45}},
        {BoneCategory.Shoulder, new SoftJointLimit() {limit = 5}},
        {BoneCategory.Hip, new SoftJointLimit() {limit = 5}},
        {BoneCategory.Other, new SoftJointLimit() {limit = 0}}
    };

    public static readonly Dictionary<BoneCategory, SoftJointLimit> defaultYLimit = new Dictionary<BoneCategory, SoftJointLimit>{
        {BoneCategory.Arm, new SoftJointLimit() {limit = 45}},
        {BoneCategory.Leg, new SoftJointLimit() {limit = 15}},
        {BoneCategory.Torso, new SoftJointLimit() {limit = 45}},
        {BoneCategory.Head, new SoftJointLimit() {limit = 45}},
        {BoneCategory.Hand, new SoftJointLimit() {limit = 45}},
        {BoneCategory.Foot, new SoftJointLimit() {limit = 5}},
        {BoneCategory.Shoulder, new SoftJointLimit() {limit = 5}},
        {BoneCategory.Hip, new SoftJointLimit() {limit = 5}},
        {BoneCategory.Other, new SoftJointLimit() {limit = 0}}
    };
    public static readonly Dictionary<BoneCategory, SoftJointLimit> defaultZLimit = new Dictionary<BoneCategory, SoftJointLimit>{
        {BoneCategory.Arm, new SoftJointLimit() {limit = 45}},
        {BoneCategory.Leg, new SoftJointLimit() {limit = 15}},
        {BoneCategory.Torso, new SoftJointLimit() {limit = 45}},
        {BoneCategory.Head, new SoftJointLimit() {limit = 45}},
        {BoneCategory.Hand, new SoftJointLimit() {limit = 45}},
        {BoneCategory.Foot, new SoftJointLimit() {limit = 5}},
        {BoneCategory.Shoulder, new SoftJointLimit() {limit = 5}},
        {BoneCategory.Hip, new SoftJointLimit() {limit = 5}},
        {BoneCategory.Other, new SoftJointLimit() {limit = 0}}
    };

}

public enum BoneCategory{
    Leg,
    Arm,
    Torso,
    Head,
    Hand,
    Foot,
    Shoulder,
    Hip,
    Other
}

public class SkeletonGenerator
{
    static float BoneTreeRadius = 0.025f;
    static float BoneTreeMass = 1.0f;

    class BoneTree{
        public Tuple<Vector3, Vector3> segment;

        public BoneTree parent;

        public List<BoneTree> children;

        private GameObject go;

        private Tuple<int,char> t;

        private string name;

        public BoneCategory boneCategory;

        public BoneTree(Tuple<Vector3, Vector3> segment, BoneTree parent, Tuple<int,char> t) {
            this.segment = segment;
            this.parent = parent;
            this.t = t;
            this.boneCategory = BoneAdd.literalCategoryMap[t.Item2];
            children = new List<BoneTree>();
        }

        public GameObject toGameObject() {
            if (parent != null) {
                go = gameObjectFromBoneTree(this, parent.go, name);
            } else {
                go = gameObjectFromBoneTree(this, null, name);
            }
            foreach (BoneTree child in children) {
                child.toGameObject();
            }
            //go.transform.position += this.min;
            return go;
        }

        public BoneTree findParent(Tuple<Vector3, Vector3> segment, bool inverse = false) {
            if (!inverse && this.segment.Item2 == segment.Item1) {
                return this;
            }
            else if(inverse && this.segment.Item1 == segment.Item1){
                return this;
            }
            foreach (BoneTree child in children) {
                var res = child.findParent(segment, inverse);
                if (res != null) {
                    return res;
                }
            }

            return null;
        }

        // public void annotate(Dictionary<char,int> d){
        //     int n = 0;
        //     d.TryGetValue(this.t.Item2,out n);
        //     d[this.t.Item2] = n+1;
        //     this.name = BoneAdd.literalStringMap[BoneAdd.categoryLiteralMap[this.boneCategory]]+"_"+(n);
        //     foreach(BoneTree child in children){
        //         child.annotate(d);
        //     }
        // }
         public void annotate(Dictionary<BoneCategory,int> d, BoneCategory last, int last_i = 0){
            int new_i = 0;
            int n = 0;
            if(this.boneCategory == last){
                new_i = last_i + 1;
            } 
            else{
                d[last] += 1;
                new_i = 0;
                d.TryGetValue(this.boneCategory,out n);
                d[this.boneCategory] = n;
            }
            this.name = BoneAdd.literalStringMap[BoneAdd.categoryLiteralMap[this.boneCategory]] + "_" + d[this.boneCategory] + "_" + new_i;
            foreach(BoneTree child in children){
                child.annotate(d, this.boneCategory, new_i);
            }
        }
    }

    public static GameObject Generate(LSystem.LSystem l) {
        BoneTree root = null;
        int i = 0;
        foreach (var segment in l.segments) {
            if (root == null) {
                root = new BoneTree(segment, null, l.fromRule[i]);
            } else {
                BoneTree parent = root.findParent(segment);
                BoneTree child = new BoneTree(segment, parent, l.fromRule[i]);
                parent.children.Add(child);
            }
            //Debug.Log(segment.ToString() + ":" + l.fromRule[i].Item2.ToString());
            i++;            
        }

        //calculate y transform, remove when l system stands above ground by itself
        // float min_f = 0;
        // foreach(var a in segments){
        //     var min_ = a.Item1.y < a.Item2.y ? a.Item1.y : a.Item2.y;
        //     if(min_ < min_f) min_f = min_;
        // }
        // root.min = Vector3.up * -min_f;
        // root.min = new Vector3(0,0,0);
        Dictionary<BoneCategory,int> d = new Dictionary<BoneCategory,int>();
        d.Add(root.boneCategory,0);
        root.annotate(d , root.boneCategory, -1);
        return root.toGameObject();
    }

    private static GameObject gameObjectFromBoneTree(BoneTree boneTree, GameObject parent, string name = "Bone") {
        Vector3 start = boneTree.segment.Item1;
        Vector3 end = boneTree.segment.Item2;
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
        }
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Limited;
        joint.angularZMotion = ConfigurableJointMotion.Limited;

        if(!parent) UnityEngine.Object.Destroy(result.GetComponent<ConfigurableJoint>());

        // TODO(markus): Scale capsule mesh correctly
        if(start != end){
            GameObject meshObject;
            if(boneTree.boneCategory == BoneCategory.Hand){
                meshObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                meshObject.transform.localScale = new Vector3(0.1f,length*0.9f,0.1f);
            }
            else{
                meshObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                meshObject.transform.localScale = new Vector3(0.1f,length*0.45f,0.1f);
            }
            meshObject.transform.parent = result.transform;
            meshObject.transform.position = result.transform.position;
            meshObject.transform.rotation = result.transform.rotation;            
        }

        joint.lowAngularXLimit = BoneAdd.defaultLowXLimit[boneTree.boneCategory];
        joint.highAngularXLimit = BoneAdd.defaultHighXLimit[boneTree.boneCategory];
        joint.angularYLimit = BoneAdd.defaultYLimit[boneTree.boneCategory];
        joint.angularZLimit = BoneAdd.defaultZLimit[boneTree.boneCategory];
        result.name = name;
        return result;
    }
}