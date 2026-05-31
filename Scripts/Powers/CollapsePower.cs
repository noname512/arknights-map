using ArknightsMap.Scripts.Monsters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
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
        Creature partner = base.CombatState.Enemies.FirstOrDefault(m => !(m.Monster is TatteredPillar), null);
        if (partner == null) return;
        if (partner.IsAlive)
        {
            MoveState curState = partner.Monster.NextMove;
            MoveState newState;
            if (partner.Monster is Mandragora)
            {
                newState = ((Mandragora)(partner.Monster)).GetSummonState();
            }
            else
            {
                newState = new MoveState("STUN", _ => { return Task.CompletedTask;}, new StunIntent());
            }
            if (curState.Intents.OfType<AttackIntent>().Any())
            {
                newState.FollowUpState = curState.FollowUpState;
                foreach (var (k, v) in partner.Monster.MoveStateMachine.States)
                {
                    if (v is not MoveState) continue;
                    MoveState moveState = (MoveState)v;
                    if (moveState.FollowUpState.Id == curState.Id)
                    {
                        moveState.FollowUpState = curState.FollowUpState;
                    }
                }
            }
            else
            {
                newState.FollowUpState = curState;
            }
            newState.RegisterStates(partner.Monster.MoveStateMachine.States);
            partner.Monster.SetMoveImmediate(newState);
            
            if (partner.HasPower<StoneshieldPower>())
            {
                await PowerCmd.Remove<StoneshieldPower>(partner);
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