using Calculo_RV_CM.Entities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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

        public static List<Periodos> CalcPorPeriodo(List<Periodos> periodos, decimal cotacaoInicial, decimal cotamaisrecente, decimal sdoctapl)
        {
            //
            // Atualiza Cotação Inicial e Fim dos Periodos
            //
            periodos[0].CotacaoInicial = cotacaoInicial;

            for (int i = 1; i < periodos.Count; i++)
            {
                periodos[i].CotacaoInicial = periodos[i - 1].CotacaoFim;
            }
            periodos[periodos.Count - 1].CotacaoFim = cotamaisrecente;

            //
            // Calcula Rendimento dos Períodos
            //

            for (int i = 0; i < periodos.Count; i++)
            {
                periodos[i].Rendimento = periodos[i].CotacaoFim - periodos[i].CotacaoInicial;
            }

            //
            // Compensa Prejuízo entre Períodos
            //

            decimal saldoPrej = 0.0m;

            for (int i = 0; i < periodos.Count; i++)
            {
                periodos[i].PrejACompensar = 0.0m;
                periodos[i].PrejCompensado = 0.0m;
                if (periodos[i].Rendimento < 0)
                {
                    periodos[i].PrejACompensar = periodos[i].Rendimento * -1;
                    saldoPrej += periodos[i].PrejACompensar;
                }
                else
                {
                    if (saldoPrej > 0)
                    {
                        if (saldoPrej > periodos[i].Rendimento)
                        {
                            periodos[i].PrejCompensado = periodos[i].Rendimento;
                            saldoPrej -= periodos[i].PrejCompensado;
                        }
                        else
                        {
                            periodos[i].PrejCompensado = saldoPrej;
                            saldoPrej = 0.0m;
                        }
                    }
                }

                periodos[i].BaseCalcIR = periodos[i].Rendimento + periodos[i].PrejACompensar - periodos[i].PrejCompensado;
                periodos[i].SaldoPrejCota = saldoPrej;
                periodos[i].SaldoPrejReais = Utils.TruncarValor(periodos[i].SaldoPrejCota * sdoctapl, 2);

                //
                // Calcula IR por cota
                //

                periodos[i].IRCota = Utils.TruncarValor(periodos[i].BaseCalcIR * periodos[i].Aliquota_Ir, 10);

            }

            return periodos;

        }
        //
        // Compensação de Prejuízo entre Certificados
        //
        public static List<Certificado> CalcCertificado(List<Certificado> certificados, decimal saldoPrejTotal)
        {
            for (int i = 0; i < certificados.Count; i++)
            {
                decimal cotasPrejMax = 0.0m;
                decimal cotasPrejMaxAjuste = 0.0m;
                decimal ValorPrejMax = 0.0m;

                //
                // Soma Prejuizo do Certificado no Saldo Prejuízo Total
                //

                saldoPrejTotal = saldoPrejTotal + certificados[i].VlrPrejCertificado;

                //
                // Calcula quantidade maxima de cotas para compensar o Saldo de Prejuízo
                //

                if (certificados[i].RendCertificado > 0 && saldoPrejTotal > 0)
                {
                    cotasPrejMax = Utils.TruncarValor(saldoPrejTotal / certificados[i].RendCertificado, 5);
                    ValorPrejMax = Utils.TruncarValor(cotasPrejMax * certificados[i].RendCertificado, 2);
                    if (saldoPrejTotal == ValorPrejMax)
                    {
                        certificados[i].CotasIsentaMax = cotasPrejMax;
                    }
                    else
                    {
                        cotasPrejMaxAjuste = cotasPrejMax + 0.00001m;
                        ValorPrejMax = Utils.TruncarValor(cotasPrejMaxAjuste * certificados[i].RendCertificado, 2);
                        if (saldoPrejTotal == ValorPrejMax)
                        {
                            certificados[i].CotasIsentaMax = cotasPrejMaxAjuste;
                        }
                        else
                        {
                            certificados[i].CotasIsentaMax = cotasPrejMax;
                        }
                    }
                }
                else
                {
                    certificados[i].CotasIsentaMax = 0.0m;
                }

                //
                // Calcula Quantidade de Cotas Isentas
                //

                if (certificados[i].CotasIsentaMax > certificados[i].Sdoctapl)
                {
                    certificados[i].CotasIsenta = certificados[i].Sdoctapl;
                }
                else
                {
                    certificados[i].CotasIsenta = certificados[i].CotasIsentaMax;
                }

                //
                // Calcula Quantidade de Cotas Tributadas
                //

                certificados[i].CotasTributada = certificados[i].Sdoctapl - certificados[i].CotasIsenta;

                //
                // Calcula Prejuizo Compensado
                //

                certificados[i].VlrPrejCompensado = Utils.TruncarValor(certificados[i].CotasIsenta * certificados[i].RendCertificado, 2);

                //
                // Calcula Novo Saldo de Prejuízo
                //

                saldoPrejTotal = saldoPrejTotal - certificados[1].VlrPrejCompensado;
                certificados[i].SaldoPrejuizo = saldoPrejTotal;

                //
                // Calcula o Valor do IR
                //
                certificados[i].IR = Utils.TruncarValor(certificados[i].CotasTributada * certificados[i].IRCota, 2);
            }

            return certificados;
        }

        public static void CabecalhoPeriodos()
        {
            Utils.GravaRegistro(" ");
            string registro = " ; ;" + "Ano;" + "Aliquota_Ir;" +
                "Cotacao_Inicial;" + "Cotacao_Fim;" + "Rendimento;" + "Prej_A_Compensar;" +
                "Prej_Compensado;" + "Saldo_Prej_Cota;" + "Base_Calc_IR;" + "IR_Cota;" + "Saldo_Prej_Reais";
            Utils.GravaRegistro(registro);
        }

        public static void GravaPeriodos(Periodos calcPorPeriodo)
        {
            string registro = " ; ;" +
                calcPorPeriodo.Ano + ";" +
                calcPorPeriodo.Aliquota_Ir.ToString("N2", new CultureInfo("pt-BR")) + ";" +
                calcPorPeriodo.CotacaoInicial.ToString("N7", new CultureInfo("pt-BR")) + ";" +
                calcPorPeriodo.CotacaoFim.ToString("N7", new CultureInfo("pt-BR")) + ";" +
                calcPorPeriodo.Rendimento.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                calcPorPeriodo.PrejACompensar.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                calcPorPeriodo.PrejCompensado.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                calcPorPeriodo.SaldoPrejCota.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                calcPorPeriodo.BaseCalcIR.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                calcPorPeriodo.IRCota.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                calcPorPeriodo.SaldoPrejReais.ToString("N2", new CultureInfo("pt-BR"));
            Utils.GravaRegistro(registro);
        }

        public static void CabecalhoCertificados()
        {
            Utils.GravaRegistro(" ");
            string registro = "Dtultrib;" + "Sdoctapl;" + "Cotaplic;" +
                "RendCertificado;" + "SaldoPrejuizo;" + "CotasIsentaMax;" +
                "CotasIsenta;" + "CotasTributada;" + "VlrPrejCompensado;" +
                "VlrPrejCertificado;" + "IRCota;" + "IR;";
            Utils.GravaRegistro(registro);

        }

        public static void GravaCertificados(Certificado cert)
        {
            string registro =
                cert.Dtultrib + ";" +
                cert.Sdoctapl.ToString("N5", new CultureInfo("pt-BR")) + ";" +
                cert.Cotaplic.ToString("N7", new CultureInfo("pt-BR")) + ";" +
                cert.RendCertificado.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                cert.SaldoPrejuizo.ToString("N2", new CultureInfo("pt-BR")) + ";" +
                cert.CotasIsentaMax.ToString("N5", new CultureInfo("pt-BR")) + ";" +
                cert.CotasIsenta.ToString("N5", new CultureInfo("pt-BR")) + ";" +
                cert.CotasTributada.ToString("N5", new CultureInfo("pt-BR")) + ";" +
                cert.VlrPrejCompensado.ToString("N2", new CultureInfo("pt-BR")) + ";" +
                cert.VlrPrejCertificado.ToString("N2", new CultureInfo("pt-BR")) + ";" +
                cert.IRCota.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                cert.IR.ToString("N2", new CultureInfo("pt-BR"));
            Utils.GravaRegistro(registro);
        }


        public static void GravaRegistro(string registro)
        {
            try
            {
                DateTime dateTime = DateTime.Now;
                string dataHora = dateTime.ToString("yyyy-MM-dd-HHmmss", CultureInfo.InvariantCulture);
                string pathSaida = @"C:\Users\fefug_skli85i\Documents\Temp\Saida";
                string pathCalc = @"C:\Users\fefug_skli85i\Documents\Temp\Saida\CALC-" + dataHora + ".csv";

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