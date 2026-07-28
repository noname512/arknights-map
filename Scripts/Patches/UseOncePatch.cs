using System.Reflection;
using ArknightsMap.Scripts.Relics;
using ArknightsMap.Scripts.Utils;
using ArknightsMap.Scripts.Utils.MerchantEnchantment;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Keywords;
using static Godot.Control;

namespace ArknightsMap.Scripts.Patches;

class UseOncePatch
{
    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
    public static class AfterCardPlayedLatePatch
    {
        public static void Postfix(ICombatState combatState, PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Card.HasModKeyword(UseOnceKeyword.Keyword))
            {
                CardPileCmd.RemoveFromCombat(cardPlay.Card);
                if (cardPlay.Card.DeckVersion != null)
                {
                    CardPileCmd.RemoveFromDeck(cardPlay.Card.DeckVersion);
                }
                
            }
        }
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardEnteredCombat))]
    public static class AfterCardEnteredCombatPatch
    {
        public static void Postfix(CardModel card)
        {
            if (card.HasModKeyword(UseOnceKeyword.Keyword))
            {
                card.SetToFreeThisCombat();
            }
        }
    }

    
    
}