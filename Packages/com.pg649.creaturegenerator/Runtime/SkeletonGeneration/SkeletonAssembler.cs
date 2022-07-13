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

    public static  GameObject Assemble(SkeletonDefinition skeleton) {
        Dictionary<BoneDefinition, GameObject> objects = new();
        // Walk over SkeletonDefinition to create gameobjects.
        pass(skeleton.RootBone, def => {
            GameObject parent = (def.ParentBone != null && objects.ContainsKey(def.ParentBone)) ? objects[def.ParentBone] : null;
            GameObject root = objects.ContainsKey(skeleton.RootBone) ? objects[skeleton.RootBone] : null;
            objects.Add(def, toGameObject(def, parent, root, skeleton.JointLimits));
        });

        // Walk over SkeletonDefinition to rotate limbs into default positions.
        // Has to be done in a separate pass, so that rotation is not undone by
        // rotating bones towards their prescribed ventral axis.
        pass(skeleton.RootBone, def => {
            GameObject current = objects[def];
            current.transform.Rotate(def.AttachmentHint.Rotation.GetValueOrDefault().eulerAngles);
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


    private static GameObject toGameObject(BoneDefinition self, GameObject parentGo, GameObject rootGo, LimitTable jointLimits) {
        bool isRoot = parentGo == null;

        // TOOD(markus): Name
        GameObject result = new GameObject("");



        Rigidbody rb = result.AddComponent<Rigidbody>();
        rb.useGravity = false;

        Bone bone = result.AddComponent<Bone>();
        bone.category = self.Category;
        bone.length = self.Length;
        // TODO(markus): Limb indices
        //bone.limbIndex = limbIndex;
        //bone.boneIndex = boneIndex;

        // Align local coordinate system to chosen proximal and ventral axis.
        result.transform.rotation = Quaternion.LookRotation(self.ProximalAxis, self.VentralAxis);
        
        if (isRoot) {
            result.AddComponent<Skeleton>();
        } else {
            // Does reparenting change a transform? Maybe realign coordinate system
            // aftwards again, if axis are not correct.
            result.transform.parent = parentGo.transform;

            Bone parentBone = parentGo.GetComponent<Bone>();
            if (self.AttachmentHint.AttachmentPoint == AttachmentPoint.DistalPoint) {
                result.transform.localPosition = parentBone.LocalDistalPoint() - 0.1f * parentBone.LocalProximalAxis();
            } else {
                result.transform.localPosition = parentBone.LocalProximalPoint() + 0.1f * parentBone.LocalProximalAxis();
            }


            if (self.AttachmentHint.Offset != null) {
                // Apply offset prescribed in AttachmentHint
                result.transform.position += self.AttachmentHint.Offset.GetValueOrDefault();
            }

            if (self.AttachmentHint.VentralDirection != null) {
                // Rotate about proximal (z) Axis, so that the world-space ventral axis matches
                // the axis prescribed by the AttachmentHint
                float angle = Vector3.Angle(bone.WorldVentralAxis(), self.AttachmentHint.VentralDirection.GetValueOrDefault());
                Debug.Log(angle);
                result.transform.Rotate(0.0f, 0.0f, angle);
                self.PropagateAttachmentRotation(angle);
            }

            ConfigurableJoint joint = result.AddComponent<ConfigurableJoint>();
            //joint.transform.rotation = result.transform.rotation;
            //joint.targetRotation = Quaternion.LookRotation(end-start);
            //joint.anchor = new Vector3(0,-length/2,0);

            joint.connectedBody = parentGo.GetComponent<Rigidbody>();
            joint.connectedAnchor = parentGo.transform.position;

            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.angularYMotion = ConfigurableJointMotion.Limited;
            joint.angularZMotion = ConfigurableJointMotion.Limited;

            if (jointLimits.HasLimits((self.Category, self.ParentBone.Category))) {
                JointLimits limits = jointLimits[(self.Category, self.ParentBone.Category)];
                joint.lowAngularXLimit = new SoftJointLimit() { limit = limits.XAxisMin};
                joint.highAngularXLimit = new SoftJointLimit() { limit = limits.XAxisMax};
                joint.angularYLimit = new SoftJointLimit() { limit = limits.YAxisSymmetric};
                joint.angularZLimit = new SoftJointLimit() { limit = limits.ZAxisSymmetric};
            } else {
                joint.lowAngularXLimit = BoneAdd.defaultLowXLimit[self.Category];
                joint.highAngularXLimit = BoneAdd.defaultHighXLimit[self.Category];
                joint.angularYLimit = BoneAdd.defaultYLimit[self.Category];
                joint.angularZLimit = BoneAdd.defaultZLimit[self.Category];
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

            if(attachPrimitiveMesh){
                meshObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);

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

            if(attachPrimitiveMesh){
                meshObject = GameObject.CreatePrimitive(PrimitiveType.Cube);                        

                meshObject.transform.parent = result.transform;
                meshObject.transform.localPosition = bone.LocalMidpoint();
                meshObject.transform.localScale = size;
                meshObject.transform.rotation = Quaternion.LookRotation(bone.WorldProximalAxis(), bone.WorldVentralAxis());
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

            if(attachPrimitiveMesh){
                meshObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);

                meshObject.transform.parent = result.transform;
                meshObject.transform.localPosition = bone.LocalMidpoint();
                meshObject.transform.localScale = new Vector3(0.5f ,self.Length*0.45f, 0.5f);
                // Rotate capsule so that y-axis points along ProximalAxis of parent, i.e. in the direction
                // of the bone
                meshObject.transform.rotation = Quaternion.LookRotation(bone.WorldVentralAxis(), bone.WorldProximalAxis());
            }
        }
        return result;
    }
}