using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculo_RV_CM.Entities
{
    public class Bloqueios
    {
        public decimal ValorBloqueadoTotal { get; set; }
        public decimal CotasBloqueadasTotal { get; set; }

        public Bloqueios() { }
        public Bloqueios(decimal valorBloqueadoTotal, decimal cotasBloqueadasTotal)
        {
            ValorBloqueadoTotal = valorBloqueadoTotal;
            CotasBloqueadasTotal = cotasBloqueadasTotal;
        }
    }
}
