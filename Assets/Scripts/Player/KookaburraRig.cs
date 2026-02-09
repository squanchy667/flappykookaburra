using UnityEngine;

public class KookaburraRig : MonoBehaviour
{
    [Header("Body Parts")]
    [SerializeField] private Transform _body;
    [SerializeField] private Transform _leftWing;
    [SerializeField] private Transform _rightWing;
    [SerializeField] private Transform _tail;
    [SerializeField] private Transform _eye;

    [Header("Renderers")]
    [SerializeField] private SpriteRenderer _bodyRenderer;
    [SerializeField] private SpriteRenderer _leftWingRenderer;
    [SerializeField] private SpriteRenderer _rightWingRenderer;
    [SerializeField] private SpriteRenderer _tailRenderer;
    [SerializeField] private SpriteRenderer _eyeRenderer;

    public Transform Body => _body;
    public Transform LeftWing => _leftWing;
    public Transform RightWing => _rightWing;
    public Transform Tail => _tail;
    public Transform Eye => _eye;

    public SpriteRenderer BodyRenderer => _bodyRenderer;
    public SpriteRenderer LeftWingRenderer => _leftWingRenderer;
    public SpriteRenderer RightWingRenderer => _rightWingRenderer;
    public SpriteRenderer TailRenderer => _tailRenderer;
    public SpriteRenderer EyeRenderer => _eyeRenderer;
}
