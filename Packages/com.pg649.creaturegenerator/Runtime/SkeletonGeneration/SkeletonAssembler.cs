using System.Collections.Generic;
using UnityEngine;
using System;

public class SkeletonAssembler {

    public const float FootHeight = 0.05f;
    public static Skeleton Assemble(SkeletonDefinition skeleton, SkeletonSettings settings, DebugSettings debugSettings) {
        Dictionary<BoneDefinition, GameObject> objects = new();
        Dictionary<BoneCategory, int> nextIndices = new();
        DensityTable densities = new(settings);
        // Walk over SkeletonDefinition to create gameobjects.
        pass(skeleton.RootBone, def => {
            GameObject parent = (def.ParentBone != null && objects.ContainsKey(def.ParentBone)) ? objects[def.ParentBone] : null;
            GameObject root = objects.ContainsKey(skeleton.RootBone) ? objects[skeleton.RootBone] : null;
            GameObject current = toGameObject(def, parent, root, densities, settings, debugSettings);

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

        var root = objects[skeleton.RootBone];
        root.GetComponent<Bone>().isRoot = true;
        root.GetComponent<Skeleton>().SettingsInstance = skeleton.SettingsInstance;
        return root.GetComponent<Skeleton>();
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


    private static void LinkWithJoint(GameObject parent, GameObject child, LimitTable jointLimits, SkeletonSettings settings)
    {
        var joint = child.AddComponent<ConfigurableJoint>();
            
        joint.connectedBody = parent.GetComponent<Rigidbody>();
        joint.connectedAnchor = parent.transform.position;
        joint.projectionMode = JointProjectionMode.PositionAndRotation;

        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        joint.angularXMotion = ConfigurableJointMotion.Locked;
        joint.angularYMotion = ConfigurableJointMotion.Locked;
        joint.angularZMotion = ConfigurableJointMotion.Locked;

        var childBone = child.GetComponent<Bone>();
        var parentBone = parent.GetComponent<Bone>();

        var childCategory = childBone.subCategory.HasValue ? childBone.subCategory.Value : childBone.category;
        var parentCategory = parentBone.subCategory.HasValue ? parentBone.subCategory.Value : parentBone.category;
        var mirrored = childBone.mirrored;

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
    private static GameObject toGameObject(BoneDefinition self, GameObject parentGo, GameObject rootGo, DensityTable densities, SkeletonSettings settings, DebugSettings debug) {
        bool isRoot = parentGo == null;

        GameObject result = new GameObject("");
        result.tag = "Agent";

        Rigidbody rb = result.AddComponent<Rigidbody>();
        rb.drag = settings.RigidbodyDrag;
        rb.collisionDetectionMode = settings.CollisionDetectionMode;
        rb.useGravity = !debug.DisableBoneGravity;
        rb.isKinematic = debug.KinematicBones;

        Bone bone = result.AddComponent<Bone>();
        bone.category = self.Category;
        bone.subCategory = self.SubCategory;
        bone.length = self.Length;
        bone.mirrored = self.Mirrored;
        bone.thickness = self.Thickness;

        // Align local coordinate system to chosen proximal and ventral axis.
        result.transform.rotation = Quaternion.LookRotation(self.DistalAxis, self.VentralAxis);

        if (isRoot) {
            result.AddComponent<Skeleton>();
            if (self.AttachmentHint.Offset != null)
            {
                // Apply offset prescribed in AttachmentHint
                result.transform.position += self.AttachmentHint.Offset.GetValueOrDefault();
            }
        } else {
            Bone parentBone = parentGo.GetComponent<Bone>();
            result.transform.parent = parentGo.transform;
            if (parentBone.category is BoneCategory.Foot or BoneCategory.Hand)
            {
                Vector3 pos = parentBone.LocalProximalPoint() +
                    self.AttachmentHint.Position.Proximal * self.ParentBone.Length * parentBone.LocalDistalAxis() +
                    self.AttachmentHint.Position.Lateral * self.ParentBone.Thickness * parentBone.LocalLateralAxis() +
                    self.AttachmentHint.Position.Ventral * self.ParentBone.Thickness * parentBone.LocalVentralAxis();
                result.transform.localPosition = pos;
            }
            else
            {
                // Parents thickness might have been clamped. Clamp here as well to ensure consistency.
                // NOTE: This fixes the issue with weirdly placed limbs in #108 for now, but is not a very clean solution.
                // We should probably instead insure that the generators don't produce bones that are thicker, than they are long.
                // Ideally that should be done in the inspector, so that it is visible to the user.
                float radius = Mathf.Clamp(parentBone.thickness, 0.0f, 0.5f * parentBone.length);
                Vector3 pos = parentBone.LocalProximalPoint() +
                    self.AttachmentHint.Position.Proximal * self.ParentBone.Length * parentBone.LocalDistalAxis() +
                    self.AttachmentHint.Position.Lateral * radius * parentBone.LocalLateralAxis() +
                    self.AttachmentHint.Position.Ventral * radius * parentBone.LocalVentralAxis();
                result.transform.localPosition = pos;
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
            
            if (self.AttachmentHint.Offset != null)
            {
                // Apply offset prescribed in AttachmentHint
                result.transform.position += self.AttachmentHint.Offset.GetValueOrDefault();
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
            rb.mass = settings.MassMultiplier * densities[self.Category] * (3.0f * (float)Math.PI * radius * radius * radius) / 4.0f;

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
        else if (self.Category == BoneCategory.Foot && self.SubCategory != BoneCategory.Leg) {
            Vector3 size = new Vector3(self.Thickness, FootHeight, self.Length * 0.9f);

            BoxCollider collider = result.AddComponent<BoxCollider>();
            collider.size = size;
            Vector3 pos = 0.25f * bone.length * (bone.LocalDistalPoint() - bone.LocalProximalPoint());
            collider.center = pos;
            rb.mass = settings.MassMultiplier * densities[self.Category] * (size.x * size.y * size.z);

            if(debug.AttachPrimitiveMesh){
                meshObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                meshObject.tag = "Agent";
                meshObject.transform.parent = result.transform;
                meshObject.transform.localPosition = pos;
                meshObject.transform.localScale = size;
                meshObject.transform.rotation = Quaternion.LookRotation(bone.WorldDistalAxis(), bone.WorldVentralAxis());
                //meshObject.transform.rotation = Quaternion.LookRotation(bone.WorldVentralAxis(), bone.WorldProximalAxis());
                meshObject.GetComponent<MeshRenderer>().shadowCastingMode = settings.PrimitiveMeshShadows;
                // Delete Collider from primitive
                UnityEngine.Object.Destroy(meshObject.GetComponent<Collider>());
            }
        } else {
            float height = self.Length;
            float radius = self.Thickness;
            if (2f * radius > height)
            {
                float width = 1.77245f * radius; // 1.77245 = sqrt(pi)
                BoxCollider box = result.AddComponent<BoxCollider>();
                box.size = new(width, width, height);
                box.center = bone.LocalMidpoint();
            }
            else
            {
                CapsuleCollider collider = result.AddComponent<CapsuleCollider>();
                collider.height = height;
                collider.radius = radius;
                //collider.radius = 0.25f;
                collider.center = bone.LocalMidpoint();
                // Colliders point along Proximal (Z) Axis
                collider.direction = 2;
            }
            // Volume consists of two half spheres + plus one cylinder
            var volume = (4.0f * (float)Math.PI * radius * radius * radius) / 3.0f +
                           (height - 2.0f * radius) * ((float)Math.PI * radius * radius);
            rb.mass = settings.MassMultiplier * densities[self.Category] * volume;

            if(debug.AttachPrimitiveMesh){
                meshObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                meshObject.tag = "Agent";
                meshObject.transform.parent = result.transform;
                meshObject.transform.localPosition = bone.LocalMidpoint();
                meshObject.transform.localScale = new Vector3(radius / 0.5f , height * 0.45f, radius / 0.5f);
                // Rotate capsule so that y-axis points along ProximalAxis of parent, i.e. in the direction
                // of the bone
                meshObject.transform.rotation = Quaternion.LookRotation(bone.WorldVentralAxis(), bone.WorldDistalAxis());

                meshObject.GetComponent<MeshRenderer>().shadowCastingMode = settings.PrimitiveMeshShadows;

                // Delete Collider from primitive
                UnityEngine.Object.Destroy(meshObject.GetComponent<Collider>());
            }
        }
        return result;
    }
}