using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teste01.Entities
{
    class Certificado
    {
        public string Dtlanct { get; set; }
        public double Qtdcota { get; set; }
        public double Cotaplic { get; set; }

        public Certificado(string dtlanc, double qtdcota, double cotaplic)
        {
            Dtlanct = dtlanc;
            Qtdcota = qtdcota;
            Cotaplic = cotaplic;
        }
    }
}
