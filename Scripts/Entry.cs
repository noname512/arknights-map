using System.Reflection;
using ArknightsMap.Scripts.Acts;
using ArknightsMap.Scripts.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Audio;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils.Persistence;

namespace ArknightsMap.Scripts;

public sealed class WheatBeerCounter
{
    public int Value { get; set; }
}

[ModInitializer(nameof(Init))]
public class Entry
{
    public const string ModId = "ArknightsMap";
    public static readonly Logger Logger = RitsuLibFramework.CreateLogger(ModId);
    public static bool isDemo = true;

    public static IHoverTip MyHoverTip(string text)
    {
        string fullText = "ARKNIGHTS_MAP_STATIC_HOVER_TIPS_" + text;
        return new HoverTip(new LocString("static_hover_tips", fullText + ".title"), new LocString("static_hover_tips", fullText + ".description"));
    }

    public static void Init()
    {
        var harmony = new Harmony(ModId);
        harmony.PatchAll();
        var assembly = Assembly.GetExecutingAssembly();
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);
        SettingsPage.Register();
        // 自动注册内容
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);

        InitActs();

        using (RitsuLibFramework.BeginModDataRegistration(ModId))
        {
            var store = RitsuLibFramework.GetDataStore(ModId);

            store.Register<WheatBeerCounter>(
                key: "wheatbeercounter",
                fileName: "wheatbeercounter.json",
                scope: SaveScope.Profile,
                defaultFactory: () => new WheatBeerCounter(),
                autoCreateIfMissing: true
            );
        }

        FmodStudioDeferredBankRegistration.RegisterBank("res://ArknightsMap/audio/ArknightsMap.bank");
        FmodStudioDeferredBankRegistration.RegisterStudioGuidMappings("res://ArknightsMap/audio/GUIDs.txt");
    }

    private static void InitActs()
    {
        ModContentPackBuilder packBuilder = RitsuLibFramework.CreateContentPack(ModId);
        // act 2
        {
            // 有原版地图
            if (SettingsPage.Act2Binding.Read())
            {
                if (SettingsPage.WildsBinding.Read())
                {
                    packBuilder.ActEnterUniformPoolCandidate<Wilds>(1, ctx => true).Apply();
                }
            }
            else
            {
                if (SettingsPage.WildsBinding.Read())
                {
                    packBuilder.ActEnterForce<Wilds>(1, 100, ctx => true).Apply();
                }
            }
        }
        // act 3
        if (isDemo)
        {
            // 有原版地图
            if (SettingsPage.Act2Binding.Read())
            {
                if (SettingsPage.SnowyMountainBinding.Read())
                {
                    packBuilder.ActEnterUniformPoolCandidate<SnowyMountain>(2, ctx => true).Apply();
                }
                if (SettingsPage.LateranoBinding.Read())
                {
                    packBuilder.ActEnterUniformPoolCandidate<Laterano>(2, ctx => true).Apply();
                }
            }
            else
            {
                List<Action<int>> validActs = [];
                if (SettingsPage.SnowyMountainBinding.Read())
                {
                    validActs.Add(res => packBuilder.ActEnterForce<SnowyMountain>(2, 100, ctx => ctx.Rng.NextInt(res) == 0).Apply());
                }
                if (SettingsPage.LateranoBinding.Read())
                {
                    validActs.Add(res => packBuilder.ActEnterForce<Laterano>(2, 100, ctx => ctx.Rng.NextInt(res) == 0).Apply());
                }
                for (int i = 0; i < validActs.Count; i++)
                {
                    validActs[i](validActs.Count - i);
                }
            }
        }
    }
}
