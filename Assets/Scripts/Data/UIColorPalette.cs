using UnityEngine;

[CreateAssetMenu(fileName = "UIColorPalette", menuName = "FlappyKookaburra/UIColorPalette")]
public class UIColorPalette : ScriptableObject
{
    [Header("Primary Colors")]
    public Color primary = new Color(0.91f, 0.66f, 0.29f);       // #E8A849
    public Color textDark = new Color(0.24f, 0.15f, 0.14f);      // #3E2723
    public Color textLight = Color.white;

    [Header("Button Colors")]
    public Color buttonGreen = new Color(0.30f, 0.69f, 0.31f);   // #4CAF50
    public Color buttonGreenHighlight = new Color(0.40f, 0.79f, 0.41f);
    public Color buttonGreenPressed = new Color(0.20f, 0.55f, 0.21f);

    [Header("Accent Colors")]
    public Color accentBlue = new Color(0.53f, 0.81f, 0.92f);    // #87CEEB

    [Header("Medal Colors")]
    public Color bronze = new Color(0.80f, 0.50f, 0.20f);        // #CC8033
    public Color silver = new Color(0.75f, 0.75f, 0.75f);        // #C0C0C0
    public Color gold = new Color(1.0f, 0.84f, 0.0f);            // #FFD700
    public Color platinum = new Color(0.90f, 0.89f, 0.85f);      // #E5E4D7
}
