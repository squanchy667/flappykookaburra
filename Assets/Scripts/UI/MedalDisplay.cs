using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MedalDisplay : MonoBehaviour
{
    [SerializeField] private Image _medalImage;
    [SerializeField] private MedalConfig _medalConfig;
    [SerializeField] private float _popDuration = 0.5f;

    public void ShowMedal(int score)
    {
        if (_medalConfig == null || _medalImage == null) return;

        var medalType = _medalConfig.GetMedalForScore(score);
        if (medalType == MedalConfig.MedalType.None)
        {
            _medalImage.gameObject.SetActive(false);
            return;
        }

        var sprite = _medalConfig.GetSpriteForMedal(medalType);
        if (sprite == null)
        {
            _medalImage.gameObject.SetActive(false);
            return;
        }

        _medalImage.sprite = sprite;
        _medalImage.color = Color.white;
        _medalImage.gameObject.SetActive(true);

        // Elastic pop-in
        var rt = _medalImage.GetComponent<RectTransform>();
        rt.localScale = Vector3.zero;
        rt.DOScale(1f, _popDuration).SetEase(Ease.OutElastic).SetUpdate(true);
    }

    public void Hide()
    {
        if (_medalImage != null)
        {
            DOTween.Kill(_medalImage.GetComponent<RectTransform>());
            _medalImage.gameObject.SetActive(false);
        }
    }
}
