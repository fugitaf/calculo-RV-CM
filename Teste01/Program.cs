using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using Calculo_RV_CM.Entities;
using System.IO.Pipes;
using System.Runtime.ConstrainedExecution;
using Microsoft.VisualBasic;

namespace Calculo_RV_CM
{
    class Program
    {
        static void Main(string[] args)
        {
            decimal cotacaoMaisRecente = 0.0m;
            decimal custoMedio = 0.0m;
            decimal valorTotalBruto = 0.0m;
            decimal valorTotalIR = 0.0m;
            decimal valorTotalLiquido = 0.0m;
            List<AliquotasIR> listAliquotasIR = new List<AliquotasIR>();
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
                        decimal sdcotmvn = decimal.Parse(fields[0], new CultureInfo("pt-BR"));
                        decimal vlcust = decimal.Parse(fields[1], new CultureInfo("pt-BR"));
                        cotacaoMaisRecente = decimal.Parse(fields[2], new CultureInfo("pt-BR"));

                        for (int i = 3; int.Parse(fields[i]) > 0 && i <= 18; i = i + 3)
                        {
                            AliquotasIR aliqIR = new AliquotasIR();
                            aliqIR.Ano = int.Parse(fields[i], new CultureInfo("pt-BR"));
                            aliqIR.AliquotaIR = decimal.Parse(fields[i + 1], new CultureInfo("pt-BR"));
                            aliqIR.CotacaoFim = decimal.Parse(fields[i + 2], new CultureInfo("pr-BR"));
                            listAliquotasIR.Add(aliqIR);
                        }
                        
                        Console.WriteLine("Aliquotas de IR do SLD");
                        foreach (AliquotasIR obj in listAliquotasIR)
                        {
                            Console.WriteLine(obj.Ano + "  " +
                                obj.AliquotaIR.ToString("N2", CultureInfo.InvariantCulture) +
                                "  " + obj.CotacaoFim.ToString("N7", CultureInfo.InvariantCulture));
                        }

                        Saldo sld = new Saldo(sdcotmvn, vlcust, cotacaoMaisRecente);

                        custoMedio = sld.CustoMedio();

                        Console.WriteLine("Cotação Mais Recente : " + sld.VlrCota.ToString("N7", CultureInfo.InvariantCulture));
                        Console.WriteLine("Custo Médio          : " + sld.CustoMedio().ToString("N7", CultureInfo.InvariantCulture));
                        Console.WriteLine("------------------------------------------------------------------------");
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

                Utils.Utils.GravaCabecalho();

                foreach (string line in lines)
                {
                    string[] fields = line.Split(';');
                    if (fields[0] != "DTULTRIB")
                    {
                        string dtultrib = fields[0];
                        decimal qtdcota = decimal.Parse(fields[1], new CultureInfo("pt-BR"));
                        decimal cotaplic = decimal.Parse(fields[2], new CultureInfo("pt-BR"));

                        List<AliquotasIR> aliqIRCert = listAliquotasIR.FindAll(x => x.Ano >= Utils.Utils.Ano(dtultrib));    

                        List<Periodos> listCalcPorPeriodo = new List<Periodos>();

                        foreach (AliquotasIR Obj in aliqIRCert)
                        {
                            Periodos periodo = new Periodos();
                            periodo.Dtultrib = dtultrib;
                            periodo.Ano = Obj.Ano;
                            periodo.Aliquota_Ir = Obj.AliquotaIR;
                            periodo.CotacaoInicial = 0.0m;
                            periodo.CotacaoFim = Obj.CotacaoFim;
                            periodo.Rendimento = 0.0m;
                            periodo.PrejACompensar = 0.0m;
                            periodo.PrejCompensado = 0.0m;
                            periodo.SaldoPrejCota = 0.0m;
                            periodo.BaseCalcIR = 0.0m;
                            periodo.SaldoPrejReais = 0.0m;
                            periodo.IR = 0.0m;
                            periodo.ValorIR = 0.0m;
                            listCalcPorPeriodo.Add(periodo);
                        }

                        Certificado cert = new Certificado(dtultrib, qtdcota, cotaplic, listCalcPorPeriodo);

                        Console.WriteLine("Data Aplicação     : " + dtultrib);
                        Console.WriteLine("Qtd Cota           : " + qtdcota.ToString("N5", CultureInfo.InvariantCulture));

                        cert.AtualizaDtlanct();

                        cert.AtualizaCotaInicial(custoMedio);

                        cert.AtualizaCotaFim(cotacaoMaisRecente);

                        cert.CalcRendimento();

                        cert.CompensaEntrePeriodos();

                        cert.CalcIR();

                        Console.WriteLine("Ano   Aliq  Cota Ini   Cota Fim   Rend p/ Cota  IR p/ Cota    Valor IR");

                        foreach (Periodos obj in cert.PeriodoCalc)
                        {
                            Console.WriteLine(obj.Ano + "  " +
                                obj.Aliquota_Ir.ToString("N2", CultureInfo.InvariantCulture) +
                                "  " + obj.CotacaoInicial.ToString("N7", CultureInfo.InvariantCulture) +
                                "  " + obj.CotacaoFim.ToString("N7", CultureInfo.InvariantCulture) +
                                "  " + obj.Rendimento.ToString("N10", CultureInfo.InvariantCulture) +
                                "  " + obj.IR.ToString("N10", CultureInfo.InvariantCulture) +
                                "  " + obj.ValorIR.ToString("N2", CultureInfo.InvariantCulture));
                            valorTotalIR = valorTotalIR + obj.ValorIR;
                            Utils.Utils.GravaCalculo(obj);
                        }
                        decimal valorBruto = cert.ValorBruto(cotacaoMaisRecente);
                        decimal valorIR = cert.ValorIR();
                        valorTotalBruto = valorTotalBruto + valorBruto;
                        Console.WriteLine("Valor Bruto   : " + valorBruto.ToString("N2", CultureInfo.InvariantCulture));
                        Console.WriteLine("Valor IR      : " + valorIR.ToString("N2", CultureInfo.InvariantCulture));
                        decimal valorLiq = valorBruto - valorIR;
                        Console.WriteLine("Valor Liquido : " + valorLiq);
                        Console.WriteLine("------------------------------------------------------------------------");
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Erro no APL");
                Console.WriteLine(e.Message);
            }
            finally
            {
                Console.WriteLine("Saldo Bruto     : " + valorTotalBruto.ToString("N2", CultureInfo.InvariantCulture));
                Console.WriteLine("IR              : " + valorTotalIR.ToString("N2", CultureInfo.InvariantCulture));
                valorTotalLiquido = valorTotalBruto - valorTotalIR;
                Console.WriteLine("Saldo Liquido   : " + valorTotalLiquido.ToString("N2", CultureInfo.InvariantCulture));
                Console.WriteLine("\n" + "\n" + "\n" + "\n");
            }
        }
    }
}
