using Unity.VisualScripting;

public abstract class TeamUnit : UnitBase
{
    public override string SpriteWaterName => "boat";

    private void Start()
    {
        base.Start();
        this.AddComponent<DestroyedByVikingComponent>();
        unitShouldShadeOnPause = true;
    }

}