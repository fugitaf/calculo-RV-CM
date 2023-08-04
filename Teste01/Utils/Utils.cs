using Calculo_RV_CM.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculo_RV_CM.Utils
{
    public static class Utils
    {
        public static decimal TruncarValor(decimal valor, int precisao = 2, MidpointRounding metodoTruncar = MidpointRounding.ToZero)
        {
            return decimal.Round(valor, precisao, metodoTruncar);
        }

        public static int Ano(string dtlanct)
        {
            return int.Parse(dtlanct.Substring(6));
        }

        public static void GravaCabecalho()
        {
            try
            {
                string pathSaida = @"C:\Users\fefug_skli85i\Documents\Temp\Saida";
                string pathCalc = @"C:\Users\fefug_skli85i\Documents\Temp\Saida\CALC.csv";

                Directory.CreateDirectory(pathSaida);

                using (StreamWriter sw = File.AppendText(pathCalc))
                {
                    sw.WriteLine("Dtultrib;" + "Ano;" + "Aliquota_Ir;" + "CotacaoInicial;" +
                                 "CotacaoFim;" + "Rendimento;" + "PrejCompensar;" +
                                 "PrejCompensado;" + "SaldoPrejCota;" + "BaseCalcIR;" +
                                 "SaldoPrejReais;" + "IrCota;" + "ValorIR");
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Erro no Calc");
                Console.WriteLine(e.Message);
            }
        }
    
    public static void GravaCalculo(CalcPorPeriodo calcPorPeriodo)
        {
            try
            {
                string pathSaida = @"C:\Users\fefug_skli85i\Documents\Temp\Saida";
                string pathCalc = @"C:\Users\fefug_skli85i\Documents\Temp\Saida\CALC.csv";

                Directory.CreateDirectory(pathSaida);

                using (StreamWriter sw = File.AppendText(pathCalc))
                {
                 sw.WriteLine(calcPorPeriodo.Dtultrib + ";" +
                         calcPorPeriodo.Ano + ";" +
                         calcPorPeriodo.Aliquota_Ir.ToString("N2", new CultureInfo("pt-BR")) + ";" +
                         calcPorPeriodo.CotacaoInicial.ToString("N7", new CultureInfo("pt-BR")) + ";" +
                         calcPorPeriodo.CotacaoFim.ToString("N7", new CultureInfo("pt-BR")) + ";" +
                         calcPorPeriodo.Rendimento.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                         calcPorPeriodo.PrejCompensar.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                         calcPorPeriodo.PrejCompensado.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                         calcPorPeriodo.SaldoPrejCota.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                         calcPorPeriodo.BaseCalcIR.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                         calcPorPeriodo.SaldoPrejReais.ToString("N2", new CultureInfo("pt-BR")) + ";" +
                         calcPorPeriodo.irCota.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                         calcPorPeriodo.valorIR.ToString("N2", new CultureInfo("pt-BR")));
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Erro no Calc");
                Console.WriteLine(e.Message);
            }
        }
    }

}


