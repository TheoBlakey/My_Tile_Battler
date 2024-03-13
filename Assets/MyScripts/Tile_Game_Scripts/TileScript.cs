using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[Serializable()]
public class TileScript : MonoBehaviour
{
    [SerializeField]
    public List<TileScript> _n;
    public List<TileScript> Neighbours
    {
        get => _n.Any() ? _n : _n = PathFindingComponent.GetallTileNeighbours(this);
    }
    public GameObject UnitRef => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/Objects/Unit.prefab").GameObject();

    SpriteRenderer SpriteRenderer => GetComponent<SpriteRenderer>();
    public TilePathFindingComponent PathFindingComponent => TryGetComponent<TilePathFindingComponent>(out var x) ? x : this.AddComponent<TilePathFindingComponent>();

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

    bool IsSuitableForBuilding => !IsNextToSea && Type == TileType.Land && Neighbours.Any(n => n.Type == TileType.City);
    public bool CurrntlyBuildAble => IsSuitableForBuilding || BuildingOnTile == null;

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
        SpriteRenderer.sortingOrder = 4;
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
                    SpriteRenderer.sprite = GetSprite("CapitalTile");
                    IncreaseByScale(1.3f);
                    return;
                }
                if (IsNextToSea)
                {
                    SpriteRenderer.sprite = GetSprite("PortTile");
                    IncreaseByScale(1.1f);
                    return;
                }
                SpriteRenderer.sprite = GetSprite("CityTile");
                IncreaseByScale(1.1f);
                return;

            case TileType.Water:

                SpriteRenderer.sprite = GetSprite("WaterTile");
                return;

            case TileType.Land:

                SpriteRenderer.sprite = GetSprite("GrassTile");
                if (IsNextToSea)
                {
                    transform.Find("Sand").gameObject.SetActive(true);
                    SetSeaBorder();
                }
                return;

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
            if (PathFindingComponent.GetNeighbour(this, direction).Type == TileType.Water)
            {
                transform.Find(direction.ToString()).gameObject.SetActive(true);
            }
        });

        //var DirectionOffsets = PathFindingComponent.GetDirectionOffsets(this);
        //foreach (var Neighbour in Neighbours)
        //{
        //    if (Neighbour.Type != TileType.Water) { continue; }

        //    TilePathFindingComponent.Direction direction = DirectionOffsets.FirstOrDefault(x => x.Value == (Neighbour.GridLocation.x - GridLocation.x, Neighbour.GridLocation.y - GridLocation.y)).Key;
        //    transform.Find(direction.ToString()).gameObject.SetActive(true);
        //}
    }
}
