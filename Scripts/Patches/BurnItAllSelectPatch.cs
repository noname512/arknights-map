using ArknightsMap.Scripts.Utils;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;

namespace ArknightsMap.Scripts.Patches;

class BurnItAllSelectPatch
{
    [HarmonyPatch(typeof(NSimpleCardSelectScreen), "OnCardClicked")]
    public static class OnCardClickedPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            NSimpleCardSelectScreen __instance,
            CardModel card,
            MegaRichTextLabel ____infoLabel,
            CardSelectorPrefs ____prefs,
            HashSet<CardModel> ____selectedCards
        )
        {
            if (ModExtensions.IsBurnItAllPrefs(____prefs))
            {
                LocString locString = new LocString("relics", "ARKNIGHTS_MAP_RELIC_BURN_IT_ALL.choosed");
                locString.Add(new CardsVar(____selectedCards.Count));
                ____infoLabel.Text = locString.GetFormattedText();
            }
        }
    }
}
