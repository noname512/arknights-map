
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace ArknightsMap.Scripts.Utils;

public class IgniteIntent : AbstractIntent
{
    public override IntentType IntentType => IntentType.Unknown;
    protected override string IntentPrefix => "ARKNIGHTS_MAP_INTENT_IGNITE";

    protected override string SpritePath => "res://ArknightsMap/images/util/IgniteIntent.tres";

    public override IEnumerable<string> AssetPaths => [SpritePath];

    public override Texture2D? GetTexture(IEnumerable<Creature> targets, Creature owner) => PreloadManager.Cache.GetTexture2D(SpritePath);
}