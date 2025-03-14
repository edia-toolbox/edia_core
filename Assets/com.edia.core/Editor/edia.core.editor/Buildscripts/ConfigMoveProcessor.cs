using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Edia;

/// <summary>
/// Handles post Unity build actions, i.e. copy config files to build directory
/// </summary>
public class ConfigMoveProcessor : IPostprocessBuildWithReport {
    public int callbackOrder {
        get { return 0; }
    }

    /// <summary>
    /// Executes post-build processes for Unity builds.
    /// This method handles actions to copy configuration files from the Assets/Configs
    /// directory of the Unity project into the build output directory. If the Assets/Configs
    /// folder does not exist, it skips the copy process.
    /// </summary>
    /// <param name="report">A BuildReport instance containing information about the completed build, including output path and summary details.</param>
    public void OnPostprocessBuild(BuildReport report) {
        string fileName = Path.GetFileName(report.summary.outputPath);
        string path     = report.summary.outputPath.Replace(fileName, "");


        if (!Directory.Exists("Assets/Configs") && !Directory.Exists("Assets/configs")) {
            UnityEngine.Debug.Log("Assets/Configs folder does not exist. Skipping config file copy.");
            return;
        }

        path = path + "Configs";
        Directory.CreateDirectory(path);
        FileManager.CopyDirectory("Assets/Configs", path, ".meta");

        UnityEngine.Debug.Log("Copied config files to " + path);
    }
}