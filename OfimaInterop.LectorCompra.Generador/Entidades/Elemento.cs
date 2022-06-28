using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfimaInterop.LectorCompra.Generador.Entidades
{
    public class Elemento
    {
        public string NombreProducto { get; set; } 
        public string CodigoProducto { get; set; }
        public string Unidad { get; set; } 
        public string ValorUnit { get; set; } 
        public string Cantidad { get; set; } 
        public string PorcentajeIVA { get; set; }

    }
}
