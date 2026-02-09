using UnityEngine;

public static class WorldBounds
{
    private static Camera _cachedCamera;

    private static Camera MainCamera
    {
        get
        {
            if (_cachedCamera == null)
                _cachedCamera = Camera.main;
            return _cachedCamera;
        }
    }

    public static float ScreenRight => MainCamera.orthographicSize * MainCamera.aspect;
    public static float ScreenLeft => -ScreenRight;
    public static float ScreenTop => MainCamera.orthographicSize;
    public static float ScreenBottom => -ScreenTop;
    public static float SpawnX => ScreenRight + 2f;
    public static float DespawnX => ScreenLeft - 2f;
}
