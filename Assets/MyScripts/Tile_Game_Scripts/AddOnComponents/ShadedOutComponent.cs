using UnityEngine;

public class ShadedOutComponent : MonoBehaviour
{
    private Color ogColor;
    private bool _so = false;
    SpriteRenderer SpriteRenderer;

    private void Start()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }
    public bool ShadedOut
    {
        get => _so;
        set
        {
            _so = value;
            if (value)
            {
                ogColor = SpriteRenderer.color = Color.black;
            }
            else
            {
                SpriteRenderer.color = ogColor;
            }
        }
    }

}