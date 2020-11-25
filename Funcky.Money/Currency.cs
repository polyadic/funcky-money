using System;
using System.Linq;
using System.Reflection;
using System.Xml;
using Funcky.Extensions;

namespace Funcky
{
    public record Currency
    {
        public Currency(string currency)
        {
            using var currencyInformation = IsoCurrencyInformation(currency);

            var first = currencyInformation.Cast<XmlNode>().First();

            // Load Data from XML Resource
            CurrencyName = first.SelectSingleNode("CcyNm").InnerText;
            AlphabeticCurrencyCode = first.SelectSingleNode("Ccy").InnerText;
            NumericCurrencyCode = first.SelectSingleNode("CcyNbr").InnerText.TryParseInt().GetOrElse(() => throw new NotImplementedException());
            MinorUnitDigits = first.SelectSingleNode("CcyMnrUnts").InnerText.TryParseInt().GetOrElse(() => throw new NotImplementedException());
        }

        public string CurrencyName { get; }

        public string AlphabeticCurrencyCode { get; }

        public int NumericCurrencyCode { get; }

        public int MinorUnitDigits { get; }

        public static XmlNodeList IsoCurrencyInformation(string currency)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var resource = assembly.GetManifestResourceStream("Funcky.Resources.list_one.xml");

            var xml = new XmlDocument();
            xml.Load(resource);

            return xml.SelectNodes($"//CcyNtry/Ccy[.='{currency}']/..");
        }

        public static Currency CHF()
            => new Currency(nameof(CHF));

        public static Currency USD()
            => new Currency(nameof(USD));
    }
}
