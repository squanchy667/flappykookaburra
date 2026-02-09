using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScreenFlash : MonoBehaviour
{
    [SerializeField] private Image _flashImage;
    [SerializeField] private float _flashAlpha = 0.6f;
    [SerializeField] private float _flashDuration = 0.15f;

    public void Flash()
    {
        if (_flashImage == null) return;

        DOTween.Kill(_flashImage);
        _flashImage.color = new Color(1f, 1f, 1f, _flashAlpha);
        _flashImage.DOFade(0f, _flashDuration).SetUpdate(true);
    }

    public void Flash(Color color, float alpha, float duration)
    {
        if (_flashImage == null) return;

        DOTween.Kill(_flashImage);
        _flashImage.color = new Color(color.r, color.g, color.b, alpha);
        _flashImage.DOFade(0f, duration).SetUpdate(true);
    }
}
