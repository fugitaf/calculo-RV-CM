namespace Calculo_RV_CM.Entities
{
    public class Saldo
    {
        public decimal SaldoCotasSubconta { get; set; }
        public decimal ValorCustoMedio { get; set; }
        public decimal SaldoPrejuizo { get; set; }
        public decimal ValorBloqueadoTotal { get; set; }
        public decimal CotasBloqueadasTotal { get; set; }

        public Saldo () { }
        public Saldo(decimal saldoCotasSubconta, decimal valorCustoMedio, decimal saldoPrejuizo)
        {
            SaldoCotasSubconta = saldoCotasSubconta;
            ValorCustoMedio = valorCustoMedio;
            SaldoPrejuizo = saldoPrejuizo;  
        }

        public decimal CustoMedio()
        {
            return Utils.Utils.TruncarValor( ValorCustoMedio / SaldoCotasSubconta, 7);
        }
    }
}
