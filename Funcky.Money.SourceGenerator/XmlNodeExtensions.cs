using System.Collections.Immutable;
using System.Xml;

namespace Funcky.Money.SourceGenerator;

internal static class XmlNodeExtensions
{
    public static IEnumerable<XmlNode> SelectNodesAsEnumerable(this XmlNode document, string xpath)
    {
        using var currencyNodes = document.SelectNodes(xpath);
        return currencyNodes?.Cast<XmlNode>().ToImmutableArray() ?? Enumerable.Empty<XmlNode>();
    }
}
