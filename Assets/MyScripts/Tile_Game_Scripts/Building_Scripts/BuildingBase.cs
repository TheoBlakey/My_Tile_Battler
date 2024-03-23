using UnityEditor;
using UnityEngine;

public abstract class BuildingBase : ComponentCacher, ITeamTileInterface
{
    public ShadeOutComponent shadedOutComponent => CreateOrGetComponent<ShadeOutComponent>();
    public CreateUnitOrBuildingComponent Creator => CreateOrGetComponent<CreateUnitOrBuildingComponent>();
    SpriteRenderer spriteRenderer => CreateOrGetComponent<SpriteRenderer>();
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
        gameObject.AddComponent<SpriteSizeColliderComponent>();
        gameObject.AddComponent<DestroyedByVikingComponent>();
        spriteRenderer.sprite = GetSprite(SpriteLandName);
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