using Calculo_RV_CM.Entities;
using System.Globalization;

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
        public static List<Periodos> CalculoPorPeriodo(List<Periodos> periodos, decimal cotacaoInicio, decimal cotamaisrecente, decimal saldoCotasCertificado,
                                                       decimal saldoAmortizacaoDePrincipal)
        {
            // Atualiza Cotacao Inicial e Ajuste com Amortização de Principal

            if (saldoAmortizacaoDePrincipal > 0)
            {
                periodos[0].SaldoAmortizacaoDePrincipalPorCota = Utils.TruncarValor(saldoAmortizacaoDePrincipal / saldoCotasCertificado, 11);
                if (periodos[0].SaldoAmortizacaoDePrincipalPorCota > cotacaoInicio)
                {
                    periodos[0].CotacaoInicio = 0.0m;
                }
                else
                {
                    periodos[0].CotacaoInicio = Utils.TruncarValor(cotacaoInicio - periodos[0].SaldoAmortizacaoDePrincipalPorCota, 7);
                }
            }
            else
            {
                periodos[0].CotacaoInicio = cotacaoInicio;
            }

            // Atualiza Cotacao Fim dos Periodos

            for (int i = 1; i < periodos.Count; i++)
            {
                periodos[i].CotacaoInicio = periodos[i - 1].CotacaoFim;
            }
            periodos[periodos.Count - 1].CotacaoFim = cotamaisrecente;

            // Calcula Rendimento dos Períodos

            for (int i = 0; i < periodos.Count; i++)
            {
                periodos[i].RendimentoPorCota = periodos[i].CotacaoFim - periodos[i].CotacaoInicio;
            }

            // Compensa Prejuízo entre Períodos

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

                // Calcula IR por cota

                periodos[i].IRPorCota = Utils.TruncarValor(periodos[i].BaseCalculoIRPorCota * periodos[i].AliquotaIR, 10);
            }

            return periodos;

        }
        //
        // Calculo do Certificado e Compensação de Prejuízo entre Certificados
        //
        public static List<Certificados> CalculoCertificado(List<Certificados> certificados, decimal saldoPrejuizo, decimal cotacaoMaisRecente)
        {
            for (int i = 0; i < certificados.Count; i++)
            {
                decimal cotasPrejuizoMaximo = 0.0m;
                decimal cotasPrejuizoMaximoAjuste = 0.0m;
                decimal ValorPrejuizoMaximo = 0.0m;

                // Soma Prejuizo a Compensar do Certificado no Saldo Prejuízo Total

                saldoPrejuizo += certificados[i].PrejuizoACompensar;

                // Calcula quantidade maxima de cotas para compensar o Saldo de Prejuízo

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

                // Calcula Quantidade de Cotas Isentas

                if (certificados[i].CotasIsentaMaximo > certificados[i].SaldoCotasCertificado)
                {
                    certificados[i].CotasIsenta = certificados[i].SaldoCotasCertificado;
                }
                else
                {
                    certificados[i].CotasIsenta = certificados[i].CotasIsentaMaximo;
                }

                // Calcula Quantidade de Cotas Tributadas

                certificados[i].CotasTributada = certificados[i].SaldoCotasCertificado - certificados[i].CotasIsenta;

                // Calcula Prejuizo Compensado

                certificados[i].PrejuizoCompensado = Utils.TruncarValor(certificados[i].CotasIsenta * certificados[i].RendimentoPorCota, 2);

                // Calcula Novo Saldo de Prejuízo

                saldoPrejuizo -= certificados[i].PrejuizoCompensado;
                certificados[i].SaldoPrejuizo = saldoPrejuizo;

                // Calcula Cota Liquida Tributada

                certificados[i].CotaLiquidaTributada = cotacaoMaisRecente - certificados[i].IRPorCota;

                // Calcula Valor Bruto

                certificados[i].ValorBruto = Utils.TruncarValor(certificados[i].SaldoCotasCertificado * cotacaoMaisRecente, 2);

                // Calcula o Valor do IR

                certificados[i].ValorIR = Utils.TruncarValor(certificados[i].CotasTributada * certificados[i].IRPorCota, 2);

                // Calcula Valor Liquido

                certificados[i].ValorLiquido = certificados[i].ValorBruto - certificados[i].ValorIR;

                // Calcula Custo da Aplicacao

                certificados[i].CustoAplicacao = Utils.TruncarValor(certificados[i].SaldoCotasCertificado * certificados[i].CotacaoAplicacao, 2);

            }
            return certificados;
        }
        public static decimal CalculaSaldoBloqueado(List<Certificados> certificados, decimal valorBloqueado, decimal cotasBloqueadas)
        {
            decimal saldoBloqueado = 0.0m;
            decimal cotasLivres = 0.0m;
            decimal valorLiquidoLivre = 0.0m;

            for (int i = certificados.Count - 1; i >= 0 && cotasBloqueadas > 0; i--)
            {
                if (certificados[i].SaldoCotasCertificado > cotasBloqueadas)
                {
                    cotasLivres = certificados[i].SaldoCotasCertificado - cotasBloqueadas;
                    cotasBloqueadas = 0.0m;
                }
                else
                {
                    cotasLivres = 0.0m;
                    cotasBloqueadas -= certificados[i].SaldoCotasCertificado;
                }

                valorLiquidoLivre = Utils.TruncarValor(cotasLivres * certificados[i].CotaLiquidaTributada, 2);
                saldoBloqueado += certificados[i].ValorLiquido - valorLiquidoLivre;
            }

            // Soma Cotas Bloqueadas Restantes

            saldoBloqueado += Utils.TruncarValor(cotasBloqueadas * certificados[0].CotaLiquidaTributada, 2);

            //Soma Valor Bloqueado

            saldoBloqueado += valorBloqueado;

            return saldoBloqueado;
        }

        public static void GravaDadosDeEntrada(Saldo saldo, Fundos fundos, Bloqueios bloqueios)
        {
            Console.WriteLine("Cotação Mais Recente : " + fundos.CotacaoMaisRecente.ToString("N7", CultureInfo.InvariantCulture));
            Console.WriteLine("Custo Médio          : " + saldo.CustoMedio().ToString("N7", CultureInfo.InvariantCulture));
            Console.WriteLine("Saldo Prejuizo       : " + saldo.SaldoPrejuizo.ToString("N2", CultureInfo.InvariantCulture));
            Console.WriteLine("Valor Bloqueado      : " + bloqueios.ValorBloqueadoTotal.ToString("N2", CultureInfo.InvariantCulture));
            Console.WriteLine("Cotas Bloqueadas     : " + bloqueios.CotasBloqueadasTotal.ToString("N5", CultureInfo.InvariantCulture));
            Console.WriteLine("------------------------------------------------------------------------");

            Utils.GravaRegistro("Cotacao Mais Recente;" + fundos.CotacaoMaisRecente.ToString("N7", new CultureInfo("pr-BR")));
            Utils.GravaRegistro("Custo Medio;" + saldo.CustoMedio().ToString("N7", new CultureInfo("pr-BR")));
            Utils.GravaRegistro("Saldo Prejuizo;" + saldo.SaldoPrejuizo.ToString("N2", new CultureInfo("pr-BR")));
            Utils.GravaRegistro("Valor Bloqueado;" + bloqueios.ValorBloqueadoTotal.ToString("N2", new CultureInfo("pr-BR")));
            Utils.GravaRegistro("Cotas Bloqueadas;" + bloqueios.CotasBloqueadasTotal.ToString("N5", new CultureInfo("pr-BR")));
        }

        public static void GravaPeriodosDoCertificado(List<Certificados> certificado)
        {
            Utils.GravaRegistro(" ");
            string registro = "*** Periodos ***; Data_Aplicacao;" + "Ano;" + "Aliquota_Ir;" +
                "Saldo Amortizacao Por Cota;" + "Cotacao_Inicio;" + "Cotacao_Fim;" + "Rendimento;" + "Prejuizo_A_Compensar;" +
                "Prejuizo_Compensado;" + "Saldo_Prejuizo_Por_Cota;" + "Base_Calc_IR_Por_Cota;" + "IR_Por_Cota;" + "Saldo_Prejuizo";
            Utils.GravaRegistro(registro);

            foreach (Certificados obj in certificado)
            {
                foreach (Periodos obj2 in obj.PeriodoCalculado)
                {
                    registro = ";" +
                               obj.DataAplicacao + ";" +
                               obj2.Ano + ";" +
                               obj2.AliquotaIR.ToString("N2", new CultureInfo("pt-BR")) + ";" +
                               obj2.SaldoAmortizacaoDePrincipalPorCota.ToString("N11", new CultureInfo("pt-BR")) + ";" +
                               obj2.CotacaoInicio.ToString("N7", new CultureInfo("pt-BR")) + ";" +
                               obj2.CotacaoFim.ToString("N7", new CultureInfo("pt-BR")) + ";" +
                               obj2.RendimentoPorCota.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                               obj2.PrejuizoACompensarPorCota.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                               obj2.PrejuizoCompensadoPorCota.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                               obj2.SaldoPrejuizoPorCota.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                               obj2.BaseCalculoIRPorCota.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                               obj2.IRPorCota.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                               obj2.SaldoPrejuizo.ToString("N2", new CultureInfo("pt-BR"));
                    Utils.GravaRegistro(registro);

                }
            }

        }
        public static void GravaCertificadosCalculados(List<Certificados> certificados)
        {
            Utils.GravaRegistro(" ");
            string registro = "*** Certificados ***;" + "Data_Aplicacao;" + "Saldo_Cotas;" + "Cotacao_Aplicacao;" +
               "Saldo Amortizacao;" + "Rendimento;" + "Saldo_Prejuizo;" + "Cotas_Isenta_Maximo;" +
                "Cotas_Isenta;" + "Cotas_Tributada;" + "Prejuizo_Compensado;" +
                "Prejuizo_A_Compensar;" + "IR_Por_Cota;" + "Cota_Liquida_Tributada;" +
                "Valor_Bruto;" + "Valor_IR;" + "Valor_Liquido;" + "Custo_Aplicacao";
            Utils.GravaRegistro(registro);

            foreach (Certificados obj in certificados)
            {
                registro = ";" +
                           obj.DataAplicacao + ";" +
                           obj.SaldoCotasCertificado.ToString("N5", new CultureInfo("pt-BR")) + ";" +
                           obj.CotacaoAplicacao.ToString("N7", new CultureInfo("pt-BR")) + ";" +
                           obj.SaldoAmortizacaoDePrincipal.ToString("N2", new CultureInfo("pt-BR")) + ";" +
                           obj.RendimentoPorCota.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                           obj.SaldoPrejuizo.ToString("N2", new CultureInfo("pt-BR")) + ";" +
                           obj.CotasIsentaMaximo.ToString("N5", new CultureInfo("pt-BR")) + ";" +
                           obj.CotasIsenta.ToString("N5", new CultureInfo("pt-BR")) + ";" +
                           obj.CotasTributada.ToString("N5", new CultureInfo("pt-BR")) + ";" +
                           obj.PrejuizoCompensado.ToString("N2", new CultureInfo("pt-BR")) + ";" +
                           obj.PrejuizoACompensar.ToString("N2", new CultureInfo("pt-BR")) + ";" +
                           obj.IRPorCota.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                           obj.CotaLiquidaTributada.ToString("N10", new CultureInfo("pt-BR")) + ";" +
                           obj.ValorBruto.ToString("N2", new CultureInfo("pt-BR")) + ";" +
                           obj.ValorIR.ToString("N2", new CultureInfo("pt-BR")) + ";" +
                           obj.ValorLiquido.ToString("N2", new CultureInfo("pt-BR")) + ";" +
                           obj.CustoAplicacao.ToString("N2", new CultureInfo("pt-BR"));
                Utils.GravaRegistro(registro);
            }
        }
        public static void GravaCertificados(Certificados certificados)
        {
            string registro = ";" +
                certificados.DataAplicacao + ";" +
                certificados.SaldoCotasCertificado.ToString("N5", new CultureInfo("pt-BR")) + ";" +
                certificados.CotacaoAplicacao.ToString("N7", new CultureInfo("pt-BR")) + ";" +
                certificados.SaldoAmortizacaoDePrincipal.ToString("N2", new CultureInfo("pt-BR")) + ";" +
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