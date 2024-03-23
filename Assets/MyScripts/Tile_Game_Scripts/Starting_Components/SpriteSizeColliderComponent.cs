using UnityEngine;

class SpriteSizeColliderComponent : ComponentCacher
{
    SpriteRenderer spriteRenderer => CreateOrGetComponent<SpriteRenderer>();
    void Start()
    {
        Rigidbody2D body = gameObject.AddComponent<Rigidbody2D>();
        body.isKinematic = true;
        BoxCollider2D unitCollider = gameObject.AddComponent<BoxCollider2D>();
        unitCollider.isTrigger = true;

        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
        unitCollider.size = spriteSize;
    }
}