using System.Diagnostics.CodeAnalysis;

#pragma warning disable SA1649 // SA1649FileNameMustMatchTypeName : Records are not yet supported by StyleCop

namespace Funcky
{
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201", Justification = "Records are not yet supported by StyleCop")]
    public record Currency
    {
        private string _currency;

        public Currency(string currency)
        {
            // Load Data from XML Resource
            _currency = currency; 
        }

        public static Currency CHF()
            => new Currency(nameof(CHF));
    }
}
