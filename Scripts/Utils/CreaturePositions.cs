using ArknightsMap.Scripts.Encounters;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace ArknightsMap.Scripts.Utils;

[RegisterSingleton]
public sealed class CreaturePositions : HookedSingletonModel
{
    private static Dictionary<Creature, int> Positions = new();
    private static DamageVar damage = new DamageVar(20, ValueProp.Move);
    private static int WindBlowTurn = 0;
    private static int WindBlowDirection = 0;

    public CreaturePositions()
        : base(HookType.Combat) { }

    public static List<Creature> GetCreaturesInPosition(int pos)
    {
        return Positions.Where(kv => kv.Value == pos).Select(kv => kv.Key).ToList();
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        Positions.Clear();
        WindBlowTurn = 0;
        WindBlowDirection = 0;
        return Task.CompletedTask;
    }

    public override async Task BeforeCombatStart()
    {
        int playerPos = 3;
        if (CurrentCombatState!.Encounter is AbstractSnowyMountainEncounter myEncounter)
        {
            playerPos = myEncounter.playerStartPosition;
            WindBlowTurn = myEncounter.windBlowTurn;
            WindBlowDirection = myEncounter.windBlowDirection;
        }
        foreach (Creature c in CurrentCombatState.PlayerCreatures)
        {
            Positions[c] = playerPos;
        }
        foreach (Creature c in CurrentCombatState.Enemies)
        {
            if (c.SlotName != null && (c.SlotName[0] <= '9') && (c.SlotName[0] >= '0'))
            {
                Positions[c] = c.SlotName[0] - '0';
            }
            else
            {
                Positions[c] = 6;
            }
        }
    }

    public override async Task AfterSideTurnEndLate(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (
            side == CombatSide.Player
            && WindBlowTurn != 0
            && CurrentCombatState!.PlayerCreatures.First().Player!.PlayerCombatState!.TurnNumber % WindBlowTurn == 0
        )
        {
            await BlowWind(WindBlowDirection);
        }
    }

    public static async Task MoveTo(Creature c, int slot)
    {
        Positions[c] = slot;
    }

    public static bool IsBlock(Creature x, Creature y)
    {
        return Math.Abs(Positions.GetValueOrDefault(x) - Positions.GetValueOrDefault(y)) <= 1;
    }

    public static async Task BlowWind(int direction)
    {
        List<Creature> allAffectedCreatures = [];
        if (direction == -1)
        {
            List<Creature> creaturesInPos = GetCreaturesInPosition(1);
            int startPos = 2;
            if (creaturesInPos.Count > 0)
            {
                await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), creaturesInPos, damage, null, null, null);
                GD.Print($"Damage Done");
                foreach (Creature c in creaturesInPos)
                {
                    GD.Print($"Start handling creature {c.Name}");
                    if (c.IsAlive)
                    {
                        if (c.IsPlayer)
                        {
                            await PowerCmd.Apply<RingingPower>(new BlockingPlayerChoiceContext(), c, 1, c, null);
                        }
                        else
                        {
                            await CreatureCmd.Stun(c);
                        }
                    }
                }
                while (GetCreaturesInPosition(startPos).Count > 0)
                {
                    startPos++;
                }
            }
            for (int pos = startPos; pos <= 9; pos++)
            {
                GD.Print($"Start handling position {pos}");
                List<Creature> creatures = GetCreaturesInPosition(pos);
                foreach (Creature c in creatures)
                {
                    GD.Print($"Start handling creature {c.Name}");
                    Positions[c] = pos - 1;
                    allAffectedCreatures.Add(c);
                    allAffectedCreatures.AddRange(c.Pets);
                }
            }
        }
        else
        {
            List<Creature> creaturesInPos = GetCreaturesInPosition(9);
            int startPos = 8;
            if (creaturesInPos.Count > 0)
            {
                await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), creaturesInPos, damage, null, null, null);
                GD.Print($"Damage Done");
                foreach (Creature c in creaturesInPos)
                {
                    GD.Print($"Start handling creature {c.Name}");
                    if (c.IsAlive)
                    {
                        if (c.IsPlayer)
                        {
                            await PowerCmd.Apply<RingingPower>(new BlockingPlayerChoiceContext(), c, 1, c, null);
                        }
                        else
                        {
                            await CreatureCmd.Stun(c);
                        }
                    }
                }
                while (GetCreaturesInPosition(startPos).Count > 0)
                {
                    startPos--;
                }
            }
            for (int pos = startPos; pos >= 1; pos--)
            {
                GD.Print($"Start handling position {pos}");
                List<Creature> creatures = GetCreaturesInPosition(pos);
                foreach (Creature c in creatures)
                {
                    GD.Print($"Start handling creature {c.Name}");
                    Positions[c] = pos + 1;
                    allAffectedCreatures.Add(c);
                    allAffectedCreatures.AddRange(c.Pets);
                }
            }
        }

        GD.Print($"allAffectedCreatures: {allAffectedCreatures}");
        Tween tween = NCombatRoom.Instance!.CreateTween().SetParallel().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        foreach (Creature c in allAffectedCreatures)
        {
            GD.Print($"Start moving creature {c.Name}");
            NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(c)!;
            tween.TweenProperty(creatureNode, "global_position:x", creatureNode.GlobalPosition.X + 200 * direction, 0.25);
        }
    }

    public static async Task Walk(Creature c, int direction)
    {
        Positions[c] = Positions.GetValueOrDefault(c) + direction;
        Tween tween = NCombatRoom.Instance!.CreateTween().SetParallel().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(c)!;
        float moveTime;
        switch (SaveManager.Instance.PrefsSave.FastMode)
        {
            case FastModeType.Instant:
                moveTime = 0.5f;
                break;
            case FastModeType.Fast:
                moveTime = 1.0f;
                break;
            default:
                moveTime = 1.25f;
                break;
        }
        tween.TweenProperty(creatureNode, "global_position:x", creatureNode.GlobalPosition.X + 200 * direction, moveTime);
        await Cmd.Wait(moveTime);
    }

    [HarmonyPatch(typeof(NCombatRoom), "CreateAllyNodes")]
    public static class PositionPlayersAndPetsPatch
    {
        [HarmonyPostfix]
        public static void Postfix(NCombatRoom __instance, ICombatRoomVisuals ____visuals)
        {
            if (____visuals.Encounter is AbstractSnowyMountainEncounter encounter)
            {
                int diff = encounter.playerStartPosition - 3;
                if (diff != 0)
                {
                    foreach (NCreature creature in __instance.CreatureNodes)
                        if (____visuals.Allies.Contains(creature.Entity))
                            creature.Position = new Vector2(creature.Position.X + 300 * diff, creature.Position.Y);
                }
            }
        }
    }
}
