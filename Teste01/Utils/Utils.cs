using Calculo_RV_CM.Entities;
using System.Globalization;
using System.Runtime.ConstrainedExecution;

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

        public static List<PosicaoAnalitica> CalculaPosicaoAnalitica(List<Certificado> certificados, Saldo saldo, Fundos fundos)
        {
            List<PosicaoAnalitica> listaPosicaoAnalitica = new List<PosicaoAnalitica>();

            foreach (Certificado Cert in certificados)
            {
                List<AliquotasIR> AliquotasIRDoCertificado = saldo.AliquotasIRDoCertificado(Cert.DataCotizacao);
                List<Periodos> listaPeriodos = new List<Periodos>();

                foreach (AliquotasIR Aliq in AliquotasIRDoCertificado)
                {
                    Periodos periodo = new Periodos();
                    periodo.Ano = Aliq.Ano;
                    periodo.AliquotaIR = Aliq.AliquotaIR;
                    periodo.CotacaoInicio = 0.0m;
                    periodo.SaldoAmortizacaoDePrincipalPorCota = 0.0m;
                    periodo.CotacaoFim = Aliq.CotacaoFim;
                    periodo.RendimentoPorCota = 0.0m;
                    periodo.PrejuizoACompensarPorCota = 0.0m;
                    periodo.PrejuizoCompensadoPorCota = 0.0m;
                    periodo.SaldoPrejuizoPorCota = 0.0m;
                    periodo.BaseCalculoIRPorCota = 0.0m;
                    periodo.IRPorCota = 0.0m;
                    periodo.SaldoPrejuizo = 0.0m;
                    listaPeriodos.Add(periodo);
                }

                listaPeriodos = Utils.CalculoPorPeriodo(listaPeriodos, saldo.CustoMedio(), fundos.CotacaoMaisRecente, Cert.SaldoCotasCertificado,
                                                        Cert.SaldoAmortizacaoDePrincipal);
                PosicaoAnalitica posicaoAnalitica = new PosicaoAnalitica();
                posicaoAnalitica.DataCotizacao = Cert.DataCotizacao;
                posicaoAnalitica.SaldoCotasCertificado = Cert.SaldoCotasCertificado;
                posicaoAnalitica.CotacaoAplicacao = Cert.CotacaoAplicacao;
                posicaoAnalitica.SaldoAmortizacaoDePrincipal = Cert.SaldoAmortizacaoDePrincipal;
                posicaoAnalitica.RendimentoPorCota = listaPeriodos.Sum(x => x.RendimentoPorCota);
                posicaoAnalitica.SaldoPrejuizo = 0.0m;
                posicaoAnalitica.CotasIsentaMaximo = 0.0m;
                posicaoAnalitica.CotasIsenta = 0.0m;
                posicaoAnalitica.CotasTributada = 0.0m;
                posicaoAnalitica.PrejuizoCompensado = 0.0m;
                posicaoAnalitica.PrejuizoACompensar = listaPeriodos[listaPeriodos.Count - 1].SaldoPrejuizo;
                posicaoAnalitica.IRPorCota = listaPeriodos.Sum(x => x.IRPorCota);
                posicaoAnalitica.CotaLiquidaTributada = 0.0m;
                posicaoAnalitica.ValorBruto = 0.0m;
                posicaoAnalitica.ValorIR = 0.0m;
                posicaoAnalitica.ValorLiquido = 0.0m;
                posicaoAnalitica.PeriodoCalculado = listaPeriodos;
                listaPosicaoAnalitica.Add(posicaoAnalitica);
            }

            listaPosicaoAnalitica = Utils.CalculoCertificado(listaPosicaoAnalitica, saldo.SaldoPrejuizo, fundos.CotacaoMaisRecente);

            return listaPosicaoAnalitica;
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
        public static List<PosicaoAnalitica> CalculoCertificado(List<PosicaoAnalitica> posicaoAnalitica, decimal saldoPrejuizo, decimal cotacaoMaisRecente)
        {
            for (int i = 0; i < posicaoAnalitica.Count; i++)
            {
                decimal cotasPrejuizoMaximo = 0.0m;
                decimal cotasPrejuizoMaximoAjuste = 0.0m;
                decimal ValorPrejuizoMaximo = 0.0m;

                // Soma Prejuizo a Compensar do Certificado no Saldo Prejuízo Total

                saldoPrejuizo += posicaoAnalitica[i].PrejuizoACompensar;

                // Calcula quantidade maxima de cotas para compensar o Saldo de Prejuízo

                if (posicaoAnalitica[i].RendimentoPorCota > 0 && saldoPrejuizo > 0)
                {
                    cotasPrejuizoMaximo = Utils.TruncarValor(saldoPrejuizo / posicaoAnalitica[i].RendimentoPorCota, 5);
                    ValorPrejuizoMaximo = Utils.TruncarValor(cotasPrejuizoMaximo * posicaoAnalitica[i].RendimentoPorCota, 2);
                    if (saldoPrejuizo == ValorPrejuizoMaximo)
                    {
                        posicaoAnalitica[i].CotasIsentaMaximo = cotasPrejuizoMaximo;
                    }
                    else
                    {
                        cotasPrejuizoMaximoAjuste = cotasPrejuizoMaximo + 0.00001m;
                        ValorPrejuizoMaximo = Utils.TruncarValor(cotasPrejuizoMaximoAjuste * posicaoAnalitica[i].RendimentoPorCota, 2);
                        if (saldoPrejuizo == ValorPrejuizoMaximo)
                        {
                            posicaoAnalitica[i].CotasIsentaMaximo = cotasPrejuizoMaximoAjuste;
                        }
                        else
                        {
                            posicaoAnalitica[i].CotasIsentaMaximo = cotasPrejuizoMaximo;
                        }
                    }
                }
                else
                {
                    posicaoAnalitica[i].CotasIsentaMaximo = 0.0m;
                }

                // Calcula Quantidade de Cotas Isentas

                if (posicaoAnalitica[i].CotasIsentaMaximo > posicaoAnalitica[i].SaldoCotasCertificado)
                {
                    posicaoAnalitica[i].CotasIsenta = posicaoAnalitica[i].SaldoCotasCertificado;
                }
                else
                {
                    posicaoAnalitica[i].CotasIsenta = posicaoAnalitica[i].CotasIsentaMaximo;
                }

                // Calcula Quantidade de Cotas Tributadas

                posicaoAnalitica[i].CotasTributada = posicaoAnalitica[i].SaldoCotasCertificado - posicaoAnalitica[i].CotasIsenta;

                // Calcula Prejuizo Compensado

                posicaoAnalitica[i].PrejuizoCompensado = Utils.TruncarValor(posicaoAnalitica[i].CotasIsenta * posicaoAnalitica[i].RendimentoPorCota, 2);

                // Calcula Novo Saldo de Prejuízo

                saldoPrejuizo -= posicaoAnalitica[i].PrejuizoCompensado;
                posicaoAnalitica[i].SaldoPrejuizo = saldoPrejuizo;

                // Calcula Cota Liquida Tributada

                posicaoAnalitica[i].CotaLiquidaTributada = cotacaoMaisRecente - posicaoAnalitica[i].IRPorCota;

                // Calcula Valor Bruto

                posicaoAnalitica[i].ValorBruto = Utils.TruncarValor(posicaoAnalitica[i].SaldoCotasCertificado * cotacaoMaisRecente, 2);

                // Calcula o Valor do IR

                posicaoAnalitica[i].ValorIR = 0.0m;

                foreach (Periodos periodos in posicaoAnalitica[i].PeriodoCalculado)
                {
                    posicaoAnalitica[i].ValorIR += Utils.TruncarValor(posicaoAnalitica[i].CotasTributada * periodos.IRPorCota, 2);
                }

                // Calcula Valor Liquido

                posicaoAnalitica[i].ValorLiquido = posicaoAnalitica[i].ValorBruto - posicaoAnalitica[i].ValorIR;

                // Calcula Custo da Aplicacao

                posicaoAnalitica[i].CustoAplicacao = Utils.TruncarValor(posicaoAnalitica[i].SaldoCotasCertificado * posicaoAnalitica[i].CotacaoAplicacao, 2);

            }
            return posicaoAnalitica;
        }
        public static decimal CalculaSaldoBloqueado(List<PosicaoAnalitica> certificados, decimal valorBloqueado, decimal cotasBloqueadas)
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

        public static decimal CalculaDisponivelResgate(decimal saldoLiquido, decimal saldoBloqueado, decimal aplicacoes, decimal resgates)
        {
            decimal disponivelResgate = saldoLiquido - saldoBloqueado + aplicacoes - resgates;

            if (disponivelResgate < 0)
            {
                disponivelResgate = 0.0m;
            }

            return disponivelResgate;
        }

        public static decimal CalcularBloqueioSubconta(string bloqueioSubconta, decimal disponivelResgate, decimal saldoBloqueado)
        {
            decimal saldoBloqueadoTotal = saldoBloqueado;

            if (bloqueioSubconta == "SIM")
            {
                saldoBloqueadoTotal += disponivelResgate;
            }

            return saldoBloqueadoTotal;
        }

        public static decimal CaluclarAjusteSaldoDisponivel(decimal saldoDisponivel, string bloqueioSubconta)
        {
            // Vamos ter outros ajustes, por exemplo se tiver resgate total o disponível sera sempre zerado

            decimal saldoDisponivelAjustado = saldoDisponivel;

            if (bloqueioSubconta == "SIM")
            {
                saldoDisponivelAjustado = 0;
            }

            return saldoDisponivelAjustado;
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

        public static void GravaPeriodosDoCertificado(List<PosicaoAnalitica> certificado)
        {
            Utils.GravaRegistro(" ");
            string registro = "*** Periodos ***; Data_Aplicacao;" + "Ano;" + "Aliquota_Ir;" +
                "Saldo Amortizacao Por Cota;" + "Cotacao_Inicio;" + "Cotacao_Fim;" + "Rendimento;" + "Prejuizo_A_Compensar;" +
                "Prejuizo_Compensado;" + "Saldo_Prejuizo_Por_Cota;" + "Base_Calc_IR_Por_Cota;" + "IR_Por_Cota;" + "Saldo_Prejuizo";
            Utils.GravaRegistro(registro);

            foreach (PosicaoAnalitica obj in certificado)
            {
                foreach (Periodos obj2 in obj.PeriodoCalculado)
                {
                    registro = ";" +
                               obj.DataCotizacao + ";" +
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
        public static void GravaCertificadosCalculados(List<PosicaoAnalitica> certificados)
        {
            Utils.GravaRegistro(" ");
            string registro = "*** Certificados ***;" + "Data_Aplicacao;" + "Saldo_Cotas;" + "Cotacao_Aplicacao;" +
               "Saldo Amortizacao;" + "Rendimento;" + "Saldo_Prejuizo;" + "Cotas_Isenta_Maximo;" +
                "Cotas_Isenta;" + "Cotas_Tributada;" + "Prejuizo_Compensado;" +
                "Prejuizo_A_Compensar;" + "IR_Por_Cota;" + "Cota_Liquida_Tributada;" +
                "Valor_Bruto;" + "Valor_IR;" + "Valor_Liquido;" + "Custo_Aplicacao";
            Utils.GravaRegistro(registro);

            foreach (PosicaoAnalitica obj in certificados)
            {
                registro = ";" +
                           obj.DataCotizacao + ";" +
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

        public static void GravaSaldoConsolidado(List<PosicaoAnalitica> certificados, Bloqueios bloqueios)
        {
            decimal custoAplicacao = certificados.Sum(x => x.CustoAplicacao);
            decimal saldoBruto = certificados.Sum(x => x.ValorBruto);
            decimal valorIR = certificados.Sum(x => x.ValorIR);
            decimal saldoLiquido = certificados.Sum(x => x.ValorLiquido);
            decimal saldoBloqueado = Utils.CalculaSaldoBloqueado(certificados, bloqueios.ValorBloqueadoTotal, bloqueios.CotasBloqueadasTotal);
            decimal aplicacoes = 0.0m;
            decimal resgates = 0.0m;
            decimal disponivelResgate = Utils.CalculaDisponivelResgate(saldoLiquido, saldoBloqueado, aplicacoes, resgates);
            saldoBloqueado = Utils.CalcularBloqueioSubconta(bloqueios.BloqueioSubconta, disponivelResgate, saldoBloqueado);
            disponivelResgate = Utils.CaluclarAjusteSaldoDisponivel(disponivelResgate, bloqueios.BloqueioSubconta);


            Utils.GravaRegistro(" ");
            Utils.GravaRegistro("*** Totais ***");
            Utils.GravaRegistro("Custo da Aplicacao;" + custoAplicacao.ToString("N2", new CultureInfo("pr-BR")));
            Utils.GravaRegistro("Saldo Bruto;" + saldoBruto.ToString("N2", new CultureInfo("pr-BR")));
            Utils.GravaRegistro("IR;" + valorIR.ToString("N2", new CultureInfo("pr-BR")));
            Utils.GravaRegistro("Saldo Liquido;" + saldoLiquido.ToString("N2", new CultureInfo("pr-BR")));
            Utils.GravaRegistro("Saldo Bloqueado;" + saldoBloqueado.ToString("N2", new CultureInfo("pr-BR")));
            Utils.GravaRegistro("Aplicacoes;" + aplicacoes.ToString("N2", new CultureInfo("pr-BR")));
            Utils.GravaRegistro("Resgates;" + resgates.ToString("N2", new CultureInfo("pr-BR")));
            Utils.GravaRegistro("Disponivel para Resgate;" + disponivelResgate.ToString("N2", new CultureInfo("pr-BR")));

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