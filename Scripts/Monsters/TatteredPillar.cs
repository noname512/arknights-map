using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class TatteredPillar : ModMonsterTemplate
{
    private int GetExtraHp()
    {
        Creature m = base.CombatState.Enemies.First(m => m.Monster is Mandragora);
        int hp = m.MaxHp - m.CurrentHp;
        return hp * AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 13, 10) / 10;
    }
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 10, 10) + GetExtraHp();
    public override int MaxInitialHp => MinInitialHp;
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: "res://ArknightsMap/scenes/monsters/TatteredPillar.tscn"
    );

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<CollapsePower>(new ThrowingPlayerChoiceContext(), Creature, 70m, Creature, null);
        await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), base.Creature, 1m, base.Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState sleep = new MoveState(
            "SLEEP",
            async targets => { },
            new SleepIntent()
        );

        sleep.FollowUpState = sleep;

        list.Add(sleep);

        return new MonsterMoveStateMachine(list, sleep);
    }
}