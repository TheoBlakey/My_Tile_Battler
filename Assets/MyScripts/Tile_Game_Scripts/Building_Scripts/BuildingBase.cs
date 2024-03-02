using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public abstract class BuildingBase : MonoBehaviour, ITeamTileInterface
{
    public ShadedOutComponent shadedOutComponent;
    public abstract string SpriteLandName { get; }
    private TileScript _ts;
    public TileScript TileOn
    {
        get => _ts;
        set
        {
            _ts = value;
            if (_ts == null) _ts.BuildingOnTile = this;

        }
    }
    public CreateUnitOrBuildingComponent Creator;

    public int Team { get; set; }

    private void Start()
    {
        this.AddComponent<DestroyedByVikingComponent>();
        shadedOutComponent = this.AddComponent<ShadedOutComponent>();
        CalculateSprite();
        Creator = this.AddComponent<CreateUnitOrBuildingComponent>();
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
        TileOn = null;
    }
}