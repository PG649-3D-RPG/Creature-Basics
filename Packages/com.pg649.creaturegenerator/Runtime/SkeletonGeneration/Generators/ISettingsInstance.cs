using System;
using System.Collections.Generic;

public interface ISettingsInstance
{
    public List<float> Observations()
    {
        List<float> result = new();
        foreach (var field in GetType().GetFields())
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