using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculo_RV_CM.Utils
{
    public static class Utils
    {
        public static decimal TruncarValor(decimal valor, int precisao = 2, MidpointRounding metodoTruncar = MidpointRounding.ToZero)
        {
            return decimal.Round(valor, precisao, metodoTruncar);
        }
    }
}
