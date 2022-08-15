using System.Collections.Generic;
using UnityEngine;

public class DensityTable
{
    private readonly Dictionary<BoneCategory, float> _overrides;
    private readonly float _defaultDensity;
    
    public DensityTable(SkeletonSettings settings)
    {
        _defaultDensity = Random.Range(settings.MinimumDensity, settings.MaximumDensity);
        _overrides = new Dictionary<BoneCategory, float>();
        foreach (var boneDensity in settings.BoneDensityOverrides)
        {
            _overrides[boneDensity.Category] = Random.Range(boneDensity.MinimumDensity, boneDensity.MaximumDensity);
        }
    }

    private float _density(BoneCategory category)
    {
        return _overrides.ContainsKey(category) ? _overrides[category] : _defaultDensity;
    }
    public float this[BoneCategory category] => _density(category);
}