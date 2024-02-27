using UnityEngine;

public class DamagedByPlayer_Component : Utility_Functions
{
    private SpriteRenderer GenericSpriteRendererComponent;
    private Color CurrentColorLevel;
    private Color OrignalColor;
    private int MaxHealth;

    private bool shouldDieAferRedFlash = false;

    private bool _isRed_Timed;
    public bool IsRed_Timed
    {
        get { return this._isRed_Timed; }
        set
        {
            this._isRed_Timed = value;
            if (_isRed_Timed)
            {
                Color RedColor = new Color(1.0f, 0.0f, 0.0f, 0.7f);
                GenericSpriteRendererComponent.color = RedColor;
                SetAllChildrenToColor(RedColor);

            }
            else
            {
                GenericSpriteRendererComponent.color = CurrentColorLevel;
                SetAllChildrenToColor(CurrentColorLevel);

                if (shouldDieAferRedFlash)
                {
                    Invoke(nameof(TriggerDeath), 0.05f);
                }
            };
        }
    }

    public int _health = 3;
    public int Health // CAN BE SET //////////////////////////
    {
        get { return this._health; }
        set
        {
            this._health = value;
            if (value <= 0)
            {
                TriggerDeath();
                //shouldDieAferRedFlash = true;
            }
            if (value > MaxHealth)
            {
                MaxHealth = _health;
            }
        }
    }

    private void TriggerDeath()
    {
        if (TryGetComponent<BlockPathing_Script>(out var blocker))
        {
            blocker.StopBlockingPathing();
        }

        Destroy(gameObject);
    }

    void Start()
    {
        GenericSpriteRendererComponent = GetComponent<SpriteRenderer>();
        MaxHealth = Health;

        CurrentColorLevel = GenericSpriteRendererComponent.color;

        Invoke(nameof(GetOriginalColor), 0.5f);
    }

    void GetOriginalColor()
    {
        OrignalColor = GenericSpriteRendererComponent.color;
    }

    public void TakeDamage(int amount = 1)
    {
        Health = Health - amount;

        if (!IsRed_Timed)
        {
            ActivateTimerOnBool(0.15f, nameof(IsRed_Timed));
        }

        float HealthRatio = (float)Health / (float)MaxHealth;
        float lessChange = (HealthRatio / 2) + 0.5f;

        CurrentColorLevel = new Color(OrignalColor.r, OrignalColor.g, OrignalColor.b, lessChange);
        //GenericSpriteRendererComponent.color = CurrentColorLevel;
        //SetAllChildrenToColor(CurrentColorLevel);

        if (TryGetComponent<FollowPath_Component>(out FollowPath_Component followPath_Script))
        {
            followPath_Script.Hit();
        }
    }

    public void HealToMax()
    {
        // Health = startingHealth;
        // CurrentColorLevel = new Color(1, 1, 1, 1);
        // GenericSpriteRendererComponent.color = CurrentColorLevel;
        // SetAllChildrenToColor(CurrentColorLevel);
        // followPath_Script.MovementSpeed = startingSpeed;
    }


    void SetAllChildrenToColor(Color color)
    {
        foreach (Transform child in this.transform)
        {
            if (!child.gameObject.TryGetComponent<Arrow_Script>(out var a) && child.gameObject.TryGetComponent<SpriteRenderer>(out var com))
            {
                com.color = color;
            }
        }
    }
}
