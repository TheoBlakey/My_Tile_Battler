using System.Collections;
using UnityEngine;

class ConstructionBuilding : BuildingBase
{
    public override string SpriteLandName => "ConstructionBuilding";
    public string BuildingToMake;
    void Start()
    {
        shadedOutComponent.ShadedOut = true;
        StartCoroutine(ConstructBuilding());
    }

    IEnumerator ConstructBuilding()
    {
        yield return new WaitForSeconds(10);
        Creator.CreateUnitOrBuilding(Team, TileOn, BuildingToMake);
        Destroy(this);
    }
}