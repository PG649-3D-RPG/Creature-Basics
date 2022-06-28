using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MirrorMode {
    None, LeftRight, UpDown, FrontBack
}

[CreateAssetMenu(fileName = "Spec", menuName = "Details/PrefabDetail", order = 1)]
public class PrefabDetail : ScriptableObject
{
    public GameObject Prefab;

    public RaycastSettings RaycastSettings;

    public Vector3 Scale = Vector3.one;

    public BoneCategory BoneFilter;

    public MirrorMode MirrorMode = MirrorMode.None;

    public void apply(GameObject target) {
        Transform t = target.transform;
        Collider c = target.GetComponent<Collider>();
        Vector3 origin = t.position;
        Vector3 direction = Quaternion.Euler(RaycastSettings.Rotation) * t.forward;
        Ray ray = new(origin, direction);

        Vector3 position = raycast(ray, c);

        GameObject detail = Instantiate(Prefab);
        detail.transform.parent = t;
        detail.transform.localScale = Vector3.Scale(detail.transform.localScale, Scale);
        detail.transform.position = position;

        if (MirrorMode != MirrorMode.None) {
            Vector3 mirrored_direction = mirror(t, MirrorMode, direction);
            ray = new(origin, mirrored_direction);

            position = raycast(ray, c);

            detail = Instantiate(Prefab);
            detail.transform.parent = t;
            detail.transform.localScale = Vector3.Scale(detail.transform.localScale, Scale);
            detail.transform.position = position;
        }
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
}
