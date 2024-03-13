using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CityComponent : MonoBehaviour
{
    TileScript tileScript;
    TextMeshPro TextComponent;
    CreateUnitOrBuildingComponent Creator;

    int _c = 0;
    private int Counter
    {
        get => _c;
        set
        {
            _c = value;
            if (_c == RequiredAmount)
            {
                Creator.CreateUnitOrBuilding(tileScript.Team, tileScript, nameof(BuilderUnit));
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

    List<TileScript> FarmNeighbours => tileScript
     .Neighbours.Where(t =>
         t.GetComponent<Farmbuilding>() != null)
     .ToList();

    void Start()
    {
        tileScript = GetComponent<TileScript>();
        StartCoroutine(CountIncrease());
        GameObject textChild = transform.Find("Text").gameObject;
        textChild.SetActive(true);
        TextComponent = textChild.GetComponent<TextMeshPro>();
        Creator = this.AddComponent<CreateUnitOrBuildingComponent>();

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
        temp.a = On ? 1 : 0;
        TextComponent.color = temp;
    }
}