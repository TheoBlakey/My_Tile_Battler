using UnityEngine;

public class Goblin_Script : Utility_Functions
{

    public GameObject SparkeRef;


    private int startingHealth = 3;
    private float startingMovementSpped = 1.1f;

    private float PercentageOfSpeedLostWhenHit = -1;
    private float amoutOfTimePausedAfterHit = -1;

    public enum GoblinLevelTypes
    { Big, SparkleFast }

    public override void SetLevel(string levelName)
    {
        if (string.IsNullOrEmpty(levelName) || levelName == "NORMAL")
        {
            return;
        }

        GoblinLevelTypes levelType = ParseEnum<GoblinLevelTypes>(levelName);

        switch (levelType)
        {
            case GoblinLevelTypes.Big:
                //SparkeRef = Resources.Load("Assets/Objects/Sparkle/Sparkle.prefab") as GameObject;

                startingHealth = 8;
                startingMovementSpped = 0.9f;
                float sizeScale = 2;
                this.transform.localScale = new Vector3(sizeScale, sizeScale, 0f);
                GetComponent<SpriteRenderer>().color = new Color(0.8f, 1, 0.8f, 1);
                amoutOfTimePausedAfterHit = 0;
                PercentageOfSpeedLostWhenHit = 0.05f;

                break;

            case GoblinLevelTypes.SparkleFast:
                var sparkel = Instantiate(SparkeRef, GetComponent<Rigidbody2D>().position, new Quaternion());
                sparkel.transform.parent = this.transform;
                startingMovementSpped = 1.5f;
                PercentageOfSpeedLostWhenHit = 0;

                break;
        }

    }


    void Start()
    {

        if (TryGetComponent<Level_Holder>(out var holder))
        {
            SetLevel(holder.StringLevel);
        }

        FaceObject_Component faceObject_Component = gameObject.AddComponent<FaceObject_Component>();
        DamagesPlayerOnContact_Component damagesPlayerOnContact_Component = gameObject.AddComponent<DamagesPlayerOnContact_Component>();

        DamagedByPlayer_Component DamagedByPlayer_Component = gameObject.AddComponent<DamagedByPlayer_Component>();
        DamagedByPlayer_Component.Health = startingHealth;

        FollowPath_Component followPath_Component = gameObject.AddComponent<FollowPath_Component>();
        followPath_Component.Aim = FollowPath_Component.PathingAim.ChaseKnight;
        followPath_Component.MovementSpeed = startingMovementSpped;

        if (PercentageOfSpeedLostWhenHit != -1)
        {
            followPath_Component.PercentageOfSpeedLostWhenHit = PercentageOfSpeedLostWhenHit;
        }
        if (amoutOfTimePausedAfterHit != -1)
        {
            followPath_Component.amoutOfTimePausedAfterHit = amoutOfTimePausedAfterHit;
        }

    }


}
