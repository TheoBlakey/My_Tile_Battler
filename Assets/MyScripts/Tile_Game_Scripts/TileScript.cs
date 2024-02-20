using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[Serializable()]
public class TileScript : MonoBehaviour
{
    public GameObject UnitRef;
    //= AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/Objects/Unit.prefab").GameObject();


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
            Color c = TileColorList[value];

            if (Type == TileType.City)
            {
                c.a = 0.25f;
                //c.a = 0.75f;
            }
            if (IsCapital)
            {
                c.a = 0.125f;
            }
            //TeamShader.GetComponent<SpriteRenderer>().color = new Color(c.r, c.g, c.b, 0.25f);

            TeamShader.GetComponent<SpriteRenderer>().color = c;
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

    private void IncreaseByScale(float scale)
    {
        spriteRenderer.sortingOrder = 4;
        transform.Find("TeamShader").gameObject.GetComponent<SpriteRenderer>().sortingOrder = 5;
        transform.localScale = new Vector3(scale, scale, 1);
    }

    public void CalculateSprite()
    {
        switch (Type)
        {
            case TileType.City:

                if (IsCapital)
                {
                    spriteRenderer.sprite = GetSprite("CapitalTile");
                    IncreaseByScale(1.3f);
                    break;
                }
                if (IsNextToSea)
                {
                    spriteRenderer.sprite = GetSprite("PortTile");
                    IncreaseByScale(1.1f);
                    break;
                }
                spriteRenderer.sprite = GetSprite("CityTile");
                IncreaseByScale(1.1f);
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



    public void PerformCityTurn(int amountIncrease = 10)
    {

        if (Type != TileType.City || IsNextToSea || Team == 0)
        {
            return;
        }

        if (UnitOnTile != null)
        {
            UnitOnTile.Health += amountIncrease;
            UnitOnTile.Morale += amountIncrease;
            return;
        }

        var Unit = Instantiate(UnitRef, transform.position, new Quaternion()).GetComponent<UnitScript>();
        Unit.Team = Team;
        Unit.TileStandingOn = this;

        Unit.Health = amountIncrease;
        Unit.Morale = amountIncrease;

        if (GameController.AsyncGame && Team != 1)
        {
            Unit.StartAsyncGameturn();
        }


    }

    public void StartAsyncGameturn()
    {
        StartCoroutine(PerformAsyncGameTurn());
    }

    public IEnumerator PerformAsyncGameTurn()
    {
        int turnsWithoutUnit = 0;
        while (true)
        {
            yield return new WaitForSeconds(GameController.CityRateOfCreation);

            if (Team == 0) { continue; }


            if (UnitOnTile != null)
            {
                PerformCityTurn(1);
                turnsWithoutUnit = 0;
            }
            else if (turnsWithoutUnit < 10)
            {
                turnsWithoutUnit++;
            }
            else
            {
                PerformCityTurn();
                turnsWithoutUnit = 0;
            }
        }

    }


}
