using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class TatteredPillar : AbstractWildsMonster
{
    private int GetExtraHp()
    {
        Creature? m = CombatState.Enemies.FirstOrDefault(m => m?.Monster is Mandragora, null);
        if (m == null)
            return 0;
        int hp = m.MaxHp - m.CurrentHp;
        return hp * AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 13, 10) / 100;
    }

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 10, 10);
    public override int MaxInitialHp => MinInitialHp;
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    public override async Task AfterAddedToRoom()
    {
        if (GetExtraHp() > 0)
        {
            await CreatureCmd.GainMaxHp(Creature, GetExtraHp());
        }
        await PowerCmd.Apply<CollapsePower>(new ThrowingPlayerChoiceContext(), Creature, 70m, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState sleep = new MoveState("SLEEP", async targets => { }, new SleepIntent());

        sleep.FollowUpState = sleep;

        list.Add(sleep);

        return new MonsterMoveStateMachine(list, sleep);
    }
}
