using UnityEditor;
using UnityEngine;

public abstract class BuildingBase : ComponentCacher, ITeamTileInterface
{
    public ShadedOutComponent shadedOutComponent => CreateOrGetComponent<ShadedOutComponent>();
    public CreateUnitOrBuildingComponent Creator => CreateOrGetComponent<CreateUnitOrBuildingComponent>();
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

    public int Team { get; set; }

    public void Start()
    {
        gameObject.AddComponent<DestroyedByVikingComponent>();
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
        TileOn = null;
    }
}