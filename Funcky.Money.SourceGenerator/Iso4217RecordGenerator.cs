using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using static System.Environment;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Funcky.Money.SourceGenerator
{
    [Generator]
    public sealed class Iso4217RecordGenerator : ISourceGenerator
    {
        private const string Indent = "    ";
        private const string CurrencyNameNode = "CcyNm";
        private const string AlphabeticCurrencyCodeNode = "Ccy";
        private const string NumericCurrencyCodeNode = "CcyNbr";
        private const string MinorUnitNode = "CcyMnrUnts";

        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var dictionaryInitializerPairs = new StringBuilder();
            foreach (var record in ReadIso4217RecordsFromAdditionalFiles(context))
            {
                dictionaryInitializerPairs.AppendLine(
                    $"{Indent}{Indent}{Indent}{Indent}[{Literal(record.AlphabeticCurrencyCode)}] = " +
                    $"new Iso4217Record(" +
                        $"{Literal(record.CurrencyName)}, " +
                        $"{Literal(record.AlphabeticCurrencyCode)}, " +
                        $"{Literal(record.NumericCurrencyCode)}, " +
                        $"{Literal(record.MinorUnitDigits ?? 0)}),");
            }

            var iso4217Information =
                $"using System.Collections.Generic;{NewLine}" +
                $"{NewLine}" +
                $"namespace Funcky{NewLine}" +
                $"{{{NewLine}" +
                $"{Indent}internal static class Iso4217Information{NewLine}" +
                $"{Indent}{Indent}{{{NewLine}" +
                $"{Indent}{Indent}{Indent}public static readonly IReadOnlyDictionary<string, Iso4217Record> Currencies = new Dictionary<string, Iso4217Record>{NewLine}" +
                $"{Indent}{Indent}{Indent}{{{NewLine}" +
                dictionaryInitializerPairs +
                $"{Indent}{Indent}}};{NewLine}" +
                $"{Indent}}}{NewLine}" +
                $"}}{NewLine}";

            context.AddSource("Iso4217Information.Generated", SourceText.From(iso4217Information, Encoding.UTF8));
        }

        private static IEnumerable<Iso4217Record> ReadIso4217RecordsFromAdditionalFiles(GeneratorExecutionContext context)
            => context.AdditionalFiles
                    .Where(f => Path.GetExtension(f.Path) == ".xml")
                    .Select(f => f.GetText(context.CancellationToken))
                    .Where(f => f is not null)
                    .SelectMany(text => CreateXmlDocumentFromString(text!.ToString())
                        .SelectNodesAsEnumerable("//CcyNtry/Ccy/..")
                        .Select(ReadIso4217RecordFromNode));

        private static XmlDocument CreateXmlDocumentFromString(string xml)
        {
            var document = new XmlDocument();
            document.LoadXml(xml);
            return document;
        }

        private static Iso4217Record ReadIso4217RecordFromNode(XmlNode node)
        {
            var currencyName = node.SelectSingleNode(CurrencyNameNode)?.InnerText;
            var alphabeticCurrencyName = node.SelectSingleNode(AlphabeticCurrencyCodeNode)?.InnerText;
            var numericCurrencyCode = int.Parse(node.SelectSingleNode(NumericCurrencyCodeNode)?.InnerText);
            var minorUnit = int.TryParse(node.SelectSingleNode(MinorUnitNode)?.InnerText, out var minorUnitTemp) ? (int?)minorUnitTemp : null;
            return new Iso4217Record(currencyName, alphabeticCurrencyName, numericCurrencyCode, minorUnit);
        }
    }
}
