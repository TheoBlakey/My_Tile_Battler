using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public abstract class UnitBase : MonoBehaviour
{
    Color ogColor;
    public GameObject FightEffect_Ref => AssetDatabase.LoadAssetAtPath<Object>("Assets/Objects/Fight_Effect.prefab").GameObject();

    SpriteRenderer SpriteRenderer => GetComponent<SpriteRenderer>();
    SpriteRenderer TeamShaderSpriteRenderer => transform.Find("TeamShader").gameObject.GetComponent<SpriteRenderer>();

    TileScript TileTravellingTo = null;
    public bool IsTravelling => TileTravellingTo != null;


    ShadedOutComponent shadedOutComponent;
    public TilePathFindingComponent tilePathFindingComponent;

    private void Start()
    {
        shadedOutComponent = this.AddComponent<ShadedOutComponent>();
        tilePathFindingComponent = this.AddComponent<TilePathFindingComponent>();
        CalculateSprite();
        AddCollider();
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
        bool travellingOnWater = TileStoodOn?.Type == TileScript.TileType.Water || TileTravellingTo?.Type == TileScript.TileType.Water;

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
    public TileScript TileStoodOn
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

    bool _p = false;
    public bool Paused
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
    int pauseTime = 2;
    bool unitShouldShadeOnPause = true;
    IEnumerator PauseUnitAfterMovement()
    {
        Paused = true;
        yield return new WaitForSeconds(pauseTime);
        Paused = false;
    }

    IEnumerator MoveToTileCoroutine()
    {
        yield return PerfromMovementAnimation();
        //Movement Finished
        TileStoodOn = TileTravellingTo = null;
        CaputreSurroundingTiles();
        PauseUnitAfterMovement();
    }
    bool PerfromMovementAnimation()
    {
        while (TileTravellingTo.transform.position != transform.position)
        {
            MoveObject();
            if (Vector2.Distance(transform.position, TileTravellingTo.transform.position) < 0.01f)
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
        if (TileStoodOn.Type == TileScript.TileType.Water) { return; }

        List<TileScript> tilesToCapture = tilePathFindingComponent.GetAllNeighboursToADistance(TileStoodOn, 2, false);
        tilesToCapture
              .Where(t => t.UnitOnTile == null)
              .ToList() // Convert to a list to avoid null reference issues
              .ForEach(t => t.Team = Team);
    }
    private void OnDestroy()
    {
        TileStoodOn = null;
    }
}
