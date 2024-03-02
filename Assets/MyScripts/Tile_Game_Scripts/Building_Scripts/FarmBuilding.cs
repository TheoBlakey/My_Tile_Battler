class Farmbuilding : BuildingBase
{
    public override string SpriteLandName => "FarmBuilding";

    public override string ComponentToAddName => nameof(BuilderUnit);

    public override int GetTeam => throw new System.NotImplementedException();
}