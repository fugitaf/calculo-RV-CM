using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using Teste01.Entities;

namespace Teste01
{
    class Program
    {
        static void Main(string[] args)
        {
            decimal cotaResg = 0.0m;
            decimal custoMedio = 0.0m;
            List<Aliquotas> listAliq = new List<Aliquotas>();
            string pathSld = @"C:\Users\fefug_skli85i\Documents\Temp\SLD.csv";
            string pathApl = @"C:\Users\fefug_skli85i\Documents\Temp\APL.csv";
            Console.WriteLine("------------------------------------------");

            try
            {
                string[] linesSld = File.ReadAllLines(pathSld);
                foreach (string line in linesSld)
                {
                    string[] fields = line.Split(';');
                    if (fields[0] != "SLDCOTA")
                    {
                        decimal sldcota = decimal.Parse(fields[0], new CultureInfo("pt-BR"));
                        decimal vlcust = decimal.Parse(fields[1], new CultureInfo("pt-BR"));
                        cotaResg = decimal.Parse(fields[2], new CultureInfo("pt-BR"));
                        
                        for (int i = 3; int.Parse(fields[i]) > 0 && i <= 18; i = i + 3)
                        {
                            Aliquotas aliquotas = new Aliquotas();
                            aliquotas.Ano = int.Parse(fields[i], new CultureInfo("pt-BR"));
                            aliquotas.Aliquota_Ir = decimal.Parse(fields[i + 1], new CultureInfo("pt-BR"));
                            aliquotas.CotacaoFim = decimal.Parse(fields[i + 2], new CultureInfo("pr-BR"));
                            listAliq.Add(aliquotas);
                        }
                        Console.WriteLine("Quantidade de Aliquotas : " + listAliq.Count);
                        foreach (Aliquotas obj in listAliq)
                        {
                            Console.WriteLine(obj.Ano + " " +
                                obj.Aliquota_Ir.ToString("N2", CultureInfo.InvariantCulture) +
                                " " + obj.CotacaoFim.ToString("N7", CultureInfo.InvariantCulture));
                        }

                        Saldo sld = new Saldo(sldcota, vlcust, cotaResg);

                        custoMedio = sld.CustoMedio();

                        Console.WriteLine("Cotação do Resgate : " + sld.CotaResg.ToString("N7", CultureInfo.InvariantCulture));
                        Console.WriteLine("Custo Médio        : " + sld.CustoMedio().ToString("N7", CultureInfo.InvariantCulture));
                        Console.WriteLine("------------------------------------------");
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
                foreach (string line in lines)
                {
                    string[] fields = line.Split(';');
                    if (fields[0] != "DTLANCT")
                    {
                        string dtlanct = fields[0];
                        decimal qtdcota = decimal.Parse(fields[1], new CultureInfo("pt-BR"));
                        decimal cotaplic = decimal.Parse(fields[2], new CultureInfo("pt-BR"));

                        List<Aliquotas> listAliqCert = listAliq.FindAll(x => x.Ano >= 2000);

                        Certificado cert = new Certificado(dtlanct, qtdcota, cotaplic, listAliqCert);


                        Console.WriteLine("Data Aplicação     : " + dtlanct);
                        Console.WriteLine("Qtd Cota           : " + qtdcota.ToString("N5", CultureInfo.InvariantCulture));
                        foreach (Aliquotas obj in cert.Aliquotas)
                        {
                            Console.WriteLine(obj.Ano + " " +
                                obj.Aliquota_Ir.ToString("N2", CultureInfo.InvariantCulture) +
                                " " + obj.CotacaoFim.ToString("N7", CultureInfo.InvariantCulture));
                        }

                        decimal rendporcota = cotaResg - custoMedio;
                        Console.WriteLine("Rendimento por cota: " + rendporcota.ToString("N10", CultureInfo.InvariantCulture));
                        decimal irporcota = rendporcota * 0.15m;
                        Console.WriteLine("IR         por cota: " + irporcota.ToString("N10", CultureInfo.InvariantCulture));
                        decimal vlir = cert.Qtdcota * irporcota;
                        Console.WriteLine("Valor do IR        : " + vlir.ToString("N2", CultureInfo.InvariantCulture));
                        Console.WriteLine("------------------------------------------");
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Erro no APL");
                Console.WriteLine(e.Message);
            }
        }
    }
}
