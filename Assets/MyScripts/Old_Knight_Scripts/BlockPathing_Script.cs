using UnityEngine;

public class BlockPathing_Script : Utility_Functions
{
    //     private int _health = 2; //SET HERE
    //     public int Health
    //     {
    //         get
    //         { return this._health; }
    //         set
    //         {
    //             this._health = value;
    //             if (_health == 0)
    //             {
    //                 foreach (Transform child in this.transform)
    //                 {
    //                     child.gameObject.layer = LayerMask.NameToLayer("Default");
    //                 }

    //                 gameObject.layer = LayerMask.NameToLayer("Default");
    //                 // AstarPath.active.Scan();

    //                 Bounds bounds = GetComponent<CapsuleCollider2D>().bounds;

    //                 bounds.extents = bounds.extents * 2;

    //                 AstarPath.active.UpdateGraphs(bounds);
    //                 // AstarPath.active.FlushGraphUpdates();

    //                 // FollowPath_Script[] yourObjects = FindObjectsOfType<FollowPath_Script>();
    //                 // foreach (FollowPath_Script o in yourObjects)
    //                 // {
    //                 //     o.StuckSoRandomPathing = false;
    //                 //     o.immediateStop = true;
    //                 //     o.BuildAPath();
    //                 //     o.immediateStop = false;
    //                 //     Debug.LogWarning("OI, buildApath!");
    //                 // };




    //                 // var graphToScan = AstarPath.active.data.gridGraph;
    //                 // AstarPath.active.Scan(graphToScan);


    //                 // AstarPath.active.ScanAsync();
    //             }
    //         }
    //     }

    // void Start()
    // {

    // }
    // void FixedUpdate()
    // {

    // }

    public void StopBlockingPathing()
    {
        foreach (Transform child in this.transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Default");
        }
        gameObject.layer = LayerMask.NameToLayer("Default");
        // AstarPath.active.Scan();

        Bounds bounds = GetComponent<CapsuleCollider2D>().bounds;
        bounds.extents = bounds.extents * 2;
        AstarPath.active.UpdateGraphs(bounds);

    }

}
