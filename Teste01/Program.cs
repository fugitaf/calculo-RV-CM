using Calculo_RV_CM.Entities;
using System.Globalization;

namespace Calculo_RV_CM
{
    class Program
    {
        static void Main(string[] args)
        {
            decimal cotacaoMaisRecente = 0.0m;
            decimal saldoPrejuizo = 0.0m;
            decimal valorBloqueadoTotal = 0.0m;
            decimal cotasBloqueadasTotal = 0.0m;
            decimal custoMedio = 0.0m;
            decimal saldoBruto = 0.0m;
            decimal valorIR = 0.0m;
            decimal saldoLiquido = 0.0m;
            decimal saldoBloqueado = 0.0m;
            decimal custoAplicacao = 0.0m;
            List<AliquotasIR> listAliquotasIR = new List<AliquotasIR>();
            List<Certificado> listCert = new List<Certificado>();
            string pathSld = @"C:\Users\fefug_skli85i\Documents\Temp\SLD.csv";
            string pathApl = @"C:\Users\fefug_skli85i\Documents\Temp\APL.csv";
            Console.WriteLine("------------------------------------------------------------------------");

            //
            // Obter informações do Arquivo SLD.CSV
            //

            try
            {
                string[] linesSld = File.ReadAllLines(pathSld);
                foreach (string line in linesSld)
                {
                    string[] fields = line.Split(';');
                    if (fields[0] != "SDCOTMVN")
                    {
                        decimal saldoCotasSubconta = decimal.Parse(fields[0], new CultureInfo("pt-BR"));
                        decimal valorCustoMedio = decimal.Parse(fields[1], new CultureInfo("pt-BR"));
                        cotacaoMaisRecente = decimal.Parse(fields[2], new CultureInfo("pt-BR"));
                        saldoPrejuizo = decimal.Parse(fields[3], new CultureInfo("pt-BR"));
                        valorBloqueadoTotal = decimal.Parse(fields[4], new CultureInfo("pt-BR"));
                        cotasBloqueadasTotal = decimal.Parse(fields[5], new CultureInfo("pt-BR"));

                        for (int i = 6; i <= 21 && int.Parse(fields[i]) > 0; i += 3)
                        {
                            AliquotasIR aliquotasIR = new AliquotasIR();
                            aliquotasIR.Ano = int.Parse(fields[i], new CultureInfo("pt-BR"));
                            aliquotasIR.AliquotaIR = decimal.Parse(fields[i + 1], new CultureInfo("pt-BR"));
                            aliquotasIR.CotacaoFim = decimal.Parse(fields[i + 2], new CultureInfo("pr-BR"));
                            listAliquotasIR.Add(aliquotasIR);
                        }

                        Console.WriteLine("Aliquotas de IR do SLD");
                        foreach (AliquotasIR obj in listAliquotasIR)
                        {
                            Console.WriteLine(obj.Ano + "  " +
                                obj.AliquotaIR.ToString("N2", CultureInfo.InvariantCulture) +
                                "  " + obj.CotacaoFim.ToString("N7", CultureInfo.InvariantCulture));
                        }

                        Saldo sld = new Saldo(saldoCotasSubconta, valorCustoMedio, cotacaoMaisRecente, saldoPrejuizo, valorBloqueadoTotal, cotasBloqueadasTotal);

                        custoMedio = sld.CustoMedio();

                        Console.WriteLine("Cotação Mais Recente : " + sld.CotacaoMaisRecente.ToString("N7", CultureInfo.InvariantCulture));
                        Console.WriteLine("Custo Médio          : " + sld.CustoMedio().ToString("N7", CultureInfo.InvariantCulture));
                        Console.WriteLine("Saldo Prejuizo       : " + sld.SaldoPrejuizo.ToString("N2", CultureInfo.InvariantCulture));
                        Console.WriteLine("Valor Bloqueado      : " + sld.ValorBloqueadoTotal.ToString("N2", CultureInfo.InvariantCulture));
                        Console.WriteLine("Cotas Bloqueadas     : " + sld.CotasBloqueadasTotal.ToString("N5", CultureInfo.InvariantCulture));
                        Console.WriteLine("------------------------------------------------------------------------");

                        Utils.Utils.GravaRegistro("Cotacao Mais Recente;" + sld.CotacaoMaisRecente.ToString("N7", new CultureInfo("pr-BR")));
                        Utils.Utils.GravaRegistro("Custo Medio;" + sld.CustoMedio().ToString("N7", new CultureInfo("pr-BR")));
                        Utils.Utils.GravaRegistro("Saldo Prejuizo;" + sld.SaldoPrejuizo.ToString("N2", new CultureInfo("pr-BR")));
                        Utils.Utils.GravaRegistro("Valor Bloqueado;" + sld.ValorBloqueadoTotal.ToString("N2", new CultureInfo("pr-BR")));
                        Utils.Utils.GravaRegistro("Cotas Bloqueadas;" + sld.CotasBloqueadasTotal.ToString("N5", new CultureInfo("pr-BR")));

                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Erro no SLD");
                Console.WriteLine(e.Message);
            }

            //
            // Obter informações do Arquivo APL.CSV
            //

            try
            {
                string[] lines = File.ReadAllLines(pathApl);

                foreach (string line in lines)
                {
                    string[] fields = line.Split(';');
                    if (fields[0] != "DTULTRIB")
                    {
                        string dataAplicacao = fields[0];
                        decimal saldoCotasCertificado = decimal.Parse(fields[1], new CultureInfo("pt-BR"));
                        decimal cotacaoAplicacao = decimal.Parse(fields[2], new CultureInfo("pt-BR"));
                        decimal saldoAmortizacaoDePrincipal = decimal.Parse(fields[3], new CultureInfo("pt-BR"));

                        //
                        // Monta Lista de Periodos de Apliquotas de IR do Certificado
                        //

                        List<AliquotasIR> ListAliquotasIRCertificado = listAliquotasIR.FindAll(x => x.Ano >= Utils.Utils.Ano(dataAplicacao));

                        List<Periodos> listCalcPorPeriodo = new List<Periodos>();

                        foreach (AliquotasIR Obj in ListAliquotasIRCertificado)
                        {
                            Periodos periodo = new Periodos();
                            periodo.Ano = Obj.Ano;
                            periodo.AliquotaIR = Obj.AliquotaIR;
                            periodo.CotacaoInicio = 0.0m;
                            periodo.SaldoAmortizacaoDePrincipalPorCota = 0.0m;
                            periodo.CotacaoFim = Obj.CotacaoFim;
                            periodo.RendimentoPorCota = 0.0m;
                            periodo.PrejuizoACompensarPorCota = 0.0m;
                            periodo.PrejuizoCompensadoPorCota = 0.0m;
                            periodo.SaldoPrejuizoPorCota = 0.0m;
                            periodo.BaseCalculoIRPorCota = 0.0m;
                            periodo.IRPorCota = 0.0m;
                            periodo.SaldoPrejuizo = 0.0m;
                            listCalcPorPeriodo.Add(periodo);
                        }

                        //
                        // Faz Calculos Por Periodo de Aliquota de IR
                        //

                        listCalcPorPeriodo = Utils.Utils.CalculoPorPeriodo(listCalcPorPeriodo, custoMedio, cotacaoMaisRecente, saldoCotasCertificado,
                                                                            saldoAmortizacaoDePrincipal);

                        //
                        // Monta Lista de Certificados
                        //

                        Certificado certificado = new Certificado();
                        certificado.DataAplicacao = dataAplicacao;
                        certificado.SaldoCotasCertificado = saldoCotasCertificado;
                        certificado.CotacaoAplicacao = cotacaoAplicacao;
                        certificado.SaldoAmortizacaoDePrincipal = saldoAmortizacaoDePrincipal;
                        certificado.RendimentoPorCota = listCalcPorPeriodo.Sum(x => x.RendimentoPorCota);
                        certificado.SaldoPrejuizo = 0.0m;
                        certificado.CotasIsentaMaximo = 0.0m;
                        certificado.CotasIsenta = 0.0m;
                        certificado.CotasTributada = 0.0m;
                        certificado.PrejuizoCompensado = 0.0m;
                        certificado.PrejuizoACompensar = listCalcPorPeriodo[listCalcPorPeriodo.Count - 1].SaldoPrejuizo;
                        certificado.IRPorCota = listCalcPorPeriodo.Sum(x => x.IRPorCota);
                        certificado.CotaLiquidaTributada = 0.0m;
                        certificado.ValorBruto = 0.0m;
                        certificado.ValorIR = 0.0m;
                        certificado.ValorLiquido = 0.0m;
                        certificado.ValorBloqueadoEmCotas = 0.0m;
                        certificado.PeriodoCalculado = listCalcPorPeriodo;
                        listCert.Add(certificado);
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Erro no APL");
                Console.WriteLine(e.Message);
            }

            //
            // Calculo por Certificado
            //

            listCert = Utils.Utils.CalculoCertificado(listCert, saldoPrejuizo, cotacaoMaisRecente, cotasBloqueadasTotal);

            //
            // Grava Resultado dos Calculos
            //

            // Grava Calculo dos Periodos dos Certificados

            Utils.Utils.CabecalhoPeriodos();

            foreach (Certificado obj in listCert)
            {
                Console.WriteLine("Data Aplicação     : " + obj.DataAplicacao);
                Console.WriteLine("Qtd Cota           : " + obj.SaldoCotasCertificado.ToString("N5", CultureInfo.InvariantCulture));

             //   Utils.Utils.GravaCertificados(obj);

                foreach (Periodos obj2 in obj.PeriodoCalculado)
                {
                    Console.WriteLine(obj2.Ano + "  " +
                        obj2.AliquotaIR.ToString("N2", CultureInfo.InvariantCulture) +
                        "  " + obj2.CotacaoInicio.ToString("N7", CultureInfo.InvariantCulture) +
                        "  " + obj2.CotacaoFim.ToString("N7", CultureInfo.InvariantCulture) +
                        "  " + obj2.RendimentoPorCota.ToString("N10", CultureInfo.InvariantCulture));
                    Utils.Utils.GravaPeriodos(obj.DataAplicacao, obj2);
                }
            }

            // Grava Calculos dos Certificados

            Utils.Utils.CabecalhoCertificados();

            foreach (Certificado obj in listCert)
            {
                Utils.Utils.GravaCertificados(obj);
            }

            // Grava Totais

            saldoBruto = listCert.Sum(x => x.ValorBruto);
            valorIR = listCert.Sum(x => x.ValorIR);
            saldoLiquido = listCert.Sum(x => x.ValorLiquido);
            saldoBloqueado = listCert.Sum(x => x.ValorBloqueadoEmCotas) + valorBloqueadoTotal;
            custoAplicacao = listCert.Sum(x => x.CustoAplicacao);
            

            Utils.Utils.GravaRegistro(" ");
            Utils.Utils.GravaRegistro("*** Totais ***");
            Utils.Utils.GravaRegistro("Saldo Bruto;" + saldoBruto.ToString("N2", new CultureInfo("pr-BR")));
            Utils.Utils.GravaRegistro("IR;" + valorIR.ToString("N2", new CultureInfo("pr-BR")));
            Utils.Utils.GravaRegistro("Saldo Liquido;" + saldoLiquido.ToString("N2", new CultureInfo("pr-BR")));
            Utils.Utils.GravaRegistro("Saldo Bloqueado;" + saldoBloqueado.ToString("N2", new CultureInfo("pr-BR")));
            Utils.Utils.GravaRegistro("Custo da Aplicacao;" + custoAplicacao.ToString("N2", new CultureInfo("pr-BR")));

        }
    }
}
