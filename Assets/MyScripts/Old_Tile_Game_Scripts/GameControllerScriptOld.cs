using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GameControllerScriptOld : MonoBehaviour
{
    public bool AsyncGame = true;
    public float TeamPlayerPause = 5;
    public int CityRateOfCreation => Random.Range(1, 1);
    public int UnitMovedPause = 2;

    int playerTurnsLeft = 0;
    const int NUMBEROFMOVESPERTURN = 5;

    public bool WaitingForUnitTurnToEnd = false;
    public List<int> WaitingForUnitTeams = new();

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

    public bool DisableMouse => (WaitingForUnitTurnToEnd || playerTurnsLeft == 0) && !AsyncGame;

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


    private TilePathFindingComponentOld _pf = null;
    public TilePathFindingComponentOld PathFindingComponent
    {
        get => _pf != null ? _pf : _pf = gameObject.AddComponent<TilePathFindingComponentOld>();
    }

    private EnemyTurnComponent _etc = null;
    public EnemyTurnComponent EnemyTurnComponent
    {
        get => _etc != null ? _etc : _etc = gameObject.AddComponent<EnemyTurnComponent>();
    }




    void Update()
    {
        CheckSelectedUnitNotDestoryed();

        if (Input.GetMouseButtonDown(0) && !DisableMouse)
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

        if (SelectedTileWithUnit == null && clickedTile.UnitOnTile != null && clickedTile.UnitOnTile.Team == 1 && !clickedTile.UnitOnTile.ShadedOut)
        {
            SelectedTileWithUnit = clickedTile;
            return;
        }

        if (HighlightedTiles.Contains(clickedTile))
        {
            SelectedTileWithUnit.UnitOnTile.MoveToOrAttackTile(clickedTile);
            playerTurnsLeft--;

            List<UnitScriptOld> moreUnitsToMove = FindObjectsOfType<UnitScriptOld>().Where(u => u.Team == 1 && !u.MovedThisTurn_Not_Used).ToList();
            if (moreUnitsToMove.Count == 0)
            {
                playerTurnsLeft = 0;
            }
        }

        SelectedTileWithUnit = null;

    }
    void CheckSelectedUnitNotDestoryed()
    {
        if (SelectedTileWithUnit != null && SelectedTileWithUnit.UnitOnTile != null && SelectedTileWithUnit.UnitOnTile.Team == 1 && SelectedTileWithUnit.UnitOnTile.ShadedOut == false)
        {
            return;
        }

        SelectedTileWithUnit = null;
    }

    private void PathfindingTest(TileScript clickedTile)
    {
        var close = AllCities.OrderBy(c => PathFindingComponent.GetRayCastApproxDistance(clickedTile.transform, c.transform)).FirstOrDefault();
        HighlightedTiles = PathFindingComponent.FindPath(close, clickedTile);
    }

    void Start()
    {
        if (!AsyncGame)
        {
            StartCoroutine(TURNBASEDGAMESTART());
            return;
        }

        AllCities.ForEach(c => c.StartAsyncGameturn());
        for (int i = 1; i < TeamList.Count; i++)
        {
            StartCoroutine(AsyncTeamTurns(TeamList[i]));

        }

    }

    IEnumerator AsyncTeamTurns(int teamNum)
    {
        while (true)
        {
            yield return new WaitForSeconds(2);
            while (WaitingForUnitTeams.Contains(teamNum))
            {
                yield return null;
            }
            EnemyTurnComponent.PerfromEnemyTeamMove(teamNum);
        }
    }
    ////////////////////////////////////////////////////////////////////////////////////////////////////////
    IEnumerator TURNBASEDGAMESTART()
    {
        int MAXGAMETURNS = 1000;

        //teamList = teamList.OrderBy(x => Random.value).ToList(); //shuffle!

        for (int i = 0; i < MAXGAMETURNS; i++)
        {
            int currentIndex = i % TeamList.Count;
            yield return StartCoroutine(PefromTeamTurn(TeamList[currentIndex]));

        }

    }

    bool unitsAvailable => FindObjectsOfType<UnitScriptOld>().Where(u => u.Team == 1 && u.MovedThisTurn_Not_Used == false).Count() > 0;
    IEnumerator PerformPlayerTurn()
    {
        playerTurnsLeft = NUMBEROFMOVESPERTURN;
        while (unitsAvailable && (playerTurnsLeft != 0 || WaitingForUnitTurnToEnd))
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

        print("TEAM TURN : " + teamNum);
        if (teamNum == 1)
        {
            yield return StartCoroutine(PerformPlayerTurn());
        }
        else
        {
            for (int i = 0; i < NUMBEROFMOVESPERTURN; i++)
            {
                while (WaitingForUnitTurnToEnd)
                {
                    yield return null;
                }
                EnemyTurnComponent.PerfromEnemyTeamMove(teamNum);

                //bool actuallyMoved = EnemyTurnComponent.PerfromEnemyTeamMove(teamNum);
                //if (!actuallyMoved)
                //{
                //    i++;
                //}
            }
        }

        List<UnitScriptOld> unitsForTeam = FindObjectsOfType<UnitScriptOld>().Where(u => u.Team == teamNum).ToList();
        unitsForTeam.ForEach(u =>
        {
            u.MovedThisTurn_Not_Used = false;
        });

    }




}
