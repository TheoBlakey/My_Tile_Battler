using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VikingControllerComponent : ComponentCacher
{
    List<TileScript> _wt = new();
    List<TileScript> AllEdgeWaterTiles
    {
        get => _wt.Any() ? _wt : _wt = GameController.FullTileList.Where(t => t.Type == TileScript.TileType.Water && t.Neighbours.Count < 6).ToList();
    }
    GameController GameController => CreateOrGetComponent<GameController>();
    CreateUnitOrBuildingComponent Creator => CreateOrGetComponent<CreateUnitOrBuildingComponent>();

    void Start()
    {
        StartCoroutine(SpawnVikingsRepeating());
    }
    IEnumerator SpawnVikingsRepeating()
    {
        while (true)
        {
            yield return new WaitForSeconds(200);
            FindTiles();
            CreateUnits();

        }
    }
    List<TileScript> TilesToMakeUnits;
    void CreateUnits()
    {
        TilesToMakeUnits.ForEach(Tile =>
           Creator.CreateUnitOrBuilding(5, Tile, nameof(VikingUnit))
        );
    }

    void FindTiles()
    {
        TilesToMakeUnits = new List<TileScript>();

        int numberOfBoats = 5;
        for (int i = 0; i < numberOfBoats; i++)
        {
            TilesToMakeUnits.Add(RandomNonChosenTile());
        }

    }

    TileScript RandomNonChosenTile()
    {

        TileScript RandomTile;
        while (true)
        {
            int randomIndex = Random.Range(0, AllEdgeWaterTiles.Count);
            RandomTile = AllEdgeWaterTiles[randomIndex];

            if (IsValidTile(RandomTile))
            {
                break;
            }
        }

        return RandomTile;
    }

    bool IsValidTile(TileScript tile)
    {
        if (TilesToMakeUnits.Contains(tile))
        {
            return false;
        }
        if (TilesToMakeUnits.Count == 0)
        {
            return true;
        }
        TileScript firstTile = TilesToMakeUnits[0];

        float distance = Vector2.Distance(firstTile.transform.position, tile.transform.position);

        return distance < 2;
    }

}