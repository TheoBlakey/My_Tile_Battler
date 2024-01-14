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
                highlightedTiles = GetTilesToADistance(_selectedTile, 2);
                highlightedTiles.ForEach(t => t.HighLighted = true);
            }

        }
    }

    [SerializeField]
    public Dictionary<string, Sprite> SpriteDict = new();


    private int LANDMOVEMENT = 2;
    private int WATERMOVEMENT = 4;

    void Start()
    {

    }

    public Sprite GetSprite(string SpiteName)
    {
        if (SpriteDict.TryGetValue(SpiteName, out Sprite sprite))
        {
            return sprite;
        }
        else
        {
            Sprite foundSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Objects/" + SpiteName + ".png");
            SpriteDict.Add("SpiteName", foundSprite);
            return foundSprite;
        }
    }

    public void PopulateTileList()
    {
        TileScript[] tileScripts = FindObjectsOfType<TileScript>();
        FullTileList = new List<TileScript>(tileScripts);
    }

    public List<TileScript> GetPossibleMovements(TileScript OriginalTile)
    {
        bool isPort = OriginalTile.isNextToSea && OriginalTile.Type == TileScript.TileType.City;
        List<TileScript> possibleMovements = new();
        switch (isPort, OriginalTile.IsLandOrCity)
        {
            case (true, _):
                possibleMovements.Union(GetLegalTilesToADistance(OriginalTile, LANDMOVEMENT, new List<TypeOfMovement> { TypeOfMovement.LandToLand }));
                possibleMovements.Union(GetLegalTilesToADistance(OriginalTile, WATERMOVEMENT, new List<TypeOfMovement> { TypeOfMovement.LandToWater, TypeOfMovement.WaterToWater }));
                break;
            case (_, true):
                possibleMovements.Union(GetLegalTilesToADistance(OriginalTile, LANDMOVEMENT, new List<TypeOfMovement> { TypeOfMovement.LandToLand }));
                break;
            case (_, false):
                possibleMovements.Union(GetLegalTilesToADistance(OriginalTile, WATERMOVEMENT, new List<TypeOfMovement> { TypeOfMovement.WaterToWater }));
                break;
        }

        return possibleMovements;
    }

    public List<TileScript> GetSimpleOptimumPath(TileScript startTile, TileScript endTile)
    {
        List<TileScript> OptimumSimplePath = new() { startTile };

        List<TileScript> tempSearchTiles = new();
        while (!OptimumSimplePath.Contains(endTile))
        {
            tempSearchTiles = GetTilesToADistance(OptimumSimplePath.Last(), WATERMOVEMENT);

            TileScript closestToFinalTile = tempSearchTiles
           .OrderBy(tempTile => Vector2.Distance(tempTile.transform.position, endTile.transform.position))
           .First();

            OptimumSimplePath.Add(closestToFinalTile);

        }
        return OptimumSimplePath;
    }

    public List<TileScript> GetLegalTilesToADistance(TileScript OriginalTile, int distance, List<TypeOfMovement> typesOfMovementsAllowed)
    {
        List<TileMovement> nonCheckedNeighbours = GetMovementNeighbours(OriginalTile, typesOfMovementsAllowed);
        List<TileMovement> allNeighbours = nonCheckedNeighbours.ToList();

        for (int i = 0; i < distance - 1; i++)
        {
            List<TileMovement> foundNeightbours = new();
            foreach (TileMovement neighbour in nonCheckedNeighbours)
            {
                if (neighbour.CanMoveOnwards)
                {
                    foundNeightbours.Union(GetMovementNeighbours(neighbour.TileToMoveTo, typesOfMovementsAllowed));
                }
            }
            nonCheckedNeighbours = foundNeightbours.Except(allNeighbours).ToList();
            allNeighbours.Union(nonCheckedNeighbours);
        }

        return allNeighbours.Select(t => t.TileToMoveTo).ToList();
    }



    public enum TypeOfMovement
    { WaterToWater, LandToLand, LandToWater, WaterToLand }

    private class TileMovement
    {
        public TileScript TileToMoveTo;
        public TypeOfMovement TypeOfMovement;
        public bool CanMoveOnwards => !(TypeOfMovement == TypeOfMovement.LandToWater);
    }

    private List<TileMovement> GetMovementNeighbours(TileScript OriginalTile, List<TypeOfMovement> typesOfMovementsAllowed)
    {
        List<TileMovement> tileMovements = new();
        foreach (TileScript neighbour in GetallTileNeighbours(OriginalTile))
        {
            TypeOfMovement typeOfMovement = (OriginalTile.IsLandOrCity, neighbour.IsLandOrCity) switch
            {
                (true, true) => TypeOfMovement.LandToLand,
                (true, false) => TypeOfMovement.WaterToLand,
                (false, true) => TypeOfMovement.LandToWater,
                (false, false) => TypeOfMovement.WaterToWater,
            };

            if (typesOfMovementsAllowed.Contains(typeOfMovement))
            {
                tileMovements.Add(new TileMovement()
                {
                    TileToMoveTo = neighbour,
                    TypeOfMovement = typeOfMovement
                });
            }
        };

        return tileMovements;
    }


    public List<TileScript> GetallTileNeighbours(TileScript OriginalTile)
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

    public List<TileScript> GetTilesToADistance(TileScript OriginalTile, int distance)
    {
        List<TileScript> allNeighbours = new List<TileScript> { };
        List<TileScript> nonCheckedNeighbours = new List<TileScript> { OriginalTile };

        for (int i = 0; i < distance; i++)
        {
            List<TileScript> neighboursTemp = new List<TileScript>();
            foreach (TileScript neighbour in nonCheckedNeighbours)
            {
                neighboursTemp.Union(GetallTileNeighbours(neighbour));
            }
            nonCheckedNeighbours = neighboursTemp.Except(allNeighbours).ToList();
            allNeighbours.Union(nonCheckedNeighbours);
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
