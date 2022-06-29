using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MirrorMode {
    None, LeftRight, UpDown, FrontBack
}

[System.Serializable]
public class RaycastSettings {
    
    public Vector3 Rotation;
    public MirrorMode MirrorMode = MirrorMode.None;

    public IEnumerable<Ray> GetRays(Transform t) {
        Vector3 direction = Quaternion.Euler(Rotation) * t.forward;
        yield return new Ray(t.position, direction);
        if (MirrorMode != MirrorMode.None)
            yield return new Ray(t.position, mirror(t, MirrorMode, direction));
    }

    public IEnumerable<Vector3> GetPositions(Transform t, Collider c) {
        foreach (Ray ray in GetRays(t)) {
            yield return raycast(ray, c);
        }
    }

    private Vector3 raycast(Ray ray, Collider c) {
        RaycastHit hit;
        Vector3 position;
        if (Physics.Raycast(ray, out hit)) {
            // We hit another collider. Do the reverse raycast from slightly before that collider.
            Vector3 origin = ray.GetPoint(hit.distance - 0.1f);
            Ray reverse = new(origin, -ray.direction);
            RaycastHit reverseHit;
            if (c.Raycast(reverse, out reverseHit, 2.0f * hit.distance)) {
                position = reverseHit.point;
                Debug.Log("Reverse Raycast used");
            } else {
                // Reverse raycast also did not hit our collider. Colliders must be overlapping.
                // Use initial hit as best approximation
                position = hit.point;
                Debug.Log("Reverse Raycast failed. Using initial hit");
            }
        } else {
            // No collider in that direction. Do the reverse raycast from (hopefully) far enough away.
            Vector3 origin = ray.GetPoint(100.0f);
            Ray reverse = new(origin, -ray.direction);
            if (Physics.Raycast(reverse, out hit)) {
                position = hit.point;
                Debug.Log("Using estimated reverse raycast");
            } else {
                // Just give up
                position = Vector3.zero;
                Debug.Log("Estimated reverse raycast failed. Falling back to zero position.");
            }
        }
        return position;
    }

    private Vector3 mirror(Transform t, MirrorMode mode, Vector3 vec) {
        switch(mode) {
            case MirrorMode.None: return vec;
            case MirrorMode.FrontBack: return Vector3.Reflect(vec, t.forward);
            case MirrorMode.LeftRight: return Vector3.Reflect(vec, t.right);
            case MirrorMode.UpDown: return Vector3.Reflect(vec, t.up);
        }
        return Vector3.zero;
    }
}