using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

public class FMBuildPostProcessor
{
#if UNITY_IOS
    [PostProcessBuildAttribute(1)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {

        if (target == BuildTarget.iOS)
        {
            PBXProject project = new PBXProject();
            string sPath = PBXProject.GetPBXProjectPath(path);
            project.ReadFromFile(sPath);

            //string tn = PBXProject.GetUnityTargetName();
#if !UNITY_2019_3_OR_NEWER
            string tn = "Unity-iPhone";
#else
            string tn = "UnityFramework";
#endif
            string g = project.TargetGuidByName(tn);
            ModifyAsDesired(project, g);
            File.WriteAllText(sPath, project.WriteToString());
        }
    }

    static void ModifyAsDesired(PBXProject project, string g)
    {
        // add frameworks, reference
        //project.AddFrameworkToProject(g, "VideoToolbox.framework", false);

        // go insane with build settings
        //project.AddBuildProperty(g, "LIBRARY_SEARCH_PATHS", "../FFmpeg-iOS/lib");

        Debug.Log("FM Build Settings: added Other Linker Flag for XCode");
        project.AddBuildProperty(g, "OTHER_LDFLAGS", "-lturbojpeg");
    }
#endif
}