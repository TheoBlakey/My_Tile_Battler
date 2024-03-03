using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class VikingUnit : UnitBase
{
    public override string SpriteLandName => "boat";
    public override string SpriteWaterName => "light_unit";

    TextMeshPro TextComponent;

    private int _h = 10;
    public int Health
    {
        get => _h;
        set
        {
            if (value <= 0) { Destroy(gameObject); }

            TextComponent.text = value.ToString();
            _h = value;
        }
    }
    GameController gameController;


    public List<TeamUnit> closeUnits;

    public List<BuildingBase> closeBuildings;
    public TileScript CurrentTargetCity;
    List<TileScript> CurrentEnemyCitiesTiles => gameController.AllCities.
        Where(c => c.Team != 0 || c.Team != 5).
        ToList();

    private void Start()
    {
        TextComponent = transform.Find("Text").gameObject.GetComponent<TextMeshPro>();
        SetUpChildColliderCircle(nameof(VikingChildCollider));
        StartCoroutine(VikingOverallCoroutine());
        GameController gameController = FindObjectOfType<GameController>();

    }
    IEnumerator VikingOverallCoroutine()
    {
        while (true)
        {
            TileScript Target = FindTarget();
            TileScript firstPathMove = tilePathFindingComponent.FindPath(TileOn, Target).First();

            MoveToTile(firstPathMove);

            while (IsFunctionalyPaused)
            {
                yield return null;
            }

        }
    }

    List<TeamUnit> VunerableCloseUnits => closeUnits.
        Where(u => u.TileOn.IsVulnerableToAttack).ToList();

    TileScript FindTarget()
    {
        if (VunerableCloseUnits.Any())
        {
            TeamUnit closestUnit = VunerableCloseUnits.
                OrderBy(unit => GetDistanceAway(unit.transform)).
                FirstOrDefault();

            return closestUnit.TileOn;
        }

        if (closeBuildings.Any())
        {
            BuildingBase closestBuilding = closeBuildings.
                OrderBy(unit => GetDistanceAway(unit.transform)).
                FirstOrDefault();

            return closestBuilding.TileOn;
        }

        if (CurrentTargetCity == null || CurrentTargetCity.Team == 0 || CurrentTargetCity.Team == 5)
        {
            CurrentTargetCity = CurrentEnemyCitiesTiles.
                OrderBy(city => GetDistanceAway(city.transform)).
                FirstOrDefault();
        }

        return CurrentTargetCity;
    }

    float GetDistanceAway(Transform place)
    {
        return Vector2.Distance(place.position, transform.position);
    }
    private void OnTriggerEnter(Collider arrow)
    {
        if (arrow.TryGetComponent<Arrow>(out var arrowObj))
        {
            Destroy(arrowObj.gameObject);
            Health--;
        }
    }

}