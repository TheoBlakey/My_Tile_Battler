using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VikingControllerComponent : MonoBehaviour
{
    List<TileScript> TilesToMakeUnits;
    GameController GameController;

    void Start()
    {
        GameController = GetComponent<GameController>();
        StartCoroutine(SpawnVikings());
    }
    IEnumerator SpawnVikings()
    {
        while (true)
        {
            yield return new WaitForSeconds(200);
            FindTiles();
            CreateUnits();

        }
    }
    void CreateUnits()
    {
        TilesToMakeUnits.ForEach(Tile =>
            GameController.Creator.CreateUnitOrBuilding(5, Tile, nameof(VikingUnit))
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
        TileScript RandomTile = new();

        while (true)
        {
            int randomIndex = Random.Range(0, TilesToMakeUnits.Count);
            RandomTile = TilesToMakeUnits[randomIndex];

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

        if (distance < 2)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}