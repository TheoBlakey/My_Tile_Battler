using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
    public bool IsCapital = false;

    [SerializeField]
    public Vector2Int GridLocation;

    List<Color> TileColorList => GameController.ColorList.Select(c => new Color(c.r, c.g, c.b, 0.5f)).ToList();


    [SerializeField]
    private GameObject TeamShader;

    [SerializeField]
    public int _t;
    public int Team
    {
        get => _t;
        set
        {
            _t = value;
            TeamShader = TeamShader != null ? TeamShader : transform.Find("TeamShader").gameObject;

            if (Type != TileType.City)
            {
                TeamShader.GetComponent<SpriteRenderer>().color = TileColorList[value];
                return;
            }
            Color c = TileColorList[value];
            TeamShader.GetComponent<SpriteRenderer>().color = new Color(c.r, c.g, c.b, 0.25f);

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

    public UnitScript? UnitOnTile;


    public void CalculateSprite()
    {
        switch (Type)
        {
            case TileType.City:

                if (IsCapital)
                {
                    spriteRenderer.sprite = GetSprite("CapitalTile");
                    break;
                }
                if (IsNextToSea)
                {
                    spriteRenderer.sprite = GetSprite("PortTile");
                    break;
                }
                spriteRenderer.sprite = GetSprite("CityTile");
                break;

            case TileType.Water:

                spriteRenderer.sprite = GetSprite("WaterTile");
                break;

            case TileType.Land:

                spriteRenderer.sprite = GetSprite("GrassTile"); ;
                if (IsNextToSea)
                {
                    transform.Find("Sand").gameObject.SetActive(true);
                    SetSeaBorder();
                }
                break;
        }


    }
    private Sprite GetSprite(string spriteName)
    {
        string path = "Assets/Art/Hex_Tiles/" + spriteName + ".png";
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
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
        //CalculateSprite();
        PerformCityTurn();
    }



    public void PerformCityTurn()
    {

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


        var Unit = Instantiate(UnitRef, transform.position, new Quaternion()).GetComponent<UnitScript>();
        Unit.Team = Team;
        Unit.TileStandingOn = this;

    }

}
