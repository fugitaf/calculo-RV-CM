using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculo_RV_CM.Entities
{
    public class Certificado
    {
        public string Dtultrib { get; set; }
        public decimal Sdoctapl { get; set; }
        public decimal Cotaplic { get; set; }
        public decimal RendCertificado { get; set; }
        public decimal SaldoPrejuizo { get; set; }
        public decimal CotasIsentaMax { get; set; }
        public decimal CotasIsenta { get; set; }
        public decimal CotasTributada { get; set; }
        public decimal VlrPrejCompensado { get; set; }
        public decimal VlrPrejCertificado { get; set; }
        public decimal IRCota { get; set; }
        public decimal IR { get; set; }
        public List<Periodos> PeriodoCalc { get; set; }
    }
}
        