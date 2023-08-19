using Calculo_RV_CM.Entities;
using System.Globalization;

namespace Calculo_RV_CM
{
    class Program
    {
        static void Main(string[] args)
        {
            Saldo saldo = new Saldo();
            Bloqueios bloqueios = new Bloqueios();
            Fundos fundos = new Fundos();
            List<AliquotasIR> listAliquotasIR = new List<AliquotasIR>();
            List<Certificados> listCert = new List<Certificados>();
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
                        saldo.SaldoCotasSubconta = decimal.Parse(fields[0], new CultureInfo("pt-BR"));
                        saldo.ValorCustoMedio = decimal.Parse(fields[1], new CultureInfo("pt-BR"));
                        fundos.CotacaoMaisRecente = decimal.Parse(fields[2], new CultureInfo("pt-BR"));
                        saldo.SaldoPrejuizo = decimal.Parse(fields[3], new CultureInfo("pt-BR"));
                        bloqueios.ValorBloqueadoTotal = decimal.Parse(fields[4], new CultureInfo("pt-BR"));
                        bloqueios.CotasBloqueadasTotal = decimal.Parse(fields[5], new CultureInfo("pt-BR"));

                        for (int i = 6; i <= 21 && int.Parse(fields[i]) > 0; i += 3)
                        {
                            AliquotasIR aliquotasIR = new AliquotasIR();
                            aliquotasIR.Ano = int.Parse(fields[i], new CultureInfo("pt-BR"));
                            aliquotasIR.AliquotaIR = decimal.Parse(fields[i + 1], new CultureInfo("pt-BR"));
                            aliquotasIR.CotacaoFim = decimal.Parse(fields[i + 2], new CultureInfo("pr-BR"));
                            listAliquotasIR.Add(aliquotasIR);
                        }

                        // O ultimo item da lista fica com o ano atual

                        DateTime dateTime = DateTime.Now;
                        listAliquotasIR[listAliquotasIR.Count - 1].Ano = int.Parse(dateTime.ToString("yyyy", CultureInfo.InvariantCulture));

                        Console.WriteLine("Aliquotas de IR do SLD");
                        foreach (AliquotasIR obj in listAliquotasIR)
                        {
                            Console.WriteLine(obj.Ano + "  " +
                                obj.AliquotaIR.ToString("N2", CultureInfo.InvariantCulture) +
                                "  " + obj.CotacaoFim.ToString("N7", CultureInfo.InvariantCulture));
                        }
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

                        // Monta Lista de Periodos de Aliquotas de IR do Certificado

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

                        // Calculos Por Periodo de Aliquotas de IR

                        listCalcPorPeriodo = Utils.Utils.CalculoPorPeriodo(listCalcPorPeriodo, saldo.CustoMedio(), fundos.CotacaoMaisRecente, saldoCotasCertificado,
                                                                            saldoAmortizacaoDePrincipal);

                        // Monta Lista de Certificados

                        Certificados certificado = new Certificados();
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

            // Calculo por Certificado

            listCert = Utils.Utils.CalculoCertificado(listCert, saldo.SaldoPrejuizo, fundos.CotacaoMaisRecente);

            // Grava Informações

            Utils.Utils.GravaDadosDeEntrada(saldo, fundos, bloqueios);

            Utils.Utils.GravaPeriodosDoCertificado(listCert);

            Utils.Utils.GravaCertificadosCalculados(listCert);

            Utils.Utils.GravaSaldoConsolidado(listCert, bloqueios);

        }
    }
}
