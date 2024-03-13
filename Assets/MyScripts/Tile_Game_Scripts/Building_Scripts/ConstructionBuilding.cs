using System.Collections;
using UnityEngine;

class ConstructionBuilding : BuildingBase
{
    public override string SpriteLandName => "Construction";
    public string BuildingToMake;
    void Start()
    {
        shadedOutComponent.ShadedOut = true;
        StartCoroutine(ConstructBuilding());
    }

    IEnumerator ConstructBuilding()
    {
        yield return new WaitForSeconds(Constants.ConstructionTime);
        Creator.CreateUnitOrBuilding(Team, TileOn, BuildingToMake);
        Destroy(this);
    }
}