using UnityEngine;

public class Archer_Script : Utility_Functions
{

    //Components
    Knight_Script KnightScriptComponenet;
    Rigidbody2D ArcherRigidBodyComponent;
    FollowPath_Component followPath_Component;
    // End Components

    public GameObject ArrowRef;


    public bool ShotRecently_Timed = false;

    float previousUpdateKnightDistance = 0;

    Vector2 KnightLocationLastUpdate = new Vector2(0, 0);
    Vector2 ArcherLocationLastUpdate = new Vector2(0, 0);

    public bool RunningAwayWithReason_Timed = false;



    void Start()
    {
        FaceObject_Component faceObject_Component = gameObject.AddComponent<FaceObject_Component>();

        DamagesPlayerOnContact_Component damagesPlayerOnContact_Component = gameObject.AddComponent<DamagesPlayerOnContact_Component>();

        DamagedByPlayer_Component DamagedByPlayer_Component = gameObject.AddComponent<DamagedByPlayer_Component>();
        DamagedByPlayer_Component.Health = 2;

        followPath_Component = gameObject.AddComponent<FollowPath_Component>();
        followPath_Component.Aim = FollowPath_Component.PathingAim.ChaseKnight;
        followPath_Component.MovementSpeed = 1.1f;
        followPath_Component.RunawayRandomAmount = 1;

        KnightScriptComponenet = GameObject.FindGameObjectWithTag("Player").GetComponent<Knight_Script>();
        ArcherRigidBodyComponent = GetComponent<Rigidbody2D>();

    }

    void Update()
    {

        bool CanSeeKnight = !Physics2D.Linecast(ArcherRigidBodyComponent.position, GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D>().position, 64);

        if (CanSeeKnight && !ShotRecently_Timed)
        {
            ActivateTimerOnBool(0.5f, nameof(ShotRecently_Timed));
            GameObject ArrowObj = Instantiate(ArrowRef, ArcherRigidBodyComponent.position, new Quaternion());
            ArrowObj.GetComponent<Arrow_Script>().IsEvilArrow = true;
        }


        float currentKnightDistance = Vector2.Distance(ArcherRigidBodyComponent.position, KnightScriptComponenet.KnightRigidBodyComponent.position);

        bool RunningAwayAndKnightStillCloser = false;
        if (RunningAwayWithReason_Timed && currentKnightDistance < previousUpdateKnightDistance)
        {
            RunningAwayAndKnightStillCloser = true;
        }


        float lastDistance = Vector2.Distance(ArcherLocationLastUpdate, KnightLocationLastUpdate);
        float CurrentDistance = Vector2.Distance(ArcherLocationLastUpdate, KnightScriptComponenet.KnightRigidBodyComponent.position);

        bool KnightMovingToGetCloser = false;
        if (CurrentDistance < lastDistance)
        {
            KnightMovingToGetCloser = true;
        }

        KnightLocationLastUpdate = KnightScriptComponenet.KnightRigidBodyComponent.position;
        ArcherLocationLastUpdate = ArcherRigidBodyComponent.position;


        if (CanSeeKnight || RunningAwayAndKnightStillCloser || KnightMovingToGetCloser)
        {
            ActivateTimerOnBool(0.4f, nameof(RunningAwayWithReason_Timed));
        }

        if (RunningAwayWithReason_Timed)
        {
            followPath_Component.Aim = FollowPath_Component.PathingAim.RunawayFromKnight;
            previousUpdateKnightDistance = currentKnightDistance;

        }
        else
        {
            followPath_Component.Aim = FollowPath_Component.PathingAim.ChaseKnight;
        }



        //end of update for new round
        previousUpdateKnightDistance = currentKnightDistance;


    }

}
