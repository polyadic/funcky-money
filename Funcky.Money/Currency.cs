namespace Funcky;

/// <summary>
/// An ISO 4217 currency. The well-known currencies and <see cref="AllCurrencies"/> are generated from the ISO 4217 list.
/// </summary>
public sealed partial record Currency(string AlphabeticCode, string Name, int NumericCode, int MinorUnitDigits);
