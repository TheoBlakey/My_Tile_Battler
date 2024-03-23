using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class TilePathFindingComponent : MonoBehaviour
{

    private List<TileScript> _ftl = new();
    private List<TileScript> FullTileList => _ftl.Any() ? _ftl : _ftl = FindObjectOfType<GameController>().FullTileList;

    private readonly int LANDMOVEMENT = 4; //2
    private readonly int WATERMOVEMENT = 6; //4

    public enum Direction { N, NE, NW, S, SE, SW }
    public static List<Direction> DirectionList => new((Direction[])Enum.GetValues(typeof(Direction)));

    public List<TileScript> GetPossibleMovementsForUnit(TileScript OriginalTile, bool AllowTeamOverlap = false)
    {
        bool isPort = OriginalTile.IsNextToSea && OriginalTile.Type == TileScript.TileType.City;
        List<TileScript> moves = new();
        switch (isPort, OriginalTile.IsLandOrCity)
        {
            case (true, _):
                moves = moves.Union(GetLegalTilesToADistance(OriginalTile, LANDMOVEMENT, new List<TypeOfMovement> { TypeOfMovement.LandToLand })).ToList();
                moves = moves.Union(GetLegalTilesToADistance(OriginalTile, WATERMOVEMENT, new List<TypeOfMovement> { TypeOfMovement.LandToWater, TypeOfMovement.WaterToWater })).ToList();
                break;
            case (_, true):
                moves = moves.Union(GetLegalTilesToADistance(OriginalTile, LANDMOVEMENT, new List<TypeOfMovement> { TypeOfMovement.LandToLand })).ToList();
                break;
            case (_, false):
                moves = moves.Union(GetLegalTilesToADistance(OriginalTile, WATERMOVEMENT, new List<TypeOfMovement> { TypeOfMovement.WaterToWater, TypeOfMovement.WaterToLand })).ToList();
                break;
        }

        moves.Remove(OriginalTile);

        if (!AllowTeamOverlap)
        {
            moves = moves.Where(x =>
                x.UnitOnTile == null || x.UnitOnTile.Team != OriginalTile.Team)
                .ToList();
        }

        return moves;
    }

    public float GetRayCastApproxDistance(Transform startPoint, Transform endPoint)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(startPoint.position, endPoint.position - startPoint.position, Vector2.Distance(startPoint.position, endPoint.position));

        List<TileScript> tiles = hits
            .Select(hit => hit.collider.gameObject.GetComponent<TileScript>())
            .Where(tile => tile != null)
            .ToList();

        return tiles.Sum(tile =>
        tile.IsLandOrCity ? 1f / LANDMOVEMENT : 1f / WATERMOVEMENT);
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
                .DistinctBy(d => d.TileToMoveTo.GridLocation)
                .ToList();

            //nonCheckedNeighbours = foundNeighbours.Except(allNeighbours).ToList();
            nonCheckedNeighbours = foundNeighbours
                .Where(x => !allNeighbours.Any(y => y.TileToMoveTo.GridLocation == x.TileToMoveTo.GridLocation))
                .ToList();

            allNeighbours = allNeighbours.Union(nonCheckedNeighbours).DistinctBy(d => d.TileToMoveTo.GridLocation).ToList();
        }

        return allNeighbours.Select(t => t.TileToMoveTo).ToList();
    }

    public enum TypeOfMovement
    { WaterToWater, LandToLand, LandToWater, WaterToLand }

    private class TileMovement
    {
        public TileScript TileToMoveTo;
        public TypeOfMovement TypeOfMovement;
        public bool CanMoveOnwards => !(TypeOfMovement == TypeOfMovement.WaterToLand);
        //attaching enermy or city
    }

    private List<TileMovement> GetMovementNeighbours(TileScript OriginalTile, List<TypeOfMovement> typesOfMovementsAllowed)
    {
        List<TileMovement> tileMovements = new();
        foreach (TileScript neighbour in GetallTileNeighbours(OriginalTile))
        {
            TypeOfMovement typeOfMovement = (OriginalTile.IsLandOrCity, neighbour.IsLandOrCity) switch
            {
                (true, true) => TypeOfMovement.LandToLand,
                (true, false) => TypeOfMovement.LandToWater,
                (false, true) => TypeOfMovement.WaterToLand,
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
        return DirectionList
            .Select(e => GetNeighbour(OriginalTile, e))
             .Where(neighbour => neighbour != null)
               .ToList();
    }

    public TileScript GetNeighbour(TileScript originalTile, Direction direction)
    {
        var (xOffset, yOffset) = GetDirectionOffsets(originalTile)[direction];

        return FullTileList.FirstOrDefault(x =>
            x.GridLocation.x == originalTile.GridLocation.x + xOffset &&
            x.GridLocation.y == originalTile.GridLocation.y + yOffset);
    }

    public Dictionary<Direction, (int, int)> GetDirectionOffsets(TileScript originalTile)
    {
        return new Dictionary<Direction, (int, int)>
    {
        { Direction.N, (0, 1) },
        { Direction.NE, (1, originalTile.GridLocation.x % 2 == 0 ? 1 : 0) },
        { Direction.NW, (-1, originalTile.GridLocation.x % 2 == 0 ? 1 : 0) },
        { Direction.S, (0, -1) },
        { Direction.SE, (1, originalTile.GridLocation.x % 2 == 0 ? 0 : -1) },
        { Direction.SW, (-1, originalTile.GridLocation.x % 2 == 0 ? 0 : -1) }
    };
        //return originalTile.GridLocation.x % 2 == 0 ? Constants.DirectionOffsetsEven : Constants.DirectionOffsetsOdd;
    }

    public List<TileScript> GetSimpleOptimumPath(TileScript startTile, TileScript endTile)
    {
        List<TileScript> OptimumSimplePath = new() { startTile };

        List<TileScript> tempSearchTiles = new();
        while (!OptimumSimplePath.Contains(endTile))
        {
            tempSearchTiles = GetAllNeighboursToADistance(OptimumSimplePath.Last(), WATERMOVEMENT);

            TileScript closestToFinalTile = tempSearchTiles
           .OrderBy(tempTile => Vector2.Distance(tempTile.transform.position, endTile.transform.position))
           .First();

            OptimumSimplePath.Add(closestToFinalTile);

        }
        return OptimumSimplePath;
    }

    public List<TileScript> GetAllNeighboursToADistance(TileScript OriginalTile, int distance, bool allowCitiesandWater = true)
    {
        List<TileScript> allNeighbours = new List<TileScript> { };
        List<TileScript> nonCheckedNeighbours = new List<TileScript> { OriginalTile };

        for (int i = 0; i < distance; i++)
        {
            List<TileScript> neighboursTemp = new List<TileScript>();
            foreach (TileScript neighbour in nonCheckedNeighbours)
            {
                var neighbours = GetallTileNeighbours(neighbour)
                         .Where(n => allowCitiesandWater || n.Type == TileScript.TileType.Land)
                         .ToList();
                neighboursTemp = neighboursTemp.Union(neighbours).ToList();
            }
            nonCheckedNeighbours = neighboursTemp.Except(allNeighbours).ToList();
            allNeighbours = allNeighbours.Union(nonCheckedNeighbours).ToList();
        }

        allNeighbours.Remove(OriginalTile);
        return allNeighbours.ToList();
    }

    public class NodeTile : IComparable<NodeTile>
    {
        public NodeTile ConnectedNode;
        public float Gcalculated = 0; // current calculating distance

        public TileScript Tile;
        public TileScript TargetTile;

        private float _optimalDistanceNumOfMoves = -1;
        public float Hoptimal
        {
            //get => _optimalDistanceNumOfMoves != -1 ? _optimalDistanceNumOfMoves : _optimalDistanceNumOfMoves = ThisTilePathFinder.GetSimpleOptimumPath(Tile, TargetTile).Count;
            get => _optimalDistanceNumOfMoves != -1 ? _optimalDistanceNumOfMoves : Vector2.Distance(Tile.transform.position, TargetTile.transform.position);
            //may need to just change this to overall distance
        }
        public float Ftotal => Gcalculated + Hoptimal;

        public TilePathFindingComponent ThisTilePathFinder;

        public List<NodeTile> NodeNeighbours =>
        ThisTilePathFinder.GetPossibleMovementsForUnit(Tile)
           .Select(newTile => new NodeTile()
           {
               Tile = newTile,
               TargetTile = TargetTile,
               ThisTilePathFinder = ThisTilePathFinder
           })
        .ToList();

        public int CompareTo(NodeTile other)
        {
            return Ftotal.CompareTo(other.Ftotal);
        }
    }


    public List<TileScript> FindPath(TileScript startTile, TileScript targetTile)
    {
        var startNode = new NodeTile()
        {
            Tile = startTile,
            TargetTile = targetTile,
            ThisTilePathFinder = this
        };

        var toSearchList = new List<NodeTile>() { startNode };
        var processed = new HashSet<NodeTile>();
        var currentNode = new NodeTile();

        int safteyVal = 0;

        while (toSearchList.Any() && safteyVal < 500)
        {
            safteyVal++;
            currentNode = toSearchList[0];
            toSearchList.RemoveAt(0);
            processed.Add(currentNode);

            //foreach (var searchNode in toSearch)
            //{
            //    if (searchNode.Ftotal < currentNode.Ftotal || (searchNode.Ftotal == currentNode.Ftotal && searchNode.Hoptimal < currentNode.Hoptimal))
            //    {
            //        currentNode = searchNode;
            //        break;
            //    }
            //}
            //var lowestFtotalNode = toSearch.OrderBy(n => n.Ftotal).First();
            //if (lowestFtotalNode.Ftotal < currentNode.Ftotal)
            //{
            //    currentNode = lowestFtotalNode;
            //}
            //else
            //{
            //    currentNode = toSearch.OrderBy(n => n.Hoptimal).First();

            //}



            if (currentNode.Tile == targetTile)
            {
                return ReturnFullPath(currentNode, startNode);
            }

            var neighboursOfCurrent = currentNode.NodeNeighbours
                .Where(neighbour => !processed
                .Any(p => p.Tile == neighbour.Tile));

            foreach (var neighbour in neighboursOfCurrent)
            {
                var neighbourInSearch = toSearchList.Any(s => s.Tile == neighbour.Tile);
                //toSearch.Contains(neighbour);

                var costToNeighborG = currentNode.Gcalculated + Vector2.Distance(currentNode.Tile.transform.position, neighbour.Tile.transform.position); // + 1;
                //var costToNeighborG = currentNode.Gcalculated + 1; // + 1;

                if (neighbourInSearch && !(costToNeighborG < neighbour.Gcalculated)) //new found cost is smaller
                {
                    continue;
                }

                neighbour.Gcalculated = costToNeighborG;
                neighbour.ConnectedNode = currentNode;

                if (!neighbourInSearch)
                {
                    //toSearchList.Add(neighbour);
                    InsertInOrder(toSearchList, neighbour);
                    //toSearchList = toSearchList.Take(10).ToList();
                }

            }


        }
        return ReturnFullPath(currentNode, startNode);
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
        }

        return finalCompleteTilePath;
    }

    static void InsertInOrder(List<NodeTile> list, NodeTile value)
    {
        int index = list.BinarySearch(value);

        if (index < 0)
        {
            index = ~index; // Get the bitwise complement of the index
        }

        list.Insert(index, value);
    }
}