using UnityEngine;

public class FaceObject_Component : Utility_Functions
{
    public bool ShouldTurnSlowly = false; //CAN BE SET ELSEWHERE

    private Rigidbody2D GenericRigidBodyComponent;
    private FollowPath_Component followPath_Component;
    private Knight_Script KnightScriptComponenet;

    private bool lookingRight = true;
    private bool shouldFaceAim = true;

    public bool DontTurn_Timer = false;
    private bool WaitingToTurn = false;

    void Start()
    {
        GenericRigidBodyComponent = GetComponent<Rigidbody2D>();
        followPath_Component = GetComponent<FollowPath_Component>();
        KnightScriptComponenet = GameObject.FindGameObjectWithTag("Player").GetComponent<Knight_Script>();
    }
    void FixedUpdate()
    {

        float PoistionOfAimX = 0;
        var Aim = followPath_Component.Aim;

        if (Aim == FollowPath_Component.PathingAim.ChaseKnight || Aim == FollowPath_Component.PathingAim.RunawayFromKnight)
        {
            PoistionOfAimX = KnightScriptComponenet.KnightRigidBodyComponent.position.x;
        }
        else if (Aim == FollowPath_Component.PathingAim.ChaseAnotherObject)
        {
            PoistionOfAimX = followPath_Component.ObjectToChase.GetComponent<Rigidbody2D>().position.x;
        }


        if (GenericRigidBodyComponent.position.x > PoistionOfAimX) //Knight is to the Left
        {
            if ((lookingRight && shouldFaceAim) || (!lookingRight && !shouldFaceAim))
            {
                PerformTurn();
            }
        }
        else if (GenericRigidBodyComponent.position.x < PoistionOfAimX) //Knight is to the Right
        {
            if ((!lookingRight && shouldFaceAim) || (lookingRight && !shouldFaceAim))
            {
                PerformTurn();
            }
        }

    }

    private void PerformTurn()
    {
        if (DontTurn_Timer)
        {
            return;
        }

        if (ShouldTurnSlowly)
        {
            if (!WaitingToTurn)
            {
                WaitingToTurn = true;
                ActivateTimerOnBool(1.2f, nameof(DontTurn_Timer));
                return;
            }

            WaitingToTurn = false;
        }

        transform.RotateAround(transform.position, transform.up, 180f);
        lookingRight = !lookingRight;

    }

}
