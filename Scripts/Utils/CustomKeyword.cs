using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace ArknightsMap.Scripts.Utils;

[RegisterOwnedCardKeyword(nameof(CustomKeyword))]
public class CustomKeyword
{
    public static readonly CardKeyword Keyword = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(CustomKeyword)).GetModCardKeyword();
}
