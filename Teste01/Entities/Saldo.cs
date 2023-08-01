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

        public Saldo(decimal sldcota, decimal vlcust)
        {
            Sldcota = sldcota;
            Vlcust = vlcust;
        }

        public decimal CustoMedio()
        {
            return Utils.TruncarValor( Vlcust / Sldcota);
        }
    }
}
