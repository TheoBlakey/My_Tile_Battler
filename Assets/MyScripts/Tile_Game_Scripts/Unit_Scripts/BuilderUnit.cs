public class BuilderUnit : TeamUnit
{
    public override string SpriteLandName => "Builder_Unit";

    public void TryCreateBuilding(string buildingName)
    {
        if (Paused) return;
        if (!TileOn.CurrntlyBuildAble) return;

        Creator.CreateUnitOrBuilding(Team, TileOn, buildingName);

        StartCoroutine(PauseUnitForTime(10));
    }

    public void TryBecomeArcher()
    {
        if (Paused) return;
        if (TileOn.BuildingOnTile.TryGetComponent<BarracksBuilding>(out var building))
        {
            if (building.busy) return;
            building.BeginArcherCreation();
            Destroy(gameObject);
        }

    }

}