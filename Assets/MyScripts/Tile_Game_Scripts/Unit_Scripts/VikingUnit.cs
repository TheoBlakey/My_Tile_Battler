using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class VikingUnit : UnitBase
{
    public override string SpriteLandName => "boat";
    public override string SpriteWaterName => "";

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

    public List<TeamUnit> closeUnits;
    List<TeamUnit> VunerableUnits => closeUnits;

    public List<BuildingBase> closeBuildings;
    public TileScript targetCity;


    private void Start()
    {
        TextComponent = transform.Find("Text").gameObject.GetComponent<TextMeshPro>();
        SetUpChildColliderCircle();
        StartCoroutine(VikingOverallCoroutine());
    }

    void SetUpChildColliderCircle()
    {
        GameObject childOb = new("ColliderCircle");
        childOb.transform.SetParent(gameObject.transform);

        gameObject.AddComponent<VikingChildCollider>();

        CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;
        circleCollider.radius = 1f;
    }
    IEnumerator VikingOverallCoroutine()
    {
        while (true)
        {
            TileScript Target = FindTarget();
            TileScript firstPathMove = tilePathFindingComponent.FindPath(TileOn, Target).First();

            MoveToTile(firstPathMove);

            while (IsTravelling || Paused)
            {
                yield return null;
            }

        }
    }

    TileScript FindTarget()
    {
        return new TileScript();
    }

    private void OnTriggerEnter(Collider arrow)
    {
        if (arrow.TryGetComponent<Arrow>(out var x))
        {
            Destroy(x.gameObject);
            Health--;
        }
    }
}