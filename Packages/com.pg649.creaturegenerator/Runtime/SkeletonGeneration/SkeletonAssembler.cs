using System.Collections.Generic;
using UnityEngine;
using System;

public class SkeletonAssembler {
    // Human density if apparently about 1000kg/m^3
    static float BodyDensity = 1000.0f;

    public static bool attachPrimitiveMesh = true;

    public static  GameObject Assemble(SkeletonDefinition skeleton, SkeletonSettings settings, DebugSettings debugSettings) {
        Dictionary<BoneDefinition, GameObject> objects = new();
        Dictionary<BoneCategory, int> nextIndices = new();
        // Walk over SkeletonDefinition to create gameobjects.
        pass(skeleton.RootBone, def => {
            GameObject parent = (def.ParentBone != null && objects.ContainsKey(def.ParentBone)) ? objects[def.ParentBone] : null;
            GameObject root = objects.ContainsKey(skeleton.RootBone) ? objects[skeleton.RootBone] : null;
            GameObject current = toGameObject(def, parent, root, skeleton.JointLimits, settings, debugSettings);

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


    private static GameObject toGameObject(BoneDefinition self, GameObject parentGo, GameObject rootGo, LimitTable jointLimits, SkeletonSettings settings, DebugSettings debug) {
        bool isRoot = parentGo == null;

        // TOOD(markus): Name
        GameObject result = new GameObject("");
        result.tag = "Agent";



        Rigidbody rb = result.AddComponent<Rigidbody>();
        rb.drag = settings.RigidbodyDrag;
        rb.collisionDetectionMode = settings.CollisionDetectionMode;
        if (debug.DisableBoneGravity)
        {
            rb.useGravity = false;
        }

        Bone bone = result.AddComponent<Bone>();
        bone.category = self.Category;
        bone.length = self.Length;
        bone.thickness = self.Thickness;
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
            Bone parentBone = parentGo.GetComponent<Bone>();
            result.transform.parent = parentGo.transform;

            Vector3 pos = parentBone.LocalProximalPoint() -
                self.AttachmentHint.Position.Proximal * self.ParentBone.Length * parentBone.LocalProximalAxis() +
                self.AttachmentHint.Position.Lateral * self.ParentBone.Thickness * parentBone.LocalLateralAxis() +
                self.AttachmentHint.Position.Ventral * self.ParentBone.Thickness * parentBone.LocalVentralAxis();
            result.transform.localPosition = pos;

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
            joint.projectionMode = JointProjectionMode.PositionAndRotation;

            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.angularYMotion = ConfigurableJointMotion.Limited;
            joint.angularZMotion = ConfigurableJointMotion.Limited;

            BoneCategory category = self.SubCategory.HasValue ? self.SubCategory.Value : self.Category;
            BoneCategory parentCategory = self.ParentBone.SubCategory.HasValue ? self.ParentBone.SubCategory.Value : self.ParentBone.Category;

            if (jointLimits.HasLimits((category, parentCategory))) {
                JointLimits limits = jointLimits[(category, parentCategory)];
                joint.lowAngularXLimit = new SoftJointLimit() { limit = limits.XAxisMin};
                joint.highAngularXLimit = new SoftJointLimit() { limit = limits.XAxisMax};
                joint.angularYLimit = new SoftJointLimit() { limit = limits.YAxisSymmetric};
                joint.angularZLimit = new SoftJointLimit() { limit = limits.ZAxisSymmetric};
            } else {
                joint.lowAngularXLimit = BoneAdd.defaultLowXLimit[category];
                joint.highAngularXLimit = BoneAdd.defaultHighXLimit[category];
                joint.angularYLimit = BoneAdd.defaultYLimit[category];
                joint.angularZLimit = BoneAdd.defaultZLimit[category];
            }

            Skeleton skeleton = rootGo.GetComponent<Skeleton>();
            skeleton.bonesByCategory[self.Category].Add(result);
        }

        GameObject meshObject;
        if(self.Category == BoneCategory.Hand){
            SphereCollider collider = result.AddComponent<SphereCollider>();
            float radius = self.Thickness;
            collider.radius = radius;
            collider.center = bone.LocalMidpoint();
            rb.mass = BodyDensity * (3.0f * (float)Math.PI * radius * radius * radius) / 4.0f;

            if(debug.AttachPrimitiveMesh){
                meshObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                meshObject.tag = "Agent";

                meshObject.transform.parent = result.transform;
                meshObject.transform.localPosition = bone.LocalMidpoint();
                meshObject.transform.localScale = new Vector3(radius / 0.5f, radius / 0.5f , radius / 0.5f);

                meshObject.GetComponent<MeshRenderer>().shadowCastingMode = settings.PrimitiveMeshShadows;
                // Delete Collider from primitive
                UnityEngine.Object.Destroy(meshObject.GetComponent<Collider>());
            }
        }
        else if (self.Category == BoneCategory.Foot) {
            Vector3 size = new Vector3(self.Thickness, 0.05f, self.Length * 0.9f);

            BoxCollider collider = result.AddComponent<BoxCollider>();
            collider.size = size;
            collider.center = bone.LocalMidpoint();
            rb.mass = BodyDensity * (size.x * size.y * size.z);

            if(debug.AttachPrimitiveMesh){
                meshObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                meshObject.tag = "Agent";
                meshObject.transform.parent = result.transform;
                meshObject.transform.localPosition = bone.LocalMidpoint();
                meshObject.transform.localScale = size;
                meshObject.transform.rotation = Quaternion.LookRotation(bone.WorldProximalAxis(), bone.WorldVentralAxis());
                //meshObject.transform.rotation = Quaternion.LookRotation(bone.WorldVentralAxis(), bone.WorldProximalAxis());
                meshObject.GetComponent<MeshRenderer>().shadowCastingMode = settings.PrimitiveMeshShadows;
                // Delete Collider from primitive
                UnityEngine.Object.Destroy(meshObject.GetComponent<Collider>());
            }
        } else {
            float height = self.Length;
            float radius = Mathf.Clamp(self.Thickness, 0.0f, 0.5f * height);
            CapsuleCollider collider = result.AddComponent<CapsuleCollider>();
            collider.height = height;
            collider.radius = radius;
            //collider.radius = 0.25f;
            collider.center = bone.LocalMidpoint();
            // Colliders point along Proximal (Z) Axis
            collider.direction = 2;
            // Ellipsoid Volume is 3/4 PI abc, with radii a, b, c
            rb.mass = BodyDensity * (3.0f * (float)Math.PI * 0.1f * self.Length * 0.45f * 0.1f) / 4;

            if(debug.AttachPrimitiveMesh){
                meshObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                meshObject.tag = "Agent";
                meshObject.transform.parent = result.transform;
                meshObject.transform.localPosition = bone.LocalMidpoint();
                meshObject.transform.localScale = new Vector3(radius / 0.5f , height * 0.45f, radius / 0.5f);
                // Rotate capsule so that y-axis points along ProximalAxis of parent, i.e. in the direction
                // of the bone
                meshObject.transform.rotation = Quaternion.LookRotation(bone.WorldVentralAxis(), bone.WorldProximalAxis());

                meshObject.GetComponent<MeshRenderer>().shadowCastingMode = settings.PrimitiveMeshShadows;

                // Delete Collider from primitive
                UnityEngine.Object.Destroy(meshObject.GetComponent<Collider>());
            }
        }
        return result;
    }
}