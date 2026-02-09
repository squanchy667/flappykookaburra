using UnityEngine;
using DG.Tweening;

public static class DOTweenInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        DOTween.Init();
        Application.quitting += () => DOTween.Clear();
    }
}
