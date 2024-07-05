using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class FindMissingScripts
{
    [MenuItem("EDIA/EditorTools/Find Missing Scripts")]
    public static void FindMissing()
    {
        CheckPrefabs();
        CheckScenes();
    }

    private static void CheckPrefabs()
    {
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");
        foreach (var prefabGuid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGuid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null)
            {
                for (int index = 0; index < prefab.transform.childCount; index++)
                {
                    var child = prefab.transform.GetChild(index);
                    var components = child.GetComponents<Component>();
                    foreach (var component in components)
                    {
                        if (component == null)
                        {
                            Debug.LogWarning($"Missing script found on game object: {child.gameObject.name} in prefab: {path}", child);
                        }
                    }
                }
            }
        }
    }

    private static void CheckScenes()
    {
        string[] sceneGUIDs = AssetDatabase.FindAssets("t:Scene");
        foreach (var sceneGuid in sceneGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(sceneGuid);
            if (path.StartsWith("Packages"))
            {
                continue;
            }

            var scene = EditorSceneManager.OpenScene(path);
            if (scene.IsValid())
            {
                GameObject[] rootObjects = scene.GetRootGameObjects();
                foreach (GameObject sceneObject in rootObjects)
                {
                    for (int index = 0; index < sceneObject.transform.childCount; index++)
                    {
                        var child = sceneObject.transform.GetChild(index);
                        var components = child.GetComponents<Component>();
                        foreach (var component in components)
                        {
                            if (component == null)
                            {
                                Debug.LogWarning($"Missing script found on game object: {child.gameObject.name} in scene: {path}", child);
                            }
                        }
                    }
                }
            }
        }
    }
}