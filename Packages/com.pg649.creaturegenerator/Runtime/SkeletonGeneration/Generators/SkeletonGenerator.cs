using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LSystem;
using UnityEngine;

public static class BoneJointSettings
{

}


public static class BoneAdd
{
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
        {BoneCategory.Arm, new SoftJointLimit() {limit = -75}},
        {BoneCategory.LowerArm, new SoftJointLimit() {limit = 0}},
        {BoneCategory.Leg, new SoftJointLimit() {limit = -15}},
        {BoneCategory.LowerLeg1,new SoftJointLimit() {limit = -90}},
        {BoneCategory.LowerLeg2, new SoftJointLimit() {limit = 0}},
        {BoneCategory.Torso, new SoftJointLimit() {limit = -15}},
        {BoneCategory.Head, new SoftJointLimit() {limit = -45}},
        {BoneCategory.Hand, new SoftJointLimit() {limit = -45}},
        {BoneCategory.Foot, new SoftJointLimit() {limit = -45}},
        {BoneCategory.Shoulder, new SoftJointLimit() {limit = -5}},
        {BoneCategory.Hip, new SoftJointLimit() {limit = -5}},
        {BoneCategory.Other, new SoftJointLimit() {limit = 0}}
    };

    public static readonly Dictionary<BoneCategory, SoftJointLimit> defaultHighXLimit = new Dictionary<BoneCategory, SoftJointLimit>{
        {BoneCategory.Arm, new SoftJointLimit() {limit = 75}},
        {BoneCategory.LowerArm, new SoftJointLimit() {limit = 90}},
        {BoneCategory.Leg, new SoftJointLimit() {limit = 145}},
        {BoneCategory.LowerLeg1,new SoftJointLimit() {limit = 0}},
        {BoneCategory.LowerLeg2, new SoftJointLimit() {limit = 90}},
        {BoneCategory.Torso, new SoftJointLimit() {limit = 15}},
        {BoneCategory.Head, new SoftJointLimit() {limit = 45}},
        {BoneCategory.Hand, new SoftJointLimit() {limit = 45}},
        {BoneCategory.Foot, new SoftJointLimit() {limit = 45}},
        {BoneCategory.Shoulder, new SoftJointLimit() {limit = 5}},
        {BoneCategory.Hip, new SoftJointLimit() {limit = 5}},
        {BoneCategory.Other, new SoftJointLimit() {limit = 0}}
    };

    public static readonly Dictionary<BoneCategory, SoftJointLimit> defaultYLimit = new Dictionary<BoneCategory, SoftJointLimit>{
        {BoneCategory.Arm, new SoftJointLimit() {limit = 0}},
        {BoneCategory.LowerArm, new SoftJointLimit() {limit = 0}},
        {BoneCategory.Leg, new SoftJointLimit() {limit = 0}}, //10
        {BoneCategory.LowerLeg1,new SoftJointLimit() {limit = 0}},
        {BoneCategory.LowerLeg2, new SoftJointLimit() {limit = 0}},
        {BoneCategory.Torso, new SoftJointLimit() {limit = 0}},
        {BoneCategory.Head, new SoftJointLimit() {limit = 90}},
        {BoneCategory.Hand, new SoftJointLimit() {limit = 0}},
        {BoneCategory.Foot, new SoftJointLimit() {limit = 0}},
        {BoneCategory.Shoulder, new SoftJointLimit() {limit = 0}},
        {BoneCategory.Hip, new SoftJointLimit() {limit = 0}},
        {BoneCategory.Other, new SoftJointLimit() {limit = 0}}
    };
    public static readonly Dictionary<BoneCategory, SoftJointLimit> defaultZLimit = new Dictionary<BoneCategory, SoftJointLimit>{
        {BoneCategory.Arm, new SoftJointLimit() {limit = 0}}, //45
        {BoneCategory.LowerArm, new SoftJointLimit() {limit = 0}},
        {BoneCategory.Leg, new SoftJointLimit() {limit = 0}}, //15
        {BoneCategory.LowerLeg1,new SoftJointLimit() {limit = 0}},
        {BoneCategory.LowerLeg2, new SoftJointLimit() {limit = 0}},
        {BoneCategory.Torso, new SoftJointLimit() {limit = 0}}, //45
        {BoneCategory.Head, new SoftJointLimit() {limit = 0}}, //45
        {BoneCategory.Hand, new SoftJointLimit() {limit = 0}}, //45
        {BoneCategory.Foot, new SoftJointLimit() {limit = 0}},
        {BoneCategory.Shoulder, new SoftJointLimit() {limit = 0}}, //5
        {BoneCategory.Hip, new SoftJointLimit() {limit = 0}}, //5
        {BoneCategory.Other, new SoftJointLimit() {limit = 0}}
    };

}
public class SkeletonGenerator
{
    static float BoneTreeRadius = 0.025f;
    // Human density if apparently about 1000kg/m^3
    static float BodyDensity = 1000.0f;

    class BoneTree
    {
        public Tuple<Vector3, Vector3> segment;

        public BoneTree root;

        public BoneTree parent;

        public List<BoneTree> children;
        public int limbIndex;
        public int sJointAngMot;

        public int boneIndex;

        private bool primitive_mesh;

        public GameObject go;

        private Tuple<int, char> t;

        public BoneCategory boneCategory;

        public BoneCategory subBoneCategory;

        public BoneTree(Tuple<Vector3, Vector3> segment, BoneTree root, BoneTree parent, Tuple<int, char> t, bool primitive_mesh = false)
        {
            this.segment = segment;
            this.root = root;
            this.parent = parent;
            this.t = t;
            this.primitive_mesh = primitive_mesh;

            this.boneCategory = BoneAdd.literalCategoryMap[t.Item2];
            children = new List<BoneTree>();
        }

        public String Name()
        {
            return BoneAdd.literalStringMap[BoneAdd.categoryLiteralMap[this.boneCategory]] + "_" + this.limbIndex + "_" + this.boneIndex;
        }

        public GameObject toGameObjectTree()
        {
            go = this.ToGameObject();
            foreach (BoneTree child in children)
            {
                child.toGameObjectTree();
            }
            return go;
        }

        public BoneTree findParent(Tuple<Vector3, Vector3> segment, bool inverse = false, bool excludeThis = false)
        {
            if (!excludeThis && !inverse && this.segment.Item2 == segment.Item1)
            {
                return this;
            }
            else if (!excludeThis && inverse && this.segment.Item1 == segment.Item1)
            {
                return this;
            }
            foreach (BoneTree child in children)
            {
                var res = child.findParent(segment, inverse);
                if (res != null)
                {
                    return res;
                }
            }
            return null;
        }
        public List<BoneTree> NodeList()
        {
            List<BoneTree> l = new List<BoneTree>();
            l.Add(this);
            foreach (var child in this.children) l.AddRange(child.NodeList());
            return l;
        }

        private GameObject ToGameObject()
        {
            Vector3 start = segment.Item1;
            Vector3 end = segment.Item2;
            float length = Vector3.Distance(start, end);
            bool isRoot = root == null;

            GameObject result = new GameObject(Name());
            result.transform.rotation = Quaternion.LookRotation(end - start) * Quaternion.FromToRotation(-Vector3.forward, Vector3.up);//Quaternion.FromToRotation(start,end);
            result.transform.position = start + ((end - start).normalized * (length / 2));
            //result.transform.localPosition = result.transform.localRotation * Vector3.forward * length * 1.5f;

            Rigidbody rb = result.AddComponent<Rigidbody>();
            rb.useGravity = false;

            Bone bone = result.AddComponent<Bone>();
            bone.category = boneCategory;
            bone.limbIndex = limbIndex;
            bone.boneIndex = boneIndex;
            bone.isRoot = false;

            if (isRoot)
            {
                Skeleton skeleton = result.AddComponent<Skeleton>();
                skeleton.nBones += 1;
            }
            else
            {
                GameObject parentGo = parent.go;
                result.transform.parent = parentGo.transform;

                ConfigurableJoint joint = result.AddComponent<ConfigurableJoint>();
                joint.rotationDriveMode = RotationDriveMode.Slerp;
                if (subBoneCategory == BoneCategory.LowerArm) joint.axis = Vector3.forward;
                joint.secondaryAxis = Vector3.forward;
                //joint.transform.rotation = result.transform.rotation;
                //joint.targetRotation = Quaternion.LookRotation(end-start);
                joint.anchor = new Vector3(0, -length / 2, 0);

                joint.connectedBody = parentGo.GetComponent<Rigidbody>();
                joint.connectedAnchor = parentGo.transform.position;

                joint.xMotion = ConfigurableJointMotion.Locked;
                joint.yMotion = ConfigurableJointMotion.Locked;
                joint.zMotion = ConfigurableJointMotion.Locked;

                SoftJointLimit alx = BoneAdd.defaultLowXLimit[subBoneCategory];
                SoftJointLimit ahx = BoneAdd.defaultHighXLimit[subBoneCategory];
                SoftJointLimit ay = BoneAdd.defaultYLimit[subBoneCategory];
                SoftJointLimit az = BoneAdd.defaultZLimit[subBoneCategory];

                GameObject rootGo = root.go;
                Skeleton skeleton = rootGo.GetComponent<Skeleton>();

                if (alx.limit == 0 && ahx.limit == 0)
                {
                    joint.angularXMotion = ConfigurableJointMotion.Locked;
                }
                else
                {
                    joint.angularXMotion = ConfigurableJointMotion.Limited;
                    joint.lowAngularXLimit = alx;
                    joint.highAngularXLimit = ahx;
                    skeleton.nAngXMotLimited += 1;
                }

                if (ay.limit == 0)
                {
                    joint.angularYMotion = ConfigurableJointMotion.Locked;
                }
                else
                {
                    joint.angularYMotion = ConfigurableJointMotion.Limited;
                    joint.angularYLimit = ay;
                    skeleton.nAngYMotLimited += 1;
                }

                if (az.limit == 0)
                {
                    joint.angularZMotion = ConfigurableJointMotion.Locked;
                }
                else
                {
                    joint.angularZMotion = ConfigurableJointMotion.Limited;
                    joint.angularZLimit = az;
                    skeleton.nAngZMotLimited += 1;
                }

                skeleton.nBones += 1;
                skeleton.bonesByCategory[boneCategory].Add(result);

                if (start != end)
                {
                    GameObject meshObject;
                    if (boneCategory == BoneCategory.Hand)
                    {
                        float r = 0.1f;
                        if (primitive_mesh)
                        {
                            meshObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            meshObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                            meshObject.transform.parent = result.transform;
                            meshObject.transform.position = result.transform.position;
                            meshObject.transform.rotation = result.transform.rotation;
                        }

                        SphereCollider collider = result.AddComponent<SphereCollider>();
                        // NOTE(markus): Needs to be scaled by anther factor of 0.1, not quite sure why
                        collider.radius = 0.1f * r;
                        rb.mass = BodyDensity * (3.0f * (float)Math.PI * r * r * r) / 4.0f;
                    }
                    else if (boneCategory == BoneCategory.Foot)
                    {
                        Vector3 size = new Vector3(0.1f, length * 0.9f, 0.05f);
                        if (primitive_mesh)
                        {
                            meshObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            meshObject.transform.localScale = size;

                            meshObject.transform.parent = result.transform;
                            meshObject.transform.position = result.transform.position;
                            meshObject.transform.rotation = result.transform.rotation;
                        }

                        BoxCollider collider = result.AddComponent<BoxCollider>();
                        collider.size = size;
                        rb.mass = BodyDensity * (size.x * size.y * size.z);
                    }
                    else
                    {
                        if (primitive_mesh)
                        {
                            meshObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                            meshObject.transform.localScale = new Vector3(0.1f, length * 0.45f, 0.1f);

                            meshObject.transform.parent = result.transform;
                            meshObject.transform.position = result.transform.position;
                            meshObject.transform.rotation = result.transform.rotation;
                        }

                        CapsuleCollider collider = result.AddComponent<CapsuleCollider>();
                        collider.height = length;
                        collider.radius = BoneTreeRadius;
                        // Ellipsoid Volume is 3/4 PI abc, with radii a, b, c
                        rb.mass = BodyDensity * (3.0f * (float)Math.PI * 0.1f * length * 0.45f * 0.1f) / 4;
                    }
                    // if(primitive_mesh){
                    //     meshObject.transform.parent = result.transform;
                    //     meshObject.transform.position = result.transform.position;
                    //     meshObject.transform.rotation = result.transform.rotation;            
                    // } 
                }
            }
            return result;
        }
    }

    private static BoneTree GenerateBoneTree(LSystem.LSystem l, bool primitive_mesh = false)
    {
        var segments = l.segments;
        if (segments.Count == 0) return null;

        Dictionary<BoneCategory, int> limbIndices = new();
        BoneTree root = new BoneTree(l.segments[0], null, null, l.fromRule[0], primitive_mesh);

        foreach (var (segment, rule) in segments.Zip(l.fromRule, (a, b) => (a, b)).Skip(1))
        {
            BoneTree parent = root.findParent(segment);
            if (parent == null) parent = root.findParent(segment, inverse: true);
            BoneTree child = new BoneTree(segment, root, parent, rule, primitive_mesh);
            parent.children.Add(child);

            if (child.boneCategory == parent.boneCategory)
            {
                // Same Limb, keep limb index from parent, increment bone index
                child.limbIndex = parent.limbIndex;
                child.boneIndex = parent.boneIndex + 1;
            }
            else
            {
                // Start of new Limb, grab next limbIndex, increment dictionary for next limb
                int limbIndex;
                limbIndices.TryGetValue(child.boneCategory, out limbIndex);
                limbIndices[child.boneCategory] = limbIndex;
                child.limbIndex = limbIndex;
                child.boneIndex = 0;

                limbIndices[child.boneCategory]++;
            }
        }

        //set sub categories for legs and arms, subcategories for other parts are the same as categories, assuming root is no leg or arm
        foreach (var node in root.NodeList())
        {
            if (node.boneCategory == BoneCategory.Leg && node.boneIndex != 0)
            {
                if (node.boneIndex % 2 == 1) node.subBoneCategory = BoneCategory.LowerLeg1;
                else node.subBoneCategory = BoneCategory.LowerLeg2;
            }
            else if (node.boneCategory == BoneCategory.Arm && node.boneIndex != 0) node.subBoneCategory = BoneCategory.LowerArm;
            else node.subBoneCategory = node.boneCategory;
            Debug.Log(node.subBoneCategory);
        }
        return root;
    }

    private static BoneTree GenerateBoneTree(ParametricCreature c, bool primitive_mesh = false)
    {
        Debug.Log(c);
        // place root at torso end point, facing forward
        Tuple<Vector3, Vector3> rootSegment = new(c.torso[0].startPoint, c.torso[0].startPoint + Vector3.forward);
        BoneTree root = new BoneTree(rootSegment, null, null, new(0, BoneAdd.categoryLiteralMap[BoneCategory.Other]), primitive_mesh);

        // make a list of all individual body parts
        List<Tuple<BoneCategory, List<Segment>>> limbs = new();
        limbs.Add(new(BoneCategory.Torso, c.torso));
        foreach (var leg in c.legs)
            limbs.Add(new(BoneCategory.Leg, leg));
        limbs.Add(new(BoneCategory.Head, c.neck));

        // initialize all limb indices with 0
        Dictionary<BoneCategory, int> limbIndices = Enum.GetValues(typeof(BoneCategory))
               .Cast<BoneCategory>()
               .ToDictionary(t => t, t => 0);

        foreach ((var category, var limb) in limbs)
        {

            // connect floating limbs with additional segments
            if (category == BoneCategory.Leg || category == BoneCategory.Arm || category == BoneCategory.Head)
            {
                Tuple<Vector3, Vector3> connector = null;
                BoneCategory connectType = BoneCategory.Other;
                // Add additional hip segment for legs
                if (category == BoneCategory.Leg)
                {
                    connector = new(c.legAttachJoints[limbIndices[BoneCategory.Leg]], limb[0].startPoint);
                    connectType = BoneCategory.Hip;
                }

                // TODO Add additional shoulder segment for arms
                else if (category == BoneCategory.Arm)
                {
                    connector = new(new(), new());
                    connectType = BoneCategory.Shoulder;
                }

                // Add additional segment connecting the neck
                else if (category == BoneCategory.Head)
                {
                    connector = new(c.torso[c.torso.Count - 1].endPoint, limb[0].startPoint);
                    connectType = BoneCategory.Head;
                }

                BoneTree parent = root.findParent(connector, false);
                if (parent == null)
                    parent = root.findParent(connector, true);
                Debug.Log(parent.boneCategory);
                if (parent.boneCategory != BoneCategory.Torso && parent.boneCategory != BoneCategory.Other)
                    parent = parent.parent;
                BoneTree child = new BoneTree(connector, root, parent, new(limbIndices[connectType], BoneAdd.categoryLiteralMap[connectType]), primitive_mesh);
                parent.children.Add(child);
                child.limbIndex = limbIndices[connectType]++;
                child.boneIndex = 0;
            }

            int boneIndex = 0;
            // add each segment of limb
            foreach (Segment s in limb)
            {
                Tuple<Vector3, Vector3> segment = new(s.startPoint, s.endPoint);

                BoneTree parent = root.findParent(segment, true);
                if (parent == null)
                    parent = root.findParent(segment, false);
                Debug.Log(segment.Item1.ToString("F8") + "  " + segment.Item2.ToString("F8"));
                Debug.Log(root.children.Count);
                BoneTree child = new BoneTree(segment, root, parent, new(limbIndices[category], BoneAdd.categoryLiteralMap[category]), primitive_mesh);
                parent.children.Add(child);
                Debug.Log(parent.children[0].segment);

                child.limbIndex = limbIndices[category];
                child.boneIndex = boneIndex;
                boneIndex++;
            }
            limbIndices[category]++;
        }

        return root;
    }

    public static GameObject Generate(LSystem.LSystem l, bool primitive_mesh = false)
    {
        BoneTree root = GenerateBoneTree(l, primitive_mesh);
        GameObject rootGo = root.toGameObjectTree();
        GameObject rootParent = root.findParent(root.segment, inverse: true, excludeThis: true).go;
        ConfigurableJoint joint = rootGo.AddComponent<ConfigurableJoint>();
        float length = Vector3.Distance(root.segment.Item1, root.segment.Item2) / 2;
        joint.anchor = new Vector3(0, -length, 0);
        joint.connectedBody = rootParent.GetComponent<Rigidbody>();
        joint.secondaryAxis = Vector3.forward;
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;

        SoftJointLimit alx = BoneAdd.defaultLowXLimit[root.subBoneCategory];
        SoftJointLimit ahx = BoneAdd.defaultHighXLimit[root.subBoneCategory];
        SoftJointLimit ay = BoneAdd.defaultYLimit[root.subBoneCategory];
        SoftJointLimit az = BoneAdd.defaultZLimit[root.subBoneCategory];

        if (alx.limit == 0 && ay.limit == 0)
        {
            joint.xMotion = ConfigurableJointMotion.Locked;
        }
        else
        {
            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.lowAngularXLimit = alx;
            joint.highAngularXLimit = ahx;
            root.go.GetComponent<Skeleton>().nAngXMotLimited += 1;
        }

        if (ay.limit == 0)
        {
            joint.angularYMotion = ConfigurableJointMotion.Locked;
        }
        else
        {
            joint.angularYMotion = ConfigurableJointMotion.Limited;
            joint.angularYLimit = ay;
            root.go.GetComponent<Skeleton>().nAngYMotLimited += 1;
        }

        if (az.limit == 0)
        {
            joint.angularZMotion = ConfigurableJointMotion.Locked;
        }
        else
        {
            joint.angularZMotion = ConfigurableJointMotion.Limited;
            joint.angularZLimit = az;
            root.go.GetComponent<Skeleton>().nAngZMotLimited += 1;
        }

        //GameObject rootGo = root.go;
        Skeleton skeleton = rootGo.GetComponent<Skeleton>();
        skeleton.bonesByCategory[root.boneCategory].Add(rootGo);
        rootGo.GetComponent<Bone>().isRoot = true;
        if (primitive_mesh)
        {
            GameObject meshObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            meshObject.transform.localScale = new Vector3(0.1f, length * 0.45f, 0.1f);
            meshObject.transform.parent = rootGo.transform;
            meshObject.transform.position = rootGo.transform.position;
            meshObject.transform.rotation = rootGo.transform.rotation;
        }
        //joint.connectedAnchor = parentGo.transform.position;

        return rootGo;

    }
}