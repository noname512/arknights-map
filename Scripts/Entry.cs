using System.Reflection;
using ArknightsMap.Scripts.Acts;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
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
    public static ReedBed reedBed => new ReedBed();
    public static void Init()
    {
        // harmony可用，但是最好用ritsu的封装patch（TODO）
        // var harmony = new Harmony("com.example.testmod");
        // harmony.PatchAll();
        var assembly = Assembly.GetExecutingAssembly();
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);
        // 自动注册内容
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);

        RitsuLibFramework.CreateContentPack(ModId)
            .ActEnterWeightedPool(1)
            .ActEnterWeightedPoolCandidate<Wilds>(1, ctx => true, weight => 99999)
            .Apply();

        using (RitsuLibFramework.BeginModDataRegistration(ModId))
        {
            var store = RitsuLibFramework.GetDataStore(ModId);

            store.Register<WheatBeerCounter>(
                key: "wheatbeercounter",
                fileName: "wheatbeercounter.json",
                scope: SaveScope.Profile,
                defaultFactory: () => new WheatBeerCounter(),
                autoCreateIfMissing: true);
        }

        /*
        RitsuLibFramework.SubscribeLifecycle<ProfileDataReadyEvent>(_ =>
        {
            var store = RitsuLibFramework.GetDataStore(ModId);
            store.Modify<WheatBeerCounter>("wheatbeercounter", data => data.Value += 1);
            store.Save("wheatbeercounter");
            Logger.Info($"counter.Value = {store.Get<WheatBeerCounter>("wheatbeercounter").Value}");
        });*/

        RitsuLibFramework.SubscribeLifecycle(reedBed);
    }
}