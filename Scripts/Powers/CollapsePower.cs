using ArknightsMap.Scripts.Monsters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class CollapsePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public override bool ShouldScaleInMultiplayer => true;

    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png"
    );

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature target, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (wasRemovalPrevented || target != base.Owner) return;
        Creature mandragora = base.CombatState.Enemies.First(m => !(m.Monster is TatteredPillar));
        if (mandragora.IsAlive)
        {
            MoveState curState = mandragora.Monster.NextMove;
            MoveState newSummonState = ((Mandragora)mandragora.Monster).GetSummonState();
            newSummonState.FollowUpState = curState.FollowUpState;
            foreach (var (k, v) in mandragora.Monster.MoveStateMachine.States)
            {
                if (v is not MoveState) continue;
                MoveState moveState = (MoveState)v;
                if (moveState.FollowUpState.Id == curState.Id)
                {
                    moveState.FollowUpState = curState.FollowUpState;
                }
            }
            newSummonState.RegisterStates(mandragora.Monster.MoveStateMachine.States);
            mandragora.Monster.SetMoveImmediate(newSummonState);
            if (mandragora.HasPower<StoneshieldPower>())
            {
                await PowerCmd.Remove<StoneshieldPower>(mandragora);
            }
        }
        foreach (var m in CombatState.Enemies)
        {
            if (m.IsAlive && m.Monster is not TatteredPillar)
            {
                await CreatureCmd.Damage(choiceContext, m, Amount, ValueProp.Unpowered | ValueProp.Unblockable, m, null);
                await CreatureCmd.LoseMaxHp(choiceContext, m, Amount, false);
            }
        }
    }
}