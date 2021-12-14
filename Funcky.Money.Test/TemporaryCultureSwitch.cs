using System.Globalization;

namespace Funcky.Test;

internal sealed class TemporaryCultureSwitch : IDisposable
{
    private readonly CultureInfo _lastCulture;

    public TemporaryCultureSwitch(string newCulture)
    {
        _lastCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo(newCulture);
    }

    public void Dispose()
        => CultureInfo.CurrentCulture = _lastCulture;
}
