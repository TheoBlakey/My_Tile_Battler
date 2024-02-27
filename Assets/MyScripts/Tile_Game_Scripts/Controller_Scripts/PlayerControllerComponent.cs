using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    TilePathFindingComponent PathFindingComponent;
    private void Start()
    {
        PathFindingComponent = GetComponent<TilePathFindingComponent>();
    }

    private TileScript _st = null;
    public TileScript SelectedTileWithUnit
    {
        get => _st;
        set
        {
            _st = value;
            HighlightedTiles = value != null ? PathFindingComponent.GetPossibleMovementsForUnit(value) : new List<TileScript>();
        }
    }

    private List<TileScript> _ht = new();
    public List<TileScript> HighlightedTiles
    {
        get => _ht;
        set
        {
            _ht.ForEach(t => t.HighLighted = false);
            value.ForEach(t => t.HighLighted = true);
            _ht = value;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PerformTileSelection();
        }
    }


    private void PerformTileSelection()
    {
        var camera = GetComponent<Camera>();
        Vector3 mouseInput = camera.ScreenToWorldPoint(Input.mousePosition);
        mouseInput.z = 0f;

        Collider2D[] selectedObjects = Physics2D.OverlapPointAll(mouseInput);

        TileScript clickedTile = selectedObjects
            .Select(col => col.gameObject.GetComponent<TileScript>())
            .Where(tile => tile != null)
            .FirstOrDefault();

        if (SelectedTileWithUnit == null && clickedTile.UnitOnTile != null && clickedTile.UnitOnTile.Team == 1 && !clickedTile.UnitOnTile.Paused)
        {
            SelectedTileWithUnit = clickedTile;
            return;
        }

        if (HighlightedTiles.Contains(clickedTile))
        {
            SelectedTileWithUnit.UnitOnTile.MoveToTile(clickedTile);

        }

        SelectedTileWithUnit = null;
    }

}
