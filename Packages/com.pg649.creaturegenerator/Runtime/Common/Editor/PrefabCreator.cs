using UnityEditor;
using UnityEngine;

/// <summary>
/// Adds a menu item to create a prefab of the selected game object.
/// 
/// from https://docs.unity3d.com/ScriptReference/PrefabUtility.SaveAsPrefabAsset.html
/// </summary>
public class PrefabCreator : MonoBehaviour //TODO make it a sample and not just a prefab
{
    // Creates a new menu item 'PG649 > Create Prefab' in the main menu.
    [MenuItem("PG649/Create Prefab")]
    private static void CreatePrefab()
    {
        // Keep track of the currently selected GameObject(s)
        GameObject[] objectArray = Selection.gameObjects;

        // Loop through every GameObject in the array above
        foreach (GameObject gameObject in objectArray)
        {
            // Create folder Prefabs and set the path as within the Prefabs folder,
            // and name it as the GameObject's name with the .Prefab format
            // if (!Directory.Exists("Packages/com.pg649.creaturegenerator/Runtime/SkeletonFromLSystem/Prefabs"))
            //     AssetDatabase.CreateFolder("Assets", "Prefabs");
            string localPath = "Packages/com.pg649.creaturegenerator/Runtime/SkeletonFromLSystem/Prefabs/" + gameObject.name + ".prefab";

            // Make sure the file name is unique, in case an existing Prefab has the same name.
            localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);

            // remove unneeded components and lock lsystem
            DestroyImmediate(gameObject.GetComponent<SkeletonTest>());
            DestroyImmediate(gameObject.GetComponent<MarchingCubesProject.MeshGenerator>());
            DestroyImmediate(gameObject.GetComponent<PrefabCreator>());
            if (gameObject.TryGetComponent(out LSystem.LSystemEditor editor))
            {
                var prop = editor.GenerateProperties();
                gameObject.AddComponent<LSystem.LSystemPropertyViewer>();
                gameObject.GetComponent<LSystem.LSystemPropertyViewer>().Populate(prop);
                DestroyImmediate(editor);
            }

            // Create the new Prefab and log whether Prefab was saved successfully.
            PrefabUtility.SaveAsPrefabAsset(gameObject, localPath, out bool prefabSuccess);
            if (prefabSuccess == true)
                Debug.Log("Prefab was saved successfully");
            else
                Debug.Log("Prefab failed to save" + prefabSuccess);
        }
    }
    // Disable the menu item if no selection is in place or editor is not in play mode.
    [MenuItem("PG649/Create Prefab", true)]
    private static bool ValidateCreatePrefab()
    {
        return Selection.activeGameObject != null && !EditorUtility.IsPersistent(Selection.activeGameObject) && EditorApplication.isPlaying;
    }
}