using System.Collections.Generic;
using System.Collections.Immutable;
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
        private const string RootNamespace = "Funcky";
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
            var records = ReadIso4217RecordsFromAdditionalFiles(context).ToImmutableArray();
            context.AddSource("CurrencyCode.Generated", SourceText.From(GenerateCurrencyClass(records), Encoding.UTF8));
            context.AddSource("Money.Generated", SourceText.From(GenerateMoneyClass(records), Encoding.UTF8));
        }

        private static string GenerateCurrencyClass(IReadOnlyList<Iso4217Record> records)
            => $"using System.Collections.Generic;{NewLine}" +
               $"using System.Collections.Immutable;{NewLine}" +
               $"using Funcky.Monads;{NewLine}" +
               $"namespace {RootNamespace}{NewLine}" +
               $"{{{NewLine}" +
               $"{Indent}public partial record Currency{NewLine}" +
               $"{Indent}{{{NewLine}" +
               $"{GenerateCurrencyProperties(records)}" +
               $"{GenerateAllCurrenciesProperty(records)}" +
               $"{GenerateParseMethod(records)}" +
               $"{Indent}}}{NewLine}" +
               $"}}{NewLine}";

        private static string GenerateParseMethod(IEnumerable<Iso4217Record> records)
        {
            var switchCases = new StringBuilder();

            foreach (var record in records)
            {
                switchCases.AppendLine($"{Indent}{Indent}{Indent}  {Literal(record.AlphabeticCurrencyCode)} => {Identifier(record.AlphabeticCurrencyCode)},");
            }

            switchCases.AppendLine($"{Indent}{Indent}{Indent}  _ => Option<Currency>.None(),");

            return $"{Indent}{Indent}public static partial Option<Currency> ParseOrNone(string input){NewLine}" +
                   $"{Indent}{Indent}  => input switch{NewLine}" +
                   $"{Indent}{Indent}  {{{NewLine}" +
                   switchCases +
                   $"{Indent}{Indent}  }};{NewLine}";
        }

        private static string GenerateCurrencyProperties(IEnumerable<Iso4217Record> records)
        {
            var properties = new StringBuilder();

            foreach (var record in records)
            {
                properties.AppendLine($"{Indent}{Indent}/// <summary>{record.CurrencyName}</summary>");
                properties.AppendLine($"{Indent}{Indent}public static Currency {Identifier(record.AlphabeticCurrencyCode)} {{ get; }} = " +
                                      $"new Currency(new Iso4217Record(" +
                                      $"{Literal(record.CurrencyName)}, " +
                                      $"{Literal(record.AlphabeticCurrencyCode)}, " +
                                      $"{Literal(record.NumericCurrencyCode)}, " +
                                      $"{Literal(record.MinorUnitDigits ?? 0)}));");
            }

            return properties.ToString();
        }

        private static string GenerateAllCurrenciesProperty(IReadOnlyCollection<Iso4217Record> records)
        {
            var property = new StringBuilder();

            property.AppendLine($"{Indent}{Indent}internal static IReadOnlyList<Currency> AllCurrencies {{ get; }} = ImmutableArray.Create(");

            foreach (var (record, index) in records.Select((r, i) => (r, i)))
            {
                var comma = index < records.Count - 1 ? "," : string.Empty;
                property.AppendLine($"{Indent}{Indent}{Indent}Currency.{Identifier(record.AlphabeticCurrencyCode)}{comma}");
            }

            property.AppendLine($"{Indent}{Indent});");

            return property.ToString();
        }

        private static string GenerateMoneyClass(IEnumerable<Iso4217Record> records)
            => $"using Funcky.Monads;{NewLine}" +
               $"namespace {RootNamespace}{NewLine}" +
               $"{{{NewLine}" +
               $"{Indent}public partial record Money{NewLine}" +
               $"{Indent}{{{NewLine}" +
               $"{GenerateMoneyFactoryMethods(records)}" +
               $"{Indent}}}{NewLine}" +
               $"}}{NewLine}";

        private static string GenerateMoneyFactoryMethods(IEnumerable<Iso4217Record> records)
            => records.Aggregate(new StringBuilder(), AppendCurrencyFactory).ToString();

        private static StringBuilder AppendCurrencyFactory(StringBuilder stringBuilder, Iso4217Record record)
            => stringBuilder.Append(CreateCurrencyFactory(record));

        private static string CreateCurrencyFactory(Iso4217Record record)
        {
            var identifier = Identifier(record.AlphabeticCurrencyCode);

            return $"{Indent}{Indent}/// <summary>Creates a new <see cref=\"Money\" /> instance using the <see cref=\"Currency.{identifier}\" /> currency.</summary>{NewLine}" +
                $"{Indent}{Indent}public static Money {identifier}(decimal amount){NewLine}" +
                $"{Indent}{Indent}  => new(amount, MoneyEvaluationContext.Builder.Default.WithTargetCurrency(Currency.{identifier}).Build());";
        }

        private static IEnumerable<Iso4217Record> ReadIso4217RecordsFromAdditionalFiles(GeneratorExecutionContext context)
            => context.AdditionalFiles
                    .Where(f => Path.GetExtension(f.Path) == ".xml")
                    .Select(f => f.GetText(context.CancellationToken))
                    .Where(f => f is not null)
                    .SelectMany(text => CreateXmlDocumentFromString(text!.ToString())
                        .SelectNodesAsEnumerable("//CcyNtry/Ccy/..")
                        .Select(ReadIso4217RecordFromNode))
                    .ToImmutableDictionary(r => r.AlphabeticCurrencyCode)
                    .Select(r => r.Value);

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
