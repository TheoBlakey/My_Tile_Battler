using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class CreateUnitOrBuildingComponent : MonoBehaviour
{
    public GameObject GenericBasePrefab => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/Objects/GenericBase.prefab").GameObject();
    public void CreateUnitOrBuilding(int Team, TileScript Tile, string addComponent)
    {
        GameObject newUnitGameObj = GenericBasePrefab;
        Type componentType = Type.GetType(addComponent);
        newUnitGameObj.AddComponent(componentType);

        ITeamTileInterface newObj = Instantiate(newUnitGameObj, Tile.transform.position, new Quaternion()).GetComponent<ITeamTileInterface>();
        newObj.Team = Team;
        newObj.TileOn = Tile;
    }
}