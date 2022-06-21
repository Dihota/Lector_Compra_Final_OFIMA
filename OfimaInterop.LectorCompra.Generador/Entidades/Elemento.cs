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
        public decimal Unidad { get; set; } 
        public decimal ValorUnit { get; set; } 
        public decimal Cantidad { get; set; } 
        public decimal PorcentajeIVA { get; set; }

    }
}
