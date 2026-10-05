using ArknightsMap.Scripts.Encounters;
using ArknightsMap.Scripts.Powers;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
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
    public static int WindBlowTurn = 0;
    public static int WindBlowDirection = 0;
    private static int POS_DIFF = 150;
    private static NSnowStormWarning? nSnowStormWarning;

    public CreaturePositions()
        : base(HookType.Combat) { }

    public static List<Creature> GetCreaturesInPosition(int pos)
    {
        return Positions.Where(kv => kv.Value == pos).Select(kv => kv.Key).ToList();
    }

    public static int PositionOfCreature(Creature c)
    {
        if (c.IsPet)
        {
            return Positions.GetValueOrDefault(c.PetOwner!.Creature);
        }
        return Positions.GetValueOrDefault(c);
    }

    public override Task BeforeRoomEntered(AbstractRoom room)
    {
        Positions.Clear();
        WindBlowTurn = 0;
        WindBlowDirection = 0;
        nSnowStormWarning = null;
        return Task.CompletedTask;
    }

    public override async Task BeforeCombatStart()
    {
        Positions.Clear();
        WindBlowTurn = 0;
        WindBlowDirection = 0;
        int playerPos;
        if (CurrentCombatState!.Encounter is AbstractSnowyMountainEncounter myEncounter)
        {
            playerPos = myEncounter.playerStartPosition;
            WindBlowTurn = myEncounter.windBlowTurn;
            WindBlowDirection = myEncounter.windBlowDirection;
            if (WindBlowTurn != 0)
            {
                nSnowStormWarning = NSnowStormWarning.Create(CurrentCombatState);
                NCombatRoom.Instance?.AddChildSafely(nSnowStormWarning);
                nSnowStormWarning.GlobalPosition = new Vector2(900, 200);
            }
        }
        else
        {
            return;
        }
        foreach (Creature c in CurrentCombatState.PlayerCreatures)
        {
            Positions[c] = playerPos;
            PositionPower power = (PositionPower)ModelDb.Power<PositionPower>().ToMutable();
            power.ChangePos(playerPos);
            await PowerCmd.Apply(new ThrowingPlayerChoiceContext(), power, c, 1, null, null);
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
            PositionPower power = (PositionPower)ModelDb.Power<PositionPower>().ToMutable();
            power.ChangePos(Positions[c]);
            await PowerCmd.Apply(new ThrowingPlayerChoiceContext(), power, c, 1, null, null);
        }
    }

    public override async Task AfterCreatureAddedToCombat(Creature c)
    {
        if (CurrentCombatState!.Encounter is AbstractSnowyMountainEncounter)
        {
            if (c.IsMonster && !c.IsPet)
            {
                if (c.SlotName != null && (c.SlotName[0] <= '9') && (c.SlotName[0] >= '0'))
                {
                    Positions[c] = c.SlotName[0] - '0';
                }
                else
                {
                    Positions[c] = 6;
                }
                PositionPower power = (PositionPower)ModelDb.Power<PositionPower>().ToMutable();
                power.ChangePos(Positions[c]);
                await PowerCmd.Apply(new ThrowingPlayerChoiceContext(), power, c, 1, null, null);
            }
            // 懒得考虑别的mod可能会导致的有玩家游戏中复活的情况了
        }
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Player && WindBlowTurn != 0 && nSnowStormWarning != null)
        {
            int restTurn = (WindBlowTurn - CurrentCombatState!.RoundNumber % WindBlowTurn) % WindBlowTurn;
            nSnowStormWarning.UpdateTips(restTurn);
        }
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEndLate(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player && WindBlowTurn != 0 && CurrentCombatState!.RoundNumber % WindBlowTurn == 0)
        {
            await BlowWind(WindBlowDirection);
        }
    }

    private static void TriggerMove(Creature c, int slot, float time)
    {
        GD.Print($"Creature {c.Name} move to pos {slot}");
        Tween tween = NCombatRoom.Instance!.CreateTween().SetParallel().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        NCreature creatureNode = NCombatRoom.Instance!.GetCreatureNode(c)!;
        tween.TweenProperty(creatureNode, "global_position:x", creatureNode.GlobalPosition.X + POS_DIFF * (slot - Positions[c]), time);
        foreach (Creature p in c.Pets)
        {
            NCreature petNode = NCombatRoom.Instance.GetCreatureNode(p)!;
            tween.TweenProperty(petNode, "global_position:x", petNode.GlobalPosition.X + POS_DIFF * (slot - Positions[c]), time);
        }
        Positions[c] = slot;
        c.GetPower<PositionPower>()!.ChangePos(slot);
    }

    public static void MoveTo(Creature c, int slot)
    {
        TriggerMove(c, slot, 0.25f);
    }

    public static async Task Walk(Creature c, int direction)
    {
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
        TriggerMove(c, Positions[c] + direction, moveTime);
        await Cmd.Wait(moveTime);
    }

    public static bool IsBlock(Creature x, Creature y)
    {
        return Math.Abs(PositionOfCreature(x) - PositionOfCreature(y)) <= 1;
    }

    public async Task BlowWind(int direction)
    {
        List<Creature> creaturesInPos = GetCreaturesInPosition(direction == 1 ? 9 : 1);
        int startPos = direction == 1 ? 8 : 2;
        bool blockMove = false;
        bool blockMoveThisPos = false;
        if (creaturesInPos.Count > 0)
        {
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), creaturesInPos, damage, null, null, null);
            foreach (Creature c in creaturesInPos)
            {
                if (!ShouldHandleCreature(c, direction))
                {
                    continue;
                }
                if (c.IsMonster && c.Monster is AbstractSnowyMountainMonster)
                {
                    await ((AbstractSnowyMountainMonster)c.Monster).OnWindBlow();
                }
                GD.Print($"Creature {c.Name} hit wall.");
                if (c.IsAlive)
                {
                    if (c.IsPlayer)
                    {
                        await PowerCmd.Apply<RingingPower>(new ThrowingPlayerChoiceContext(), c, 1, c, null);
                    }
                    else
                    {
                        await CreatureCmd.Stun(c);
                    }
                }
            }
            while (startPos <= 9)
            {
                creaturesInPos = GetCreaturesInPosition(startPos);
                int res = 0;
                if (creaturesInPos.Count > 0)
                {
                    foreach (Creature c in creaturesInPos)
                        if (c.HasPower<RollingPower>())
                        {
                            await c.GetPower<RollingPower>()!.Explode();
                        }
                        else
                        {
                            res++;
                        }
                }
                if (res == 0)
                    break;
            }
        }
        for (int pos = startPos; pos <= 9 && pos >= 1; pos -= direction)
        {
            blockMoveThisPos = false;
            List<Creature> creatures = GetCreaturesInPosition(pos);
            foreach (Creature c in creatures)
            {
                if (!ShouldHandleCreature(c, direction))
                {
                    blockMoveThisPos = true;
                    continue;
                }
                int moveDis = 1;
                if (c.HasPower<RollingPower>())
                {
                    if (Positions[CurrentCombatState!.PlayerCreatures.First()] == pos + direction * 2)
                    {
                        await c.GetPower<RollingPower>()!.Explode();
                        continue;
                    }
                    moveDis = 2;
                }

                if (blockMove)
                {
                    continue;
                }
                if (c.IsMonster && c.Monster is AbstractSnowyMountainMonster)
                {
                    await ((AbstractSnowyMountainMonster)c.Monster).OnWindBlow();
                }
                TriggerMove(c, pos + direction * moveDis, 0.25f);
            }
            blockMove = blockMoveThisPos;
        }
    }

    private static bool ShouldHandleCreature(Creature c, int direction)
    {
        if (!c.IsAlive)
        {
            return false;
        }
        for (int i = Positions[c]; i >= 1 && i <= 9; i -= direction)
        {
            List<Creature> creatures = GetCreaturesInPosition(i);
            foreach (Creature c2 in creatures)
            {
                if (c2.HasPower<StandStillPower>())
                {
                    return false;
                }
            }
        }
        return true;
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
                            creature.Position = new Vector2(creature.Position.X + POS_DIFF * diff, creature.Position.Y);
                }
            }
        }
    }
}
