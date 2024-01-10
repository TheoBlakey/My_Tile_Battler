using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable()]
public class TileScript : MonoBehaviour
{
    public GameObject UnitRef;

    public Sprite CapitalHexSprite, WaterHexSprite, GrassHexSprite, CityHexSprite, HarborHexSprite;

    [SerializeField]
    SpriteRenderer spriteRenderer;

    public List<TileScript> _neighbours;
    public List<TileScript> Neighbours
    {
        get => _neighbours.Any() ? _neighbours : _neighbours = GameConroller.GetallTileNeighbours(this);
        set => _neighbours = value;
    }

    [SerializeField]
    public bool isCapital = false;

    [SerializeField]
    public Vector2Int GridLocation;






    private UnitScript _currentUnit;
    public UnitScript CurrentUnit
    {
        get => _currentUnit;
        set
        {
            _currentUnit = value;
            if (Team != value.Team)
            {
                Team = value.Team;
            }
        }
    }


    List<Color> ColorList = new List<Color>
    {
       new (1, 1, 1, 1), //SPARE
       new (0, 0, 1, 0.5f), //blue
       new (1, 0, 0, 0.5f), //red
       new (0, 0.25f, 0, 0.5f), //dark green
       new (1, 0, 1, 0.5f) //magenta
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




    private GameObject HighLightedShader;
    private bool _highLighted = false;
    public bool HighLighted
    {
        get => _highLighted;
        set
        {
            this._highLighted = value;
            HighLightedShader = HighLightedShader != null ? HighLightedShader : transform.Find("HighLighted").gameObject;
            HighLightedShader.SetActive(value);

        }
    }

    public enum TileType
    { Water, City, Land }
    [SerializeField]
    public TileType Type;


    public bool isNextToSea => Neighbours.Any(t => t.Type == TileType.Water);

    public UnitScript UnitOnTile;

    [SerializeField]
    public GameControllerScript _gameController = null;
    public GameControllerScript GameConroller
    {
        get => _gameController != null ? _gameController : _gameController = FindObjectsOfType<GameControllerScript>().FirstOrDefault().GetComponent<GameControllerScript>();
    }

    public void CalculateSprite()
    {
        spriteRenderer = spriteRenderer != null ? spriteRenderer : GetComponent<SpriteRenderer>();

        switch (Type)
        {
            case TileType.City:

                //List<string> borders = new List<string> { "N", "S", "SE", "SW", "NE", "NW" };
                //borders.ForEach(e => transform.Find(e).gameObject.SetActive(true));

                if (isCapital)
                {
                    spriteRenderer.sprite = CapitalHexSprite;
                    break;
                }
                if (isNextToSea)
                {
                    spriteRenderer.sprite = HarborHexSprite;
                    break;
                }
                spriteRenderer.sprite = CityHexSprite;
                break;

            case TileType.Water:

                spriteRenderer.sprite = WaterHexSprite;
                break;

            case TileType.Land:

                spriteRenderer.sprite = GrassHexSprite;
                if (isNextToSea)
                {
                    transform.Find("Sand").gameObject.SetActive(true);
                    SetSeaBorder();
                }
                break;
        }


    }

    void SetSeaBorder()
    {
        if (GameConroller.GetNeighbourN(this).Type == TileScript.TileType.Water)
        {
            transform.Find("N").gameObject.SetActive(true);
        }
        if (GameConroller.GetNeighbourS(this).Type == TileScript.TileType.Water)
        {
            transform.Find("S").gameObject.SetActive(true);
        }
        if (GameConroller.GetNeighbourSE(this).Type == TileScript.TileType.Water)
        {
            transform.Find("SE").gameObject.SetActive(true);
        }
        if (GameConroller.GetNeighbourSW(this).Type == TileScript.TileType.Water)
        {
            transform.Find("SW").gameObject.SetActive(true);
        }
        if (GameConroller.GetNeighbourNW(this).Type == TileScript.TileType.Water)
        {
            transform.Find("NW").gameObject.SetActive(true);
        }
        if (GameConroller.GetNeighbourNE(this).Type == TileScript.TileType.Water)
        {
            transform.Find("NE").gameObject.SetActive(true);
        }
    }



    void Start()
    {
        CalculateSprite();
        PerformCityTurn();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void PerformCityTurn()
    {
        Debug.Log("hELLo I am city and my team is:" + Team);

        if (Type != TileType.City || isNextToSea || Team == 0)
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
