using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CityComponent : MonoBehaviour
{
    TileScript tileScript;

    int Team;


    int _c = 0;

    private int Counter
    {
        get => _c;
        set
        {
            _c = value;
            if (_c == RequiredAmount)
            {
                createBuilderUnit();
                _c = 0;
            }
        }
    }
    int RequiredAmount => 100 - (FarmNeighbours.Count() * 10);

    List<TileScript> FarmNeighbours => tileScript
     .Neighbours.Where(t =>
         t.GetComponent<Farmbuilding>() != null)
     .ToList();


    void Start()
    {
        tileScript = GetComponent<TileScript>();
        StartCoroutine(CountIncrease());
    }

    IEnumerator CountIncrease()
    {
        while (true)
        {
            yield return new WaitForSeconds(2);
            Counter++;
        }
    }

    void createBuilderUnit()
    {

    }

    void CalculateText()
    {

    }
}