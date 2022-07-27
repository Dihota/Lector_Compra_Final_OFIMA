using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfimaInteropLectorCompra.Entidades
{
    public class DetalleDocumento
    {
        public int Id { get; set; }
        public bool Actualizar { get; set; }
        public string NitProveedor { get; set; }
        public string NombreProveedor { get; set; }
        public string TipoDcto { get; set; }
        public string NroDcto { get; set; }
        public string CUDE { get; set; }
        public string FechaExpedicion { get; set; }
        public string HoraExpedicion { get; set; }
        public string Currency { get; set; }

    }
}
