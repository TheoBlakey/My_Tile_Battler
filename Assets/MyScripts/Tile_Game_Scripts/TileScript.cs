using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[Serializable()]
public class TileScript : MonoBehaviour
{
    public GameObject UnitRef => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/Objects/Unit.prefab").GameObject();

    SpriteRenderer spriteRenderer => GetComponent<SpriteRenderer>();
    public List<TileScript> Neighbours;
    TilePathFindingComponent _pathFindingComponent;

    private UnitBase _u;
    public UnitBase UnitOnTile
    {
        get => _u;
        set
        {
            _u = value;
            if (TryGetComponent<CityComponent>(out var city))
            {
                city.IsUnitOnTile = value != null;
            }

        }
    }

    public BuildingBase BuildingOnTile;

    bool IsSuitableForBuilding = false;
    public bool CurrntlyBuildAble => IsSuitableForBuilding || BuildingOnTile == null;
    void Start()
    {
        _pathFindingComponent = this.AddComponent<TilePathFindingComponent>();
        Neighbours = _pathFindingComponent.GetallTileNeighbours(this);

        IsSuitableForBuilding = !IsNextToSea && Type == TileType.Land && Neighbours.Any(n => n.Type == TileType.City);
    }

    [SerializeField]
    public bool IsCapital = false;

    [SerializeField]
    public Vector2Int GridLocation;

    List<Color> TileColorList => Constants.ColorList.Select(c => new Color(c.r, c.g, c.b, 0.5f)).ToList();


    [SerializeField]
    private GameObject TeamShader => transform.Find("TeamShader").gameObject;

    [SerializeField]
    public int _t;
    public int Team
    {
        get => _t;
        set
        {
            if (value == _t) return;

            _t = value;


            ShadeCities();


            if (TryGetComponent<CityComponent>(out var city))
            {
                Destroy(city);
            }

            if (_t != 0 || _t != 5)
            {
                this.AddComponent<CityComponent>();
            }
        }
    }

    void ShadeCities()
    {
        Color c = TileColorList[Team];
        if (Type == TileType.City)
        {
            c.a = 0.25f;
            //c.a = 0.75f;
        }
        if (IsCapital)
        {
            c.a = 0.125f;
        }
        TeamShader.GetComponent<SpriteRenderer>().color = c;
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

    public bool IsVulnerableToAttack => Type != TileType.City || Neighbours.All(n => n.BuildingOnTile == null);

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
            if (_pathFindingComponent.GetNeighbour(this, direction).Type == TileType.Water)
            {
                transform.Find(direction.ToString()).gameObject.SetActive(true);
            }
        });
    }
}
