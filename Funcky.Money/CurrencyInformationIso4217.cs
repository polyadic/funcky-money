using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Funcky.Extensions;
using Funcky.Monads;

namespace Funcky
{
    internal class CurrencyInformationIso4217
    {
        private const string Iso4217Resource = "Funcky.Resources.list_one.xml";
        private const string CurrencyNameNode = "CcyNm";
        private const string AlphabeticCurrencyCodeNode = "Ccy";
        private const string NumericCurrencyCodeNode = "CcyNbr";
        private const string MinorUnitNode = "CcyMnrUnts";

        private XmlDocument _xml = new();
        private IDictionary<string, Iso4217Record> _records = new Dictionary<string, Iso4217Record>();

        private static readonly Lazy<CurrencyInformationIso4217> _instance = new Lazy<CurrencyInformationIso4217>(() => new());

        public CurrencyInformationIso4217()
        {
            ParseIsoXml();
        }

        public static CurrencyInformationIso4217 Instance
            => _instance.Value;

        public Iso4217Record this[string currencyCode] =>
            _records.TryGetValue(currencyCode).Match(
                    none: () => LoadFromXml(currencyCode),
                    some: Functional.Identity);

        private Iso4217Record LoadFromXml(string currencyCode)
        {
            using var currencyInformation = IsoCurrencyInformation(currencyCode);

            var result = new Iso4217Record(
                CurrencyName(currencyInformation),
                AlphabeticCurrencyCode(currencyInformation),
                NumericCurrencyCode(currencyInformation),
                MinorUnitDigits(currencyInformation));

            _records.Add(result.AlphabeticCurrencyCode, result);

            return result;
        }

        private static string CurrencyName(XmlNodeList currencyInformation)
            => ExtractNodeText(currencyInformation, CurrencyNameNode);

        private static string AlphabeticCurrencyCode(XmlNodeList currencyInformation)
            => ExtractNodeText(currencyInformation, AlphabeticCurrencyCodeNode);

        private static int NumericCurrencyCode(XmlNodeList currencyInformation)
            => ExtractNodeText(currencyInformation, NumericCurrencyCodeNode).TryParseInt().GetOrElse(() => throw new NotImplementedException());

        private static int MinorUnitDigits(XmlNodeList currencyInformation)
            => ExtractNodeText(currencyInformation, MinorUnitNode).TryParseInt().GetOrElse(() => throw new NotImplementedException());

        private static string ExtractNodeText(XmlNodeList currencyInformation, string nodeName)
            => currencyInformation
                .Cast<XmlNode>()
                .First()
                .SelectSingleNode(nodeName)
                .InnerText;

        private void ParseIsoXml()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var resource = assembly.GetManifestResourceStream(Iso4217Resource);

            _xml.Load(resource);
        }

        private XmlNodeList IsoCurrencyInformation(string currency)
        {
            return _xml.SelectNodes($"//CcyNtry/Ccy[.='{currency}']/..");
        }
    }
}
