using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class PackageUnlocker : EditorWindow
{
    private List<string> packageNames = new List<string>();
    private string newPackageName = "";

    [MenuItem("eDIA/Package Unlocker")]
    public static void ShowWindow()
    {
        GetWindow<PackageUnlocker>("Package Unlocker");
    }

    private void OnGUI()
    {
        GUILayout.Label("Add packages to unlock", EditorStyles.boldLabel);

        if (GUILayout.Button("Add all non-Unity dependencies"))
        {
            packageNames = GetCurrentDependencies();
        }

        newPackageName = EditorGUILayout.TextField("Add by (partial) name:", newPackageName);


        if (GUILayout.Button("Add package"))
        {
            if (!string.IsNullOrEmpty(newPackageName))
            {
                packageNames.Add(newPackageName);
                newPackageName = "";
            }
        }

        if (packageNames.Count > 0)
        {
            foreach (var packageName in packageNames)
            {
                //EditorGUILayout.LabelField("To unlock:", packageName);
                bool rmPackage = EditorGUILayout.LinkButton(packageName);
                if (rmPackage)
                {
                    packageNames.Remove(packageName);
                    rmPackage = false;
                    break;
                }
            }

            if (GUILayout.Button("Unlock Packages (click package to remove from list)"))
            {
                UnlockPackages();
                packageNames.Clear();
            }
        }


    }

    private List<string> GetCurrentDependencies()
    {
        string path = Application.dataPath + "/../Packages/packages-lock.json";
        if (File.Exists(path))
        {
            var json = JObject.Parse(File.ReadAllText(path));
            JObject dependencies = (JObject)json["dependencies"];

            return dependencies.Properties()
                                .Where(p => !p.Name.Contains("com.unity"))
                                .Select(p => p.Name)
                                .ToList();
        }
        else
        {
            Debug.LogError("packages-lock.json not found.");
            return new List<string>();
        }
    }

    private void UnlockPackages()
    {
        string path = Application.dataPath + "/../Packages/packages-lock.json";

        if (File.Exists(path))
        {
            var json = JObject.Parse(File.ReadAllText(path));
            JObject dependencies = (JObject)json["dependencies"];

            List<string> packagesChanged = new List<string>();

            foreach (var partialName in packageNames)
            {
                var matches = dependencies.Properties()
                                        .Where(p => p.Name.Contains(partialName))
                                        .ToList();

                if (matches.Count == 1)
                {
                    matches[0].Remove();
                    packagesChanged.Add(matches[0].Name);
                }
                else if (matches.Count > 1)
                {
                    Debug.LogWarning($"Multiple packages found for '{partialName}': {string.Join(", ", matches.Select(m => m.Name))}. No packages removed.");
                }
            }

            if (packagesChanged.Count == 0)
            {
                Debug.Log("No packages found to unlock.");
                return;
            }

            File.WriteAllText(path, json.ToString());

            Debug.Log($"Unlocked packages: {string.Join(", ", packagesChanged)}. Un- and re-focus Unity to apply changes.");
        }
        else
        {
            Debug.LogError("packages-lock.json not found.");
        }
    }
}
