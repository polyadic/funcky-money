namespace Funcky.Extensions
{
    internal static class SignExtension
    {
        public static decimal CopySign(this decimal positiveNumber, decimal signSource)
            => signSource switch
            {
                < 0 => -positiveNumber,
                >= 0 => positiveNumber,
            };
    }
}
