using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : ComponentCacher
{

    [SerializeField]
    public List<TileScript> FullTileList;
    [SerializeField]
    public List<TileScript> AllStartingTeamCities;
    [SerializeField]
    public List<TileScript> AllWaterTiles;
    [SerializeField]
    public List<TileScript> AllCities;

    CreateUnitOrBuildingComponent Creator => CreateOrGetComponent<CreateUnitOrBuildingComponent>();
    IEnumerator GenerateStartingUnits()
    {
        yield return new WaitForSeconds(0.1f);
        foreach (TileScript tile in AllStartingTeamCities)
        {
            int ogTeam = tile.Team;
            tile.Team = 0;
            Destroy(tile.GetComponent<CityComponent>());
            Creator.CreateUnitOrBuilding(ogTeam, tile, nameof(BuilderUnit));
        }
    }

    void Start()
    {
        gameObject.AddComponent<PlayerControllerComponent>();
        //gameObject.AddComponent<VikingControllerComponent>();

        StartCoroutine(GenerateStartingUnits());
    }



}
