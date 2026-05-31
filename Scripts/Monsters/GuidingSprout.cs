using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class GuidingSprout : ModMonsterTemplate
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 55, 50);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 55, 50);
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn"
    );
    
    private int AttackDamage = AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState attack1 = new MoveState(
            "ATTACK1",
            async targets =>
            {
                await DamageCmd.Attack(AttackDamage).FromMonster(this)
                    .Execute(null);
            },
            new SingleAttackIntent(AttackDamage)
        );
        MoveState attack2 = new MoveState(
            "ATTACK2",
            async targets =>
            {
                await DamageCmd.Attack(AttackDamage).FromMonster(this)
                    .Execute(null);
            },
            new SingleAttackIntent(AttackDamage)
        );
        MoveState summon = new MoveState(
            "SUMMON",
            async targets =>
            {
                Creature m = await CreatureCmd.Add<TreeShield>(CombatState, "first");
                await CreatureCmd.SetCurrentHp(m, m.MaxHp * Creature.CurrentHp / Creature.MaxHp);
                await CreatureCmd.Kill(Creature);
            },
            new SummonIntent()
        );

        attack1.FollowUpState = attack2;
        attack2.FollowUpState = summon;
        summon.FollowUpState = summon;

        list.Add(attack1);
        list.Add(attack2);
        list.Add(summon);

        return new MonsterMoveStateMachine(list, attack1);
    }
}