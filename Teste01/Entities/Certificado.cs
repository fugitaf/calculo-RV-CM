namespace Calculo_RV_CM.Entities
{
    public class Certificado
    {
        public string DataAplicacao { get; set; }
        public decimal SaldoCotasCertificado { get; set; }
        public decimal CotacaoAplicacao { get; set; }
        public decimal SaldoAmortizacaoDePrincipal { get; set; }
        public decimal RendimentoPorCota { get; set; }
        public decimal SaldoPrejuizo { get; set; }
        public decimal CotasIsentaMaximo { get; set; }
        public decimal CotasIsenta { get; set; }
        public decimal CotasTributada { get; set; }
        public decimal PrejuizoCompensado { get; set; }
        public decimal PrejuizoACompensar { get; set; }
        public decimal IRPorCota { get; set; }
        public decimal CotaLiquidaTributada { get; set; }
        public decimal ValorBruto { get; set; }
        public decimal ValorIR { get; set; }
        public decimal ValorLiquido { get; set; }
        public decimal ValorBloqueadoEmCotas { get; set; }
        public decimal CustoAplicacao { get; set; }
        public List<Periodos> PeriodoCalculado { get; set; }
    }
}
        