using ArknightsMap.Scripts.Powers;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;

public abstract class AbstractSankta : ModMonsterTemplate
{
    // 配置项：由子类指定的常量，不是运行时状态
    protected abstract int BulletMax { get; }
    protected abstract int InitialBullet { get; }

    private SanktaBulletBar? _bulletBar;

    // 子弹数量永远以 BulletPower 层数为准，单一数据源
    public int Bullet => Creature?.GetPowerAmount<BulletPower>() ?? 0;

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<BulletPower>(new ThrowingPlayerChoiceContext(), Creature, InitialBullet, Creature, null);

        var creatureNode = Creature.GetCreatureNode();
if (creatureNode != null)
{
    // 等一帧，确保 Visuals 子场景实例化完成
    await creatureNode.ToSignal(creatureNode.GetTree(), SceneTree.SignalName.ProcessFrame);

    // 方式一：用游戏自己的 API（TestSubject 里 GetSpecialNode<CanvasGroup>("%CanvasGroup") 就是这么查的）
    var bounds = creatureNode.GetSpecialNode<Control>("%Bounds");



    GD.Print($"[Sankta] bounds: {bounds != null}, size: {bounds?.Size}");

    _bulletBar = SanktaBulletBar.Create(Creature, BulletMax);
    if (bounds != null && bounds.Size.X > 0)
    {
        bounds.AddChild(_bulletBar);
        _bulletBar.FitToWidth(bounds.Size.X, BulletMax);
        _bulletBar.Position = new Vector2(0, bounds.Size.Y - 50); // 贴在血条正下方
    }
    else
    {
        creatureNode.AddChild(_bulletBar);
        _bulletBar.FitToWidth(180, BulletMax);
        _bulletBar.Position = new Vector2(-90, 60);
    }
}

    Creature.PowerApplied += OnBulletPowerChanged;
    Creature.PowerRemoved += OnBulletPowerChanged;
    }

    private void OnBulletPowerChanged(PowerModel power)
{
    
}

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is BulletPower)
        _bulletBar?.Refresh();
        return base.AfterPowerAmountChanged(choiceContext, power, amount, applier, cardSource);
    }

public override void BeforeRemovedFromRoom()
{
    Creature.PowerApplied -= OnBulletPowerChanged;
    Creature.PowerRemoved -= OnBulletPowerChanged;
}

    public async Task UseBullet(int count)
    {
        int actual = Math.Min(count, Bullet);   // 不够就全打光，不会扣成负数
        if (actual > 0)
            await PowerCmd.Apply<BulletPower>(new ThrowingPlayerChoiceContext(), Creature, -actual, Creature, null);
    }

    public async Task AddBullet(int count)
    {
        int actual = Math.Min(count, BulletMax - Bullet);  // 只补到上限
        if (actual > 0)
            await PowerCmd.Apply<BulletPower>(new ThrowingPlayerChoiceContext(), Creature, actual, Creature, null);
    }
}