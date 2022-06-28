using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spec", menuName = "Details/Spec", order = 1)]
public class Spec : ScriptableObject
{
    public List<PrefabDetail> DetailList;

    public void addDetails(GameObject skeletonRoot) {
        Skeleton skeleton = skeletonRoot.GetComponent<Skeleton>();

        Dictionary<BoneCategory, List<PrefabDetail>> detailMap = new();

        // TODO(markus): Ideally the Spec should contain the DetailMap instead of DetailList.
        // Unity can't handle dictionaries in ScriptableObjects by default though.
        // Apparently that can be achieved via custom serializers.
        // Look into that or think about splitting the spec per bone type.
        foreach (var detail in DetailList) {
            if (!detailMap.ContainsKey(detail.BoneFilter)) {
                detailMap[detail.BoneFilter] = new List<PrefabDetail>();
            }
            detailMap[detail.BoneFilter].Add(detail);
        }

        foreach (var (category, details) in detailMap) {
            foreach (var bone in skeleton.bonesByCategory[category]) {
                foreach (var detail in details) {
                    detail.apply(bone);
                }
            }
        }
    }
}
