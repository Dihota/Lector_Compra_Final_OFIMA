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
        public string Ruta;

        public LectorXML()
        {
            SalidaXML = 0;
            Nodo = string.Empty;
            Ruta = Directory.GetCurrentDirectory();
        }


        //Metodo para todo el proceso de el lector de compras 
        public string LectorCompras(string Carpeta, int Nit, string Conexion)
        {
            Emisor emisor = new Emisor();

            //se instancia objeto para guardar los datos obtenidos
            GuardarXML guardar = new GuardarXML();

            //Se inicializa variable que almacenara el nit del XML.
            int NitXML = 0;

            //Llama el metodo encargado de identificar los diferentes xml en la ruta
            foreach (var item in ObtenerXML(Carpeta))
            {
                //Se inicializa la informacion en el log
                LogSeguimientoLectorCompra("-----------------------------------------------------------------------------------------", Ruta);
                LogSeguimientoLectorCompra("--Inicia Analisis para el documento : ( " + item.nombreXML + ".xml).", Ruta);

                //Se captura el Nit del adquiriente en el XML
                NitXML = LectorAdquiriente(item.RutaXML);

                //Se valida si el Nit del XML, corresponde con el nit de la empresa, para gestionar XML.
                if (Nit == NitXML)
                {
                    //Se capturan los datos del emisor.
                    emisor = LectorEmisor(item.RutaXML);

                    //Se crea variable que captura la respuesta del guardado del emisor.
                    bool SaveEmisor;

                    LogSeguimientoLectorCompra("--Inicia el proceso de almacenar informacion del emisor", Ruta);

                    //Se guarda la informacion del emisor
                    SaveEmisor = guardar.GuardarEmisor(Conexion, emisor);

                    //se valida si la informacion del emisosr se guardo con exito.
                    if (SaveEmisor is false)
                    {
                        LogSeguimientoLectorCompra("--No se logra almacenar los datos del emisor : " + emisor.Nit, Ruta);
                    }
                    else
                    {
                        LogSeguimientoLectorCompra("--Se almacena correctamente la informacion del emisor en las tablas (MTPROCLI y NIT) : " + emisor.Nit, Ruta);
                    }

                    //Se captura los datos del detalle del documento.
                    LectorDetalle(item.RutaXML);
                }
                else
                {
                    //Mensaje que indica que el documento no se examina, por que no es una factura de compra
                    LogSeguimientoLectorCompra("--No se trata el XML: ( " + item.nombreXML + ".xml ), Nit no corresponde con el registrado en NITCIA.", Ruta);
                }
            }

            return "Finalizo";
        }

        //Leer los archivos XML de la carpeta
        public List<Xml> ObtenerXML(string Carpeta)
        {
            //Se crea list, para almacenar los archivox XML
            List<Xml> NewXML = new List<Xml>();       

            //Se agregan los archivos encontrados a un array
            string[] Files = Directory.GetFiles(Carpeta,"*.xml");
            
            //Se recorre el array con los Xml capturados y se agregan a la List.
            foreach (string File in Files)
            {
                //Se almacenan los datos en la lista
                NewXML.Add(new Xml
                {
                    RutaXML = File,
                    nombreXML = stringBetween(File,Carpeta,".xml"),
                });
            }

            //Se retorna la lista 
            return NewXML;
        }
        
        //Metodo para obtener la informacion del adquiriente 
        public int LectorAdquiriente(string RutaXML)
        {

            //Se crea un objeto del tipo adquiriente
            Adquiriente NewAdquiriente = new Adquiriente();
            
            //Se inicializa variable para la salida de los ciclos.
            SalidaXML = 0;

            //Se llama el metodo, que leera el XML.
            XmlDocument ReadXML = new XmlDocument();
            ReadXML.Load(RutaXML);
            
            //Se guarda mensaje en log.
            LogSeguimientoLectorCompra("--Inicia validacion del documento del Adquiriente.", Ruta);

            try
            {
                //Se comienza a rrecorrer los nodo del XML.
                foreach (XmlNode N1 in ReadXML.DocumentElement.ChildNodes)
                {
                    if (N1.Name == "cac:ReceiverParty")
                    {
                        foreach (XmlNode N2 in N1.ChildNodes)
                        {
                            foreach (XmlNode N3 in N2.ChildNodes)
                            {
                                if (N3.Name == "cbc:CompanyID")
                                {
                                    NewAdquiriente.Nit = Convert.ToInt32(N3.InnerText);
                                    SalidaXML = SalidaXML = 1;
                                    break;
                                }
                            }
                            if (SalidaXML == 1){break;}
                        }
                    }
                    if (SalidaXML == 1) { break; }
                }

                //Se guarda mensaje en log.
                LogSeguimientoLectorCompra("--Se captura documento del adquiriente: (" + NewAdquiriente.Nit + ").", Ruta);

                //Se retorna el objeto adquiriente.
                return NewAdquiriente.Nit;
            }
            catch (Exception error)
            {

                //Se guarda mensaje en log.
                LogSeguimientoLectorCompra("--No se logra capturar documento del adquiriente: (" + NewAdquiriente.Nit + ", Error : " + error + ").", Ruta);

                //Se retorna un cero, por que no se capturo nit del adquiriente.
                return 0;
            }
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

            //Se comienza a llenar la informacion por archivo
            LogSeguimientoLectorCompra("--Inicia proceso de extraer informacion del XML, para el EMISOR.", Ruta);

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
                                //Se captura el nodo y se asigna a una variable
                                Nodo = N3.InnerText;
                                SalidaXML = SalidaXML + 1;

                                //Se invoca metodo que remplaza los valores no necesarios en el XML nuevo
                                string NodoAuxiliar = Replace(Nodo);

                                //Se invoca el LectorEmisorAuxiliar, para que lea los valores del nuevo XML
                                LectorEmisorAuxiliar(NodoAuxiliar, NewEmisor);
                            }
                        }
                    }
                }    
            }

            //Se retorna objeto Emisor con la informacion encontrada en el XML
            return NewEmisor;
        }

        //Metodo para obtener los datos del Detalle del Documento.
        public dynamic LectorDetalle(string RutaXML)
        {
            DetalleDocumento NewDetalle = new DetalleDocumento();

            //Se inicializa la variable encargada de salir de los ciclos
            SalidaXML = 0;

            XmlDocument XmlRead = new XmlDocument();
            XmlRead.Load(RutaXML);

            //Se comienza a llenar la informacion por archivo
            LogSeguimientoLectorCompra("--Inicia proceso de extraer informacion del detalle del XML.", Ruta);

            //Se recorre el XML, para capturar los datos del emisor
            foreach (XmlNode N1 in XmlRead.DocumentElement.ChildNodes)
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
                                SalidaXML = SalidaXML + 1;

                                //Se invoca metodo que remplaza los valores no necesarios en el XML nuevo
                                string NodoAuxiliar = Replace(Nodo);

                                //Se invoca el LectorEmisorAuxiliar, para que lea los valores del nuevo XML
                                LectorDetalleAuxiliar(NodoAuxiliar, NewDetalle);

                            }
                        }
                    }
                }
            }

            return "";
        }

        //Se captura la informacion del emisor en el XML
        public Emisor LectorEmisorAuxiliar(string NewXML, Emisor emisor)
        {
            //Se invoca el metodo para abrir el XML
            XmlDocument ReadXML = new XmlDocument();
            ReadXML.LoadXml(NewXML);

            //Se comienza a llenar la informacion por archivo
            LogSeguimientoLectorCompra("--Inicia proceso de extraer informacion del EMISOR.", Ruta);

            //Se inicia el ciclo para tomar la informacion, correspondiente al emisor
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
                                    switch (N3.Name)
                                    {
                                        case "cac:PartyTaxScheme":
                                            foreach (XmlNode N4 in N3.ChildNodes)
                                            {
                                                switch (N4.Name)
                                                {
                                                    case "cbc:RegistrationName":
                                                            emisor.Nombre = N4.InnerText;
                                                            SalidaXML = SalidaXML + 1;
                                                            break;

                                                    case "cbc:CompanyID":
                                                            emisor.Nit = N4.InnerText + "-" + N4.Attributes["schemeID"].Value;
                                                            SalidaXML = SalidaXML + 1;
                                                            break;

                                                    case "cac:RegistrationAddress":
                                                            foreach (XmlNode N5 in N4.ChildNodes)
                                                            {
                                                                switch (N5.Name)
                                                                {
                                                                    case "cbc:ID":
                                                                            emisor.Ciudad = N5.InnerText;
                                                                            SalidaXML = SalidaXML + 1;
                                                                            break;

                                                                    case "cac:AddressLine":
                                                                            foreach (XmlNode N6 in N5.ChildNodes)
                                                                            {
                                                                                emisor.Direccion = N6.InnerText;
                                                                                SalidaXML = SalidaXML + 1;
                                                                            }
                                                                            break;

                                                                    case "cac:Country":
                                                                            foreach (XmlNode N6 in N5.ChildNodes)
                                                                            {
                                                                                if (N6.Name == "cbc:Name")
                                                                                {
                                                                                    emisor.Pais = N6.InnerText;
                                                                                    SalidaXML = SalidaXML + 1;
                                                                                }
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
                                                if (SalidaXML == 6)
                                                {
                                                    break;
                                                }
                                            }
                                            break;

                                        case "cac:Contact":
                                            foreach (XmlNode N4 in N3.ChildNodes)
                                            {
                                                switch (N4.Name)
                                                {
                                                    case "cbc:Telephone":
                                                        emisor.TeLefono = N4.InnerText;
                                                        SalidaXML = SalidaXML + 1;
                                                        break;
                                                    case "cbc:ElectronicMail":
                                                        emisor.Email = N4.InnerText;
                                                        SalidaXML = SalidaXML + 1;
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                            break;

                                            if (SalidaXML == 8)
                                            {
                                                break;
                                            }
                                        default:
                                            break;

                                    }

                                }
                            }
                        }
                        break;

                    default:
                        break;

                }
            }

            //Se comienza a llenar la informacion por archivo
            LogSeguimientoLectorCompra("--Finaliza proceso de extraer informacion del EMISOR.", Ruta);

            return emisor;

        }

        //Se captura la informacion del emisor en el XML
        public DetalleDocumento LectorDetalleAuxiliar(string NewXML, DetalleDocumento detalle)
        {
            // 
            XmlDocument ReadXML = new XmlDocument();
            ReadXML.LoadXml(NewXML);

            // 
            foreach (XmlNode N1 in ReadXML.DocumentElement.ChildNodes)
            {
                switch (N1.Name)
                {
                    case "cbc:ID":
                        detalle.NroDcto = N1.InnerText;
                        SalidaXML = SalidaXML + 1;
                        break;

                    case "cbc:UUID":
                        detalle.CUDE = N1.InnerText;
                        SalidaXML = SalidaXML + 1;
                        break;

                    case "cbc:IssueDate":
                        detalle.FechaExpedicion = Convert.ToDateTime(N1.InnerText);
                        SalidaXML = SalidaXML + 1;
                        break;

                    case "cbc:IssueTime":
                        detalle.HoraExpedicion = N1.InnerText;
                        break;

                    case "cac:AccountingSupplierParty":
                        foreach (XmlNode N2 in N1.ChildNodes)
                        {
                            if (N2.Name == "cac:Party")
                            {
                                foreach (XmlNode N3 in N2.ChildNodes)
                                {

                                    if (N3.Name == "cac:PartyLegalEntity")
                                    {
                                        foreach (XmlNode N4 in N3.ChildNodes)
                                        {
                                            switch (N4.Name)
                                            {
                                                case "cbc:RegistrationName":
                                                    detalle.NombreProveedor = N4.InnerText;
                                                    SalidaXML = SalidaXML + 1;
                                                    break;

                                                case "cbc:CompanyID":
                                                    detalle.NitProveedor = N4.InnerText;
                                                    SalidaXML = SalidaXML + 1;
                                                    break;

                                                case "cac:CorporateRegistrationScheme":
                                                    foreach (XmlNode N5 in N4.ChildNodes)
                                                    {
                                                        detalle.TipoDcto = N5.InnerText;
                                                        string Nrodcto = detalle.NroDcto;
                                                        detalle.NroDcto = Nrodcto.Replace(detalle.TipoDcto, "");
                                                        SalidaXML = SalidaXML + 1;
                                                        break;
                                                    }
                                                    break;

                                                default:
                                                    break;
                                            }

                                        }
                                    }

                                    if (SalidaXML == 7) { break; }

                                }
                            }
                            if (SalidaXML == 7) { break; }
                        }
                        break;

                    default:
                        break;
                }
            }

            if (SalidaXML == 7)
            {
                detalle.Actualizar = true;
            }

            return detalle;
        }

        //Metodo encargado de quitar las etiquetas que no son requeridas en el nuevo xml.
        public string Replace(string Nodo)
        {
            try
            {
                //Se guarda el estado del proceso actual 
                LogSeguimientoLectorCompra("--Inicia adecuacion del Nodo resultante ", Ruta);

                //Se remplaza  informacion en el Nodo resultante
                string Replace1 = Nodo.Replace("<![CDATA[", "");

                //Se remplaza  informacion en el Nodo resultante
                string Nodo2 = Replace1.Replace("]]>", "");

                return Nodo2;
            }
            catch (Exception Error)
            {
                //Se guarda el estado del proceso actual 
                LogSeguimientoLectorCompra("--Se declina adecuacion del Nodo resultante, error : " + Error.Message, Ruta);

                return "Error";
            }
        }
        
        //Metodo para guardar el log del proceso del lector de compras
        public static void LogSeguimientoLectorCompra(string Texto, string Path, string NombreArchivo = "LogSeguimientoLectorCompra.txt")
        {
            //Se crea un directorio
            Directory.CreateDirectory(Path);

            //Se asigna a una variable el Path y el nombre del archivo.
            string LogSeguimiento = System.IO.Path.Combine(Path, NombreArchivo);

            //Se crea un objeto de escritura para llenar el archivo
            using (System.IO.StreamWriter WriterLog = new System.IO.StreamWriter(LogSeguimiento, true))
            {
                //Se captura la fecha del sistema
                string Fecha = DateTime.Now.ToString();

                //Se llena el archivo con el texto enviado como parametro
                WriterLog.WriteLine("\r\n\r\n" + Fecha + "\r\n" + Texto);
            }
        }

        //Metodo para extraer el nombre del xml de la ruta.
        public static string stringBetween(string Source, string Start, string End)
        {
            // Se declara variable para almacenar el nombre del xml
            string result = "";

            //Se valida si el texto de inicio y final, estan dentro de la cadena de texto
            if (Source.Contains(Start) && Source.Contains(End))
            {
                //Se declara variable para determinar la posicion de la palabra inicial.
                int StartIndex = Source.IndexOf(Start, 0) + Start.Length;

                //Se declara variable para determinar la posicion de la palabra Final.
                int EndIndex = Source.IndexOf(End, StartIndex);

                //Se quita de la cadena el "\". 
                result = Source.Substring(StartIndex, EndIndex - StartIndex);
                result = result.Replace("\\", "");
                
                return result;
            }

            return result;
        }

    }
}
