using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameControllerScript : MonoBehaviour
{
    [SerializeField]
    public List<TileScript> FullTileList;
    [SerializeField]
    public Camera camera;

    public bool DisableMouse = false;

    public List<TileScript> highlightedTiles = new List<TileScript>();
    private TileScript _selectedTile;
    public TileScript SelectedTileWithUnit
    {
        get { return this._selectedTile; }
        set
        {
            if (_selectedTile == value)
            {
                return;
            }
            this._selectedTile = value;

            highlightedTiles.ForEach(t => t.HighLighted = false);

            if (SelectedTileWithUnit != null)
            {
                highlightedTiles = GetNeighboursToADistance(_selectedTile, 2, false);
                highlightedTiles.ForEach(t => t.HighLighted = true);
            }

        }
    }

    void Start()
    {

    }

    public void PopulateTileList()
    {
        TileScript[] tileScripts = Object.FindObjectsOfType<TileScript>();
        FullTileList = new List<TileScript>(tileScripts);
    }

    //public void CreatelandBorder()
    //{
    //    List<TileScript> LandTiles = new List<TileScript>(FullTileList.Where(x => x.Type == TileScript.TileType.Land).ToArray());

    //    foreach (TileScript Tile in LandTiles)
    //    {
    //        List<string> sideList = new List<string> { };

    //        if (GetNeighbourN(Tile).Type == TileScript.TileType.Water)
    //        {
    //            transform.Find("N").gameObject.SetActive(true);
    //        }
    //        if (GetNeighbourS(Tile).Type == TileScript.TileType.Water)
    //        {
    //            transform.Find("S").gameObject.SetActive(true);
    //        }
    //        if (GetNeighbourSE(Tile).Type == TileScript.TileType.Water)
    //        {
    //            transform.Find("SE").gameObject.SetActive(true);
    //        }
    //        if (GetNeighbourSW(Tile).Type == TileScript.TileType.Water)
    //        {
    //            transform.Find("SW").gameObject.SetActive(true);
    //        }
    //        if (GetNeighbourNW(Tile).Type == TileScript.TileType.Water)
    //        {
    //            transform.Find("NW").gameObject.SetActive(true);
    //        }
    //        if (GetNeighbourNE(Tile).Type == TileScript.TileType.Water)
    //        {
    //            transform.Find("NE").gameObject.SetActive(true);
    //        }


    //    }
    //}

    public List<TileScript> GetallTileNeighbours(TileScript OriginalTile, bool travelOnWater = true)
    {
        List<TileScript> neighbours = new List<TileScript>
        {
            GetNeighbourN(OriginalTile),
            GetNeighbourNE(OriginalTile),
            GetNeighbourNW(OriginalTile),
            GetNeighbourSE(OriginalTile),
            GetNeighbourSW(OriginalTile),
            GetNeighbourS(OriginalTile)
        };

        if (!travelOnWater)
        {
            neighbours = neighbours
                .Where(t => t.Type != TileScript.TileType.Water
                ).ToList();
        }

        return neighbours;
    }

    public TileScript GetNeighbourN(TileScript OriginalTile)
    {
        return FullTileList.Where(x =>
        x.GridLocation.x == OriginalTile.GridLocation.x &&
        x.GridLocation.y == OriginalTile.GridLocation.y + 1
        ).FirstOrDefault();
    }

    public TileScript GetNeighbourNE(TileScript OriginalTile)
    {
        return FullTileList.Where(x =>
        x.GridLocation.x == OriginalTile.GridLocation.x + 1 &&
        x.GridLocation.y == OriginalTile.GridLocation.y + (OriginalTile.GridLocation.x % 2 == 0 ? 1 : 0)
        ).FirstOrDefault();
    }

    public TileScript GetNeighbourNW(TileScript OriginalTile)
    {
        return FullTileList.Where(x =>
        x.GridLocation.x == OriginalTile.GridLocation.x - 1 &&
        x.GridLocation.y == OriginalTile.GridLocation.y + (OriginalTile.GridLocation.x % 2 == 0 ? 1 : 0)
        ).FirstOrDefault();
    }

    public TileScript GetNeighbourS(TileScript OriginalTile)
    {
        return FullTileList.Where(x =>
        x.GridLocation.x == OriginalTile.GridLocation.x &&
        x.GridLocation.y == OriginalTile.GridLocation.y - 1
        ).FirstOrDefault();
    }

    public TileScript GetNeighbourSE(TileScript OriginalTile)
    {
        return FullTileList.Where(x =>
        x.GridLocation.x == OriginalTile.GridLocation.x + 1 &&
        x.GridLocation.y == OriginalTile.GridLocation.y - (OriginalTile.GridLocation.x % 2 == 0 ? 0 : 1)
        ).FirstOrDefault();
    }

    public TileScript GetNeighbourSW(TileScript OriginalTile)
    {
        return FullTileList.Where(x =>
        x.GridLocation.x == OriginalTile.GridLocation.x - 1 &&
        x.GridLocation.y == OriginalTile.GridLocation.y - (OriginalTile.GridLocation.x % 2 == 0 ? 0 : 1)
        ).FirstOrDefault();
    }

    public List<TileScript> GetNeighboursToADistance(TileScript OriginalTile, int distance, bool travelOnWater)
    {
        HashSet<TileScript> allNeighbours = new HashSet<TileScript> { };
        HashSet<TileScript> nonCheckedNeighbours = new HashSet<TileScript> { OriginalTile };

        for (int i = 0; i < distance; i++)
        {
            HashSet<TileScript> neighboursTemp = new HashSet<TileScript>();
            foreach (TileScript neighbour in nonCheckedNeighbours)
            {
                neighboursTemp.UnionWith(GetallTileNeighbours(neighbour, travelOnWater));
            }
            nonCheckedNeighbours = neighboursTemp.Except(allNeighbours).ToHashSet();
            allNeighbours.UnionWith(nonCheckedNeighbours);
        }

        return allNeighbours.ToList();
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
