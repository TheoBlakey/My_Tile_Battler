using UnityEngine;

public class Arrow_Script : Utility_Functions
{
    Vector2 thrownDirection;
    public Rigidbody2D ArrowRigidBodyComponent;
    SpriteRenderer ArrowSpriteRenderer;

    bool moving = true;
    public bool IsEvilArrow = false;

    public Sprite SpriteEvilVersion;

    void Start()
    {

        ArrowRigidBodyComponent = GetComponent<Rigidbody2D>();
        float AngleRad;

        if (IsEvilArrow)
        {
            var KnightRigidBodyComponent = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D>();
            thrownDirection = (KnightRigidBodyComponent.position - ArrowRigidBodyComponent.position).normalized;
            AngleRad = Mathf.Atan2(KnightRigidBodyComponent.position.y - ArrowRigidBodyComponent.position.y, KnightRigidBodyComponent.position.x - ArrowRigidBodyComponent.position.x);


            SpriteRenderer ArrowSpriteRenderer = GetComponent<SpriteRenderer>();
            ArrowSpriteRenderer.sprite = SpriteEvilVersion;
            gameObject.AddComponent<DamagesPlayerOnContact_Component>();

        }
        else
        {
            Vector2 mouseScreenPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            thrownDirection = (mouseScreenPosition - ArrowRigidBodyComponent.position).normalized;
            AngleRad = Mathf.Atan2(mouseScreenPosition.y - ArrowRigidBodyComponent.position.y, mouseScreenPosition.x - ArrowRigidBodyComponent.position.x);
        }

        float AngleDeg = (180 / Mathf.PI) * AngleRad;
        this.transform.rotation = Quaternion.Euler(0, 0, AngleDeg);
    }

    // void Update()
    // {
    //     Debug.LogWarning("notconctime " + NonCollisionTime);
    //     if (NonCollisionTime > 0.0f)
    //     {
    //         NonCollisionTime -= Time.deltaTime;
    //     }

    // }

    void FixedUpdate()
    {
        if (moving == true)
        {
            float moveSpeed = 2.3f;
            ArrowRigidBodyComponent.MovePosition(ArrowRigidBodyComponent.position + (thrownDirection * moveSpeed) * Time.fixedDeltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D otherThing)
    {
        if (!moving)
        { return; }

        if (otherThing.TryGetComponent<ArmoredGoblin_Script>(out var a))
        {
            return;
        }
        if (otherThing.name == "ArrowBounce")
        {
            thrownDirection.x *= -1;
            thrownDirection.y *= -1;

            var AngleRad = Mathf.Atan2(thrownDirection.y, thrownDirection.x);
            float AngleDeg = (180 / Mathf.PI) * AngleRad;
            this.transform.rotation = Quaternion.Euler(0, 0, AngleDeg);
            return;
        }
        if (otherThing.name == "WeakSpot")
        {
            KnightArrowCollisions(otherThing.gameObject.transform.parent.gameObject);
            return;
        }




        if (otherThing.gameObject.tag == "StopArrow")
        {
            moving = false;

        }

        if (otherThing.TryGetComponent<BlockPathing_Script>(out var barrel) && moving)
        {
            barrel.GetComponent<DamagedByPlayer_Component>().TakeDamage();
            this.transform.parent = otherThing.transform;
            moving = false;
        }

        if (IsEvilArrow && moving)
        {
            GreenArrowCollisions(otherThing);

        }
        else if (moving)
        {
            KnightArrowCollisions(otherThing.gameObject);

        }

        if (!moving && TryGetComponent<DamagesPlayerOnContact_Component>(out var dam))
        {
            Destroy(dam);
        }
    }


    private void KnightArrowCollisions(GameObject otherThing)
    {
        if (otherThing.TryGetComponent<DamagedByPlayer_Component>(out var enemy))
        {
            this.transform.parent = otherThing.transform;
            enemy.TakeDamage();
            moving = false;
            return;

        }

    }

    private void GreenArrowCollisions(Collider2D otherThing)
    {
        if (otherThing.gameObject.name == "KnightShield")
        {
            moving = false;
            this.transform.parent = otherThing.transform;
            return;

        }

        if (otherThing.gameObject.TryGetComponent<Knight_Script>(out var knight) && !knight.GetComponent<PlayerHealth_Component>().GreyAfterDamage_Timed)
        {
            moving = false;
            this.transform.parent = otherThing.transform;
            if (knight.GetComponent<PlayerHealth_Component>().DamageFlashingOccuring_Timed)
            {
                Destroy(gameObject);
            }
            return;
        }

    }

}

