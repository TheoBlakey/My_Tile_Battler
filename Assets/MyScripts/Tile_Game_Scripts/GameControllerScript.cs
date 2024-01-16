using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GameControllerScript : MonoBehaviour
{
    [SerializeField]
    public List<TileScript> FullTileList;
    [SerializeField]
    public Camera camera;

    public bool DisableMouse = false;





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


    private TilePathFindingComponent _pf = null;
    public TilePathFindingComponent PathFindingComponent
    {
        get => _pf != null ? _pf : _pf = gameObject.AddComponent<TilePathFindingComponent>();
    }

    private TileScript _st = null;
    public TileScript SelectedTileWithUnit
    {
        get => _st;
        set
        {
            _st = value;
            HighlightedTiles = value != null ? PathFindingComponent.GetPossibleMovementsForUnit(value) : new List<TileScript>();


            //print("AVALIABLE MOVEMENTS" + PathFindingComponent.GetPossibleMovementsForUnit(value).Count());

        }
    }



    void Start()
    {

    }


    void Update()
    {
        if (DisableMouse) { return; }


        if (Input.GetMouseButtonDown(0))
        {
            PerformTileSelection();
        }
    }

    //private void HandleMouseClick()
    //{
    //    startPosition = GetMousePosition();

    //    firstSelection = selectedObjects.Length == 0;
    //    if (firstSelection)
    //    {
    //        PerformTileSelection();
    //    }
    //}

    private void PerformTileSelection()
    {
        Vector3 mouseInput = camera.ScreenToWorldPoint(Input.mousePosition);
        mouseInput.z = 0f;

        Collider2D[] selectedObjects = Physics2D.OverlapPointAll(mouseInput);

        TileScript clickedTile = selectedObjects
            .Select(col => col.gameObject.GetComponent<TileScript>())
            .Where(tile => tile != null)
            .FirstOrDefault();

        PathfindingTest(clickedTile);
        return;

        if (clickedTile.UnitOnTile != null && clickedTile.UnitOnTile.Team == 1)
        {
            SelectedTileWithUnit = clickedTile;
            return;
        }

        if (HighlightedTiles.Contains(clickedTile))
        {
            SelectedTileWithUnit.UnitOnTile.MoveToOrAttackTile(clickedTile);
        }

        SelectedTileWithUnit = null;

    }

    private void PathfindingTest(TileScript clickedTile)
    {
        var team1cap = FindObjectsOfType<TileScript>().Where(tag => tag.Team == 1).FirstOrDefault();
        if (team1cap != null)
        {
            HighlightedTiles = PathFindingComponent.FindPath(clickedTile, team1cap);
        }
    }

    //public void TryToSelectUnit(TileScript clickedTile)
    //{
    //    if (clickedTile.UnitOnTile == null || clickedTile.UnitOnTile.Team != 0)
    //    {
    //        return;
    //    }

    //    SelectedTileWithUnit = clickedTile;
    //}


    private void PerformPlayerUnitMovement()
    {

    }

}
