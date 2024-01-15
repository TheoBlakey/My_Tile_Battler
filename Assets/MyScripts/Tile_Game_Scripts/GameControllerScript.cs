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

    [SerializeField]
    public Dictionary<string, Sprite> SpriteDict = new();

    public List<TileScript> highlightedTiles = new();

    private TilePathFindingComponent _pf = null;
    public TilePathFindingComponent PathFindingComponent
    {
        get => _pf != null ? _pf : _pf = gameObject.AddComponent<TilePathFindingComponent>();
    }

    private TileScript _st;
    public TileScript SelectedTileWithUnit
    {
        get => _st;
        set
        {
            if (_st == value)
            {
                return;
            }
            _st = value;

            highlightedTiles.ForEach(t => t.HighLighted = false);

            if (SelectedTileWithUnit != null)
            {
                PathFindingComponent.GetPossibleMovementsForUnit(_st)
                    .ForEach(t => t.HighLighted = true);
            }

        }
    }



    void Start()
    {

    }

    public Sprite GetSprite(string spriteName)
    {
        return SpriteDict.TryGetValue(spriteName, out Sprite sprite)
            ? sprite
            : (SpriteDict[spriteName] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Objects/" + spriteName + ".png"));
    }

    //public void PopulateTileList()
    //{
    //    TileScript[] tileScripts = FindObjectsOfType<TileScript>();
    //    FullTileList = new List<TileScript>(tileScripts);
    //}




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

        print(clickedTile.GridLocation);

        if (SelectedTileWithUnit == null)
        {
            TryToSelectUnit(clickedTile);
            return;
        }

        if (SelectedTileWithUnit == null && clickedTile?.UnitOnTile?.Team == 0)
        {
            SelectedTileWithUnit = clickedTile;
            return;
        }

        if (SelectedTileWithUnit != clickedTile && highlightedTiles.Contains(clickedTile))
        {
            SelectedTileWithUnit.CurrentUnit.TryToMoveTile(clickedTile);
        }

        SelectedTileWithUnit = null;


    }
    public void TryToSelectUnit(TileScript clickedTile)
    {
        if (clickedTile.UnitOnTile == null || clickedTile.UnitOnTile.Team != 0)
        {
            return;
        }

        SelectedTileWithUnit = clickedTile;
    }


    private void PerformPlayerUnitMovement()
    {

    }

}
