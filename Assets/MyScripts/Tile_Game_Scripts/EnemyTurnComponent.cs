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

    public void PerfromEnemyTeamMove(int Team, UnitScript soloUnit = null)
    {
        List<TileScript> AllFriendlyCities = GameController.AllCities.Where(c => c.Team == Team).ToList();
        List<TileScript> AllNonFriendlyCities = GameController.AllCities.Where(c => c.Team != Team).ToList();

        List<UnitScript> ALLUNITS = FindObjectsOfType<UnitScript>().Where(u => u.TileStandingOn != null).ToList();

        List<UnitScript> AllTeamUnitsReady = ALLUNITS.Where(u => u.Team == Team && !u.MovedThisTurn).ToList();
        if (soloUnit != null) { AllTeamUnitsReady = new List<UnitScript> { soloUnit }; };
        if (AllTeamUnitsReady.Count == 0) { return; }

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

        var orderedPossibleMoves = possibleMoves
            .OrderByDescending(m => m.Desire); // decending!!!!!!


        PossibleMove bestMove = orderedPossibleMoves.FirstOrDefault();

        while (bestMove != null && bestMove.UnitMoving.TileStandingOn == bestMove.LocationOfMovement)
        {
            possibleMoves = possibleMoves.Where(m => m.UnitMoving != bestMove.UnitMoving).ToList();
            bestMove = orderedPossibleMoves.FirstOrDefault();
        }

        if (bestMove == null) { return; }

        TileScript firstMove = GameController.PathFindingComponent.FindPath(bestMove.UnitMoving.TileStandingOn, bestMove.LocationOfMovement).Last();
        bestMove.UnitMoving.MoveToOrAttackTile(firstMove);

    }


    public enum TypeOfMove { CaptureCity, DefendCity, DestroyUnit, HealUnit, CombineUnits }

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
                    if (LocationOfMovement.IsNextToSea)
                    {
                        calcDesire /= 2;
                    }
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
