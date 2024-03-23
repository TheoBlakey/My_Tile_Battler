using UnityEngine;

public class ShadeOutComponent : ComponentCacher
{
    private Color ogColor = default;
    SpriteRenderer SpriteRenderer => CreateOrGetComponent<SpriteRenderer>();

    private bool _so = false;
    public bool ShadedOut
    {
        get => _so;
        set
        {
            if (ogColor == default) { ogColor = SpriteRenderer.color; }

            _so = value;
            SpriteRenderer.color = value ? Color.black : ogColor;
        }
    }

}