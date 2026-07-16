    

using Archetto.Scripts.Pools;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using System.Collections.Immutable;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Unlocks;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Keywords;
using Archetto.Scripts.Enums;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace Archetto.Scripts.Relics.Ancient.Paganini;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class WeiLaterano : ModRelicTemplate

{

    // 小图标
    public override string PackedIconPath => $"res://Archetto/images/relic/{Id.Entry.ToLowerInvariant()}.png";
    // 轮廓图标
    protected override string PackedIconOutlinePath => $"res://Archetto/images/relic/{Id.Entry.ToLowerInvariant()}.png";
    // 大图标
    protected override string BigIconPath => $"res://Archetto/images/relic/{Id.Entry.ToLowerInvariant()}.png";
    // 
    private bool _isActivating;
    private int _combatRewardsSeen;

    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override bool ShowCounter => true;

    public override int DisplayAmount
    {
        get
        {
            if (!IsActivating)
            {
                return CombatRewardsSeen % 2;
            }
            return 2;
        }
    }

    private bool IsActivating
    {
        get => _isActivating;
        set
        {
            AssertMutable();
            _isActivating = value;
            InvokeDisplayAmountChanged();
        }
    }

    [SavedProperty]
    public int CombatRewardsSeen
    {
        get => _combatRewardsSeen;
        set
        {
            AssertMutable();
            _combatRewardsSeen = value;
        }
    }

    private bool IsInTriggeringCombat => CombatRewardsSeen > 0 && CombatRewardsSeen % 2 == 1;

    public override bool IsAllowed(IRunState runState)
    {
        return runState.Players.Count == 1;
    }
    
    public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
	{
		if (player != base.Owner)
		{
			return false;
		}
		if (room == null || (room.RoomType != RoomType.Monster && room.RoomType != RoomType.Boss && room.RoomType != RoomType.Elite))
		{
			return false;
		}
        if (!IsInTriggeringCombat)
        {
            return false;
        }
        
        var options = CardCreationOptions.ForRoom(Owner, RoomType.Boss)
            .WithFlags(CardCreationFlags.NoUpgradeRoll | CardCreationFlags.NoHookUpgrades);
        var results = CardFactory.CreateForReward(player, 3, options).ToList();
        var cards = new List<CardModel>();
        foreach (var result in results)
        {
            var card = result.Card;
            if (card.IsUpgradable)
                CardCmd.Upgrade(card);
            cards.Add(card);
        }

        var rerollOptions = CardCreationOptions.ForRoom(Owner, RoomType.Boss)
            .WithFlags(CardCreationFlags.NoUpgradeRoll | CardCreationFlags.NoHookUpgrades);

        rewards.Add(new CardReward(cards, CardCreationSource.Encounter, player, rerollOptions));
        
		return true;
	}

    public override bool TryModifyCardRewardOptionsLate(Player player, List<CardCreationResult> cardRewards, CardCreationOptions options)
	{
		if (player != base.Owner)
		{
			return false;
		}
		if (!options.Flags.HasFlag(CardCreationFlags.NoUpgradeRoll | CardCreationFlags.NoHookUpgrades))
		{
			return false;
		}
		foreach (CardCreationResult cardReward in cardRewards)
		{
			CardModel card = cardReward.Card;
			if (card.IsUpgradable)
			{
				CardModel card2 = base.Owner.RunState.CloneCard(card);
				CardCmd.Upgrade(card2);
				cardReward.ModifyCard(card2, this);
			}
		}
		return true;
	}
        
    

    public override Task BeforeCombatRewardOffered(RewardsSet rewards, CombatRoom room)
    {
        if (rewards.Player != base.Owner)
            return Task.CompletedTask;
        if (rewards.Rewards.All((Reward r) => !(r is CardReward)))
            return Task.CompletedTask;

        if (IsInTriggeringCombat)
        {
            TaskHelper.RunSafely(DoActivateVisuals());
        }

        CombatRewardsSeen++;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    private async Task DoActivateVisuals()
    {
        IsActivating = true;
        Flash();
        await Cmd.Wait(1f);
        IsActivating = false;
    }

    private static bool CardPoolFilter(CardModel card, List<CardCreationResult> rewardOptions, bool allowDupes)
    {
        if (card.Rarity == CardRarity.Rare)
        {
            if (!allowDupes)
            {
                return rewardOptions.TrueForAll((CardCreationResult o) => o.originalCard.Id != card.Id);
            }
            return true;
        }
        return false;
        }
}