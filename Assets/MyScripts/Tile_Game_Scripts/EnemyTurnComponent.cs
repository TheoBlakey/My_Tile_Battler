using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyTurnComponent : MonoBehaviour
{

    private GameControllerScript _gc = null;
    private GameControllerScript GameController
    {
        get => _gc != null ? _gc : _gc = GetComponent<GameControllerScript>();
    }

    public bool PerfromEnemyTeamMove(int Team)
    {
        List<TileScript> AllFriendlyCities = GameController.AllCities.Where(c => c.Team == Team).ToList();
        List<TileScript> AllNonFriendlyCities = GameController.AllCities.Where(c => c.Team != Team).ToList();

        List<UnitScript> ALLUNITS = FindObjectsOfType<UnitScript>().ToList();

        List<UnitScript> AllTeamUnitsReady = ALLUNITS.Where(u => u.Team == Team && !u.MovedThisTurn).ToList();
        if (AllTeamUnitsReady.Count == 0) { return true; }

        List<UnitScript> AllEnemyUnits = ALLUNITS.Where(u => u.Team != Team).ToList();
        List<TileScript> AllTilesWithNonFriendlyUnits = AllEnemyUnits.Select(s => s.TileStandingOn).ToList();

        List<PossibleMove> possibleMoves = new();

        var defendCityMoves = from unit in AllTeamUnitsReady
                              from city in AllFriendlyCities
                              select new PossibleMove
                              {
                                  PathFindingComponent = GameController.PathFindingComponent,
                                  UnitMoving = unit,
                                  TypeOfMove = TypeOfMove.DefendCity,
                                  LocationOfMovement = city,
                                  EnemyUnitsAttacking = AllEnemyUnits
                              };
        //possibleMoves.AddRange(defendCityMoves);

        var attackCityMoves = from unit in AllTeamUnitsReady
                              from city in AllNonFriendlyCities
                              select new PossibleMove
                              {
                                  PathFindingComponent = GameController.PathFindingComponent,
                                  UnitMoving = unit,
                                  TypeOfMove = TypeOfMove.CaptureCity,
                                  LocationOfMovement = city
                              };
        possibleMoves.AddRange(attackCityMoves);

        var attackUnitMoves = from unit in AllTeamUnitsReady
                              from city in AllTilesWithNonFriendlyUnits
                              select new PossibleMove
                              {
                                  PathFindingComponent = GameController.PathFindingComponent,
                                  UnitMoving = unit,
                                  TypeOfMove = TypeOfMove.DestroyUnit,
                                  LocationOfMovement = city
                              };
        //possibleMoves.AddRange(attackUnitMoves);

        PossibleMove bestMove = possibleMoves
            .OrderByDescending(m => m.Desire) // decending!!!!!!
            .FirstOrDefault();

        if (bestMove.UnitMoving.TileStandingOn == bestMove.LocationOfMovement)
        {
            bestMove.UnitMoving.MovedThisTurn = true;
            return false;
        }

        //var paths = GameController.PathFindingComponent.FindPath(bestMove.UnitMoving.TileStandingOn, bestMove.LocationOfMovement);
        TileScript firstMove = GameController.PathFindingComponent.FindPath(bestMove.UnitMoving.TileStandingOn, bestMove.LocationOfMovement).Last();

        bestMove.UnitMoving.MoveToOrAttackTile(firstMove);
        return true;
    }


    public enum TypeOfMove { CaptureCity, DefendCity, DestroyUnit }

    public class PossibleMove
    {
        public TilePathFindingComponent PathFindingComponent;
        public UnitScript UnitMoving;
        public TypeOfMove TypeOfMove;
        public TileScript LocationOfMovement;

        public List<UnitScript> EnemyUnitsAttacking;
        //float Distance => PathFindingComponent.GetRayCastApproxDistance(UnitMoving.transform, LocationOfMovement.transform);

        float Distance(UnitScript unit)
        {
            return PathFindingComponent.GetRayCastApproxDistance(unit.transform, LocationOfMovement.transform);
        }

        public float Desire => GetDesire();
        float GetDesire()
        {
            float calcDesire = 0;
            int CAPTIALTAKEMULTIPLIER = 1;
            int CAPTIALDEFENDMULTIPLIER = 1;

            switch (TypeOfMove)
            {
                case TypeOfMove.CaptureCity:

                    calcDesire = UnitMoving.ChanceToWInBattle(LocationOfMovement) / (Distance(UnitMoving) * 2);
                    if (LocationOfMovement.IsCapital)
                    {
                        calcDesire *= CAPTIALTAKEMULTIPLIER;
                    }
                    break;
                case TypeOfMove.DefendCity:

                    UnitScript closestEnemyUnit = EnemyUnitsAttacking
                        .OrderBy(enemy => PathFindingComponent.GetRayCastApproxDistance(enemy.transform, LocationOfMovement.transform))
                        .FirstOrDefault();

                    calcDesire = closestEnemyUnit.ChanceToWInBattle(LocationOfMovement) / (Distance(closestEnemyUnit) + Distance(UnitMoving) * 2);
                    if (LocationOfMovement.IsCapital)
                    {
                        calcDesire *= CAPTIALDEFENDMULTIPLIER;
                    }
                    break;

                case TypeOfMove.DestroyUnit:
                    calcDesire = UnitMoving.ChanceToWInBattle(LocationOfMovement) / (Distance(UnitMoving) * 2);
                    break;
            }

            return calcDesire;
        }


    }
}
