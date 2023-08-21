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
            List<Certificado> listaCertificados = new List<Certificado>();
            List<PosicaoAnalitica> listaPosicaoAnalitica = new List<PosicaoAnalitica>();
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
                            saldo.AddAliquotaIR(aliquotasIR);
                        }

                        // O ultimo item da lista de aliquota deve ficar com o ano atual

                        DateTime dateTime = DateTime.Now;
                        saldo.AliquotasIR[saldo.AliquotasIR.Count - 1].Ano = int.Parse(dateTime.ToString("yyyy", CultureInfo.InvariantCulture));

                        Console.WriteLine("Aliquotas de IR do SLD");
                        foreach (AliquotasIR obj in saldo.AliquotasIR)
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
                        Certificado certificado = new Certificado();
                        certificado.DataCotizacao = fields[0];
                        certificado.SaldoCotasCertificado = decimal.Parse(fields[1], new CultureInfo("pt-BR"));
                        certificado.CotacaoAplicacao = decimal.Parse(fields[2], new CultureInfo("pt-BR"));
                        certificado.SaldoAmortizacaoDePrincipal = decimal.Parse(fields[3], new CultureInfo("pt-BR"));
                        listaCertificados.Add(certificado);
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Erro no APL");
                Console.WriteLine(e.Message);
            }

            // Calcula Posicao Analitica "por Certificado"

            listaPosicaoAnalitica = Utils.Utils.CalculaPosicaoAnalitica(listaCertificados, saldo, fundos);

            // Grava Informações

            Utils.Utils.GravaDadosDeEntrada(saldo, fundos, bloqueios);

            Utils.Utils.GravaPeriodosDoCertificado(listaPosicaoAnalitica);

            Utils.Utils.GravaCertificadosCalculados(listaPosicaoAnalitica);

            Utils.Utils.GravaSaldoConsolidado(listaPosicaoAnalitica, bloqueios);

        }
    }
}
