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
        public decimal SaldoPrejuizo { get; set; }
        public decimal VlrBloq { get; set; }
        public decimal CotBloq { get; set; }

        public Saldo(decimal sdcotamvn, decimal vlcust, decimal vlrcota, decimal saldoPrejuizo, decimal vlrbloq, decimal cotbloq)
        {
            Sdcotmvn = sdcotamvn;
            Vlcust = vlcust;
            VlrCota = vlrcota;
            SaldoPrejuizo = saldoPrejuizo;  
            VlrBloq = vlrbloq;
            CotBloq = cotbloq;
        }

        public decimal CustoMedio()
        {
            return Utils.Utils.TruncarValor( Vlcust / Sdcotmvn, 7);
        }
    }
}
