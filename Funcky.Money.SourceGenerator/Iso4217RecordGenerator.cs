using System.Collections.Immutable;
using System.Text;
using System.Xml;
using Funcky.Extensions;
using Funcky.Monads;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Funcky.Money.SourceGenerator;

[Generator]
public sealed class Iso4217RecordGenerator : IIncrementalGenerator
{
    private const string RootNamespace = "Funcky";
    private const string Indent = "    ";
    private const string CurrencyNameNode = "CcyNm";
    private const string AlphabeticCurrencyCodeNode = "Ccy";
    private const string NumericCurrencyCodeNode = "CcyNbr";
    private const string MinorUnitNode = "CcyMnrUnts";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var xmlFiles = context.AdditionalTextsProvider.Where(f => Path.GetExtension(f.Path) == ".xml").Collect();
        context.RegisterSourceOutput(xmlFiles, GenerateSource);
    }

    private static void GenerateSource(SourceProductionContext context, ImmutableArray<AdditionalText> xmlFiles)
    {
        var records = ReadIso4217RecordsFromAdditionalFiles(xmlFiles, context.CancellationToken).ToImmutableArray();
        context.AddSource("CurrencyCode.Generated", SourceText.From(GenerateCurrencyClass(records), Encoding.UTF8));
        context.AddSource("Money.Generated", SourceText.From(GenerateMoneyClass(records), Encoding.UTF8));
    }

    private static string GenerateCurrencyClass(IReadOnlyList<Iso4217Record> records)
        => $"using System.Collections.Generic;\n" +
           $"using System.Collections.Immutable;\n" +
           $"using Funcky.Monads;\n" +
           $"namespace {RootNamespace}\n" +
           $"{{\n" +
           $"{Indent}public partial record Currency\n" +
           $"{Indent}{{\n" +
           $"{GenerateCurrencyProperties(records)}" +
           $"{GenerateAllCurrenciesProperty(records)}" +
           $"{GenerateParseMethod(records)}" +
           $"{Indent}}}\n" +
           $"}}\n";

    private static string GenerateParseMethod(IEnumerable<Iso4217Record> records)
    {
        var switchCases = new StringBuilder();

        foreach (var record in records)
        {
            switchCases.AppendLine($"{Indent}{Indent}{Indent}  {Literal(record.AlphabeticCurrencyCode)} => {Identifier(record.AlphabeticCurrencyCode)},");
        }

        switchCases.AppendLine($"{Indent}{Indent}{Indent}  _ => Option<Currency>.None,");

        return $"{Indent}{Indent}public static partial Option<Currency> ParseOrNone(string input)\n" +
               $"{Indent}{Indent}  => input switch\n" +
               $"{Indent}{Indent}  {{\n" +
               switchCases +
               $"{Indent}{Indent}  }};\n";
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
        => $"using Funcky.Monads;\n" +
           $"namespace {RootNamespace}\n" +
           $"{{\n" +
           $"{Indent}public partial record Money<TUnderlyingType>\n" +
           $"{Indent}{{\n" +
           $"{GenerateMoneyFactoryMethods(records)}" +
           $"{Indent}}}\n" +
           $"}}\n";

    private static string GenerateMoneyFactoryMethods(IEnumerable<Iso4217Record> records)
        => records.Aggregate(new StringBuilder(), AppendCurrencyFactory).ToString();

    private static StringBuilder AppendCurrencyFactory(StringBuilder stringBuilder, Iso4217Record record)
        => stringBuilder.Append(CreateCurrencyFactory(record));

    private static string CreateCurrencyFactory(Iso4217Record record)
    {
        var identifier = Identifier(record.AlphabeticCurrencyCode);

        return $"{Indent}{Indent}/// <summary>Creates a new <see cref=\"Money{{TUnderlyingType}}\" /> instance using the <see cref=\"Currency.{identifier}\" /> currency.</summary>\n" +
            $"{Indent}{Indent}public static Money<TUnderlyingType> {identifier}(TUnderlyingType amount)\n" +
            $"{Indent}{Indent}  => new(amount, MoneyEvaluationContext<TUnderlyingType>.Builder.Default.WithTargetCurrency(Currency.{identifier}).Build());";
    }

    private static IEnumerable<Iso4217Record> ReadIso4217RecordsFromAdditionalFiles(
        ImmutableArray<AdditionalText> additionalTexts,
        CancellationToken cancellationToken)
        => additionalTexts
            .Select(f => f.GetText(cancellationToken))
            .Where(f => f is not null)
            .SelectMany(text => CreateXmlDocumentFromString(text!.ToString())
                .SelectNodesAsEnumerable("//CcyNtry/Ccy/..")
                .WhereSelect(ReadIso4217RecordFromNode))
            .ToImmutableDictionary(r => r.AlphabeticCurrencyCode)
            .Select(r => r.Value);

    private static XmlDocument CreateXmlDocumentFromString(string xml)
    {
        var document = new XmlDocument();
        document.LoadXml(xml);
        return document;
    }

    private static Option<Iso4217Record> ReadIso4217RecordFromNode(XmlNode node)
        => from currencyName in CurrencyName(node)
           from alphabeticCurrencyName in AlphabeticCurrencyName(node)
           from numericCurrencyCode in NumericCurrencyCode(node)
           select new Iso4217Record(currencyName, alphabeticCurrencyName, numericCurrencyCode, MinorUnit(node));

    private static Option<string> CurrencyName(XmlNode node)
        => GetInnerText(node, CurrencyNameNode);

    private static Option<string> AlphabeticCurrencyName(XmlNode node)
        => GetInnerText(node, AlphabeticCurrencyCodeNode);

    private static Option<int> NumericCurrencyCode(XmlNode node)
        => GetInnerText(node, NumericCurrencyCodeNode)
            .SelectMany(ParseExtensions.ParseInt32OrNone);

    private static int MinorUnit(XmlNode node)
        => GetInnerText(node, MinorUnitNode)
            .SelectMany(ParseExtensions.ParseInt32OrNone)
            .GetOrElse(0);

    private static Option<string> GetInnerText(XmlNode node, string nodeName)
        => Option
            .FromNullable(node.SelectSingleNode(nodeName))
            .AndThen(n => n.InnerText);
}
