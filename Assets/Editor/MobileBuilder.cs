using UnityEditor;
using UnityEngine;

public static class MobileBuilder
{
    [MenuItem("Tools/Build/iOS")]
    public static void BuildIOS()
    {
        ConfigureMobileSettings();
        PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
        PlayerSettings.statusBarHidden = true;

        var scenes = GetEnabledScenes();
        BuildPipeline.BuildPlayer(scenes, "Builds/iOS", BuildTarget.iOS, BuildOptions.None);
        Debug.Log("[MobileBuilder] iOS build complete");
    }

    [MenuItem("Tools/Build/Android")]
    public static void BuildAndroid()
    {
        ConfigureMobileSettings();
        PlayerSettings.statusBarHidden = true;

        var scenes = GetEnabledScenes();
        BuildPipeline.BuildPlayer(scenes, "Builds/FlappyKookaburra.apk", BuildTarget.Android, BuildOptions.None);
        Debug.Log("[MobileBuilder] Android build complete");
    }

    [MenuItem("Tools/Build/Configure Mobile Settings")]
    public static void ConfigureMobileSettings()
    {
        // Portrait lock
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;

        // Hide status bar
        PlayerSettings.statusBarHidden = true;

        Debug.Log("[MobileBuilder] Mobile settings configured: portrait locked, status bar hidden");
    }

    private static string[] GetEnabledScenes()
    {
        var scenes = EditorBuildSettings.scenes;
        var enabledScenes = new System.Collections.Generic.List<string>();
        foreach (var scene in scenes)
        {
            if (scene.enabled)
                enabledScenes.Add(scene.path);
        }
        return enabledScenes.ToArray();
    }
}
