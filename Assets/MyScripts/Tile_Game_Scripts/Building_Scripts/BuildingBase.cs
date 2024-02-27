using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public abstract class BuildingBase : MonoBehaviour
{
    public ShadedOutComponent shadedOutComponent;
    public abstract string SpriteLandName { get; }
    private TileScript _ts;
    public TileScript TileStoodOn
    {
        get => _ts;
        set
        {
            if (_ts != null) _ts.BuildingOnTile = null;

            _ts = value;

            if (_ts != null) _ts.BuildingOnTile = this;

        }
    }
    int Team;
    private void Start()
    {
        this.AddComponent<DestroyedByVikingComponent>();
        shadedOutComponent = this.AddComponent<ShadedOutComponent>();
        CalculateSprite();
    }

    void CalculateSprite()
    {
        GetComponent<SpriteRenderer>().sprite = GetSprite(SpriteLandName);
    }

    private Sprite GetSprite(string spriteName)
    {
        string path = "Assets/Art/Hex_Buildings/" + spriteName + ".png";
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private void OnDestroy()
    {
        TileStoodOn = null;
    }
}