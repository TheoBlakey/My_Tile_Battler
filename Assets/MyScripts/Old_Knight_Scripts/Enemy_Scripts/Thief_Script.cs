//using Pathfinding;
//using UnityEngine;

//public class Thief_Script : Utility_Functions
//{

//    public int Health { get; set; } = 5;
//    public float MovementSpeed { get; set; } = 1.2f;
//    public float PercentageOfSpeedLostWhenHit { get; set; } = 0f;
//    public int AmountOfTimeStoppedAfterHit { get; set; } = 1;

//    //Components
//    Knight_Script KnightScriptComponenet;
//    Rigidbody2D ThiefRigidBodyComponent;
//    FollowPath_Script ThiefFollowPath_Script;
//    Seeker ThiefSeekerComponent;
//    // End Components

//    public GameObject UpgradeRef;

//    public bool _hasStolenBow = false;
//    public bool CantStealBowBack = false;
//    public bool HasStolenBow
//    {
//        get { return this._hasStolenBow; }
//        set
//        {
//            SetChildActive(value, "StolenBow");
//            if (_hasStolenBow)
//            {
//                DamagesPlayerOnContact = false;

//            }
//            else
//            {
//                DamagesPlayerOnContact = true;
//            }
//            this._hasStolenBow = value;


//        }
//    }


//    void Start()
//    {
//        //Components
//        KnightScriptComponenet = GameObject.FindGameObjectWithTag("Player").GetComponent<Knight_Script>();
//        ThiefRigidBodyComponent = GetComponent<Rigidbody2D>();
//        ThiefFollowPath_Script = GetComponent<FollowPath_Script>();
//        ThiefSeekerComponent = GetComponent<Seeker>();
//        // End Components

//        ThiefFollowPath_Script.shouldFaceAim = false;
//        ThiefFollowPath_Script.RunawayRandomAmount = 1;
//        ThiefFollowPath_Script.Aim = FollowPath_Script.PathingAim.RunawayFromKnight;

//    }



//    void Update()
//    {
//        if (KnightScriptComponenet.UpgradeBow == false || CantStealBowBack)
//        {
//            ThiefFollowPath_Script.Aim = FollowPath_Script.PathingAim.RunawayFromKnight;
//            ThiefFollowPath_Script.shouldFaceAim = false;
//        }
//        else
//        {
//            ThiefFollowPath_Script.Aim = FollowPath_Script.PathingAim.ChaseKnight;
//            ThiefFollowPath_Script.shouldFaceAim = true;
//        }

//        // GameObject BowUpgrade = Instantiate(UpgradeRef, this.transform);
//        // BowUpgrade.name = "BowUpgrade";
//    }

//    private void OnTriggerEnter2D(Collider2D otherThing)
//    {

//        if (otherThing.tag == "Player")
//        {
//            if (KnightScriptComponenet.UpgradeBow == true && !HasStolenBow)
//            {
//                KnightScriptComponenet.UpgradeBow = false;
//                HasStolenBow = true;

//            }
//            else if (KnightScriptComponenet.UpgradeBow == false && HasStolenBow)
//            {

//                GameObject emptyGO = new GameObject();
//                Transform newTransform = new GameObject().transform;
//                newTransform.position = ThiefRigidBodyComponent.position;
//                GameObject BowUpgrade = Instantiate(UpgradeRef, newTransform);
//                BowUpgrade.name = "BowUpgrade";



//                HasStolenBow = false;
//                Invoke("ChangeBack", 2f);
//                CantStealBowBack = true;

//            }
//        }
//    }
//    void ChangeBack()
//    {
//        CantStealBowBack = false;
//    }


//}
