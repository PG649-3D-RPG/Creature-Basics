using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LSystem
{
    public enum INITIAL_DIRECTION { UP, DOWN, LEFT, RIGHT, FORWARD, BACK, DIAGONAL };

    public class LSystemEditor : MonoBehaviour
    {
        [Header("Default Settings")]
        [Tooltip("Default forward distance")]
        public float m_Distance = 1f;
        [Tooltip("Default turning angle")]
        public short m_Angle = 90;

        [Tooltip("Initial direction the turtle faces")]
        public INITIAL_DIRECTION m_InitialDirection = INITIAL_DIRECTION.DOWN;

        [Tooltip("Default segment thickness")]
        public float m_Thickness = .1f;

        [Tooltip("Default number of cross sections")]
        public uint m_CrossSections = 4;
        [Tooltip("Default number of cross section divisions")]
        public uint m_CrossSectionDivisions = 2;

        [Header("L-System")]
        [Tooltip("Translate points to above ground")]
        public bool m_TranslatePoints = true;
        [Tooltip("Initial string to evaluate")]
        public string m_StartString = "FABFCD";//"FF[+F][-F]F(42)";
        [Tooltip("Number of iterations to apply the replacement rules")]
        public uint m_Iterations = 1;
        [Tooltip("Replacement rules")]
        public string[] m_Rules = { "A=[+F(1.5)]", "B=[-F(1.5)]", "C=[+(30)F(1.5)C]", "D=[-(30)F(1.5)D]" };

        private Dictionary<char, List<string>> ParseRuleInput(string[] rules)
        {
            var nt = new Dictionary<char, List<string>>();
            foreach (var r in rules)
            {
                //check syntax
                if (r.Length < 3) throw new ArgumentException("Rule is in a wrong format");
                if (!r.Contains('=')) throw new ArgumentException("Rule needs to include an =");
                // search index of '='
                int end = r.IndexOf('=');
                string non_terminal = r[..end];
                if (non_terminal.Length > 1) throw new ArgumentException("Non Terminals may only be exactly one character.");
                string replacement = r[(end + 1)..];
                // check if non_terminal is already contained or a terminal has the same symbol
                if (LSystem.TERMINALS.Contains(non_terminal[0])) throw new ArgumentException("Cannot add a rule with a symbol that is a terminal: " + non_terminal[0]);
                // add to list for stochastic l-system if there are multiple rules with same non-terminal; otherwise create new list
                if (nt.ContainsKey(non_terminal[0])) nt[non_terminal[0]].Add(replacement);
                else nt.Add(non_terminal[0], new List<string> { replacement });
            }
            return nt;
        }

        public LSystem BuildLSystem()
        {
            var rules = ParseRuleInput(m_Rules);
            LSystem l = new(m_Distance, m_Angle, m_CrossSections, m_CrossSectionDivisions, m_InitialDirection, m_StartString, m_Iterations, rules, true);
            return l;
        }

        public LSystemProperties GenerateProperties()
        {
            return new(m_Distance, m_Angle, m_InitialDirection, m_Thickness, m_CrossSections, m_CrossSectionDivisions, m_TranslatePoints, m_StartString, m_Iterations, m_Rules);
        }

        public void ApplyLSystemSettings(LSystemProperties prop)
        {
            this.m_Distance = prop.distance;
            this.m_Angle = prop.angle;
            this.m_InitialDirection = prop.initialDirection;
            this.m_Thickness = prop.thickness;
            this.m_CrossSections = prop.crossSections;
            this.m_CrossSectionDivisions = prop.crossSectionDivisions;
            this.m_TranslatePoints = prop.translatePoints;
            this.m_StartString = prop.startString;
            this.m_Iterations = prop.iterations;
            this.m_Rules = prop.rules;
        }

        // Creates a new menu item 'PG649 > Create Prefab' in the main menu.
        [MenuItem("PG649/Export LSystem Settings")]
        private static void ExportLSystem()
        {
            // Keep track of the currently selected GameObject
            GameObject go = Selection.activeObject as GameObject;

            if (go == null || go.TryGetComponent(out LSystemEditor skeleton) == false)
            {
                EditorUtility.DisplayDialog(
                                "Select LSystemEditor",
                                "Thou shalt select a gameobject with an attached LSystemEditor Component!",
                                "Sure thing!");
                return;
            }

            string path = EditorUtility.SaveFilePanel(
            "Save texture as JSON",
            "Packages" + Path.DirectorySeparatorChar + "com.pg649.creaturegenerator",
            go.name + " lsystem.json",
            "json");// absolute path

            if (path.Length != 0)
            {
                var prop = skeleton.GenerateProperties();
                var jsonData = JsonUtility.ToJson(prop);
                StreamWriter writer = new(path, false);
                writer.WriteLine(jsonData);
                writer.Close();
                Debug.Log("JSON was saved successfully at: " + path);
                var projectpath = Application.dataPath[..Application.dataPath.LastIndexOf(Path.DirectorySeparatorChar)];
                // Re-import the file to update the reference in the editor
                if (path.StartsWith(projectpath)) // if path is under project make path relative to project to import into assetDatabase
                {
                    path = path[(projectpath.Length + 1)..];
                    AssetDatabase.ImportAsset(path);
                }
            }
        }

        // Disable the menu item if no selection is in place or editor is not in play mode.
        [MenuItem("PG649/Export LSystem Settings", true)]
        private static bool ValidateExportLSystem()
        {
            return Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<LSystemEditor>() != null && !EditorUtility.IsPersistent(Selection.activeGameObject);
        }


        // Creates a new menu item 'PG649 > Create Prefab' in the main menu.
        [MenuItem("PG649/Import LSystem Settings")]
        private static void ImportLSystem()
        {
            // Keep track of the currently selected GameObject
            GameObject go = Selection.activeObject as GameObject;

            if (go == null || go.TryGetComponent(out LSystemEditor skeleton) == false)
            {
                EditorUtility.DisplayDialog(
                                "Select LSystemEditor",
                                "Thou shalt select a gameobject with an attached LSystemEditor Component!",
                                "Sure thing!");
                return;
            }

            string path = EditorUtility.OpenFilePanel(
            "Load L-System settings JSON",
            "Packages" + Path.DirectorySeparatorChar + "com.pg649.creaturegenerator",
            "json");// absolute path

            if (path.Length != 0)
            {
                StreamReader reader = new(path);
                string json = reader.ReadToEnd();
                reader.Close();

                LSystemProperties prop = JsonUtility.FromJson<LSystemProperties>(json);
                skeleton.ApplyLSystemSettings(prop);
                Debug.Log("L-System settings successfully imported");
            }
        }

        // Disable the menu item if no selection is in place or editor is not in play mode.
        [MenuItem("PG649/Import LSystem Settings", true)]
        private static bool ValidateImportLSystem()
        {
            return Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<LSystemEditor>() != null && !EditorUtility.IsPersistent(Selection.activeGameObject);
        }
    }
}