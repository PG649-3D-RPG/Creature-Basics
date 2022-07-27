using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using LSystem;

public class SkeletonAssembler {
    // Human density if apparently about 1000kg/m^3
    static float BodyDensity = 1000.0f;

    public static bool attachPrimitiveMesh = true;

    public static  GameObject Assemble(SkeletonDefinition skeleton, SkeletonAssemblerSettings settings) {
        Dictionary<BoneDefinition, GameObject> objects = new();
        Dictionary<BoneCategory, int> nextIndices = new();
        // Walk over SkeletonDefinition to create gameobjects.
        pass(skeleton.RootBone, def => {
            GameObject parent = (def.ParentBone != null && objects.ContainsKey(def.ParentBone)) ? objects[def.ParentBone] : null;
            GameObject root = objects.ContainsKey(skeleton.RootBone) ? objects[skeleton.RootBone] : null;
            GameObject current = toGameObject(def, parent, root, settings);

            Bone currentBone = current.GetComponent<Bone>();
            Bone parentBone = parent != null ? parent.GetComponent<Bone>() : null;

            objects.Add(def, current);

            setBoneIndices(nextIndices, currentBone, parentBone);
            current.name = name(currentBone);
        });

        // Walk over SkeletonDefinition to rotate limbs into default positions.
        // Has to be done in a separate pass, so that rotation is not undone by
        // rotating bones towards their prescribed ventral axis.

        pass(skeleton.RootBone, def => {
            GameObject current = objects[def];
            current.transform.Rotate(def.AttachmentHint.Rotation.GetValueOrDefault().eulerAngles);

            if (def != skeleton.RootBone)
                LinkWithJoint(objects[def.ParentBone], current, skeleton.JointLimits, settings);
        });

        return objects[skeleton.RootBone];
    }

    private static void pass(BoneDefinition root, Action<BoneDefinition> f) {
        Queue<BoneDefinition> todo = new();
        todo.Enqueue(root);
        while (todo.Count > 0) {
            BoneDefinition current = todo.Dequeue();

            f(current);

            foreach (var child in current.ChildBones) {
                todo.Enqueue(child);
            }
        }
    }

    private static void setBoneIndices(Dictionary<BoneCategory, int> nextLimbIndices, Bone current, Bone parent) {
        int nextIndex(Dictionary<BoneCategory, int> nextLimbIndices, BoneCategory category) {
            if (nextLimbIndices.ContainsKey(category)) {
                nextLimbIndices[category] += 1;
                return nextLimbIndices[category] - 1;
            } else {
                nextLimbIndices[category] = 1;
                return 0;
            }
        }

        if (parent != null && current.category == parent.category) {
            // Not Root and same category as parent. Keep limb index and increment bone index.
            current.limbIndex = parent.limbIndex;
            current.boneIndex = parent.boneIndex + 1;
        } else {
            // Root or start of new limb. Grab next limb index from dictionary.
            current.limbIndex = nextIndex(nextLimbIndices, current.category);
            current.boneIndex = 0;
        }
    }

    private static String name(Bone bone)
    {
        return Enum.GetName(typeof(BoneCategory), bone.category).ToLower() + "_" + bone.limbIndex + "_" + bone.boneIndex;
    }


    private static void LinkWithJoint(GameObject parent, GameObject child, LimitTable jointLimits, SkeletonAssemblerSettings settings)
    {
            var joint = child.AddComponent<ConfigurableJoint>();
            
            joint.connectedBody = parent.GetComponent<Rigidbody>();
            joint.connectedAnchor = parent.transform.position;

            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;

            var childCategory = child.GetComponent<Bone>().category;
            var parentCategory = parent.GetComponent<Bone>().category;
            var mirrored = child.GetComponent<Bone>().mirrored;
            
            if (jointLimits.HasLimits((parentCategory, childCategory))) {
                var limits = jointLimits[(parentCategory, childCategory)];
                if (limits.XAxisMin != 0.0f || limits.XAxisMax != 0.0f)
                {
                    joint.angularXMotion = ConfigurableJointMotion.Limited;
                    joint.lowAngularXLimit = new SoftJointLimit() { limit = limits.XAxisMin};
                    joint.highAngularXLimit = new SoftJointLimit() { limit = limits.XAxisMax};
                }
                if (limits.YAxisSymmetric != 0.0f)
                {
                    joint.angularYMotion = ConfigurableJointMotion.Limited;
                    joint.angularYLimit = new SoftJointLimit() { limit = limits.YAxisSymmetric};
                }
                if (limits.ZAxisSymmetric != 0.0f)
                {
                    joint.angularZMotion = ConfigurableJointMotion.Limited;
                    joint.angularZLimit = new SoftJointLimit() { limit = limits.ZAxisSymmetric};
                }
                if (limits.Axis != null)
                {
                    joint.axis = (mirrored ? -1.0f : 1.0f) * limits.Axis.GetValueOrDefault();
                }
                if (limits.SecondaryAxis != null)
                {
                    joint.secondaryAxis = (mirrored ? -1.0f : 1.0f) * limits.SecondaryAxis.GetValueOrDefault();
                }
            }

            JointDrive slerp = new()
            {
                positionSpring = 10000.0f,
                positionDamper = 500.0f,
                maximumForce = float.MaxValue
            };
            joint.slerpDrive = slerp;
            joint.rotationDriveMode = RotationDriveMode.Slerp;
    }
    private static GameObject toGameObject(BoneDefinition self, GameObject parentGo, GameObject rootGo, SkeletonAssemblerSettings settings) {
        bool isRoot = parentGo == null;

        GameObject result = new GameObject("");
        result.tag = "Agent";

        Rigidbody rb = result.AddComponent<Rigidbody>();

        Bone bone = result.AddComponent<Bone>();
        bone.category = self.Category;
        bone.length = self.Length;
        bone.mirrored = self.Mirrored;

        // Align local coordinate system to chosen proximal and ventral axis.
        result.transform.rotation = Quaternion.LookRotation(self.DistalAxis, self.VentralAxis);
        
        if (isRoot) {
            result.AddComponent<Skeleton>();
        } else {
            // Does reparenting change a transform? Maybe realign coordinate system
            // aftwards again, if axis are not correct.
            Bone parentBone = parentGo.GetComponent<Bone>();
            result.transform.parent = parentGo.transform;
            result.transform.localPosition = Vector3.Lerp(parentBone.LocalProximalPoint(), parentBone.LocalDistalPoint(), self.AttachmentHint.AttachmentPoint);

            if (self.AttachmentHint.Offset != null) {
                // Apply offset prescribed in AttachmentHint
                result.transform.position += self.AttachmentHint.Offset.GetValueOrDefault();
            }

            if (self.AttachmentHint.VentralDirection != null) {
                // Rotate so that the world-space ventral axis matches
                // the axis prescribed by the AttachmentHint
                var target = Quaternion.LookRotation(self.DistalAxis, self.AttachmentHint.VentralDirection.GetValueOrDefault());
                var current = result.transform.rotation;
                result.transform.rotation = target;
                var delta = target * Quaternion.Inverse(current);
                self.PropagateAttachmentRotation(delta);

            }
            Skeleton skeleton = rootGo.GetComponent<Skeleton>();
            skeleton.bonesByCategory[self.Category].Add(result);
        }

        GameObject meshObject;
        if(self.Category == BoneCategory.Hand){
            float r = 0.1f;
            SphereCollider collider = result.AddComponent<SphereCollider>();                        
            // NOTE(markus): Needs to be scaled by anther factor of 0.1, not quite sure why
            collider.radius = 0.1f * self.Thickness;
            rb.mass = BodyDensity * (3.0f * (float)Math.PI * r * r * r) / 4.0f;

            if(settings.AttachPrimitiveMesh){
                meshObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                meshObject.tag = "Agent";

                meshObject.transform.parent = result.transform;
                meshObject.transform.localPosition = bone.LocalMidpoint();
                meshObject.transform.localScale = new Vector3(0.1f, 0.1f ,0.1f);
            }
        }
        else if (self.Category == BoneCategory.Foot) {
            Vector3 size = new Vector3(0.1f, self.Length * 0.9f, 0.05f);

            BoxCollider collider = result.AddComponent<BoxCollider>();
            collider.size = size;
            rb.mass = BodyDensity * (size.x * size.y * size.z);

            if(settings.AttachPrimitiveMesh){
                meshObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                meshObject.tag = "Agent";
                meshObject.transform.parent = result.transform;
                meshObject.transform.localPosition = bone.LocalMidpoint();
                meshObject.transform.localScale = size;
                meshObject.transform.rotation = Quaternion.LookRotation(bone.WorldDistalAxis(), bone.WorldVentralAxis());
            }
        } else {
            CapsuleCollider collider = result.AddComponent<CapsuleCollider>();
            collider.height = self.Length;
            //collider.radius = self.Thickness;
            collider.radius = 0.25f;
            collider.center = bone.LocalMidpoint();
            // Colliders point along Proximal (Z) Axis
            collider.direction = 2;
            // Ellipsoid Volume is 3/4 PI abc, with radii a, b, c
            rb.mass = BodyDensity * (3.0f * (float)Math.PI * 0.1f * self.Length * 0.45f * 0.1f) / 4;

            if(settings.AttachPrimitiveMesh){
                meshObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                meshObject.tag = "Agent";
                meshObject.transform.parent = result.transform;
                meshObject.transform.localPosition = bone.LocalMidpoint();
                meshObject.transform.localScale = new Vector3(0.5f ,self.Length*0.45f, 0.5f);
                // Rotate capsule so that y-axis points along ProximalAxis of parent, i.e. in the direction
                // of the bone
                meshObject.transform.rotation = Quaternion.LookRotation(bone.WorldVentralAxis(), bone.WorldDistalAxis());
            }
        }
        return result;
    }
}