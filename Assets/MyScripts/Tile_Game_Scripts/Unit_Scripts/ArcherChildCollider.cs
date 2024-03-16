using UnityEngine;

public class ArcherChildCollider : ComponentCacher
{
    ArcherUnit ParentUnit => CreateOrGetComponent<ArcherUnit>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<VikingUnit>(out var vikingUnit))
        {
            ParentUnit.VikingUnitList.Add(vikingUnit);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<VikingUnit>(out var vikingUnit))
        {
            ParentUnit.VikingUnitList.Remove(vikingUnit);
        }
    }


}