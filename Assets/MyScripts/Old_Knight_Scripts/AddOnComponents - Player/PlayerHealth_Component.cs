using UnityEngine;

public class PlayerHealth_Component : Utility_Functions
{
    private Knight_Script Knight_Script;
    private SpriteRenderer KnightSpriteRenderer;

    public bool IsInvunerable = false;


    private System.Collections.Generic.List<GameObject> HeartsList = new();

    private int _maxHealth = 2;
    public int MaxHealth // CAN BE SET //////////////////////////
    {
        get { return _maxHealth; }
        set
        {
            if (value % 2 != 0)
            {
                value--;
            }

            foreach (var heart in HeartsList)
            {
                Destroy(heart);
            }

            float offsetX = 0;
            float amontOfHeatOffSet = 0.2f;


            for (int i = 0; i < value / 2; i++)
            {

                Vector2 heartLocation = Knight_Script.GetComponent<Rigidbody2D>().position;
                heartLocation.y += 0.9f;
                heartLocation.x += -2.1f;

                heartLocation.x += offsetX;

                var heart = Instantiate(Knight_Script.HeartObjectRef, heartLocation, new Quaternion());
                heart.transform.parent = this.transform;
                HeartsList.Add(heart);

                offsetX += amontOfHeatOffSet;
            }

            _maxHealth = value;
            Health = Health;

        }
    }

    private int _health = 1;
    public int Health // CAN BE SET //////////////////////////
    {
        get { return _health; }
        set
        {

            int countHealth = value;
            foreach (var heart in HeartsList)
            {
                var heartSpriteRenderer = heart.GetComponent<SpriteRenderer>();
                //heartSprite = Knight_Script.EmptyHeartSprite;

                if (countHealth >= 2)
                {
                    heartSpriteRenderer.sprite = Knight_Script.FullHeartSprite;
                    countHealth -= 2;
                }
                else if (countHealth == 1)
                {
                    heartSpriteRenderer.sprite = Knight_Script.HalfHeartSpirte;
                    countHealth--;
                }
                else if (countHealth == 0)
                {
                    heartSpriteRenderer.sprite = Knight_Script.EmptyHeartSprite;
                }
            }

            if (_health <= 0)
            {
            }
            _health = value;
        }
    }

    private bool _greyAfterDamage_Timed = false;
    public bool GreyAfterDamage_Timed
    {
        get { return this._greyAfterDamage_Timed; }
        set
        {
            this._greyAfterDamage_Timed = value;
            if (_greyAfterDamage_Timed)
            {
                KnightSpriteRenderer.color = new Color(1, 1, 1, 0.4f);
            }
            else
            {
                KnightSpriteRenderer.color = new Color(1, 1, 1, 1);
                IsInvunerable = false;
            }
        }
    }

    private bool _damageFlashingOccuring_Timed = false;
    public bool DamageFlashingOccuring_Timed
    {
        get { return this._damageFlashingOccuring_Timed; }
        set
        {
            this._damageFlashingOccuring_Timed = value;
            if (_damageFlashingOccuring_Timed)
            {
                InvokeRepeating(nameof(FlashDamagedColor_Repeating), 0, 0.1f);
                Knight_Script.InvokeRepeatingDifferentClass(nameof(Knight_Script.DamagePanicMovement_Repeating), 0f, 0.1f);
                //InvokeRepeating(nameof(Knight_Script.DamagePanicMovement_Repeating), 0f, 0.1f);
            }
            else
            {
                CancelInvoke(nameof(FlashDamagedColor_Repeating));
                Knight_Script.CancelInvokeDifferentClass(nameof(Knight_Script.DamagePanicMovement_Repeating));
                //CancelInvoke(nameof(Knight_Script.DamagePanicMovement_Repeating));

                ActivateTimerOnBool(1.5f, nameof(GreyAfterDamage_Timed));

                changeHeartColor(new Color(1, 1, 1, 1));
            }
        }
    }

    void Start()
    {
        Knight_Script = GetComponent<Knight_Script>();
        KnightSpriteRenderer = GetComponent<SpriteRenderer>();

        MaxHealth = 8; //STARTING MAKE HEALTH
        Health = MaxHealth;

    }

    private void OnTriggerEnter2D(Collider2D otherThing)
    {
        if (!IsInvunerable && otherThing.TryGetComponent<DamagesPlayerOnContact_Component>(out var DamagingObject) && DamagingObject.CurrentlyDamaging)
        {
            Health -= DamagingObject.AmountOfDamage;

            IsInvunerable = true;

            Knight_Script.CancelInvokeDifferentClass(nameof(Knight_Script.ShootArrow_Repeating));

            //CancelInvoke(nameof(Knight_Script.ShootArrow_Repeating));
            ActivateTimerOnBool(1f, nameof(DamageFlashingOccuring_Timed));

            Knight_Script.damagedByDirection = (GetComponent<Rigidbody2D>().position - otherThing.gameObject.GetComponent<Rigidbody2D>().position).normalized;

            ActivateTimerOnBool(0.1f, nameof(Knight_Script.ExperiencingKnockBack_Timed));

            GetComponent<Animator>().SetBool("isKnightMoving", true);

        }

    }

    void FlashDamagedColor_Repeating()
    {
        Color DamageFlashColor = new Color32(255, 51, 0, 255);

        if (KnightSpriteRenderer.color == DamageFlashColor)
        {
            KnightSpriteRenderer.color = new Color(1, 1, 1, 1);
            changeHeartColor(new Color(1, 1, 1, 1));
            return;
        }

        KnightSpriteRenderer.color = DamageFlashColor;
        changeHeartColor(DamageFlashColor);

    }

    void changeHeartColor(Color changeColor)
    {
        foreach (var heart in HeartsList)
        {
            var heartSpriteRenderer = heart.GetComponent<SpriteRenderer>();
            heartSpriteRenderer.color = changeColor;
        }
    }

}


