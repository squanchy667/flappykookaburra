using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ButtonAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float _pressScale = 0.9f;
    [SerializeField] private float _pressDuration = 0.1f;
    [SerializeField] private float _releaseDuration = 0.15f;

    private Vector3 _originalScale;

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        DOTween.Kill(transform);
        transform.DOScale(_originalScale * _pressScale, _pressDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        DOTween.Kill(transform);
        transform.DOScale(_originalScale, _releaseDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    private void OnDisable()
    {
        DOTween.Kill(transform);
        transform.localScale = _originalScale;
    }
}
