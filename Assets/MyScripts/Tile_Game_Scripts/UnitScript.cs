using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

public class UnitScript : MonoBehaviour
{
    Color ogColor;
    SpriteRenderer spriteRenderer => GetComponent<SpriteRenderer>();
    SpriteRenderer teamShaderSpriteRenderer => transform.Find("TeamShader").gameObject.GetComponent<SpriteRenderer>();
    TextMeshPro HealthText => transform.Find("Text").gameObject.GetComponent<TextMeshPro>();

    TileScript CurrentlyTravellingTo = null;
    bool TurnHasEnded = false;

    public GameControllerScript _gc = null;
    public GameControllerScript GameController
    {
        get => _gc != null ? _gc : _gc = FindObjectsOfType<GameControllerScript>().FirstOrDefault().GetComponent<GameControllerScript>();
    }
    List<Color> UnitColorList => GameController.ColorList.Select(c => new Color(c.r, c.g, c.b, 0.5f)).ToList();



    public int _m = 10;
    public int Morale
    {
        get => _m;
        set
        {
            _m = System.Math.Min(100, Health);
            CaculateText();
        }
    }

    private int _h = 10;
    public int Health
    {
        get => _h;
        set
        {
            _h = System.Math.Min(100, value);
            if (TileStandingOn.Type != TileScript.TileType.Water)
            {
                CalculateSprite();
            }
            CaculateText();
        }
    }
    void CalculateSprite(bool IsBoat = false)
    {
        Sprite sprite = (IsBoat || TileStandingOn.Type == TileScript.TileType.Water) ?
            GetSprite("boat")
            : SpriteLevels();

        spriteRenderer.sprite = sprite;
        teamShaderSpriteRenderer.sprite = sprite;
    }

    void CaculateText()
    {
        HealthText.text = Morale + "/" +
            "<br>" + "   /" + Health;
    }
    void ToggleTextOnOff(bool On)
    {
        Color tempColor = HealthText.color;
        tempColor.a = On ? 1 : 0;
        HealthText.color = tempColor;
    }

    public Sprite SpriteLevels() =>
    Health switch
    {
        var h when h < 33 => GetSprite("light_unit"),
        var h when h >= 33 && h <= 66 => GetSprite("mid_unit"),
        var h when h > 66 => GetSprite("heavy_unit"),
        _ => null
    };

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
            teamShaderSpriteRenderer.color = UnitColorList[value];
            //spriteRenderer.color = UnitColorList[Team];
        }
    }

    [SerializeField]
    private TileScript _ts;
    public TileScript TileStandingOn
    {
        get => _ts;
        set
        {
            if (_ts != null && _ts.UnitOnTile != null)
            {
                _ts.UnitOnTile = null;
            }

            _ts = value;

            value.UnitOnTile = this;

            if (value.Team != Team && value.Type != TileScript.TileType.Water)
            {
                value.Team = Team;
                value.IsCapital = false;
            }

            //transform.position = value.transform.position;  OLLDDDDDDDDD
        }
    }


    public bool _mt = false;
    public bool MovedThisTurn
    {
        get => _mt;
        set
        {
            _mt = value;
            //ogColor = (ogColor == Color.white) ? spriteRenderer.color : ogColor;
            //spriteRenderer.color = value ? Color.black : ogColor;
            ////ogColor = value ? ogColor : spriteRenderer.color;
            //print("VALUE IS" + value);

            if (value)
            {
                ogColor = spriteRenderer.color;
                //spriteRenderer.color = Color.grey;
                spriteRenderer.color = Color.black;
            }
            else
            {
                spriteRenderer.color = ogColor;
            }


        }
    }

    public void MoveToOrAttackTile(TileScript tileToMoveTo)
    {
        StartCoroutine(MoveToOrAttackTileCoroutine(tileToMoveTo));
    }

    IEnumerator MoveToOrAttackTileCoroutine(TileScript tileToMoveTo)
    {
        GameController.WaitingForUnitTurnToEnd = true;
        CurrentlyTravellingTo = tileToMoveTo;

        if (CurrentlyTravellingTo.Type == TileScript.TileType.Water)
        {
            CalculateSprite(true);
        }

        while (CurrentlyTravellingTo != null)
        {
            yield return null;
        }

        PerformMergeOrAttack(tileToMoveTo);
        TurnHasEnded = true;
    }

    void OnDestroy()
    {
        CheckIfEndturn();
    }
    private void Update()
    {
        CheckIfEndturn();
    }

    void CheckIfEndturn()
    {
        if (TurnHasEnded)
        {
            GameController.WaitingForUnitTurnToEnd = false;
            TurnHasEnded = false;
        }
    }

    private void FixedUpdate() //travelling update
    {
        if (CurrentlyTravellingTo == null) { return; }

        if (Vector2.Distance(transform.position, CurrentlyTravellingTo.transform.position) < 0.01f)
        {
            this.transform.position = CurrentlyTravellingTo.transform.position;
        }
        if (CurrentlyTravellingTo.transform.position == this.transform.position)
        {
            CurrentlyTravellingTo = null;
            return;
        }

        float moveSpeed = 1f;
        Vector3 direction = CurrentlyTravellingTo.transform.position - transform.position;
        direction.Normalize();
        this.transform.position += direction * moveSpeed * Time.fixedDeltaTime;
    }


    void PerformMergeOrAttack(TileScript desination)
    {

        UnitScript desinationUnit = desination.UnitOnTile;

        if (desinationUnit != null && desinationUnit != this)
        {
            if (desinationUnit.Team == Team)
            {
                Health += desinationUnit.Health;
                Destroy(desinationUnit.gameObject);

            }
            else
            {
                (int thisHealth, int enemyHealth) = OutComeOfBattle(this, desinationUnit);
                if (thisHealth <= 0)
                {
                    desinationUnit.Health = enemyHealth;
                    Destroy(this.gameObject);
                    return;
                }
                else
                {
                    Health = thisHealth;
                    Destroy(desinationUnit.gameObject);

                }

            }
        }

        TileStandingOn = desination;
        CaputreSurroundingTiles();
        MovedThisTurn = true;
        CalculateSprite();
    }

    void CaputreSurroundingTiles()
    {
        if (TileStandingOn.Type == TileScript.TileType.Water)
        {
            return;
        }

        List<TileScript> tilesToCapture = GameController.PathFindingComponent.GetAllNeighboursToADistance(TileStandingOn, 2, false);
        tilesToCapture
              .Where(t => t.UnitOnTile == null)
              .ToList() // Convert to a list to avoid null reference issues
              .ForEach(t => t.Team = Team);
    }

    (int, int) OutComeOfBattle(UnitScript unit1, UnitScript unit2)
    {
        float RANDOMNESSOFWAR = 1;

        int unit1Health = unit1.Health - (int)(unit2.Health * unit2.Morale * Random.Range(1f, RANDOMNESSOFWAR));
        int unit2Health = unit2.Health - (int)(unit2.Health * unit2.Morale * Random.Range(1f, RANDOMNESSOFWAR));

        if (unit1Health < 0 && unit2Health < 0)
        {
            unit2Health = 1;
        }

        return (unit1Health, unit2Health);
    }

    //bool WillWinBattle(TileScript Loction)
    //{
    //    UnitScript enemeyUnit = Loction.UnitOnTile;
    //    if (enemeyUnit == null) { return true; }

    //    (int unit1Health, int unit2Health) = OutComeOfBattle(this, enemeyUnit);

    //    return unit1Health > 0;
    //}

    public float ChanceToWInBattle(TileScript Loction)
    {
        UnitScript enemeyUnit = Loction.UnitOnTile;
        if (enemeyUnit == null) { return 1; }

        (int unit1Health, int unit2Health) = OutComeOfBattle(this, enemeyUnit);

        if (unit1Health > 0) { return 1; }

        return 1 / (unit2Health);
    }

    //public void Start()
    //{
    //    CaculateText();
    //}
}
