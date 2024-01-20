using System.Linq;
using UnityEngine;

public class UnitScript : MonoBehaviour
{
    Color ogColor;
    SpriteRenderer spriteRenderer => GetComponent<SpriteRenderer>();

    private TileScript _ctt = null;
    TileScript CurrentlyTravellingTo
    {
        get => _ctt;
        set
        {
            _ctt = value;
            bool CurrentlyTravelling = value != null;
            GameController.WaitingForMovementToEnd = CurrentlyTravelling;
        }
    }

    public GameControllerScript _gc = null;
    public GameControllerScript GameController
    {
        get => _gc != null ? _gc : _gc = FindObjectsOfType<GameControllerScript>().FirstOrDefault().GetComponent<GameControllerScript>();
    }

    public int Health = 10;
    public int Morale = 10;

    public int Team;

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
            if (value.Team != Team)
            {
                value.Team = Team;
                value.IsCapital = false;
            }

            //transform.position = value.transform.position;  OLLDDDDDDDDD
        }
    }


    private bool _mt = false;
    public bool MovedThisTurn
    {
        get => _mt;
        set
        {
            _mt = value;
            if (value)
            {
                ogColor = spriteRenderer.color;
                spriteRenderer.color = Color.grey;
            }
            else
            {
                spriteRenderer.color = ogColor;
            }
        }
    }


    public void MoveToOrAttackTile(TileScript tileToMoveTo)
    {
        CurrentlyTravellingTo = tileToMoveTo;

        while (CurrentlyTravellingTo != null)
        {
        }

        PerformMergeOrAttack(tileToMoveTo);
    }

    private void FixedUpdate()
    {
        if (CurrentlyTravellingTo == null) { return; }

        if (CurrentlyTravellingTo.transform.position == this.transform.position)
        {
            CurrentlyTravellingTo = null;
        }

        float moveSpeed = 0.5f;
        Vector3 direction = CurrentlyTravellingTo.transform.position - transform.position;
        direction.Normalize();
        transform.position += direction * moveSpeed * Time.fixedDeltaTime;

    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void PerformMergeOrAttack(TileScript desination)
    {

        UnitScript desinationUnit = desination.UnitOnTile;
        bool survived = true;

        if (desinationUnit != null)
        {
            if (desinationUnit.Team == Team)
            {
                Health += desinationUnit.Health;
                Destroy(desinationUnit);
            }
            else
            {
                (int thisHealth, int enemyHealth) = OutComeOfBattle(this, desinationUnit);
                if (thisHealth <= 0)
                {
                    survived = false;
                    desinationUnit.Health = enemyHealth;
                    Destroy(this);
                }
                else
                {
                    Health = thisHealth;
                    Destroy(desinationUnit);
                }

            }
        }

        if (survived)
        {
            MovedThisTurn = true;
            TileStandingOn = desination;
        }

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

    bool WillWinBattle(TileScript Loction)
    {
        UnitScript enemeyUnit = Loction.UnitOnTile;
        if (enemeyUnit == null) { return true; }

        (int unit1Health, int unit2Health) = OutComeOfBattle(this, enemeyUnit);

        return unit1Health > 0;
    }

    public float ChanceToWInBattle(TileScript Loction)
    {
        UnitScript enemeyUnit = Loction.UnitOnTile;
        if (enemeyUnit == null) { return 1; }

        (int unit1Health, int unit2Health) = OutComeOfBattle(this, enemeyUnit);

        if (unit1Health > 0) { return 1; }

        return 1 / (unit2Health);
    }
}
