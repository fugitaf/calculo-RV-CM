using Calculo_RV_CM.Entities;
using Microsoft.Win32;
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
            string registro = "Dtultrib;" + "Ano;" + "Aliquota_Ir;" +
                "Cotacao_Inicial;" + "Cotacao_Fim;" + "Rendimento;" + "Prej_A_Compensar;" +
                "Prej_Compensado;" + "Saldo_Prej_Cota;" + "Base_Calc_IR;" + "Saldo_Prej_Reais;" + "Ir_Cota;" + "Valor_IR";
            Utils.GravaRegistro(registro);
        }

        public static void GravaCalculo(Periodos calcPorPeriodo)
        {
            string registro = calcPorPeriodo.Dtultrib + ";" +
                calcPorPeriodo.Ano + ";" +
                calcPorPeriodo.Aliquota_Ir.ToString("N2", new CultureInfo("pt-BR")) + ";" +
                calcPorPeriodo.CotacaoInicial.ToString("N7", new CultureInfo("pt-BR")) + ";" +
                calcPorPeriodo.CotacaoFim.ToString("N7", new CultureInfo("pt-BR")) + ";" +
                calcPorPeriodo.Rendimento.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                calcPorPeriodo.PrejACompensar.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                calcPorPeriodo.PrejCompensado.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                calcPorPeriodo.SaldoPrejCota.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                calcPorPeriodo.BaseCalcIR.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                calcPorPeriodo.SaldoPrejReais.ToString("N2", new CultureInfo("pt-BR")) + ";" +
                calcPorPeriodo.IR.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                calcPorPeriodo.ValorIR.ToString("N2", new CultureInfo("pt-BR"));
            Utils.GravaRegistro(registro);
        }

        public static void GravaRegistro(string registro)
        {
            try
            {
                DateTime dateTime = DateTime.Now;
                string dataHora = dateTime.ToString("yyyy-mm-dd hh.mm.ss");
                string pathSaida = @"C:\Users\fefug_skli85i\Documents\Temp\Saida";
                string pathCalc = @"C:\Users\fefug_skli85i\Documents\Temp\Saida\CALC " + dataHora + ".csv";

                Directory.CreateDirectory(pathSaida);

                using (StreamWriter sw = File.AppendText(pathCalc))
                {
                    sw.WriteLine(registro);
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