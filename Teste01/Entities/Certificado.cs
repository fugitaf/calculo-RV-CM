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
        public decimal Qtdcota { get; set; }
        public decimal Cotaplic { get; set; }

        public Certificado(string dtlanc, decimal qtdcota, decimal cotaplic)
        {
            Dtlanct = dtlanc;
            Qtdcota = qtdcota;
            Cotaplic = cotaplic;
        }
    }
}
