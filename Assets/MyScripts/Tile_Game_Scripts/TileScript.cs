using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable()]
public class TileScript : MonoBehaviour
{
    public GameObject UnitRef;


    [SerializeField]
    SpriteRenderer spriteRenderer => GetComponent<SpriteRenderer>();

    [SerializeField]
    public GameControllerScript _gc = null;
    public GameControllerScript GameController
    {
        get => _gc != null ? _gc : _gc = FindObjectsOfType<GameControllerScript>().FirstOrDefault().GetComponent<GameControllerScript>();
    }

    public List<TileScript> _n;
    public List<TileScript> Neighbours
    {
        get => _n.Any() ? _n : _n = GameController.PathFindingComponent.GetallTileNeighbours(this);
    }

    [SerializeField]
    public bool isCapital = false;

    [SerializeField]
    public Vector2Int GridLocation;

    private UnitScript _cu;
    public UnitScript CurrentUnit
    {
        get => _cu;
        set
        {
            _cu = value;
            Team = value.Team;

        }
    }


    List<Color> ColorList = new List<Color>
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
    private GameObject TeamShader;

    [SerializeField]
    public int _team;
    public int Team
    {
        get => _team;
        set
        {
            this._team = value;
            TeamShader = TeamShader != null ? TeamShader : transform.Find("TeamShader").gameObject;
            TeamShader.GetComponent<SpriteRenderer>().color = ColorList[value];

        }
    }




    private bool _h = false;
    public bool HighLighted
    {
        get => _h;
        set
        {
            _h = value;
            transform.Find("HighLighted").gameObject.SetActive(value);
        }
    }

    public enum TileType
    { Water, City, Land }

    [SerializeField]
    public TileType Type;

    public bool IsLandOrCity => Type == TileType.Land || Type == TileType.City;
    public bool IsNextToSea => Neighbours.Any(t => t.Type == TileType.Water);

    public UnitScript UnitOnTile;


    public void CalculateSprite()
    {
        switch (Type)
        {
            case TileType.City:

                if (isCapital)
                {
                    spriteRenderer.sprite = GameController.GetSprite("CapitalTile");
                    break;
                }
                if (IsNextToSea)
                {
                    spriteRenderer.sprite = GameController.GetSprite("PortTile");
                    break;
                }
                spriteRenderer.sprite = GameController.GetSprite("CityTile");
                break;

            case TileType.Water:

                spriteRenderer.sprite = GameController.GetSprite("WaterTile");
                break;

            case TileType.Land:

                spriteRenderer.sprite = GameController.GetSprite("GrassTile"); ;
                if (IsNextToSea)
                {
                    transform.Find("Sand").gameObject.SetActive(true);
                    SetSeaBorder();
                }
                break;
        }


    }

    void SetSeaBorder()
    {
        TilePathFindingComponent.DirectionList.ForEach(direction =>
        {
            if (GameController.PathFindingComponent.GetNeighbour(this, direction).Type == TileScript.TileType.Water)
            {
                transform.Find(direction.ToString()).gameObject.SetActive(true);
            }
        });
    }



    void Start()
    {
        CalculateSprite();
        PerformCityTurn();
    }



    void PerformCityTurn()
    {
        Debug.Log("hELLo I am city and my team is:" + Team);

        if (Type != TileType.City || IsNextToSea || Team == 0)
        {
            return;
        }

        if (UnitOnTile != null)
        {
            UnitOnTile.Health += 10;
            UnitOnTile.Morale += 10;
            return;
        }


        var tempUnit = Instantiate(UnitRef, transform.position, new Quaternion()).GetComponent<UnitScript>();
        tempUnit.Team = Team;
        tempUnit.TileStandingOn = this;
        UnitOnTile = tempUnit;

    }

}
