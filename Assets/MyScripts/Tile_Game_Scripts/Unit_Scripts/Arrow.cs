using Unity.VisualScripting;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public VikingUnit UnitHeadingFor;

    private void Start()
    {
        this.AddComponent<SpriteSizeColliderComponent>();
        //gameObject.AddComponent<Rigidbody2D>();
        //BoxCollider unitCollider = gameObject.AddComponent<BoxCollider>();
        //unitCollider.isTrigger = true;

        //Vector3 spriteSize = GetComponent<SpriteRenderer>().bounds.size;
        //unitCollider.size = spriteSize;
    }
    private void FixedUpdate()
    {
        Vector2 arrowPosition = transform.position;
        Vector2 unitPosition = UnitHeadingFor.transform.position;

        FaceUnit(unitPosition, arrowPosition);

        Vector2 direction = (unitPosition - arrowPosition).normalized;
        float moveSpeed = 2f;

        Vector2 vectorChange = moveSpeed * Time.fixedDeltaTime * direction;
        transform.position += (Vector3)vectorChange;

    }
    void FaceUnit(Vector2 unitPosition, Vector2 arrowPosition)
    {
        var AngleRad = Mathf.Atan2(unitPosition.y - arrowPosition.y, unitPosition.x - arrowPosition.x);
        float AngleDeg = (180 / Mathf.PI) * AngleRad;
        transform.rotation = Quaternion.Euler(0, 0, AngleDeg);
    }

}