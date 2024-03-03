using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class CreateUnitOrBuildingComponent : MonoBehaviour
{
    public GameObject GenericBasePrefab => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/Objects/GenericBase.prefab").GameObject();
    public GameObject CreateUnitOrBuilding(int Team, TileScript Tile, string addComponent)
    {
        GameObject newUnitGameObj = GenericBasePrefab;
        Type componentType = Type.GetType(addComponent);
        newUnitGameObj.AddComponent(componentType);

        GameObject newObj = Instantiate(newUnitGameObj, Tile.transform.position, new Quaternion());
        ITeamTileInterface Interace = newObj.GetComponent<ITeamTileInterface>();
        Interace.Team = Team;
        Interace.TileOn = Tile;

        return newObj;
    }
}