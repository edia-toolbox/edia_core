using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using eDIA;

/// <summary>
/// Handles post Unity build actions, i.e. copy config files to build directory
/// </summary>
public class ConfigMoveProcessor : IPostprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPostprocessBuild(BuildReport report)
    {
        string fileName = Path.GetFileName(report.summary.outputPath);
        string path = report.summary.outputPath.Replace(fileName, "");
        // string datapath = PlayerSettings.productName + "_Data/";
        
        path = path + "Configs";
        Directory.CreateDirectory (path);
        FileManager.CopyDirectory("Assets/Configs", path, ".meta");

        UnityEngine.Debug.Log("Copied config files to " + path);
    }
}