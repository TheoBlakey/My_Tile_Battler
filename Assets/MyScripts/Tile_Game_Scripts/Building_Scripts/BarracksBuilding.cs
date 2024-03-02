using System.Collections;
using UnityEngine;

class BarracksBuilding : BuildingBase
{
    public override string SpriteLandName => "BarracksBuilding";
    public bool busy = false;
    bool readyToCreate = false;

    public void BeginArcherCreation()
    {
        shadedOutComponent.ShadedOut = true;
        StartCoroutine(BuildArcher());
    }
    IEnumerator BuildArcher()
    {
        busy = true;
        shadedOutComponent.ShadedOut = true;
        yield return new WaitForSeconds(10);
        readyToCreate = true;

    }

    void Update()
    {
        if (readyToCreate & TileOn.UnitOnTile == null)
        {
            Creator.CreateUnitOrBuilding(Team, TileOn, nameof(ArcherUnit));
            shadedOutComponent.ShadedOut = false;
            busy = false;
            readyToCreate = false;
        }
    }

}