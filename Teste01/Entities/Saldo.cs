using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Calculo_RV_CM.Utils;

namespace Calculo_RV_CM.Entities
{
    public class Saldo
    {
        public decimal Sdcotmvn { get; set; }
        public decimal Vlcust { get; set; }
        public decimal VlrCota { get; set; }

        public Saldo(decimal sdcotamvn, decimal vlcust, decimal vlrcota)
        {
            Sdcotmvn = sdcotamvn;
            Vlcust = vlcust;
            VlrCota = vlrcota;
        }

        public decimal CustoMedio()
        {
            return Utils.Utils.TruncarValor( Vlcust / Sdcotmvn, 7);
        }
    }
}
