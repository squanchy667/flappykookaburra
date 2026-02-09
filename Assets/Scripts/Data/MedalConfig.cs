using UnityEngine;

[CreateAssetMenu(fileName = "MedalConfig", menuName = "FlappyKookaburra/MedalConfig")]
public class MedalConfig : ScriptableObject
{
    [Header("Score Thresholds")]
    public int bronzeThreshold = 10;
    public int silverThreshold = 20;
    public int goldThreshold = 30;
    public int platinumThreshold = 40;

    [Header("Medal Sprites")]
    public Sprite bronzeSprite;
    public Sprite silverSprite;
    public Sprite goldSprite;
    public Sprite platinumSprite;

    public enum MedalType { None, Bronze, Silver, Gold, Platinum }

    public MedalType GetMedalForScore(int score)
    {
        if (score >= platinumThreshold) return MedalType.Platinum;
        if (score >= goldThreshold) return MedalType.Gold;
        if (score >= silverThreshold) return MedalType.Silver;
        if (score >= bronzeThreshold) return MedalType.Bronze;
        return MedalType.None;
    }

    public Sprite GetSpriteForMedal(MedalType medal)
    {
        switch (medal)
        {
            case MedalType.Bronze: return bronzeSprite;
            case MedalType.Silver: return silverSprite;
            case MedalType.Gold: return goldSprite;
            case MedalType.Platinum: return platinumSprite;
            default: return null;
        }
    }
}
