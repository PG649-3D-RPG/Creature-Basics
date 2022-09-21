using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class SkeletonLinter
{
    public static void Lint(Skeleton skeleton, SkeletonLinterSettings settings)
    {
        if (settings.FixColliderPenetrations)
        {
            FixColliderPenetrations(skeleton);
        }
        if (settings.WarnOnColliderPenetration)
        {
            WarnOnColliderPenetration(skeleton);
        }
        if (settings.FixIdenticalBonePositions)
        {
            FixIdenticalBonePositions(skeleton);
        }
    }

    private static void FixIdenticalBonePositions(Skeleton skeleton)
    {
        foreach (var (obj, bone, _, _) in skeleton.Iterator())
        {
            if (obj.transform.parent == null) continue;

            if (obj.transform.position == obj.transform.parent.position)
            {
                obj.transform.position += bone.WorldDistalAxis() * 0.01f;
            }
        }
    }


    private static void WarnOnColliderPenetration(Skeleton skeleton)
    {
        foreach (var ((a, boneA, _, _), (b, boneB, _, _)) in skeleton.UnrelatedPairs())
        {
            var penetration = Physics.ComputePenetration(
                a.GetComponent<Collider>(),
                a.transform.position,
                a.transform.rotation,
                b.GetComponent<Collider>(),
                b.transform.position,
                b.transform.rotation,
                out var direction, out var distance);


            if (!penetration || distance == 0.0f) continue;
            Debug.Log($"Found Penetration between {a.name} and {b.name}.", a);
        }
    }

    private static void FixColliderPenetrations(Skeleton skeleton)
    {
        // Check if any non parent-child colliders overlap, and try to move them apart
        var penetrationFound = false;
        var iteration = 0;
        do
        {
            penetrationFound = false;
            foreach (var ((a, boneA, _, _), (b, boneB, _, _)) in skeleton.UnrelatedPairs())
            {
                var penetration = Physics.ComputePenetration(
                    a.GetComponent<Collider>(),
                    a.transform.position,
                    a.transform.rotation,
                    b.GetComponent<Collider>(),
                    b.transform.position,
                    b.transform.rotation,
                    out var direction, out var distance);


                if (!penetration || distance == 0.0f) continue;

                penetrationFound = true;
                if (boneA.limbIndex != boneB.limbIndex)
                {
                    // Colliders are in different limbs. Move whole limbs to avoid misshapen limbs
                    var aLimbRoot = skeleton.FindLimbRoot(a);
                    var bLimbRoot = skeleton.FindLimbRoot(b);
                    Debug.Log($"Moving {aLimbRoot.name} and {bLimbRoot.name}");
                    Debug.Log(
                        $"Pre Translation: {aLimbRoot.transform.position} | {bLimbRoot.transform.position}");
                    aLimbRoot.transform.Translate(0.5f * distance * direction, Space.World);
                    bLimbRoot.transform.Translate(-0.5f * distance * direction, Space.World);
                    Debug.Log(
                        $"Post Translation: {aLimbRoot.transform.position} | {bLimbRoot.transform.position}");
                }
                else
                {
                    a.transform.Translate(0.5f * distance * direction, Space.World);
                    b.transform.Translate(-0.5f * distance * direction, Space.World);
                }
            }

            iteration++;
        } while (penetrationFound && iteration < 10);
    }
}