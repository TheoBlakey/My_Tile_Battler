using UnityEngine;

public class UnitScript : MonoBehaviour
{
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
            transform.position = value.transform.position;
        }
    }


    public void MoveToOrAttackTile(TileScript newTile)
    {
        TileStandingOn = newTile;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
