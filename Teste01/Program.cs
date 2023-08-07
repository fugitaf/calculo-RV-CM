using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using Calculo_RV_CM.Entities;
using System.IO.Pipes;
using System.Runtime.ConstrainedExecution;
using Microsoft.VisualBasic;
using System.Linq;

namespace Calculo_RV_CM
{
    class Program
    {
        static void Main(string[] args)
        {
            decimal cotacaoMaisRecente = 0.0m;
            decimal saldoPrejuizo = 0.0m;
            decimal custoMedio = 0.0m;
            decimal valorTotalBruto = 0.0m;
            decimal valorTotalIR = 0.0m;
            decimal valorTotalLiquido = 0.0m;
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
                        decimal sdcotmvn = decimal.Parse(fields[0], new CultureInfo("pt-BR"));
                        decimal vlcust = decimal.Parse(fields[1], new CultureInfo("pt-BR"));
                        cotacaoMaisRecente = decimal.Parse(fields[2], new CultureInfo("pt-BR"));
                        saldoPrejuizo = decimal.Parse(fields[3], new CultureInfo("pt-BR"));

                        for (int i = 4; i <= 19 && int.Parse(fields[i]) > 0; i = i + 3)
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

                        Saldo sld = new Saldo(sdcotmvn, vlcust, cotacaoMaisRecente, saldoPrejuizo);

                        custoMedio = sld.CustoMedio();

                        Console.WriteLine("Cotação Mais Recente : " + sld.VlrCota.ToString("N7", CultureInfo.InvariantCulture));
                        Console.WriteLine("Custo Médio          : " + sld.CustoMedio().ToString("N7", CultureInfo.InvariantCulture));
                        Console.WriteLine("Saldo Prejuizo       : " + sld.SaldoPrejuizo.ToString("N2", CultureInfo.InvariantCulture));
                        Console.WriteLine("------------------------------------------------------------------------");

                        Utils.Utils.GravaRegistro("Cotacao Mais Recente:;" + sld.VlrCota.ToString("N7", new CultureInfo("pr-BR")));
                        Utils.Utils.GravaRegistro("Custo Medio:;" + sld.CustoMedio().ToString("N7", new CultureInfo("pr-BR")));
                        Utils.Utils.GravaRegistro("Saldo Prejuizo:;" + sld.SaldoPrejuizo.ToString("N2", new CultureInfo("pr-BR")));

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
                        string dtultrib = fields[0];
                        decimal sdoctapl = decimal.Parse(fields[1], new CultureInfo("pt-BR"));
                        decimal cotaplic = decimal.Parse(fields[2], new CultureInfo("pt-BR"));

                        //
                        // Monta Lista de Periodos de Apliquotas de IR do Certificado
                        //

                        List<AliquotasIR> aliqIRCert = listAliquotasIR.FindAll(x => x.Ano >= Utils.Utils.Ano(dtultrib));

                        List<Periodos> listCalcPorPeriodo = new List<Periodos>();

                        foreach (AliquotasIR Obj in aliqIRCert)
                        {
                            Periodos periodo = new Periodos();
                            periodo.Ano = Obj.Ano;
                            periodo.Aliquota_Ir = Obj.AliquotaIR;
                            periodo.CotacaoInicial = 0.0m;
                            periodo.CotacaoFim = Obj.CotacaoFim;
                            periodo.Rendimento = 0.0m;
                            periodo.PrejACompensar = 0.0m;
                            periodo.PrejCompensado = 0.0m;
                            periodo.SaldoPrejCota = 0.0m;
                            periodo.BaseCalcIR = 0.0m;
                            periodo.IRCota = 0.0m;
                            periodo.SaldoPrejReais = 0.0m;
                            listCalcPorPeriodo.Add(periodo);
                        }

                        //
                        // Faz Calculos Por Periodo de Aliquota de IR
                        //

                        listCalcPorPeriodo = Utils.Utils.CalcPorPeriodo(listCalcPorPeriodo, custoMedio, cotacaoMaisRecente, sdoctapl);

                        //
                        // Monta Lista de Certificados
                        //

                        Certificado certificado = new Certificado();
                        certificado.Dtultrib = dtultrib;
                        certificado.Sdoctapl = sdoctapl;
                        certificado.Cotaplic = cotaplic;
                        certificado.RendCertificado = listCalcPorPeriodo.Sum(x => x.Rendimento);
                        certificado.SaldoPrejuizo = 0.0m;
                        certificado.CotasIsentaMax = 0.0m;
                        certificado.CotasIsenta = 0.0m;
                        certificado.CotasTributada = 0.0m;
                        certificado.VlrPrejCompensado = 0.0m;
                        certificado.VlrPrejCertificado = listCalcPorPeriodo[listCalcPorPeriodo.Count - 1].SaldoPrejReais;
                        certificado.IRCota = listCalcPorPeriodo.Sum(x => x.IRCota);
                        certificado.IR = 0.0m;
                        certificado.PeriodoCalc = listCalcPorPeriodo;
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

            listCert = Utils.Utils.CalcCertificado(listCert, saldoPrejuizo);

            //
            // Grava Resultado dos Calculos
            //

            foreach (Certificado obj in listCert)
            {
                Console.WriteLine("Data Aplicação     : " + obj.Dtultrib);
                Console.WriteLine("Qtd Cota           : " + obj.Sdoctapl.ToString("N5", CultureInfo.InvariantCulture));

                Utils.Utils.CabecalhoCertificados();

                Utils.Utils.GravaCertificados(obj);
                
                Utils.Utils.CabecalhoPeriodos();
                
                foreach (Periodos obj2 in obj.PeriodoCalc)
                {
                    Console.WriteLine(obj2.Ano + "  " +
                        obj2.Aliquota_Ir.ToString("N2", CultureInfo.InvariantCulture) +
                        "  " + obj2.CotacaoInicial.ToString("N7", CultureInfo.InvariantCulture) +
                        "  " + obj2.CotacaoFim.ToString("N7", CultureInfo.InvariantCulture) +
                        "  " + obj2.Rendimento.ToString("N10", CultureInfo.InvariantCulture));
                    Utils.Utils.GravaPeriodos(obj2);
                }
            }

        }
    }
}
