using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculo_RV_CM.Entities
{
    public class Periodos
    {
        public string Dtultrib { get; set; }
        public int Ano { get; set; }
        public decimal Aliquota_Ir { get; set; }
        public decimal CotacaoInicial { get; set; }
        public decimal CotacaoFim { get; set; }
        public decimal Rendimento { get; set; }
        public decimal PrejACompensar { get; set; }
        public decimal PrejCompensado { get; set; }
        public decimal SaldoPrejCota { get; set; }
        public decimal BaseCalcIR { get; set; }
        public decimal SaldoPrejReais { get; set; }
        public decimal IR { get; set; }
        public decimal ValorIR { get; set; }
    }
}

