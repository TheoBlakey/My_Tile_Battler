using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TilePathFindingComponent : MonoBehaviour
{
    private GameControllerScript _gc = null;
    private GameControllerScript GameController
    {
        get => _gc != null ? _gc : _gc = GetComponent<GameControllerScript>();
    }
    private List<TileScript> FullTileList => GameController.FullTileList;

    private readonly int LANDMOVEMENT = 2;
    private readonly int WATERMOVEMENT = 4;

    public enum Direction { N, NE, NW, S, SE, SW }
    public static List<Direction> DirectionList => new((Direction[])Enum.GetValues(typeof(Direction)));


    public List<TileScript> GetPossibleMovementsForUnit(TileScript OriginalTile)
    {
        bool isPort = OriginalTile.IsNextToSea && OriginalTile.Type == TileScript.TileType.City;
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
                possibleMovements.Union(GetLegalTilesToADistance(OriginalTile, WATERMOVEMENT, new List<TypeOfMovement> { TypeOfMovement.WaterToWater, TypeOfMovement.WaterToLand }));
                break;
        }

        return possibleMovements;
    }


    public List<TileScript> GetLegalTilesToADistance(TileScript OriginalTile, int distance, List<TypeOfMovement> typesOfMovementsAllowed)
    {
        List<TileMovement> nonCheckedNeighbours = GetMovementNeighbours(OriginalTile, typesOfMovementsAllowed);
        List<TileMovement> allNeighbours = nonCheckedNeighbours.ToList();

        for (int i = 0; i < distance - 1; i++)
        {
            List<TileMovement> foundNeighbours = nonCheckedNeighbours
                .Where(neighbour => neighbour.CanMoveOnwards)
                .SelectMany(neighbour => GetMovementNeighbours(neighbour.TileToMoveTo, typesOfMovementsAllowed))
                .Distinct()
                .ToList();

            nonCheckedNeighbours = foundNeighbours.Except(allNeighbours).ToList();
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
        return DirectionList.Select(e => GetNeighbour(OriginalTile, e)).ToList();
    }

    public TileScript GetNeighbour(TileScript originalTile, Direction direction)
    {
        Dictionary<Direction, (int xOffset, int yOffset)> directionOffsets = new Dictionary<Direction, (int, int)>
    {
        { Direction.N, (0, 1) },
        { Direction.NE, (1, originalTile.GridLocation.x % 2 == 0 ? 1 : 0) },
        { Direction.NW, (-1, originalTile.GridLocation.x % 2 == 0 ? 1 : 0) },
        { Direction.S, (0, -1) },
        { Direction.SE, (1, originalTile.GridLocation.x % 2 == 0 ? 0 : -1) },
        { Direction.SW, (-1, originalTile.GridLocation.x % 2 == 0 ? 0 : -1) }
    };

        var (xOffset, yOffset) = directionOffsets[direction];

        return FullTileList.FirstOrDefault(x =>
            x.GridLocation.x == originalTile.GridLocation.x + xOffset &&
            x.GridLocation.y == originalTile.GridLocation.y + yOffset);
    }

    public List<TileScript> GetSimpleOptimumPath(TileScript startTile, TileScript endTile)
    {
        List<TileScript> OptimumSimplePath = new() { startTile };

        List<TileScript> tempSearchTiles = new();
        while (!OptimumSimplePath.Contains(endTile))
        {
            tempSearchTiles = GetStraightTilesToALocation(OptimumSimplePath.Last(), WATERMOVEMENT);

            TileScript closestToFinalTile = tempSearchTiles
           .OrderBy(tempTile => Vector2.Distance(tempTile.transform.position, endTile.transform.position))
           .First();

            OptimumSimplePath.Add(closestToFinalTile);

        }
        return OptimumSimplePath;
    }

    public List<TileScript> GetStraightTilesToALocation(TileScript OriginalTile, int distance)
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

    public class NodeTile
    {
        public NodeTile ConnectedNode;
        public float Gcalculated = 0; // current calculating distance

        public TileScript Tile;
        public TileScript TargetTile;

        private int _optimalDistanceNumOfMoves = -1;
        public int Hoptimal
        {
            //get => _optimalDistanceNumOfMoves != -1 ? _optimalDistanceNumOfMoves : _optimalDistanceNumOfMoves = ThisTilePathFinder.GetSimpleOptimumPath(Tile, TargetTile).Count;
            get => (int)(_optimalDistanceNumOfMoves != -1 ? _optimalDistanceNumOfMoves : Vector2.Distance(Tile.transform.position, TargetTile.transform.position));
            //may need to just change this to overall distance
        }
        public float Ftotal => Gcalculated + Hoptimal;

        public TilePathFindingComponent ThisTilePathFinder;

        public List<NodeTile> NodeNeighbours =>
        ThisTilePathFinder.GetPossibleMovementsForUnit(Tile)
           .Select(t => new NodeTile()
           {
               Tile = Tile,
               TargetTile = TargetTile,
               ThisTilePathFinder = ThisTilePathFinder
           })
           .ToList();
    }


    public List<TileScript> FindPath(TileScript startTile, TileScript targetTile)
    {
        var startNode = new NodeTile()
        {
            Tile = startTile,
            TargetTile = targetTile,
            ThisTilePathFinder = this
        };

        var toSearch = new List<NodeTile>() { startNode };
        var processed = new List<NodeTile>();

        while (toSearch.Any())
        {
            var currentNode = toSearch[0];
            foreach (var searchNode in toSearch)
            {
                if (searchNode.Ftotal < currentNode.Ftotal || (searchNode.Ftotal == currentNode.Ftotal && searchNode.Hoptimal < currentNode.Hoptimal))
                {
                    currentNode = searchNode;
                }
            }

            if (currentNode.Tile == targetTile)
            {
                return ReturnFullPath(currentNode, startNode);
            }

            foreach (var neighbour in currentNode.NodeNeighbours.Where(t => !processed.Contains(t)))
            {
                var inSearch = toSearch.Contains(neighbour);

                var costToNeighborG = currentNode.Gcalculated + Vector2.Distance(currentNode.Tile.transform.position, neighbour.Tile.transform.position); // + 1;

                if (inSearch && !(costToNeighborG < neighbour.Gcalculated)) //new found cost is smaller
                {
                    continue;
                }

                neighbour.Gcalculated = costToNeighborG;
                neighbour.ConnectedNode = currentNode;

                if (!inSearch)
                {
                    toSearch.Add(neighbour);
                }

            }

            processed.Add(currentNode);
            toSearch.Remove(currentNode);
        }
        return null;
    }

    public List<TileScript> ReturnFullPath(NodeTile finalNode, NodeTile startNode)
    {
        var currentPathNode = finalNode;
        var finalCompleteTilePath = new List<TileScript>();
        var count = 100;
        while (currentPathNode.Tile != startNode.Tile)
        {
            finalCompleteTilePath.Add(currentPathNode.Tile);
            currentPathNode = currentPathNode.ConnectedNode;

            count--;
            if (count < 0)
            {
                throw new Exception("AHHHHHH");
            }
        }

        return finalCompleteTilePath;


    }
}