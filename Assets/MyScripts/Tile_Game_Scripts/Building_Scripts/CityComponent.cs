using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CityComponent : ComponentCacher
{
    TileScript TileScript => CreateOrGetComponent<TileScript>();
    TextMeshPro TextComponent => CreateOrGetComponent<TextMeshPro>("Text");
    CreateUnitOrBuildingComponent Creator => CreateOrGetComponent<CreateUnitOrBuildingComponent>();


    int _c = 0;
    private int Counter
    {
        get => _c;
        set
        {
            _c = value;
            if (_c == RequiredAmount)
            {
                Creator.CreateUnitOrBuilding(TileScript.Team, TileScript, nameof(BuilderUnit));
                _c = 0;
            }
            CalculateText();
        }
    }
    int RequiredAmount => 100 - (FarmNeighbours.Count() * 10);
    IEnumerator CountIncrease()
    {
        while (true)
        {
            yield return new WaitForSeconds(2);
            if (!IsUnitOnTile) Counter++;
        }
    }

    List<TileScript> FarmNeighbours => TileScript
     .Neighbours.Where(t =>
         t.GetComponent<Farmbuilding>() != null)
     .ToList();

    public void Start()
    {
        StartCoroutine(CountIncrease());
    }

    void CalculateText()
    {
        TextComponent.text = Counter + "/" +
            "<br>" + "   /" + RequiredAmount;
    }

    private bool _ut;
    public bool IsUnitOnTile
    {
        get => _ut;
        set
        {
            _ut = value;
            HideText(value);
            Counter = 0;
        }
    }
    void HideText(bool On)
    {
        Color temp = TextComponent.color;
        temp.a = On ? 0 : 1;
        TextComponent.color = temp;
    }
}