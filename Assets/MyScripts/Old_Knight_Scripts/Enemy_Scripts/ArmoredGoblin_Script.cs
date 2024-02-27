public class ArmoredGoblin_Script : Utility_Functions
{
    public enum LevelTypes
    { Big, SparkleFast }
    //Components

    // End Components

    void Start()
    {
        FaceObject_Component faceObject_Component = gameObject.AddComponent<FaceObject_Component>();
        faceObject_Component.ShouldTurnSlowly = true;

        DamagesPlayerOnContact_Component damagesPlayerOnContact_Component = gameObject.AddComponent<DamagesPlayerOnContact_Component>();

        DamagedByPlayer_Component DamagedByPlayer_Component = gameObject.AddComponent<DamagedByPlayer_Component>();
        DamagedByPlayer_Component.Health = 3;

        FollowPath_Component followPath_Component = gameObject.AddComponent<FollowPath_Component>();
        followPath_Component.Aim = FollowPath_Component.PathingAim.ChaseKnight;
        followPath_Component.MovementSpeed = 0.4f;
        followPath_Component.PercentageOfSpeedLostWhenHit = 0.4f;
        followPath_Component.amoutOfTimePausedAfterHit = 1;
    }


}
