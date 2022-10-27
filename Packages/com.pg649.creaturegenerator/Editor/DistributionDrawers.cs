using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(NormalDistribution))]
public class NormalDistributionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var muLabelRect = new Rect(position.x, position.y, position.width * 0.1f, position.height / 2);
        var muValueRect = new Rect(position.x + 0.1f * position.width, position.y, position.width * 0.4f,
            position.height / 2);
        var sigmaLabelRect = new Rect(position.x + 0.5f * position.width, position.y, position.width * 0.1f,
            position.height / 2);
        var sigmaValueRect = new Rect(position.x + 0.6f * position.width, position.y, position.width * 0.4f,
            position.height / 2);

        var minLabelRect = new Rect(position.x, position.y + position.height / 2, position.width * 0.2f,
            position.height / 2);
        var minValueRect = new Rect(position.x + 0.2f * position.width, position.y + position.height / 2,
            position.width * 0.3f,
            position.height / 2);
        var maxLabelRect = new Rect(position.x + 0.5f * position.width, position.y + position.height / 2,
            position.width * 0.2f,
            position.height / 2);
        var maxValueRect = new Rect(position.x + 0.7f * position.width, position.y + position.height / 2,
            position.width * 0.3f,
            position.height / 2);

        var mu = property.FindPropertyRelative("Mean");
        var sigma = property.FindPropertyRelative("StandardDeviation");
        var min = property.FindPropertyRelative("Min");
        var max = property.FindPropertyRelative("Max");

        var style = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
        };

        EditorGUI.LabelField(muLabelRect, "μ:", style);
        mu.floatValue = EditorGUI.FloatField(muValueRect, mu.floatValue);
        EditorGUI.LabelField(sigmaLabelRect, "σ:", style);
        sigma.floatValue = EditorGUI.FloatField(sigmaValueRect, sigma.floatValue);

        EditorGUI.LabelField(minLabelRect, "Min:", style);
        min.floatValue = EditorGUI.FloatField(minValueRect, min.floatValue);
        EditorGUI.LabelField(maxLabelRect, "Max:", style);
        max.floatValue = EditorGUI.FloatField(maxValueRect, max.floatValue);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 2 * EditorGUIUtility.singleLineHeight;
    }
}

[CustomPropertyDrawer(typeof(UniformIntegerDistribution))]
public class UniformIntegerDistributionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var minLabelRect = new Rect(position.x, position.y, position.width * 0.2f, position.height);
        var minValueRect = new Rect(position.x + 0.2f * position.width, position.y, position.width * 0.3f,
            position.height);
        var maxLabelRect = new Rect(position.x + 0.5f * position.width, position.y, position.width * 0.2f,
            position.height);
        var maxValueRect = new Rect(position.x + 0.7f * position.width, position.y, position.width * 0.3f,
            position.height);

        var min = property.FindPropertyRelative("Min");
        var max = property.FindPropertyRelative("Max");

        var style = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
        };

        EditorGUI.LabelField(minLabelRect, "Min:", style);
        min.intValue = EditorGUI.IntField(minValueRect, min.intValue);
        EditorGUI.LabelField(maxLabelRect, "Max:", style);
        max.intValue = EditorGUI.IntField(maxValueRect, max.intValue);

        EditorGUI.EndProperty();
    }
}