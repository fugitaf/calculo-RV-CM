using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculo_RV_CM.Entities
{
    public class Certificado
    {
        public string Dtultrib { get; set; }
        public decimal Sdoctapl { get; set; }
        public decimal Cotaplic { get; set; }

        public List<Periodos> PeriodoCalc { get; set; }

        public Certificado(string dtultrib, decimal sdoctapl, decimal cotaplic, List<Periodos> periodoCalc)
        {
            Dtultrib = dtultrib;
            Sdoctapl = sdoctapl;
            Cotaplic = cotaplic;
            PeriodoCalc = periodoCalc;
        }

        public decimal ValorBruto(decimal cotacao)
        {
            return Utils.Utils.TruncarValor(Sdoctapl * cotacao);
        }

        public void AtualizaDtlanct()
        {
            for (int i = 0; i < PeriodoCalc.Count; i++)
            {
                PeriodoCalc[i].Dtultrib = Dtultrib;
            }
        }

        public void AtualizaCotaInicial(decimal cotacaoInicial)
        {
            PeriodoCalc[0].CotacaoInicial = cotacaoInicial;

            for (int i = 1; i < PeriodoCalc.Count; i++)
            {
                PeriodoCalc[i].CotacaoInicial = PeriodoCalc[i - 1].CotacaoFim;
            }
        }

        public void AtualizaCotaFim(decimal cotacaoFim)
        {
            PeriodoCalc[PeriodoCalc.Count - 1].CotacaoFim = cotacaoFim;
        }

        public void CalcRendimento()
        {
            for (int i = 0; i < PeriodoCalc.Count; i++)
            {
                PeriodoCalc[i].Rendimento = PeriodoCalc[i].CotacaoFim - PeriodoCalc[i].CotacaoInicial;
            }
        }

        public void CompensaEntrePeriodos()
        {
            decimal saldoPrej = 0.0m;

            for (int i = 0; i < PeriodoCalc.Count; i++)
            {
                PeriodoCalc[i].PrejACompensar = 0.0m;
                PeriodoCalc[i].PrejCompensado = 0.0m;
                if (PeriodoCalc[i].Rendimento < 0)
                {
                    PeriodoCalc[i].PrejACompensar = PeriodoCalc[i].Rendimento * -1;
                    saldoPrej = saldoPrej + PeriodoCalc[i].PrejACompensar;
                }
                else
                {
                    if (saldoPrej > 0)
                    {
                        if (saldoPrej > PeriodoCalc[i].Rendimento)
                        {
                            PeriodoCalc[i].PrejCompensado = PeriodoCalc[i].Rendimento;
                            saldoPrej = saldoPrej - PeriodoCalc[i].PrejCompensado;
                        }
                        else
                        {
                            PeriodoCalc[i].PrejCompensado = saldoPrej;
                            saldoPrej = 0.0m;
                        }
                    }
                }
                PeriodoCalc[i].BaseCalcIR = PeriodoCalc[i].Rendimento + PeriodoCalc[i].PrejACompensar - PeriodoCalc[i].PrejCompensado;
                PeriodoCalc[i].SaldoPrejCota = saldoPrej;
                PeriodoCalc[i].SaldoPrejReais = Utils.Utils.TruncarValor(PeriodoCalc[i].SaldoPrejCota * Sdoctapl, 2);
            }
        }

        public void CalcIR()
        {
            for (int i = 0; i < PeriodoCalc.Count; i++)
            {
                PeriodoCalc[i].IR = Utils.Utils.TruncarValor(PeriodoCalc[i].BaseCalcIR * PeriodoCalc[i].Aliquota_Ir, 10);
                PeriodoCalc[i].ValorIR = Utils.Utils.TruncarValor(Sdoctapl * PeriodoCalc[i].IR, 2);
            }
        }

        public decimal ValorIR()
        {
            decimal valorIR = 0.0m;
            for (int i = 0; i < PeriodoCalc.Count; i++)
            {
                valorIR = valorIR + PeriodoCalc[i].IR;
            }
            return valorIR;
        }
    }
}
