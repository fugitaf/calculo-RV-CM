namespace Calculo_RV_CM.Entities
{
    public class Saldo
    {
        public decimal SaldoCotasSubconta { get; set; }
        public decimal ValorCustoMedio { get; set; }
        public decimal SaldoPrejuizo { get; set; }
        
        public Saldo () { }
        
        public decimal CustoMedio()
        {
            return Utils.Utils.TruncarValor( ValorCustoMedio / SaldoCotasSubconta, 7);
        }
    }
}
