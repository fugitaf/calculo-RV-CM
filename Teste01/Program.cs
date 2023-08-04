using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using Calculo_RV_CM.Entities;
using System.IO.Pipes;
using System.Runtime.ConstrainedExecution;

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
            List<CalcPorPeriodo> listAliq = new List<CalcPorPeriodo>();
            string pathSld = @"C:\Users\fefug_skli85i\Documents\Temp\SLD.csv";
            string pathApl = @"C:\Users\fefug_skli85i\Documents\Temp\APL.csv";
            Console.WriteLine("------------------------------------------------------------------------");

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
                            CalcPorPeriodo aliquotas = new CalcPorPeriodo();
                            aliquotas.Ano = int.Parse(fields[i], new CultureInfo("pt-BR"));
                            aliquotas.Aliquota_Ir = decimal.Parse(fields[i + 1], new CultureInfo("pt-BR"));
                            aliquotas.CotacaoFim = decimal.Parse(fields[i + 2], new CultureInfo("pr-BR"));
                            aliquotas.Rendimento = 0.0m;
                            aliquotas.PrejCompensar = 0.0m;
                            aliquotas.PrejCompensado = 0.0m;
                            aliquotas.SaldoPrejCota = 0.0m;
                            aliquotas.BaseCalcIR = 0.0m;
                            aliquotas.SaldoPrejReais = 0.0m;
                            listAliq.Add(aliquotas);
                        }
                        Console.WriteLine("Aliquotas de IR do SLD");
                        foreach (CalcPorPeriodo obj in listAliq)
                        {
                            Console.WriteLine(obj.Ano + "  " +
                                obj.Aliquota_Ir.ToString("N2", CultureInfo.InvariantCulture) +
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

            try
            {
                string[] lines = File.ReadAllLines(pathApl);

                Utils.Utils.GravaCabecalho();

                foreach (string line in lines)
                {
                    string[] fields = line.Split(';');
                    if (fields[0] != "DTULTRIB")
                    {
                        string dtlanct = fields[0];
                        decimal qtdcota = decimal.Parse(fields[1], new CultureInfo("pt-BR"));
                        decimal cotaplic = decimal.Parse(fields[2], new CultureInfo("pt-BR"));

                        List<CalcPorPeriodo> listCalcPorPeriodo = listAliq.FindAll(x => x.Ano >= Utils.Utils.Ano(dtlanct));

                        Certificado cert = new Certificado(dtlanct, qtdcota, cotaplic, listCalcPorPeriodo);

                        Console.WriteLine("Data Aplicação     : " + dtlanct);
                        Console.WriteLine("Qtd Cota           : " + qtdcota.ToString("N5", CultureInfo.InvariantCulture));

                        cert.AtualizaDtlanct();

                        cert.AtualizaCotaInicial(custoMedio);

                        cert.AtualizaCotaFim(cotacaoMaisRecente);

                        cert.CalcRendimento();

                        cert.CompensaEntrePeriodos();

                        cert.CalcIR();

                        Console.WriteLine("Ano   Aliq  Cota Ini   Cota Fim   Rend p/ Cota  IR p/ Cota    Valor IR");

                        foreach (CalcPorPeriodo obj in cert.Aliquotas)
                        {
                            Console.WriteLine(obj.Ano + "  " +
                                obj.Aliquota_Ir.ToString("N2", CultureInfo.InvariantCulture) +
                                "  " + obj.CotacaoInicial.ToString("N7", CultureInfo.InvariantCulture) +
                                "  " + obj.CotacaoFim.ToString("N7", CultureInfo.InvariantCulture) +
                                "  " + obj.Rendimento.ToString("N10", CultureInfo.InvariantCulture) +
                                "  " + obj.irCota.ToString("N10", CultureInfo.InvariantCulture) +
                                "  " + obj.valorIR.ToString("N2", CultureInfo.InvariantCulture));
                            valorTotalIR = valorTotalIR + obj.valorIR;
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
