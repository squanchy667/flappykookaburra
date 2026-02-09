using UnityEngine;

public static class WorldBounds
{
    public static float ScreenRight => Camera.main.orthographicSize * Camera.main.aspect;
    public static float ScreenLeft => -ScreenRight;
    public static float ScreenTop => Camera.main.orthographicSize;
    public static float ScreenBottom => -ScreenTop;
    public static float SpawnX => ScreenRight + 2f;
    public static float DespawnX => ScreenLeft - 2f;
}
