using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculo_RV_CM.Entities
{
    class Certificado
    {
        public string Dtlanct { get; set; }
        public decimal Qtdcota { get; set; }
        public decimal Cotaplic { get; set; }

        public List<CalcPorPeriodo> Aliquotas { get; set; }  

        public Certificado(string dtlanc, decimal qtdcota, decimal cotaplic, List<CalcPorPeriodo> aliquotas)
        {
            Dtlanct = dtlanc;
            Qtdcota = qtdcota;
            Cotaplic = cotaplic;
            Aliquotas = aliquotas;
        }
    }
}
