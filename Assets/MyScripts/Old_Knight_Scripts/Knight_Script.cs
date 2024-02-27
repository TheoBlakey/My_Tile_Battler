using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Knight_Script : Utility_Functions
{
    //Components
    InputAction FireAction;
    public Rigidbody2D KnightRigidBodyComponent;
    Animator KnightAnimatorComponent;
    SpriteRenderer KnightSpriteRendererComponent;
    PlayerInput KnightPlayerInputComponent;
    Collider2D KnightCollider;
    // End Components

    public GameObject ArrowRef;

    Vector2 MovementInput;

    private Vector2 moveLeftandRight = new(1, 0);

    public bool ExperiencingKnockBack_Timed = false;
    public Vector2 damagedByDirection;

    bool lookingRight = true;

    //Health Refs
    public GameObject HeartObjectRef;

    public Sprite FullHeartSprite;
    public Sprite HalfHeartSpirte;
    public Sprite EmptyHeartSprite;
    //End Health Refs

    private int mask64;

    private void OnTriggerEnter2D(Collider2D otherThing)
    {

        if (otherThing.name == "Ladder")
        {

            int CurrentSceneNumber = SceneManager.GetActiveScene().buildIndex;

            SceneManager.LoadScene(CurrentSceneNumber + 1);
            if (CurrentSceneNumber < SceneManager.sceneCount)
            {
                // SceneManager.LoadScene("Scene 3");
            }

        }

        //if (otherThing.tag == "Upgrade" && otherThing.name == "BowUpgrade")
        //{
        //    Destroy(otherThing.gameObject);
        //}
    }

    public void DamagePanicMovement_Repeating()
    {
        float moveSpeed = 1f; //push back strength
        moveLeftandRight.x *= -1;
        TryMove(moveLeftandRight, moveSpeed);
    }

    void Start()
    {
        //Components
        KnightRigidBodyComponent = GetComponent<Rigidbody2D>();
        KnightAnimatorComponent = GetComponent<Animator>();
        KnightSpriteRendererComponent = GetComponent<SpriteRenderer>();
        KnightPlayerInputComponent = GetComponent<PlayerInput>();
        KnightCollider = GetComponent<Collider2D>();

        gameObject.AddComponent<PlayerHealth_Component>();
        // End Components

        FireAction = KnightPlayerInputComponent.actions["Fire"];
        FireAction.started += FireActionPressed;
        FireAction.canceled += FireActionReleased;

        mask64 = 1 << LayerMask.NameToLayer("WallsAndBarrels");

    }
    void OnDisable()
    {
        FireAction.started -= FireActionPressed;
        FireAction.canceled -= FireActionReleased;
    }
    public void FireActionPressed(InputAction.CallbackContext context)
    {
        if (GetComponent<PlayerHealth_Component>().IsInvunerable)
        {
            return;
        }
        var fireRate = 0.2f;
        InvokeRepeating(nameof(ShootArrow_Repeating), 0f, fireRate);
    }
    void FireActionReleased(InputAction.CallbackContext context)
    {
        CancelInvoke(nameof(ShootArrow_Repeating));
    }

    void OnMove(InputValue movementValue)
    {
        MovementInput = movementValue.Get<Vector2>();
    }

    private void FixedUpdate()
    {
        bool check1 = MovementInput.x < 0 && lookingRight;
        bool check2 = MovementInput.x > 0 && !lookingRight;
        if (!IsInvoking(nameof(ShootArrow_Repeating)) && (check1 || check2))
        {
            FlipSprites();
        }

        if (ExperiencingKnockBack_Timed)
        {
            float pushBackStrength = 2f; //push back strength
            TryMove(damagedByDirection, pushBackStrength);
            return;
        }

        if (IsInvoking(nameof(DamagePanicMovement_Repeating)) || MovementInput == Vector2.zero)
        {
            KnightAnimatorComponent.SetBool("isKnightMoving", false);
            return;
        }


        float moveSpeed = 1f; //SET HERE

        var successOfMovement = TryMove(MovementInput, moveSpeed);

        if (!successOfMovement)
        {
            successOfMovement = TryMove(new Vector2(MovementInput.x, 0), moveSpeed);
        }
        if (!successOfMovement)
        {
            successOfMovement = TryMove(new Vector2(0, MovementInput.y), moveSpeed);
        }

        KnightAnimatorComponent.SetBool("isKnightMoving", successOfMovement);

    }

    private bool TryMove(Vector2 direction, float MovementSpeed)
    {
        List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();
        ContactFilter2D contactFilter2D = new ContactFilter2D();

        //contactFilter2D.SetLayerMask(64);

        contactFilter2D.SetLayerMask(mask64);

        contactFilter2D.useTriggers = true;

        float CollisionOffSet = 0f;

        int castCount = KnightCollider.Cast(
                direction,
                contactFilter2D,
                castCollisions,
                MovementSpeed * Time.fixedDeltaTime + CollisionOffSet,
                true
                );

        if (castCount == 0)
        {
            KnightRigidBodyComponent.MovePosition(KnightRigidBodyComponent.position + direction * MovementSpeed * Time.fixedDeltaTime);
            return true;
        }
        else
        {
            return false;
        }
    }

    void OnFire()
    {
        // GameObject emptyGO = new GameObject();
        // Transform newTransform = new GameObject().transform;

    }

    public void ShootArrow_Repeating()
    {
        Instantiate(ArrowRef, KnightRigidBodyComponent.position, new Quaternion());

        bool check1 = KnightRigidBodyComponent.position.x > Camera.main.ScreenToWorldPoint(Input.mousePosition).x && lookingRight;
        bool check2 = KnightRigidBodyComponent.position.x < Camera.main.ScreenToWorldPoint(Input.mousePosition).x && !lookingRight;
        if (check1 || check2)
        {
            FlipSprites();
        }

    }

    void FlipSprites()
    {
        lookingRight = !lookingRight;
        transform.RotateAround(transform.position, transform.up, 180f);

        foreach (Transform g in transform.GetComponentsInChildren<Transform>())
        {
            if (g.name == "KnightCamera" || g.name == "Heart(Clone)")
            {
                g.GetComponent<Transform>().RotateAround(transform.position, transform.up, 180f);
            }
        }
    }


}
