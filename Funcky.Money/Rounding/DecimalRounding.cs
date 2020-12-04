using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funcky
{
    internal static class DecimalRounding
    {
        public static decimal Truncate(decimal amount, decimal precision)
            => decimal.Truncate(amount / precision) * precision;
    }
}
