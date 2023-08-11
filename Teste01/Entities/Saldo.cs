namespace Calculo_RV_CM.Entities
{
    public class Saldo
    {
        public decimal SaldoCotasSubconta { get; set; }
        public decimal ValorCustoMedio { get; set; }
        public decimal CotacaoMaisRecente { get; set; }
        public decimal SaldoPrejuizo { get; set; }
        public decimal ValorBloqueadoTotal { get; set; }
        public decimal CotasBloqueadasTotal { get; set; }

        public Saldo(decimal saldoCotasSubconta, decimal valorCustoMedio, decimal cotacaoMaisRecente, decimal saldoPrejuizo, decimal valorBloqueadoTotal, decimal cotasBloqueadasTotal)
        {
            SaldoCotasSubconta = saldoCotasSubconta;
            ValorCustoMedio = valorCustoMedio;
            CotacaoMaisRecente = cotacaoMaisRecente;
            SaldoPrejuizo = saldoPrejuizo;  
            ValorBloqueadoTotal = valorBloqueadoTotal;
            CotasBloqueadasTotal = cotasBloqueadasTotal;
        }

        public decimal CustoMedio()
        {
            return Utils.Utils.TruncarValor( ValorCustoMedio / SaldoCotasSubconta, 7);
        }
    }
}
