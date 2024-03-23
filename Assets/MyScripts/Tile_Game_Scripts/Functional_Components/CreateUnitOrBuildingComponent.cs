using System;
using UnityEngine;

public class CreateUnitOrBuildingComponent : ComponentCacher
{
    public GameObject GenericBasePrefab => LoadPrefab("GenericBase");
    public GameObject CreateUnitOrBuilding(int Team, TileScript Tile, string addComponent)
    {
        GameObject newObj = Instantiate(GenericBasePrefab, Tile.transform.position, new Quaternion());

        Type componentType = Type.GetType(addComponent);
        newObj.AddComponent(componentType);

        ITeamTileInterface Interace = newObj.GetComponent<ITeamTileInterface>();
        Interace.Team = Team;
        Interace.TileOn = Tile;

        return newObj;
    }
}