using UnityEngine;

public class ArcherChildCollider : MonoBehaviour
{
    ArcherUnit parentUnit;
    private void Start()
    {
        parentUnit = GetComponent<ArcherUnit>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<VikingUnit>(out var vikingUnit))
        {
            parentUnit.VikingUnitList.Add(vikingUnit);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<VikingUnit>(out var vikingUnit))
        {
            parentUnit.VikingUnitList.Remove(vikingUnit);
        }
    }


}