using UnityEngine;

public class VikingChildCollider : ComponentCacher
{
    VikingUnit parentUnit => CreateOrGetComponent<VikingUnit>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<TeamUnit>(out var u))
        {
            parentUnit.closeUnits.Add(u);
        }
        else if (collision.TryGetComponent<BuildingBase>(out var b))
        {
            parentUnit.closeBuildings.Add(b);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<TeamUnit>(out var u))
        {
            parentUnit.closeUnits.Remove(u);
        }
        else if (collision.TryGetComponent<BuildingBase>(out var b))
        {
            parentUnit.closeBuildings.Remove(b);
        }
    }


}