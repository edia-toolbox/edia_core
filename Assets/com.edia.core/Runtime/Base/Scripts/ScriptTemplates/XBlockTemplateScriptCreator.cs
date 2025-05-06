using UnityEditor;
using System.IO;

public static class XBlockTemplateScriptCreator {

    private const string Template =
        @"using Edia;
using UnityEngine;

public class XBlockTemplate : XBlock
{
    // Add your implementation here.
}";

#if UNITY_EDITOR
    
    [MenuItem("Assets/Create/EDIA/XBlockTemplate Script", false, 80)]
    public static void CreateXBlockTemplateScript() {
        string path       = GetSelectedPathOrFallback();
        string scriptPath = Path.Combine(path, "NewXBlockTemplate.cs");
        scriptPath = AssetDatabase.GenerateUniqueAssetPath(scriptPath);
        File.WriteAllText(scriptPath, Template);
        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(scriptPath);
    }

    private static string GetSelectedPathOrFallback() {
        string path = "Assets";
        foreach (var obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets)) {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path)) {
                path = Path.GetDirectoryName(path);
                break;
            }
        }

        return path;
    }

#endif
    
}
