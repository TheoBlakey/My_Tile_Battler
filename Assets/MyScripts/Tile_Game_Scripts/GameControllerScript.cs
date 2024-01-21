using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GameControllerScript : MonoBehaviour
{
    int playerTurnsLeft = 5;
    const int NUMBEROFMOVESPERTURN = 5;

    public bool WaitingForMovementToEnd = false;

    public List<Color> ColorList = new()
    {
       new (1, 1, 1, 1), //SPARE
       new (0, 0, 1, 0.5f), //blue
       new (1, 0, 0, 0.5f), //red
       new (0, 0.25f, 0, 0.5f), //dark green
       new (1, 0, 1, 0.5f), //magenta
       new (0, 1, 0, 1), //green
       new(1, 1, 1, 1) //white
    };

    [SerializeField]
    public List<int> TeamList;

    [SerializeField]
    public List<TileScript> FullTileList;

    private List<TileScript> _ac = new();
    public List<TileScript> AllCities
    {
        get => _ac.Count != 0 ? _ac : _ac = FullTileList.Where(t => t.Type == TileScript.TileType.City).ToList();
    }


    [SerializeField]
    public Camera camera;

    public bool DisableMouse => WaitingForMovementToEnd || playerTurnsLeft == 0;

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


    private TilePathFindingComponent _pf = null;
    public TilePathFindingComponent PathFindingComponent
    {
        get => _pf != null ? _pf : _pf = gameObject.AddComponent<TilePathFindingComponent>();
    }

    private EnemyTurnComponent _etc = null;
    public EnemyTurnComponent EnemyTurnComponent
    {
        get => _etc != null ? _etc : _etc = gameObject.AddComponent<EnemyTurnComponent>();
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


    private void PerformTileSelection()
    {
        Vector3 mouseInput = camera.ScreenToWorldPoint(Input.mousePosition);
        mouseInput.z = 0f;

        Collider2D[] selectedObjects = Physics2D.OverlapPointAll(mouseInput);

        TileScript clickedTile = selectedObjects
            .Select(col => col.gameObject.GetComponent<TileScript>())
            .Where(tile => tile != null)
            .FirstOrDefault();

        if (clickedTile.UnitOnTile != null && clickedTile.UnitOnTile.Team == 1 && !clickedTile.UnitOnTile.MovedThisTurn)
        {
            SelectedTileWithUnit = clickedTile;
            return;
        }

        if (HighlightedTiles.Contains(clickedTile))
        {
            SelectedTileWithUnit.UnitOnTile.MoveToOrAttackTile(clickedTile);
            playerTurnsLeft--;

            List<UnitScript> moreUnitsToMove = FindObjectsOfType<UnitScript>().Where(u => u.Team == 1 && !u.MovedThisTurn).ToList();
            if (moreUnitsToMove.Count == 0)
            {
                playerTurnsLeft = 0;
            }
        }

        SelectedTileWithUnit = null;

    }

    private void PathfindingTest(TileScript clickedTile)
    {
        //var team1cap = FindObjectsOfType<TileScript>().Where(tag => tag.Team == 1).FirstOrDefault();


        var close = AllCities.OrderBy(c => PathFindingComponent.GetRayCastApproxDistance(clickedTile.transform, c.transform)).FirstOrDefault();
        //print(close.GridLocation);

        //HighlightedTiles = new List<TileScript>() { close };
        HighlightedTiles = PathFindingComponent.FindPath(close, clickedTile);
    }


    IEnumerator MASTERGAMESTART()
    {
        int MAXGAMETURNS = 1000;

        //teamList = teamList.OrderBy(x => Random.value).ToList(); //shuffle!

        for (int i = 0; i < MAXGAMETURNS; i++)
        {
            int currentIndex = i % TeamList.Count;
            yield return StartCoroutine(PefromTeamTurn(TeamList[currentIndex]));

        }

        yield return null;

    }


    IEnumerator PerformPlayerTurn()
    {
        playerTurnsLeft = NUMBEROFMOVESPERTURN;
        while (playerTurnsLeft != 0 || WaitingForMovementToEnd)
        {
            yield return null;
        }
    }

    IEnumerator PefromTeamTurn(int teamNum)
    {
        List<TileScript> teamCities = AllCities.Where(t => t.Team == teamNum).ToList();
        bool TeamHasCapital = AllCities.Any(c => c.IsCapital);
        if (!TeamHasCapital) { yield break; }

        teamCities.ForEach(c => c.PerformCityTurn());

        if (teamNum == 1)
        {
            yield return StartCoroutine(PerformPlayerTurn());
        }
        else
        {
            for (int i = 0; i < NUMBEROFMOVESPERTURN; i++)
            {
                while (WaitingForMovementToEnd)
                {
                    yield return null;
                }
                EnemyTurnComponent.PerfromEnemyTeamMove(teamNum);
            }
        }

        List<UnitScript> unitsForTeam = FindObjectsOfType<UnitScript>().Where(u => u.Team == teamNum).ToList();
        unitsForTeam.ForEach(u =>
        {
            u.MovedThisTurn = false;
        });

    }




}
