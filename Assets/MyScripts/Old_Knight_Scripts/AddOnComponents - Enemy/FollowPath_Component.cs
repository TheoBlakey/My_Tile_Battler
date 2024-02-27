using Pathfinding;
using System.Collections.Generic;
using UnityEngine;

public class FollowPath_Component : Utility_Functions
{
    public float MovementSpeed = 2f; //CAN BE SET
    public float amoutOfTimePausedAfterHit = 0.2f; // CAN BE SET
    public bool CanMoveThroughEnemies = false; // CAN BE SET
    public float PercentageOfSpeedLostWhenHit = 0.2f; // CAN BE SET


    //Components
    private Knight_Script KnightScriptComponenet;
    private Rigidbody2D GenericRigidBodyComponent;
    private Seeker GenericSeekerComponent;
    // End Components

    private Vector2 GlobalRandomLocation = new Vector2(0, 0);
    private Path pathToFollow;
    private int currentWayPointNumber = 0;
    public bool immediateStop = false;

    public bool PausedAfterDamage_Timed = false;
    public bool RecentlyMovedSucessfully_Timed = false;
    public bool StuckSoRandomPathing_Timed = false;

    private Vector2 StartLocation = new(0, 0);
    private Vector2 MeanderToLocation = new(0, 0);
    private int TimeLeftOnMeaderLocation = 0;

    private PathingAim _aim = PathingAim.None;
    public PathingAim Aim
    {
        get { return _aim; }
        set
        {
            if (_aim != value)
            {
                AimHasChanged = true;
            }
            _aim = value;
        }
    }

    public float RunawayRandomAmount = 0;
    int SafetyTryNumber = 0;
    private bool AimHasChanged = false;

    public enum PathingAim
    { None, ChaseKnight, RunawayFromKnight, ChaseAnotherObject, MeanderNearStart }

    public GameObject ObjectToChase;

    private int EnemyOnlyMask;
    private int WallsEtcMask;
    private GameObject movementOrb;

    public void Hit()
    {
        float divisionNumber = 1 - PercentageOfSpeedLostWhenHit;
        MovementSpeed = MovementSpeed * divisionNumber;

        if (PausedAfterDamage_Timed)
        {
            return;
        }

        ActivateTimerOnBool(amoutOfTimePausedAfterHit, nameof(PausedAfterDamage_Timed));
    }

    GameObject inTheWayPathingObject;

    // public bool SeekerIsBusy()
    // {
    //     if (StuckSoRandomPathing || Aim != PathingAim.None)
    //     { return true; }
    //     else
    //     { return false; }
    // }

    // public void AddNewPathToFollow(Path newPathToFollow)
    // {
    //     if (!StuckSoRandomPathing)
    //     {
    //         pathToFollow = newPathToFollow;
    //         currentWayPointNumber = 0;
    //     }

    // }

    void Start()
    {

        //Components
        GenericRigidBodyComponent = GetComponent<Rigidbody2D>();
        KnightScriptComponenet = GameObject.FindGameObjectWithTag("Player").GetComponent<Knight_Script>();
        //GenericSeekerComponent = GetComponent<Seeker>();

        GenericSeekerComponent = gameObject.AddComponent<Seeker>();
        // End Components

        InvokeRepeating(nameof(BuildAPath_Repeating), 0f, 0.3f);

        int WallsAndBarrels = 1 << LayerMask.NameToLayer("WallsAndBarrels");
        int EnemyOnlyCollider = 1 << LayerMask.NameToLayer("EnemyOnlyCollider");

        WallsEtcMask = WallsAndBarrels + EnemyOnlyCollider;
        EnemyOnlyMask = 1 << LayerMask.NameToLayer("Enemy");

        movementOrb = GetChildByName("MovementOrb");
    }

    public void BuildAPath_Repeating()
    {
        if (TryGetComponent<FaceObject_Component>(out FaceObject_Component component) && component.DontTurn_Timer)
        {
            return;
        }

        if (StuckSoRandomPathing_Timed)
        {
            GenericSeekerComponent.StartPath(GenericRigidBodyComponent.position, GlobalRandomLocation, OnPathBuilt);
            return;
        }

        switch (Aim)
        {
            case PathingAim.None:
                {
                    pathToFollow = null;
                    break;
                }
            case PathingAim.ChaseKnight:
                {
                    GenericSeekerComponent.StartPath(GenericRigidBodyComponent.position, KnightScriptComponenet.KnightRigidBodyComponent.position, OnPathBuilt);
                    break;
                }
            case PathingAim.RunawayFromKnight:
                {
                    FindRunawayPath();
                    break;
                }
            case PathingAim.ChaseAnotherObject:
                {
                    if (ObjectToChase != null)
                    {
                        GenericSeekerComponent.StartPath(GenericRigidBodyComponent.position, ObjectToChase.GetComponent<Rigidbody2D>().position, OnPathBuilt);

                    }
                    break;
                }
            case PathingAim.MeanderNearStart:
                {
                    var braveryDistance = 1;

                    var distance = Vector3.Distance(StartLocation, MeanderToLocation);

                    if (TimeLeftOnMeaderLocation == 0 || distance < 1)
                    {
                        TimeLeftOnMeaderLocation = 20;
                        MeanderToLocation = new Vector2(Random.Range(StartLocation.x - braveryDistance, StartLocation.x + braveryDistance), Random.Range(StartLocation.y - braveryDistance, StartLocation.y + braveryDistance));
                    }
                    else
                    {
                        TimeLeftOnMeaderLocation--;
                    }

                    GenericSeekerComponent.StartPath(GenericRigidBodyComponent.position, MeanderToLocation, OnPathBuilt);
                    break;
                }
            default: break;
        }
        AimHasChanged = false;

    }

    void FindRunawayPath()
    {
        GenericSeekerComponent.StartPath(GenericRigidBodyComponent.position, RandomPointInDungeon(), OnRandomPathBuilt);
    }

    void OnRandomPathBuilt(Path TempPath)
    {
        if (!TempPath.error)
        {
            return;
        }

        float currentKnightDistance = Vector2.Distance(GenericRigidBodyComponent.position, KnightScriptComponenet.KnightRigidBodyComponent.position);
        float proposedKnightDistance = Vector2.Distance(TempPath.vectorPath[1], KnightScriptComponenet.KnightRigidBodyComponent.position);

        if (proposedKnightDistance > currentKnightDistance || (Random.Range(0, RunawayRandomAmount) + currentKnightDistance) > RunawayRandomAmount * 2)
        {
            pathToFollow = TempPath;
            currentWayPointNumber = 0;
        }
        else if (SafetyTryNumber < 10)
        {
            FindRunawayPath();
            SafetyTryNumber++;
        }

        SafetyTryNumber = 0;

    }

    void OnPathBuilt(Path TempPath)
    {
        if (TempPath.error)
        {
            return;
        }

        pathToFollow = TempPath;
        currentWayPointNumber = 0;

    }

    void FixedUpdate()
    {
        if (PausedAfterDamage_Timed || immediateStop || AimHasChanged)
        {
            return;
        }

        if (pathToFollow != null && currentWayPointNumber < pathToFollow.vectorPath.Count)
        {
            TryToFollowPath();
        }

    }

    private void TryToFollowPath()
    {
        Vector2 CurrentPathPoint = pathToFollow.vectorPath[currentWayPointNumber];
        Vector2 direction = (CurrentPathPoint - GenericRigidBodyComponent.position).normalized;

        bool skipEnemyCollisions = false;

        if (StuckSoRandomPathing_Timed || CanMoveThroughEnemies)
        {
            skipEnemyCollisions = true;
        }


        TryMoveVaryingSpeeds(direction, skipEnemyCollisions);
        //64 = walls, 128 = enemies

        if (!RecentlyMovedSucessfully_Timed && !StuckSoRandomPathing_Timed)
        {
            //var dir = new Vector2(0, 0);
            //dir.x = GenericRigidBodyComponent.position.x - otherPathingObject.GetComponent<Rigidbody2D>().position.x;
            //dir.y = GenericRigidBodyComponent.position.y - otherPathingObject.GetComponent<Rigidbody2D>().position.y;
            //dir = dir.normalized;

            //GlobalRandomLocation = GenericRigidBodyComponent.position + dir * 3;


            GlobalRandomLocation = RandomPointInDungeon();
            ActivateTimerOnBool(0.4f, nameof(StuckSoRandomPathing_Timed));
            //ActivateTimerOnBool(0.4f, nameof(StuckSoRandomPathing_Timed));

            //var safeCount = 0;
            //int degree = 0;
            //while (!RecentlyMovedSucessfully_Timed && safeCount < 4 && degree < 360)
            //{
            //    var angleDirection = Vector2FromAngle(degree);

            //    var small = 0.2f;
            //    angleDirection = angleDirection.normalized * small;
            //    TryMoveVaryingSpeeds(angleDirection, 128);

            //    degree += 45;
            //    safeCount++;
            //}


        }

        if (Vector2.Distance(GenericRigidBodyComponent.position, CurrentPathPoint) <= 0.1)
        {
            currentWayPointNumber++;
        }
    }

    public Vector2 Vector2FromAngle(float a)
    {
        a *= Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(a), Mathf.Sin(a));
    }

    private void TryMoveVaryingSpeeds(Vector2 direction, bool skipEnemyCollisions)
    {

        double movementSpeed = MovementSpeed;
        int iterations = 8; //(CLUMP LEVEL OF ENEMIES) OG 4
        bool successMovement = false;
        while (!successMovement && iterations > 0)
        {
            successMovement = TryMoveOneSpeed(direction, skipEnemyCollisions, movementSpeed);
            iterations--;
            movementSpeed = movementSpeed / 2;
        }

        if (successMovement)
        {
            ActivateTimerOnBool(0.2f, nameof(RecentlyMovedSucessfully_Timed));
            inTheWayPathingObject = null;
        }

    }

    private bool TryMoveOneSpeed(Vector2 direction, bool skipEnemyCollisions, double movementSpeedDub)
    {
        List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();
        float movementSpeed = (float)movementSpeedDub;

        ContactFilter2D contactFilter2D = new ContactFilter2D();
        contactFilter2D.useTriggers = true;

        int castCountForBody = 0;
        if (!skipEnemyCollisions)
        {
            contactFilter2D.SetLayerMask(EnemyOnlyMask);
            castCountForBody = GenericRigidBodyComponent.Cast(
                    direction,
                    contactFilter2D,
                    castCollisions,
                    movementSpeed * Time.fixedDeltaTime
                    );
        }

        contactFilter2D.SetLayerMask(WallsEtcMask);
        int castCountForMovementOrb = movementOrb.GetComponent<Rigidbody2D>().Cast(
               direction,
               contactFilter2D,
               castCollisions,
               (movementSpeed * Time.fixedDeltaTime) / 2
               );

        if (castCountForBody == 0 && castCountForMovementOrb == 0)
        {
            GenericRigidBodyComponent.MovePosition(GenericRigidBodyComponent.position + direction * movementSpeed * Time.fixedDeltaTime);
            return true;

        }
        else
        {

            //var possibleObject = castCollisions[0].transform.gameObject;
            //if (possibleObject != null && possibleObject.TryGetComponent<FollowPath_Component>(out var x))
            //{
            //    otherPathingObject = possibleObject;
            //}

            return false;
        }

    }

    private Vector2 RandomPointInDungeon()
    {
        return new Vector2(Random.Range(0f, 12f), Random.Range(0f, 12f));
    }



}
