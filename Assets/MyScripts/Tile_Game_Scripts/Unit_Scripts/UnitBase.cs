using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public abstract class UnitBase : MonoBehaviour, ITeamTileInterface
{
    Color ogColor;
    public GameObject FightEffect_Ref => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/Objects/Fight_Effect.prefab").GameObject();

    SpriteRenderer SpriteRenderer => GetComponent<SpriteRenderer>();
    SpriteRenderer TeamShaderSpriteRenderer => transform.Find("TeamShader").gameObject.GetComponent<SpriteRenderer>();

    TileScript TileTravellingTo = null;
    public bool IsTravelling => TileTravellingTo != null;


    ShadedOutComponent shadedOutComponent;
    public TilePathFindingComponent tilePathFindingComponent;
    public CreateUnitOrBuildingComponent Creator;

    private void Start()
    {
        shadedOutComponent = this.AddComponent<ShadedOutComponent>();
        tilePathFindingComponent = this.AddComponent<TilePathFindingComponent>();
        CalculateSprite();
        AddCollider();
        Creator = this.AddComponent<CreateUnitOrBuildingComponent>();
    }

    private void AddCollider()
    {
        gameObject.AddComponent<Rigidbody2D>();
        BoxCollider unitCollider = gameObject.AddComponent<BoxCollider>();
        unitCollider.isTrigger = true;

        Vector3 spriteSize = SpriteRenderer.bounds.size;
        unitCollider.size = spriteSize;
    }

    public abstract string SpriteLandName { get; }
    public abstract string SpriteWaterName { get; }

    void CalculateSprite()
    {
        bool travellingOnWater = TileOn?.Type == TileScript.TileType.Water || TileTravellingTo?.Type == TileScript.TileType.Water;

        Sprite sprite = travellingOnWater ?
            GetSprite(SpriteWaterName) :
            GetSprite(SpriteLandName);

        SpriteRenderer.sprite = sprite;
        TeamShaderSpriteRenderer.sprite = sprite;
    }

    private Sprite GetSprite(string spriteName)
    {
        string path = "Assets/Art/Hex_Units/" + spriteName + ".png";
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private int _t;
    public int Team
    {
        get => _t;
        set
        {
            _t = value;
            List<Color> UnitColorList = Constants.ColorList.Select(c => new Color(c.r, c.g, c.b, 0.5f)).ToList();
            TeamShaderSpriteRenderer.color = UnitColorList[value];
        }
    }

    private TileScript _ts;
    public TileScript TileOn
    {
        get => _ts;
        set
        {
            if (_ts != null) _ts.UnitOnTile = null;

            _ts = value;

            if (_ts != null) _ts.UnitOnTile = this;

            CalculateSprite();
        }
    }

    public void MoveToTile(TileScript tileToMoveTo)
    {
        TileTravellingTo = tileToMoveTo;
        StartCoroutine(MoveToTileCoroutine());
    }

    public bool IsFunctionalyPaused => Paused || IsTravelling;

    bool _p = false;
    private bool Paused
    {
        get => _p;
        set
        {
            _p = value;
            if (unitShouldShadeOnPause)
            {
                shadedOutComponent.ShadedOut = value;
            }
        }
    }

    protected bool unitShouldShadeOnPause = true;

    public IEnumerator PauseUnitForTime(int time)
    {
        Paused = true;
        yield return new WaitForSeconds(time);
        Paused = false;
    }

    int movementPauseTime = 2;
    IEnumerator MoveToTileCoroutine()
    {
        yield return PerfromMovementAnimation();
        //Movement Finished
        TileOn = TileTravellingTo = null;
        //CaputreSurroundingTiles();
        StartCoroutine(PauseUnitForTime(movementPauseTime));
    }
    bool PerfromMovementAnimation()
    {
        while (TileTravellingTo.transform.position != transform.position)
        {
            MoveObject();
            if (Vector2.Distance(transform.position, TileTravellingTo.transform.position) < 0.005f) //0.01
            {
                this.transform.position = TileTravellingTo.transform.position;
            }
        }
        return true;
    }

    void MoveObject()
    {
        float moveSpeed = 1f;
        Vector3 direction = TileTravellingTo.transform.position - transform.position;
        direction.Normalize();
        transform.position += moveSpeed * Time.fixedDeltaTime * direction;
    }

    void CaputreSurroundingTiles()
    {
        if (TileOn.Type == TileScript.TileType.Water) { return; }

        List<TileScript> tilesToCapture = tilePathFindingComponent.GetAllNeighboursToADistance(TileOn, 2, false);
        tilesToCapture
              .Where(t => t.UnitOnTile == null)
              .ToList() // Convert to a list to avoid null reference issues
              .ForEach(t => t.Team = Team);
    }
    private void OnDestroy()
    {
        TileOn = null;
    }

    protected void SetUpChildColliderCircle(string colliderName)
    {
        GameObject childOb = new("ColliderCircle");
        Type componentType = Type.GetType(colliderName);
        childOb.AddComponent(componentType);

        CircleCollider2D circleCollider = childOb.AddComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;
        circleCollider.radius = 1f;

        Instantiate(childOb, transform);
        //childOb.transform.SetParent(gameObject.transform);
    }
}
