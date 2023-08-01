using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Teste01.Entities
{
    class Saldo
    {
        public double Sldcota { get; set; }
        public double Vlcust { get; set; }

        public Saldo(double sldcota, double vlcust)
        {
            Sldcota = sldcota;
            Vlcust = vlcust;
        }

        public double CustoMedio()
        {
            return Vlcust / Sldcota;
        }
    }
}
