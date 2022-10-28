using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ParametricCreatureSettings))]
public class ParametricCreatureSettingsInspector : Editor
{
    private float normalDistributionScaling = 1.0f;
    private float normalDistributionShift = 1.0f;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Editing Tools", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical("label");
        
        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.PrefixLabel("Shift normal distributions by:");
        normalDistributionShift = EditorGUILayout.FloatField(normalDistributionShift);
        if (GUILayout.Button("+"))
        {
            Shift("Mean", normalDistributionShift);
            Repaint();
        }

        if (GUILayout.Button("-"))
        {
            Shift("Mean", -normalDistributionShift);
            Repaint();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.PrefixLabel("Scale normal distributions by:");
        normalDistributionScaling = EditorGUILayout.FloatField(normalDistributionScaling);
        if (GUILayout.Button("*"))
        {
            Scale("Mean", normalDistributionScaling);
            Scale("StandardDeviation", normalDistributionScaling);
            Repaint();
        }

        if (GUILayout.Button("/"))
        {
            Scale("Mean", 1.0f / normalDistributionScaling);
            Scale("StandardDeviation", 1.0f / normalDistributionScaling);
            Repaint();
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Zero Standard Deviation"))
        {
            Zero("StandardDeviation"); 
            Repaint();
        }

        if (GUILayout.Button("Reset Normal Distribution Min/Max"))
        {
            ResetMinMax(); 
            Repaint();
        }
        
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }

    private void ResetMinMax()
    {
        var s = serializedObject.GetIterator();
        s.Next(true);
        do
        {
            var mean = s.FindPropertyRelative("Mean");
            var min = s.FindPropertyRelative("Min");
            var max = s.FindPropertyRelative("Max");
            // NOTE: Check for mean field to ensure we only apply this to normal distributions
            if (mean == null || min == null || max == null) continue;
            min.floatValue = 0.0f;
            max.floatValue = float.MaxValue;
        } while (s.Next(false));
    }
    private void Zero(string path)
    {
        var s = serializedObject.GetIterator();
        s.Next(true);
        do
        {
            var prop = s.FindPropertyRelative(path);
            if (prop != null)
            {
                prop.floatValue = 0.0f;
            }
        } while (s.Next(false));
    }
    private void Shift(string path, float amount)
    {
        var s = serializedObject.GetIterator();
        s.Next(true);
        do
        {
            var prop = s.FindPropertyRelative(path);
            if (prop != null)
            {
                prop.floatValue += amount;
            }
        } while (s.Next(false));
    }
    private void Scale(string path, float factor)
    {
        var s = serializedObject.GetIterator();
        s.Next(true);
        do
        {
            var prop = s.FindPropertyRelative(path);
            if (prop != null)
            {
                prop.floatValue *= factor;
            }
        } while (s.Next(false));
    }
}