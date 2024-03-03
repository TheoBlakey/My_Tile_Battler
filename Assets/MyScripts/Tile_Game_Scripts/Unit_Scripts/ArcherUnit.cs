using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class ArcherUnit : TeamUnit
{
    public override string SpriteLandName => "archer_unit";
    public GameObject Arrow_Ref => AssetDatabase.LoadAssetAtPath<Object>("Assets/Objects/Arrow.prefab").GameObject();
    public List<VikingUnit> VikingUnitList = new();

    bool canShoot = true;
    private void Start()
    {
        SetUpChildColliderCircle(nameof(ArcherChildCollider));
    }

    void ShootOneArrow(VikingUnit unit)
    {
        Arrow arrow = Instantiate(Arrow_Ref, transform.position, new Quaternion()).GetComponent<Arrow>();
        arrow.UnitHeadingFor = unit;
    }


    void Update()
    {
        if (canShoot & VikingUnitList.Count > 0)
        {
            VikingUnit closestUnit = VikingUnitList.
                OrderBy(unit => Vector2.Distance(unit.transform.position, transform.position)).
                FirstOrDefault();

            ShootOneArrow(closestUnit);
            StartCoroutine(TriggerShootDelay());
        }

    }

    IEnumerator TriggerShootDelay()
    {
        canShoot = false;
        yield return new WaitForSeconds(2);
        canShoot = true;
    }

}