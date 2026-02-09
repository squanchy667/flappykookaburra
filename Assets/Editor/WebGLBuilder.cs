using UnityEditor;
using UnityEngine;

public static class WebGLBuilder
{
    private const string BuildPath = "Builds/WebGL";

    [MenuItem("Build/WebGL Build")]
    public static void Build()
    {
        PlayerSettings.productName = "FlappyKookaburra";
        PlayerSettings.companyName = "FlappyKookaburra";
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.memorySize = 256;
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
        PlayerSettings.defaultScreenWidth = 540;
        PlayerSettings.defaultScreenHeight = 960;

        string[] scenes = { "Assets/Scenes/MainScene.unity" };

        BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = BuildPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        });

        Debug.Log($"WebGL build complete: {BuildPath}");
    }
}
