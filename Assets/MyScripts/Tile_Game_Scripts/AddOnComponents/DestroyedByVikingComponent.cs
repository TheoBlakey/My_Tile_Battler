using UnityEngine;

public class DestroyedByVikingComponent : MonoBehaviour
{
    void OnTriggerEnter(Collider collision)
    {
        if (collision.TryGetComponent<VikingUnit>(out var x))
        {
            Destroy(gameObject);
        }
    }
}