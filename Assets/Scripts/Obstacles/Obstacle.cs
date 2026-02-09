using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private float _scrollSpeed;

    public void SetSpeed(float speed)
    {
        _scrollSpeed = speed;
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        transform.position += Vector3.left * _scrollSpeed * Time.deltaTime;
    }
}
