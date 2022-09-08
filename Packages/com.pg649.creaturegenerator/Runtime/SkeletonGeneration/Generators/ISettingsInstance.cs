using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;

public interface ISettingsInstance
{
    public List<float> Observations()
    {
        // Each field has a Order attribute
        Debug.Assert(GetType().GetFields().All(f => f.GetCustomAttributes(typeof(ObservationOrderAttribute), false).Length == 1));
        // Order indices are unique
        Debug.Assert(
            GetType().GetFields()
                .Select(f => ((ObservationOrderAttribute)f.GetCustomAttributes(typeof(ObservationOrderAttribute), false).Single()).Index)
                .Distinct().Count() == GetType().GetFields().Length);
        
        List<float> result = new();
        foreach (var field in GetType().GetFields().OrderBy(f => ((ObservationOrderAttribute) f.GetCustomAttributes(typeof(ObservationOrderAttribute), false).Single()).Index))
        {
            var val = field.GetValue(this);
            switch (val)
            {
                case float f:
                    result.Add(f);
                    break;
                case List<float> list:
                    result.AddRange(list);
                    break;
                case int i:
                    result.Add(i);
                    break;
                default:
                    throw new NotImplementedException("Unknown field type when constructing instance observations.");
            }
        }

        return result;
    }
}

public class ObservationOrderAttribute : Attribute
{
    public ObservationOrderAttribute(int index)
    {
        this.Index = index;
    }

    public int Index { get; }
}