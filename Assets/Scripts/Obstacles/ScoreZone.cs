using UnityEngine;

public class ScoreZone : MonoBehaviour
{
    private bool _scored;

    public void ResetScored()
    {
        _scored = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_scored) return;
        if (!other.CompareTag("Player")) return;

        _scored = true;
        ScoreManager.Instance.IncrementScore();
    }
}
