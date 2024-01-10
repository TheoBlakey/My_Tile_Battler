using UnityEngine;

public class UnitScript : MonoBehaviour
{
    public int Team;
    private TileScript _tileStandingOn;
    public TileScript TileStandingOn
    {
        get => _tileStandingOn;
        set
        {
            //if (_tileStandingOn != null || _tileStandingOn != value)
            //{
            //    _tileStandingOn.CurrentUnit = null;
            //}

            _tileStandingOn = value;
            transform.position = value.transform.position;

        }
    }

    public int Health = 10;
    public int Morale = 10;

    public void TryToMoveTile(TileScript newTile)
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
