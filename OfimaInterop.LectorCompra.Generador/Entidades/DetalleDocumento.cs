using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfimaInterop.LectorCompra.Generador.Entidades
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
        public DateTime FechaExpedicion { get; set; }
        public string HoraExpedicion { get; set; }

    }
}
