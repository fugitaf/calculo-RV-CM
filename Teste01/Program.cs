using System;
using System.Globalization;
using System.IO;
using Teste01.Entities;

namespace Teste01
{
    class Program
    {
        static void Main(string[] args)
        {
            decimal cotaresg = 2.1234567m;
            decimal customedio = 0.0m;
            string pathSld = @"C:\Users\fefug_skli85i\Documents\Temp\SLD.csv";
            string pathApl = @"C:\Users\fefug_skli85i\Documents\Temp\APL.csv";

            try
            {
                string[] linesSld = File.ReadAllLines(pathSld);
                foreach (string line in linesSld)
                {
                    string[] fields = line.Split(';');
                    if (fields[0] != "SLDCOTA")
                    {
                        decimal sldcota = decimal.Parse(fields[0], new CultureInfo("pt-BR"));
                        var vlcust = decimal.Parse(fields[1], new CultureInfo("pt-BR"));

                        Saldo sld = new Saldo(sldcota, vlcust);

                        customedio = sld.CustoMedio();

                        Console.WriteLine("Custo Médio:" + sld.CustoMedio().ToString("N7", CultureInfo.InvariantCulture));
                        Console.WriteLine("----------------------------");
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Erro ");
                Console.WriteLine(e.Message);
            }

            try
            {
                string[] lines = File.ReadAllLines(pathApl);
                foreach (string line in lines)
                {
                    string[] fields = line.Split(';');
                    string dtlanct = fields[0];
                    decimal qtdcota = decimal.Parse(fields[1], new CultureInfo("pt-BR"));
                    decimal cotaplic = decimal.Parse(fields[2], new CultureInfo("pt-BR"));

                    Certificado cert = new Certificado(dtlanct, qtdcota, cotaplic);

                    Console.WriteLine("Data Aplicação     : " + dtlanct);
                    Console.WriteLine("Qtd Cota           : " + qtdcota.ToString("N5", CultureInfo.InvariantCulture));
                    decimal rendporcota = cotaresg - customedio;
                    Console.WriteLine("Rendimento por cota: " + rendporcota.ToString("N10", CultureInfo.InvariantCulture));
                    decimal irporcota = rendporcota * 0.15m;
                    Console.WriteLine("IR         por cota: " + irporcota.ToString("N10", CultureInfo.InvariantCulture));
                    decimal vlir = cert.Qtdcota * irporcota;
                    Console.WriteLine("Valor do IR        : " + vlir.ToString("N2",CultureInfo.InvariantCulture));
                    Console.WriteLine("----------------------------");
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Erro ");
                Console.WriteLine(e.Message);
            }
        }
    }
}
