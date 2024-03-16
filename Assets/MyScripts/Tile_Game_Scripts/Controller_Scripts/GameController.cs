using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : ComponentCacher
{

    [SerializeField]
    public List<TileScript> FullTileList;

    public List<TileScript> AllCities;
    public List<TileScript> AllEdgeWaterTilese;

    CreateUnitOrBuildingComponent Creator => CreateOrGetComponent<CreateUnitOrBuildingComponent>();
    IEnumerator GenerateStartingUnits()
    {
        yield return new WaitForSeconds(0.1f);
        foreach (TileScript tile in AllCities)
        {
            if (tile.Team == 0) continue;

            int ogTeam = tile.Team;
            tile.Team = 0;
            Destroy(tile.GetComponent<CityComponent>());
            Creator.CreateUnitOrBuilding(ogTeam, tile, nameof(BuilderUnit));
        }
    }

    void Start()
    {
        AllCities = FullTileList.Where(t => t.Type == TileScript.TileType.City).ToList();

        gameObject.AddComponent<PlayerControllerComponent>();
        //gameObject.AddComponent<VikingControllerComponent>();

        StartCoroutine(GenerateStartingUnits());
    }



}
