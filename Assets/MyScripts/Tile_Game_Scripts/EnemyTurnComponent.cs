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

    public void PerfromEnemyTeamMove(int Team)
    {
        List<TileScript> AllFriendlyCities = GameController.AllCities.Where(c => c.Team == Team).ToList();
        List<TileScript> AllNonFriendlyCities = GameController.AllCities.Where(c => c.Team != Team).ToList();

        List<UnitScript> ALLUNITS = FindObjectsOfType<UnitScript>().ToList();

        List<UnitScript> AllUnitsReadyToMove = ALLUNITS.Where(u => u.Team == Team && !u.MovedThisTurn).ToList();
        if (AllUnitsReadyToMove.Count == 0) { return; }

        List<UnitScript> AllEnemyUnits = ALLUNITS.Where(u => u.Team == Team && !u.MovedThisTurn).ToList();

        List<PossibleMove> possibleMoves = new();

        var defendMoves = from unit in AllUnitsReadyToMove
                          from city in AllFriendlyCities
                          select new PossibleMove
                          {
                              PathFindingComponent = GameController.PathFindingComponent,
                              UnitMoving = unit,
                              TypeOfMove = TypeOfMove.DefendCity,
                              LocationOfMovement = city,
                              EnemyUnitsAttacking = AllEnemyUnits
                          };
        possibleMoves.AddRange(defendMoves);

        var attackMoves = from unit in AllUnitsReadyToMove
                          from city in AllNonFriendlyCities
                          select new PossibleMove
                          {
                              PathFindingComponent = GameController.PathFindingComponent,
                              UnitMoving = unit,
                              TypeOfMove = TypeOfMove.CaptureCity,
                              LocationOfMovement = city
                          };
        possibleMoves.AddRange(attackMoves);

        PossibleMove bestMove = possibleMoves
            .OrderBy(m => m.Desire)
            .FirstOrDefault();

        TileScript firstMove = GameController.PathFindingComponent.FindPath(bestMove.UnitMoving.TileStandingOn, bestMove.LocationOfMovement).FirstOrDefault();

        bestMove.UnitMoving.MoveToOrAttackTile(firstMove);
    }


    public enum TypeOfMove { CaptureCity, DefendCity, DestroyUnit }

    public class PossibleMove
    {
        public TilePathFindingComponent PathFindingComponent;
        public UnitScript UnitMoving;
        public TypeOfMove TypeOfMove;
        public TileScript LocationOfMovement;

        public List<UnitScript> EnemyUnitsAttacking;
        float Distance => PathFindingComponent.GetRayCastApproxDistance(UnitMoving.transform, LocationOfMovement.transform);


        public float Desire => GetDesire();
        float GetDesire()
        {
            float calcDesire = 0;
            int CAPTIALTAKEMULTIPLIER = 40;
            int CAPTIALDEFENDMULTIPLIER = 40;

            switch (TypeOfMove)
            {
                case TypeOfMove.CaptureCity:

                    calcDesire = UnitMoving.ChanceToWInBattle(LocationOfMovement) / Distance;
                    if (LocationOfMovement.IsCapital)
                    {
                        calcDesire *= CAPTIALTAKEMULTIPLIER;
                    }
                    break;
                case TypeOfMove.DefendCity:

                    UnitScript closestEnemyUnit = EnemyUnitsAttacking
                        .OrderBy(enemy => PathFindingComponent.GetRayCastApproxDistance(enemy.transform, LocationOfMovement.transform))
                        .FirstOrDefault();

                    calcDesire = closestEnemyUnit.ChanceToWInBattle(LocationOfMovement) / Distance;
                    if (LocationOfMovement.IsCapital)
                    {
                        calcDesire *= CAPTIALDEFENDMULTIPLIER;
                    }
                    break;


                case TypeOfMove.DestroyUnit:
                    // code block
                    break;
            }

            return calcDesire;
        }


    }
}
