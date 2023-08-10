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
        //
        // Trunca o valor igual o padrao do mainframe
        //
        public static decimal TruncarValor(decimal valor, int precisao = 2, MidpointRounding metodoTruncar = MidpointRounding.ToZero)
        {
            return decimal.Round(valor, precisao, metodoTruncar);
        }

        //
        // Obtem o Ano de uma Data
        //

        public static int Ano(string data)
        {
            return int.Parse(data.Substring(6));
        }

        //
        // Calcula os periodos de aliquotas de IR do Certificado e compensacao de prejuizo entre periodos
        //
        public static List<Periodos> CalculoPorPeriodo(List<Periodos> periodos, decimal cotacaoInicio, decimal cotamaisrecente, decimal saldoCotasCertificado)
        {
            //
            // Atualiza Cotação Inicial e Fim dos Periodos
            //
            periodos[0].CotacaoInicio = cotacaoInicio;

            for (int i = 1; i < periodos.Count; i++)
            {
                periodos[i].CotacaoInicio = periodos[i - 1].CotacaoFim;
            }
            periodos[periodos.Count - 1].CotacaoFim = cotamaisrecente;

            //
            // Calcula Rendimento dos Períodos
            //

            for (int i = 0; i < periodos.Count; i++)
            {
                periodos[i].RendimentoPorCota = periodos[i].CotacaoFim - periodos[i].CotacaoInicio;
            }

            //
            // Compensa Prejuízo entre Períodos
            //

            decimal saldoPrejuizoPorCota = 0.0m;

            for (int i = 0; i < periodos.Count; i++)
            {
                periodos[i].PrejuizoACompensarPorCota = 0.0m;
                periodos[i].PrejuizoCompensadoPorCota = 0.0m;
                if (periodos[i].RendimentoPorCota < 0)
                {
                    periodos[i].PrejuizoACompensarPorCota = periodos[i].RendimentoPorCota * -1;
                    saldoPrejuizoPorCota += periodos[i].PrejuizoACompensarPorCota;
                }
                else
                {
                    if (saldoPrejuizoPorCota > 0)
                    {
                        if (saldoPrejuizoPorCota > periodos[i].RendimentoPorCota)
                        {
                            periodos[i].PrejuizoCompensadoPorCota = periodos[i].RendimentoPorCota;
                            saldoPrejuizoPorCota -= periodos[i].PrejuizoCompensadoPorCota;
                        }
                        else
                        {
                            periodos[i].PrejuizoCompensadoPorCota = saldoPrejuizoPorCota;
                            saldoPrejuizoPorCota = 0.0m;
                        }
                    }
                }

                periodos[i].BaseCalculoIRPorCota = periodos[i].RendimentoPorCota + periodos[i].PrejuizoACompensarPorCota - periodos[i].PrejuizoCompensadoPorCota;
                periodos[i].SaldoPrejuizoPorCota = saldoPrejuizoPorCota;
                periodos[i].SaldoPrejuizo = Utils.TruncarValor(periodos[i].SaldoPrejuizoPorCota * saldoCotasCertificado, 2);

                //
                // Calcula IR por cota
                //

                periodos[i].IRPorCota = Utils.TruncarValor(periodos[i].BaseCalculoIRPorCota * periodos[i].AliquotaIR, 10);

            }

            return periodos;

        }
        //
        // Calculo do Certificado e Compensação de Prejuízo entre Certificados
        //
        public static List<Certificado> CalculoCertificado(List<Certificado> certificados, decimal saldoPrejuizo, decimal cotacaoMaisRecente, decimal cotasBloqueadas)
        {
            for (int i = 0; i < certificados.Count; i++)
            {
                decimal cotasPrejuizoMaximo = 0.0m;
                decimal cotasPrejuizoMaximoAjuste = 0.0m;
                decimal ValorPrejuizoMaximo = 0.0m;

                //
                // Soma Prejuizo do Certificado no Saldo Prejuízo Total
                //

                saldoPrejuizo += certificados[i].PrejuizoACompensar;

                //
                // Calcula quantidade maxima de cotas para compensar o Saldo de Prejuízo
                //

                if (certificados[i].RendimentoPorCota > 0 && saldoPrejuizo > 0)
                {
                    cotasPrejuizoMaximo = Utils.TruncarValor(saldoPrejuizo / certificados[i].RendimentoPorCota, 5);
                    ValorPrejuizoMaximo = Utils.TruncarValor(cotasPrejuizoMaximo * certificados[i].RendimentoPorCota, 2);
                    if (saldoPrejuizo == ValorPrejuizoMaximo)
                    {
                        certificados[i].CotasIsentaMaximo = cotasPrejuizoMaximo;
                    }
                    else
                    {
                        cotasPrejuizoMaximoAjuste = cotasPrejuizoMaximo + 0.00001m;
                        ValorPrejuizoMaximo = Utils.TruncarValor(cotasPrejuizoMaximoAjuste * certificados[i].RendimentoPorCota, 2);
                        if (saldoPrejuizo == ValorPrejuizoMaximo)
                        {
                            certificados[i].CotasIsentaMaximo = cotasPrejuizoMaximoAjuste;
                        }
                        else
                        {
                            certificados[i].CotasIsentaMaximo = cotasPrejuizoMaximo;
                        }
                    }
                }
                else
                {
                    certificados[i].CotasIsentaMaximo = 0.0m;
                }

                //
                // Calcula Quantidade de Cotas Isentas
                //

                if (certificados[i].CotasIsentaMaximo > certificados[i].SaldoCotasCertificado)
                {
                    certificados[i].CotasIsenta = certificados[i].SaldoCotasCertificado;
                }
                else
                {
                    certificados[i].CotasIsenta = certificados[i].CotasIsentaMaximo;
                }

                //
                // Calcula Quantidade de Cotas Tributadas
                //

                certificados[i].CotasTributada = certificados[i].SaldoCotasCertificado - certificados[i].CotasIsenta;

                //
                // Calcula Prejuizo Compensado
                //

                certificados[i].PrejuizoCompensado = Utils.TruncarValor(certificados[i].CotasIsenta * certificados[i].RendimentoPorCota, 2);

                //
                // Calcula Novo Saldo de Prejuízo
                //

                saldoPrejuizo -= certificados[i].PrejuizoCompensado;
                certificados[i].SaldoPrejuizo = saldoPrejuizo;

                //
                // Calcula Cota Liquida Tributada
                //

                certificados[i].CotaLiquidaTributada = cotacaoMaisRecente - certificados[i].IRPorCota;

                //
                // Calcula Valor Bruto
                //

                certificados[i].ValorBruto = Utils.TruncarValor(certificados[i].SaldoCotasCertificado * cotacaoMaisRecente, 2);

                //
                // Calcula o Valor do IR
                //
                certificados[i].ValorIR = Utils.TruncarValor(certificados[i].CotasTributada * certificados[i].IRPorCota, 2);

                //
                // Calcula Valor Liquido
                //

                certificados[i].ValorLiquido = certificados[i].ValorBruto - certificados[i].ValorIR;

                //
                // Calcula Custo da Aplicacao
                //

                certificados[i].CustoAplicacao = Utils.TruncarValor(certificados[i].SaldoCotasCertificado * certificados[i].CotacaoAplicacao, 2);

            }

            //
            // Calcula Valor do Bloqueio em Cotas
            //

            for (int i2 = certificados.Count - 1; i2 >= 0 && cotasBloqueadas > 0; i2--)
            {
                decimal cotasLivres = 0.0m;
                decimal valorLiquidoLivre = 0.0m;

                if (certificados[i2].SaldoCotasCertificado > cotasBloqueadas)
                {
                    cotasLivres = certificados[i2].SaldoCotasCertificado - cotasBloqueadas;
                    cotasBloqueadas = 0.0m;
                }
                else
                {
                    cotasLivres = 0.0m;
                    cotasBloqueadas -= certificados[i2].SaldoCotasCertificado;
                }

                valorLiquidoLivre = Utils.TruncarValor(cotasLivres * certificados[i2].CotaLiquidaTributada, 2);

                certificados[i2].ValorBloqueadoEmCotas = certificados[i2].ValorLiquido - valorLiquidoLivre;
            }

            return certificados;
        }

        public static void CabecalhoPeriodos()
        {
            Utils.GravaRegistro(" ");
            string registro = "*** Periodos ***; Data_Aplicacao;" + "Ano;" + "Aliquota_Ir;" +
                "Cotacao_Inicio;" + "Cotacao_Fim;" + "Rendimento;" + "Prejuizo_A_Compensar;" +
                "Prejuizo_Compensado;" + "Saldo_Prejuizo_Por_Cota;" + "Base_Calc_IR_Por_Cota;" + "IR_Por_Cota;" + "Saldo_Prejuizo";
            Utils.GravaRegistro(registro);
        }

        public static void GravaPeriodos(string dtultrib, Periodos calculoPorPeriodo)
        {
            string registro = ";" +
                dtultrib + ";" +
                calculoPorPeriodo.Ano + ";" +
                calculoPorPeriodo.AliquotaIR.ToString("N2", new CultureInfo("pt-BR")) + ";" +
                calculoPorPeriodo.CotacaoInicio.ToString("N7", new CultureInfo("pt-BR")) + ";" +
                calculoPorPeriodo.CotacaoFim.ToString("N7", new CultureInfo("pt-BR")) + ";" +
                calculoPorPeriodo.RendimentoPorCota.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                calculoPorPeriodo.PrejuizoACompensarPorCota.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                calculoPorPeriodo.PrejuizoCompensadoPorCota.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                calculoPorPeriodo.SaldoPrejuizoPorCota.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                calculoPorPeriodo.BaseCalculoIRPorCota.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                calculoPorPeriodo.IRPorCota.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                calculoPorPeriodo.SaldoPrejuizo.ToString("N2", new CultureInfo("pt-BR"));
            Utils.GravaRegistro(registro);
        }

        public static void CabecalhoCertificados()
        {
            Utils.GravaRegistro(" ");
            string registro = "*** Certificados ***;" + "Data_Aplicacao;" + "Saldo_Cotas;" + "Cotacao_Aplicacao;" +
                "Rendimento;" + "Saldo_Prejuizo;" + "Cotas_Isenta_Maximo;" +
                "Cotas_Isenta;" + "Cotas_Tributada;" + "Prejuizo_Compensado;" +
                "Prejuizo_A_Compensar;" + "IR_Por_Cota;" + "Cota_Liquida_Tributada;" +
                "Valor_Bruto;" + "Valor_IR;" + "Valor_Liquido;" + "Valor_Bloqueio_Em_Cotas;" +
                "Custo_Aplicacao";
            Utils.GravaRegistro(registro);

        }

        public static void GravaCertificados(Certificado certificados)
        {
            string registro = ";" +
                certificados.DataAplicacao + ";" +
                certificados.SaldoCotasCertificado.ToString("N5", new CultureInfo("pt-BR")) + ";" +
                certificados.CotacaoAplicacao.ToString("N7", new CultureInfo("pt-BR")) + ";" +
                certificados.RendimentoPorCota.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                certificados.SaldoPrejuizo.ToString("N2", new CultureInfo("pt-BR")) + ";" +
                certificados.CotasIsentaMaximo.ToString("N5", new CultureInfo("pt-BR")) + ";" +
                certificados.CotasIsenta.ToString("N5", new CultureInfo("pt-BR")) + ";" +
                certificados.CotasTributada.ToString("N5", new CultureInfo("pt-BR")) + ";" +
                certificados.PrejuizoCompensado.ToString("N2", new CultureInfo("pt-BR")) + ";" +
                certificados.PrejuizoACompensar.ToString("N2", new CultureInfo("pt-BR")) + ";" +
                certificados.IRPorCota.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                certificados.CotaLiquidaTributada.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                certificados.ValorBruto.ToString("N2", new CultureInfo("pt-BR")) + ";" +
                certificados.ValorIR.ToString("N2", new CultureInfo("pt-BR")) + ";" +
                certificados.ValorLiquido.ToString("N2", new CultureInfo("pt-BR")) + ";" +
                certificados.ValorBloqueadoEmCotas.ToString("N2", new CultureInfo("pt-BR")) + ";" +
                certificados.CustoAplicacao.ToString("N2", new CultureInfo("pt-BR"));
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