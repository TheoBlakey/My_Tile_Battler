using UnityEngine;

public class BuilderUnit : TeamUnit
{
    public override string SpriteLandName => "builder_unit";
    CreateUnitOrBuildingComponent Creator => CreateOrGetComponent<CreateUnitOrBuildingComponent>();

    public void TryCreateBuilding(string buildingName)
    {
        if (IsFunctionalyPaused) return;
        if (!TileOn.CurrntlyBuildAble) return;

        GameObject newConstruction = Creator.CreateUnitOrBuilding(Team, TileOn, nameof(ConstructionBuilding));

        newConstruction.GetComponent<ConstructionBuilding>().BuildingToMake = buildingName;

        StartCoroutine(PauseUnitForTime(Constants.ConstructionTime));
    }

    public void TryBecomeArcher()
    {
        if (IsFunctionalyPaused) return;
        if (TileOn.BuildingOnTile.TryGetComponent<BarracksBuilding>(out var building))
        {
            if (building.busy) return;
            building.BeginArcherCreation();
            Destroy(gameObject);
        }

    }

}