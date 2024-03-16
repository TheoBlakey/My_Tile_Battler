using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public abstract class UnitBase : ComponentCacher, ITeamTileInterface
{
    SpriteRenderer SpriteRenderer => CreateOrGetComponent<SpriteRenderer>();
    SpriteRenderer TeamShaderSpriteRenderer => CreateOrGetComponent<SpriteRenderer>("TeamShader");
    ShadedOutComponent shadedOutComponent => CreateOrGetComponent<ShadedOutComponent>();
    public TilePathFindingComponent tilePathFindingComponent => CreateOrGetComponent<TilePathFindingComponent>();


    TileScript TileTravellingTo = null;
    public bool IsTravelling => TileTravellingTo != null;
    public void Start()
    {
        CalculateSprite();
        AddCollider();
    }

    private void AddCollider()
    {
        Rigidbody2D body = gameObject.AddComponent<Rigidbody2D>();
        body.isKinematic = true;
        BoxCollider2D unitCollider = gameObject.AddComponent<BoxCollider2D>();
        unitCollider.isTrigger = true;

        Vector3 spriteSize = SpriteRenderer.sprite.bounds.size;
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
        TileOn = TileTravellingTo;
        TileTravellingTo = null;
        //CaputreSurroundingTiles();
        StartCoroutine(PauseUnitForTime(movementPauseTime));
    }
    IEnumerator PerfromMovementAnimation()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.01f);

            MoveObject();
            if (Vector2.Distance(transform.position, TileTravellingTo.transform.position) < 0.01f) //0.01
            {
                transform.position = TileTravellingTo.transform.position;
                break;
            }
        }

        yield return true;
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
