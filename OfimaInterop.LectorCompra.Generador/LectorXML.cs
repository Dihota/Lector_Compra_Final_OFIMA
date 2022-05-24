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
        private int SalidaXML;
        private string Nodo;

        public LectorXML()
        {
            SalidaXML = 0;
            Nodo = string.Empty;
        }


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
            //Se crea un nuevo objeto del tipo emisor
            Emisor NewEmisor = new Emisor();

            //Se inicializa la variable encargada de salir de los ciclos
            SalidaXML = 0;


            //Se crea un objeto del tipo XmlDocument, para abrir el xml
            XmlDocument ReadXML = new XmlDocument();
            ReadXML.Load(RutaXML);

            //Se recorre el XML, para capturar los datos del emisor
            foreach (XmlNode N1 in ReadXML.DocumentElement.ChildNodes)
            {
                if (N1.Name == "cac:Attachment")
                {
                    foreach (XmlNode N2 in N1.ChildNodes)
                    {
                        foreach (XmlNode N3 in N2.ChildNodes)
                        {
                            if (N3.Name == "cbc:Description")
                            {
                                //Se captura el nodo u se asigna a una variable
                                Nodo = N3.InnerText;

                                //Se invoca metodo que remplaza los valores no necesarios en el XML nuevo
                                string NodoAuxiliar = Replace(Nodo);

                                //Se invoca el LectorEmisorAuxiliar, para que lea los valores del nuevo XML
                                LectorEmisorAuxiliar(NodoAuxiliar, NewEmisor);
                            }
                        }
                    }
                }
            }

            return "";
        }

        public void LectorEmisorAuxiliar(string NewXML, Emisor emisor)
        {
            XmlDocument ReadXML = new XmlDocument();
            ReadXML.LoadXml(NewXML);

            foreach (XmlNode N1 in ReadXML.DocumentElement.ChildNodes)
            {
                switch (N1.Name)
                {
                    case "cac:AccountingSupplierParty":
                        foreach (XmlNode N2 in N1.ChildNodes)
                        {
                            if (N2.Name == "cac:Party")
                            {
                                foreach (XmlNode N3 in N2.ChildNodes)
                                {
                                    foreach (XmlNode N4 in N3.ChildNodes)
                                    {
                                        switch (N4.Name)
                                        {
                                            case "cbc:RegistrationName":
                                                emisor.Nombre = N4.InnerText;
                                                break;

                                            case "cbc:CompanyID":
                                                emisor.Nit = N4.InnerText + "-" + N4.Attributes["schemeID"].Value;
                                                break;

                                            case "cac:RegistrationAddress":
                                                foreach (XmlNode N5 in N4.ChildNodes)
                                                {
                                                    switch (N5.Name)
                                                    {
                                                        case "cbc:CityName":
                                                            emisor.Ciudad = N5.InnerText;
                                                            break;
                                                        case "cac:AddressLine":
                                                            foreach (XmlNode N6 in N5.ChildNodes)
                                                            {
                                                                emisor.Direccion = N6.InnerText;
                                                            }
                                                            break;
                                                        default:
                                                            break;
                                                    }
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

        }


            //Metodo encargado de quitar las etiquetas que no son requeridas en el nuevo xml.
            public string Replace(string Nodo)
        {
            string Replace1 = Nodo.Replace("<![CDATA[", "");
            string Nodo2 = Replace1.Replace("]]>", "");
            return Nodo2;
        }

    }
}
