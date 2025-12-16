#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Build;

[InitializeOnLoad]
public static class AutoSampleInstaller {
    
    private const string DefineSamplesInstalled = "XR_SAMPLES_INSTALLED";

    private static double nextAttemptTime = 0;
    private const double retryDelaySecs = 2;
    private static int retryCount = 0;
    private static int retryCountMax = 10;
    
    private readonly struct PackageSampleInfo {
        public readonly string PackageName;
        public readonly string[] SamplesToInstall;

        public PackageSampleInfo(string packageName, string[] samplesToInstall) {
            PackageName = packageName;
            SamplesToInstall = samplesToInstall;
        }
    }

    private static List<PackageSampleInfo> packageSampleInfos = new List<PackageSampleInfo>() {
        new PackageSampleInfo(
            "com.unity.xr.interaction.toolkit",
            new string[] { "Starter Assets", "Hands Interaction Demo" }),

        new PackageSampleInfo(
            "com.unity.xr.hands",
            new string[] { "HandVisualizer"})
    };
    
    

    private static void AddDefineForCurrentBuildTarget(string define) {
        var buildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        PlayerSettings.GetScriptingDefineSymbols(buildTarget, out var defines);
        
        var set = new HashSet<string>(defines ?? Array.Empty<string>(), StringComparer.Ordinal);

        if (set.Add(define)) {
            PlayerSettings.SetScriptingDefineSymbols(buildTarget, set.ToArray());
            Debug.Log($"[EDIA] Define added: {define}");
        }
        else {
            Debug.Log($"[EDIA] Define '{define}' already exists!");
        }
    }


    private static void RemoveDefineForCurrentBuildTarget(string define) {
        var buildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        PlayerSettings.GetScriptingDefineSymbols(buildTarget, out var defines);
        
        var set = new HashSet<string>(defines ?? Array.Empty<string>(), StringComparer.Ordinal);

        if (set.Remove(define)) {
            PlayerSettings.SetScriptingDefineSymbols(buildTarget, set.ToArray());
            Debug.Log($"[EDIA] Define removed: {define}");
        }
        else {
            Debug.Log($"[EDIA] Define '{define}' did not yet exist!");
        }
    }
    
    static AutoSampleInstaller()
    {
        Debug.Log("Checking packages ...");
        
        // Run after editor loads but before scripts compile
        EditorApplication.update += CheckPackageInstalled;
        EditorApplication.projectChanged += CheckPackageInstalled;
    }

    private static void CheckPackageInstalled() {
        
        if (EditorApplication.timeSinceStartup < nextAttemptTime)
            return;
        
        var packages = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages();

        if (!packages.Any())
        {
            Debug.LogError("[EDIA] Failed to list installed packages.");
            retryCount++;
            if (retryCount > retryCountMax) {
                Debug.LogError("Retry count exceeded. Giving up.");
                EditorApplication.update -= CheckPackageInstalled;
                return;
            }

            nextAttemptTime = EditorApplication.timeSinceStartup + retryDelaySecs;
            return;
        }

        bool allSamplesInstalled = true;
        
        foreach (var packageSampleInfo in packageSampleInfos) {
            var pkg = packages.FirstOrDefault(p => p.name == packageSampleInfo.PackageName);

            if (pkg == null) {
                Debug.LogWarning($"[EDIA] Required package '{packageSampleInfo.PackageName}' not installed.");
                allSamplesInstalled = false;
                break;
            }

            // If package installed → check samples
            allSamplesInstalled &= InstallSampleIfNeeded(pkg, packageSampleInfo);
            if (!allSamplesInstalled)
                break;
        }

        if (allSamplesInstalled) {
            AddDefineForCurrentBuildTarget(DefineSamplesInstalled);
        }
        else {
            RemoveDefineForCurrentBuildTarget(DefineSamplesInstalled);
        }
        
        EditorApplication.update -= CheckPackageInstalled;
    }

    private static bool InstallSampleIfNeeded(UnityEditor.PackageManager.PackageInfo pkg, PackageSampleInfo packageSampleInfo)
    {
        var samples = Sample.FindByPackage(pkg.name, pkg.version).ToList();

        if (samples.Count == 0)
        {
            Debug.LogWarning($"[EDIA] Package '{packageSampleInfo.PackageName}' has no samples?");
            return false;
        }

        foreach (var sampleInfo in packageSampleInfo.SamplesToInstall) {
            var sample = samples.FirstOrDefault(s => s.displayName == sampleInfo);

            if (string.IsNullOrEmpty(sample.displayName)) {
                Debug.LogError($"[EDIA] Sample '{sampleInfo}' not found in package '{packageSampleInfo.PackageName}'.");
                return false;
            }

            if (sample.isImported) {
                Debug.Log($"[EDIA] Sample '{sampleInfo}' already imported.");
                continue;
            }

            Debug.Log($"[EDIA] Installing sample '{sampleInfo}' from '{packageSampleInfo.PackageName}'...");

            var success = sample.Import();

            if (success)
                Debug.Log($"[EDIA] Successfully installed sample '{sampleInfo}' from '{packageSampleInfo.PackageName}'.");
            else {
                Debug.LogWarning($"[EDIA] Failed to import sample '{sampleInfo}'.");
                return false;
            }
        }
        return true;
    }
}
#endif
