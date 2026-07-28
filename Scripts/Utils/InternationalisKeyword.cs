using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace ArknightsMap.Scripts.Utils;

[RegisterOwnedCardKeyword(nameof(InternationalisKeyword))]
public class InternationalisKeyword
{
    public static readonly CardKeyword Keyword = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(InternationalisKeyword)).GetModCardKeyword();
}