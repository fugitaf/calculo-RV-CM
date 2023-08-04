﻿using System;
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

        public List<CalcPorPeriodo> Aliquotas { get; set; }

        public Certificado(string dtultrib, decimal sdoctapl, decimal cotaplic, List<CalcPorPeriodo> aliquotas)
        {
            Dtultrib = dtultrib;
            Sdoctapl = sdoctapl;
            Cotaplic = cotaplic;
            Aliquotas = aliquotas;
        }

        public decimal ValorBruto(decimal cotacao)
        {
            return Utils.Utils.TruncarValor(Sdoctapl * cotacao);
        }

        public void AtualizaDtlanct()
        {
            for (int i = 0; i < Aliquotas.Count; i++)
            {
                Aliquotas[i].Dtultrib = Dtultrib;
            }
        }

        public void AtualizaCotaInicial(decimal cotacaoInicial)
        {
            Aliquotas[0].CotacaoInicial = cotacaoInicial;

            for (int i = 1; i < Aliquotas.Count; i++)
            {
                Aliquotas[i].CotacaoInicial = Aliquotas[i - 1].CotacaoFim;
            }
        }

        public void AtualizaCotaFim(decimal cotacaoFim)
        {
            Aliquotas[Aliquotas.Count - 1].CotacaoFim = cotacaoFim;
        }

        public void CalcRendimento()
        {
            for (int i = 0; i < Aliquotas.Count; i++)
            {
                Aliquotas[i].Rendimento = Aliquotas[i].CotacaoFim - Aliquotas[i].CotacaoInicial;
            }
        }

        public void CompensaEntrePeriodos()
        {
            decimal saldoPrej = 0.0m;

            for (int i = 0; i < Aliquotas.Count; i++)
            {
                Aliquotas[i].PrejCompensar = 0.0m;
                Aliquotas[i].PrejCompensado = 0.0m;
                if (Aliquotas[i].Rendimento < 0)
                {
                    Aliquotas[i].PrejCompensar = Aliquotas[i].Rendimento * -1;
                    saldoPrej = saldoPrej + Aliquotas[i].PrejCompensar;
                }
                else
                {
                    if (saldoPrej > 0)
                    {
                        if (saldoPrej > Aliquotas[i].Rendimento)
                        {
                            Aliquotas[i].PrejCompensado = Aliquotas[i].Rendimento;
                            saldoPrej = saldoPrej - Aliquotas[i].PrejCompensado;
                        }
                        else
                        {
                            Aliquotas[i].PrejCompensado = saldoPrej;
                            saldoPrej = 0.0m;
                        }
                    }
                }
                Aliquotas[i].BaseCalcIR = Aliquotas[i].Rendimento + Aliquotas[i].PrejCompensar - Aliquotas[i].PrejCompensado;
                Aliquotas[i].SaldoPrejCota = saldoPrej;
                Aliquotas[i].SaldoPrejReais = Utils.Utils.TruncarValor(Aliquotas[i].SaldoPrejCota * Sdoctapl, 2);
            }
        }

        public void CalcIR()
        {
            for (int i = 0; i < Aliquotas.Count; i++)
            {
                Aliquotas[i].irCota = Utils.Utils.TruncarValor(Aliquotas[i].BaseCalcIR * Aliquotas[i].Aliquota_Ir, 10);
                Aliquotas[i].valorIR = Utils.Utils.TruncarValor(Sdoctapl * Aliquotas[i].irCota, 2);
            }
        }

        public decimal ValorIR()
        {
            decimal valorIR = 0.0m;
            for (int i = 0; i < Aliquotas.Count; i++)
            {
                valorIR = valorIR + Aliquotas[i].valorIR;
            }
            return valorIR;
        }
    }
}
