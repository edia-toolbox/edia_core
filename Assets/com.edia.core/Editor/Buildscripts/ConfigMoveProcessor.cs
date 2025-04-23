using System.IO;
using System.Linq;
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
        var fileName          = Path.GetFileName(report.summary.outputPath);
        var outputPath        = Path.GetDirectoryName(report.summary.outputPath) ?? "";
        var outputPathConfigs = "";

        if (outputPath == "") {
            UnityEngine.Debug.LogError("No valid output path provided");
        }

        outputPathConfigs = Path.Combine(outputPath, "configs");

        var source = new[] { "Assets/Configs", "Assets/configs" }
            .FirstOrDefault(Directory.Exists) ?? "";

        if (source == "") {
            UnityEngine.Debug.LogWarning("Assets/configs folder does not exist. Skipping config file copy. " +
                                         "Your build might not work! Make sure you have a Configs folder in your Assets " +
                                         "folder.");
            return;
        }

        Directory.CreateDirectory(outputPath);
        FileManager.CopyDirectory(source, outputPathConfigs, ".meta");

        UnityEngine.Debug.Log("Copied config files to " + outputPath);
    }
}