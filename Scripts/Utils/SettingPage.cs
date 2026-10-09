using System.Reflection;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2RitsuLib;
using STS2RitsuLib.Data;
using STS2RitsuLib.Settings;
using STS2RitsuLib.Utils.Persistence;

namespace ArknightsMap.Scripts.Utils;

public sealed class ArknightsSettings
{
    public bool Act1 { get; set; } = true;
    public bool Act2 { get; set; } = true;
    public bool Act3 { get; set; } = true;
    public bool Wilds { get; set; } = true;
    public bool Laterano { get; set; } = true;
    public bool SnowyMountain { get; set; } = true;
}

public static class SettingsPage
{
    private const string DataKey = "ArknightsMap";

    private static IModSettingsValueBinding<bool> Bind(string propertyName)
    {
        PropertyInfo property = typeof(ArknightsSettings).GetProperty(propertyName)!;

        return ModSettingsBindings.WithDefault(
            ModSettingsBindings.Global<ArknightsSettings, bool>(
                Entry.ModId,
                DataKey,
                s => (bool)property.GetValue(s)!,
                (s, v) =>
                {
                    // NModalContainer.Instance!.Add(NSettingsConfirmPopup.Create());
                    property.SetValue(s, v);
                }
            ),
            () => true
        );
    }

    public static readonly IModSettingsValueBinding<bool> Act1Binding = Bind("Act1");
    public static readonly IModSettingsValueBinding<bool> Act2Binding = Bind("Act2");
    public static readonly IModSettingsValueBinding<bool> Act3Binding = Bind("Act3");
    public static readonly IModSettingsValueBinding<bool> WildsBinding = Bind("Wilds");
    public static readonly IModSettingsValueBinding<bool> LateranoBinding = Bind("Laterano");
    public static readonly IModSettingsValueBinding<bool> SnowyMountainBinding = Bind("SnowyMountain");

    private static ModSettingsText T(string id)
    {
        return ModSettingsText.Dynamic(() => new LocString("settings_ui", "ARKNIGHTS_MAP_SETTINGS_UI_" + id).GetRawText());
    }

    public static void Register()
    {
        // 注册 DataStore
        ModDataStore
            .For(Entry.ModId)
            .Register(
                key: DataKey, // 持久化数据ID，需要和别人防撞
                fileName: "settings.json", // 你的数据文件名
                scope: SaveScope.Global, // Profile 表示每个存档独立，可改成 Global 表示所有存档共享
                defaultFactory: () => new ArknightsSettings(),
                autoCreateIfMissing: true
            );

        // 注册页面UI
        RitsuLibFramework.RegisterModSettings(
            Entry.ModId,
            page =>
            {
                var result = page.WithModDisplayName(T("MOD_NAME"))
                    .WithDescription(T("DESCRIPTION"))
                    .WithTitle(T("TITLE"))
                    .WithVisibleOnHostSurfaces(ModSettingsHostSurface.MainMenu | ModSettingsHostSurface.RunPause)
                    .AddSection(
                        "act2",
                        section =>
                            section
                                .WithTitle(T("SECTION_2")) //
                                .AddToggle("act2origin", T("ORIGIN"), Act2Binding)
                                .AddToggle("wilds", T("WILDS"), WildsBinding)
                    );
                if (Entry.isDemo)
                {
                    result.AddSection(
                        "act3",
                        section =>
                            section
                                .WithTitle(T("SECTION_3"))
                                .AddToggle("act3origin", T("ORIGIN"), Act3Binding)
                                .AddToggle("snowymountain", T("SNOWY_MOUNTAIN"), SnowyMountainBinding)
                                .AddToggle("laterano", T("LATERANO"), LateranoBinding)
                    );
                }
            }
        );
    }
}
