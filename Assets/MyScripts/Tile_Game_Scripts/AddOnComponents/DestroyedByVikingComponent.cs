using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class DestroyedByVikingComponent : MonoBehaviour
{
    GameObject FightEffect_Ref => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/Objects/Fight_Effect.prefab").GameObject();
    void OnTriggerEnter(Collider collision)
    {
        if (collision.TryGetComponent<VikingUnit>(out var x))
        {
            Instantiate(FightEffect_Ref);
            Destroy(gameObject);
        }
    }
}