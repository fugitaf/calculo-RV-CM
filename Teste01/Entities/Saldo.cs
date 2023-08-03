using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Teste01.Utils;

namespace Teste01.Entities
{
    class Saldo
    {
        public decimal Sldcota { get; set; }
        public decimal Vlcust { get; set; }
        public decimal CotaResg { get; set; }

        public Saldo(decimal sldcota, decimal vlcust, decimal cotaResg)
        {
            Sldcota = sldcota;
            Vlcust = vlcust;
            CotaResg = cotaResg;
        }

        public decimal CustoMedio()
        {
            return Utils.Utils.TruncarValor( Vlcust / Sldcota, 7);
        }
    }
}
