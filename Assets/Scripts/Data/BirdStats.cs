using UnityEngine;

[CreateAssetMenu(fileName = "BirdStats", menuName = "FlappyKookaburra/BirdStats")]
public class BirdStats : ScriptableObject
{
    public float flapForce = 5f;
    public float gravityScale = 2.5f;
    public float maxUpwardVelocity = 8f;
    public float rotationSpeed = 10f;
    public float deathRotation = -90f;
}
