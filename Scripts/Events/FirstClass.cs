using ArknightsMap.Scripts.Acts;
using ArknightsMap.Scripts.Encounters;
using ArknightsMap.Scripts.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Events;

[RegisterActEvent(typeof(Laterano))]
public sealed class FirstClass : ModEventTemplate
{
    public override EventAssetProfile AssetProfile => new(InitialPortraitPath: $"res://ArknightsMap/images/events/{GetType().Name}.png");
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(13m, ValueProp.Unblockable | ValueProp.Unpowered), new IntVar("Health", 5)];

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
        [
            new EventOption(this, TakeDamage, InitialOptionKey("TAKE_DAMAGE"), [.. HoverTipFactory.FromRelic<Awake>()]),
            new EventOption(this, GainHealth, InitialOptionKey("GAIN_HEALTH")),
        ];

    // 失去生命
    private async Task TakeDamage()
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, DynamicVars.Damage, null, null);
        await GainRelic();
    }

    // 获得生命
    private async Task GainHealth()
    {
        await CreatureCmd.GainMaxHp(Owner!.Creature, DynamicVars["Health"].BaseValue);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.GAIN_HEALTH.description"));
    }

    // 进入事件第二阶段，两个选项：选择药水或者选择卡牌

    private async Task GainRelic()
    {
        await RewardsCmd.OfferCustom(Owner!, [new RelicReward(ModelDb.Relic<Awake>().ToMutable(), Owner!)]);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.GAIN_RELIC.description"));
    }

    public override bool IsAllowed(IRunState runState)
    {
        return runState.Act is Laterano && runState.Act.AllBossEncounters.Any(encounter => encounter is TheSaintBoss);
    }
}
