namespace Calculo_RV_CM.Entities
{
    public class Periodos
    {
        public int Ano { get; set; }
        public decimal AliquotaIR { get; set; }
        public decimal SaldoAmortizacaoDePrincipalPorCota { get; set; }
        public decimal CotacaoInicio { get; set; }
        public decimal CotacaoFim { get; set; }
        public decimal RendimentoPorCota { get; set; }
        public decimal PrejuizoACompensarPorCota { get; set; }
        public decimal PrejuizoCompensadoPorCota { get; set; }
        public decimal SaldoPrejuizoPorCota { get; set; }
        public decimal BaseCalculoIRPorCota { get; set; }
        public decimal IRPorCota { get; set; }
        public decimal SaldoPrejuizo { get; set; }
    }
}

