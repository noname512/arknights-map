using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Enemies;

[RegisterMonster]
public class DublinnFlamechaserSoldier : ModMonsterTemplate
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 35, 30);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 35, 30);
    private int Damage1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);
    private int Damage2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    // 怪物场景
    // public override MonsterAssetProfile AssetProfile => new(
    //     VisualsScenePath: "res://Test/scenes/test_monster.tscn"
    // );

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<StrengthPower>(Creature, 5m, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var attack1 = new MoveState(
            "ATTACK1",
            async targets => await DamageCmd
                .Attack(Damage1)
                .FromMonster(this)
                // .WithAttackerAnim("Attack", 0.5f) // 如果有攻击动画，可以取消注释并替换成实际动画名称和延迟
                .Execute(null),
            new SingleAttackIntent(Damage1)
        );
        var attack2 = new MoveState(
            "ATTACK2",
            async targets => await DamageCmd
                .Attack(Damage2)
                .FromMonster(this)
                .Execute(null),
            new SingleAttackIntent(Damage2)
        );
        var attack3 = new MoveState(
            "ATTACK3",
            async targets => await DamageCmd
                .Attack(Damage1)
                .FromMonster(this)
                .Execute(null),
            new SingleAttackIntent(Damage1)
        );

        attack1.FollowUpState = attack2;
        attack2.FollowUpState = attack3;
        attack3.FollowUpState = attack1;

        return new MonsterMoveStateMachine([attack1, attack2, attack3], attack1);
    }


}