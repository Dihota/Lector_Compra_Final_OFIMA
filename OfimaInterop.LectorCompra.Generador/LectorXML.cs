using OfimaInterop.LectorCompra.Generador.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace OfimaInterop.LectorCompra.Generador
{
    public class LectorXML
    {
        //Metodo para todo el proceso de el lector de compras 
        public string LectorCompras(string Carpeta)
        {
            //Llama el metodo encargado de identificar los diferentes xml en la ruta
            foreach (var item in ObtenerXML(Carpeta))
            {
                LectorEmisor(item.RutaXML);
            }
            
            return "";
        }

        //Leer los archivos XML de la carpeta
        public List<Xml> ObtenerXML(string Carpeta)
        {
            //Se crea list, para almacenar los archivox XML
            List<Xml> NewXML = new List<Xml>();       

            string[] Files = Directory.GetFiles(Carpeta, "*.xml");
            
            //Se recorre el array con los Xml capturados y se agregan a la List.
            foreach (string File in Files)
            {
                NewXML.Add(new Xml
                {
                    RutaXML = File,
                });
            }

            return NewXML;
        } 
        
        //Metodo para obtener los datos del Emisor(proveedor)
        public dynamic LectorEmisor(string RutaXML)
        {


            return "";
        }
        
    }
}
