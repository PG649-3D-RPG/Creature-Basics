using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Adds a menu item to create a prefab of the selected game object.
/// 
/// adapted from https://docs.unity3d.com/ScriptReference/PrefabUtility.SaveAsPrefabAsset.html
/// </summary>
public class PrefabCreator : MonoBehaviour
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
            string path = "";
            string name = "";
            if (gameObject.TryGetComponent(out SkeletonTest skeleton))
            {
                path = skeleton.creatureExportPath;
                name = skeleton.creatureExportName;
            }

            // set default values if custom values were not set
            path = path != "" ? path : "Packages/com.pg649.creaturegenerator/Runtime/SkeletonFromLSystem/Prefabs/";
            name = name != "" ? name : gameObject.name;

            // Create folder if it does not exist and set path for exported prefab.
            if (!Directory.Exists(path)) CreateFoldersRecursively(path);
            string localPath = path + name + ".prefab";

            // Make sure the file name is unique, in case an existing Prefab has the same name.
            localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);

            // remove unneeded components and lock lsystem
            if (gameObject.TryGetComponent(out SkeletonTest skeletonTest)) DestroyImmediate(skeletonTest);
            // if (gameObject.TryGetComponent(out MarchingCubesProject.MeshGenerator meshGen)) DestroyImmediate(meshGen);
            if (gameObject.TryGetComponent(out PrefabCreator prefabCreator)) DestroyImmediate(prefabCreator);
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

    /// <summary>
    /// Create folders recursively specified by path, separated by /
    /// </summary>
    /// <param name="path">Path alongside which the folders will be created. Separated by /</param>
    /// <returns></returns>
    private static string CreateFoldersRecursively(string path)
    {
        string[] folders = path.Split('/');
        string last = "";
        for (int i = 1; i < folders.Length; i++)
        {
            last = AssetDatabase.CreateFolder(folders[i - 1], folders[i]);
        }
        return last;
    }
}