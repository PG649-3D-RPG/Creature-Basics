using UnityEditor;
using UnityEngine;

namespace LSystem
{
    /// <summary>
    /// Read-Only viewer of L-System properties.
    /// </summary>
    public class LSystemPropertyViewer : MonoBehaviour
    {
        public float m_Distance;
        public short m_Angle;
        public INITIAL_DIRECTION m_InitialDirection;
        public float m_Thickness;
        public uint m_CrossSections;
        public uint m_CrossSectionDivisions;

        public bool m_TranslatePoints;
        public string m_StartString;
        public uint m_Iterations;


        public string[] m_Rules;

        public void Populate(LSystemProperties properties)
        {
            m_Distance = properties.distance;
            m_Angle = properties.angle;
            m_InitialDirection = properties.initialDirection;
            m_Thickness = properties.thickness;
            m_CrossSections = properties.crossSections;
            m_CrossSectionDivisions = properties.crossSectionDivisions;
            m_TranslatePoints = properties.translatePoints;
            m_StartString = properties.startString;
            m_Iterations = properties.iterations;
            m_Rules = properties.rules;

        }
    }

    [CustomEditor(typeof(LSystemPropertyViewer))]
    public class LSystemPropertyViewerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            LSystemPropertyViewer prop = (LSystemPropertyViewer)target;

            GUI.enabled = false;

            EditorGUILayout.LabelField("Default Settings", EditorStyles.boldLabel);
            EditorGUILayout.FloatField("Distance", prop.m_Distance);

            EditorGUILayout.IntField("Angle", prop.m_Angle);
            EditorGUILayout.EnumFlagsField("Initial Direction", prop.m_InitialDirection);
            EditorGUILayout.FloatField("Thickness", prop.m_Thickness);
            EditorGUILayout.LongField("Cross Sections", prop.m_CrossSections);
            EditorGUILayout.LongField("Cross Section Divisions", prop.m_CrossSectionDivisions);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("L-System", EditorStyles.boldLabel);
            EditorGUILayout.Toggle("Translate Points?", prop.m_TranslatePoints);
            EditorGUILayout.TextField("Start String", prop.m_StartString);
            EditorGUILayout.LongField("Number of Generations", prop.m_Iterations);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rules", EditorStyles.boldLabel);
            foreach (var r in prop.m_Rules)
            {
                EditorGUILayout.TextField(r);
            }

            GUI.enabled = true;
        }
    }
}