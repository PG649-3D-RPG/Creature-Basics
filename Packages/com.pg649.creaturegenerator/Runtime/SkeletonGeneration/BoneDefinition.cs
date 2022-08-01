using System.Collections.Generic;
using UnityEngine;

public class BoneDefinition {
    public BoneDefinition ParentBone;

    public HashSet<BoneDefinition> ChildBones;

    public float Length;

    /// Direction pointing along the bone (towards the child bone / away from the torso).
    public Vector3 DistalAxis;

    /// Direction pointing towards the front of the body part.
    public Vector3 VentralAxis;

    public BoneCategory Category;

    public AttachmentHint AttachmentHint;

    public float Thickness;

    public bool Mirrored;

    public BoneDefinition() {
        ChildBones = new();
        AttachmentHint = new();
    }

    public void LinkChild(BoneDefinition child) {
        this.ChildBones.Add(child);
        child.ParentBone = this;
    }

    public void PropagateAttachmentRotation(Quaternion delta) {
        VentralAxis = delta * VentralAxis;
        foreach (var child in ChildBones) {
            child.PropagateAttachmentRotation(delta);
        }
    }
}

public static class RelativePositions {
    public static readonly RelativePosition ProximalPoint = new(0.0f, 0.0f, 0.0f);
    public static readonly RelativePosition MidPoint = new(0.0f, 0.0f, 0.5f);
    public static readonly RelativePosition DistalPoint = new(0.0f, 0.0f, 1.0f);
}

/// Position on the plane spanned by the Proximal and Lateral axis. Normalized to Length and Thickness.
/// Proximal 0 is the proximal point, Proximal 1 is the distal point.
/// Lateral 1 is one thickness towards Lateral axis, Lateral -1 is one thickness behind Lateral axis.
public struct RelativePosition {
    public float Lateral;
    public float Ventral;
    public float Proximal;

    public RelativePosition(float lateral, float ventral, float proximal) {
        Lateral = lateral;
        Ventral = ventral;
        Proximal = proximal;
    }
}

/// Provides a hint for how a bone should be attached to its parent.
/// The world-space VentralAxis of the bone should point towards the VentralDirection
/// after attachment.
public class AttachmentHint {
    public RelativePosition Position;
    public Vector3? VentralDirection;
    public Vector3? Offset;
    public Quaternion? Rotation;

    public AttachmentHint() {
        Position = RelativePositions.DistalPoint;
        VentralDirection = null;
        Offset = null;
        Rotation = null;
    }
}