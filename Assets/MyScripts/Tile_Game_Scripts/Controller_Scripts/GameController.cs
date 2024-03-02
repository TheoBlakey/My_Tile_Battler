using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GameController : MonoBehaviour
{

    [SerializeField]
    public List<TileScript> FullTileList;

    public List<TileScript> AllCities;

    CreateUnitOrBuildingComponent Creator;
    void GenerateStartingUnits()
    {
        foreach (TileScript tile in AllCities)
        {
            if (tile.Team == 0) continue;
            Creator.CreateUnitOrBuilding(tile.Team, tile, nameof(BuilderUnit));
        }
    }

    void Start()
    {
        AllCities = FullTileList.Where(t => t.Type == TileScript.TileType.City).ToList();
        Creator = this.AddComponent<CreateUnitOrBuildingComponent>();
        GenerateStartingUnits();
    }



}
