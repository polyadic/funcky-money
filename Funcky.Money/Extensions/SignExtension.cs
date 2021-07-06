namespace Funcky.Extensions
{
    internal static class SignExtension
    {
        public static decimal ApplySignFrom(this decimal positiveNumber, decimal signSource)
            => signSource switch
            {
                < 0 => -positiveNumber,
                >= 0 => positiveNumber,
            };
    }
}
