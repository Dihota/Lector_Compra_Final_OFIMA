using OfimaInteropLectorCompra.Entidades;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;


namespace OfimaInteropLectorCompra
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
        public string LectorCompras(string Carpeta, int Nit, string Conexion, string Destino)
        {
            try
            {
                //Se inicializa la informacion en el log
                LogSeguimientoLectorCompra("-----------------------------------------------------------------------------------------");

                //Se crea un objeto del tipo emisor
                Emisor emisor = new Emisor();
                LogSeguimientoLectorCompra("--Se crea el objeto tipo Emisor.");

                //Se crea un objeto del tipo DettaleDocumeto 
                DetalleDocumento detalle = new DetalleDocumento();
                LogSeguimientoLectorCompra("--Se crea el objeto tipo Detalle.");

                //Se crea un objeto del tipo Adquiriente
                Adquiriente adquiriente = new Adquiriente();
                LogSeguimientoLectorCompra("--Se crea el objeto tipo Adquiriente.");

                //Se crea un objeto del tipo elemento
                Elemento element = new Elemento();

                //Se crea lista de elementos
                List<Elemento> listaElementos = new List<Elemento>();
                LogSeguimientoLectorCompra("--Se crea lista de productos, en XML.");

                //se instancia objeto para guardar los datos obtenidos
                GuardarXML guardar = new GuardarXML();

                //Se inicializa variable que almacenara el nit del XML.
                int NitXML = 0;

                // se declara variable que almacena el xml que hay dentro del xml principal.
                var NodoAuxiliar = "";

                //Llama el metodo encargado de identificar los diferentes xml en la ruta
                foreach (var item in ObtenerXML(Carpeta))
                {

                    LogSeguimientoLectorCompra("--Inicia Analisis para el documento : ( " + item.nombreXML + ".xml).");

                    //Se captura el Nit del adquiriente en el XML
                    NitXML = LectorAdquiriente(item.RutaXML, adquiriente, ref NodoAuxiliar);

                    //Se valida si el Nit del XML, corresponde con el nit de la empresa, para gestionar XML.
                    if (Nit == NitXML)
                    {
                        LogSeguimientoLectorCompra("--Nit corresponde con el registrado en NITCIA.");

                        //Se capturan los datos del emisor.
                        LectorEmisor(NodoAuxiliar, emisor);

                        //Se crea variable que captura la respuesta del guardado del emisor.
                        bool SaveEmisor;

                        LogSeguimientoLectorCompra("--Inicia el proceso de almacenar informacion del emisor");

                        //Se guarda la informacion del emisor
                        SaveEmisor = guardar.GuardarEmisor(Conexion, emisor);

                        //se valida si la informacion del emisosr se guardo con exito.
                        if (SaveEmisor is false)
                        {
                            LogSeguimientoLectorCompra("--No se logra almacenar los datos del emisor : " + emisor.Nit);
                            MoverArchivo(Carpeta, Destino, item.nombreXML, false);
                            return "Error";
                        }
                        else
                        {
                            LogSeguimientoLectorCompra("--Se almacena correctamente la informacion del emisor en las tablas (MTPROCLI y NIT) : " + emisor.Nit);
                        }

                        //Se captura los datos del detalle del documento.
                        LectorDetalle(NodoAuxiliar, detalle);

                        if (detalle.Actualizar is true)
                        {
                            //Se comienza a llenar la informacion por archivo
                            LogSeguimientoLectorCompra("--Finaliza proceso de extraer informacion del detalle del XML.");

                            //Se captura la informacion de cada elemento de la compra
                            listaElementos = ObtenerElementosXML(NodoAuxiliar, element);
                            if (listaElementos.Count > 0)
                            {
                                //Se comienza a llenar la informacion por archivo
                                LogSeguimientoLectorCompra("--Inicia proceso de almacenar informacion del detalle del XML.");
                                //
                                bool SaveDetalle;

                                //Se llama el metodo encargado de guardar la informacion del detalle del XML
                                SaveDetalle = guardar.GuardarDetalle(Conexion, detalle, listaElementos);

                                if (SaveDetalle is false)
                                {
                                    LogSeguimientoLectorCompra("--No se logra almacenar los datos del detalle: ");
                                    MoverArchivo(Carpeta, Destino, item.nombreXML, false);
                                    return "Error";
                                }

                                MoverArchivo(Carpeta, Destino, item.nombreXML, true);
                            }
                            else
                            {
                                LogSeguimientoLectorCompra("--No se logra almacenar los datos del emisor : " + emisor.Nit);
                                MoverArchivo(Carpeta, Destino, item.nombreXML, false);
                                return "Error";
                            }

                            LogSeguimientoLectorCompra("--Se almacena correctamente la informacion del detalle en la tabla (CARGARXML) : ");
                        }
                        else
                        {
                            LogSeguimientoLectorCompra("--No se logra almacenar los datos del emisor : " + emisor.Nit);
                            MoverArchivo(Carpeta, Destino, item.nombreXML, false);
                            return "Error";

                        }
                    }
                    else
                    {
                        //Mensaje que indica que el documento no se examina, por que no es una factura de compra
                        LogSeguimientoLectorCompra("--No se trata el XML: ( " + item.nombreXML + ".xml ), Nit no corresponde con el registrado en NITCIA.");
                        MoverArchivo(Carpeta, Destino, item.nombreXML, false);
                    }
                }
            }
            catch (Exception error)
            {
                LogSeguimientoLectorCompra("--Errror detectado en la ejecucion del proceso: " + error);
                return "Error en el proceso";

            }
            return "Finalizo";
        }

        ////Leer los elementos o productos del detalle del XML
        public List<Elemento> ObtenerElementosXML(string RutaXML,Elemento Newelemento)
        {
            //Se inicializa variable para salir de los ciclos
            SalidaXML = 0;

            //Se crea lista que contendra los producots presente en la factura
            List<Elemento> Lista = new List<Elemento>();

            
            XmlDocument ReadXML = new XmlDocument();
            ReadXML.LoadXml(RutaXML);

            //Se inicializa Ciclo encargado de capturar los productos de la factura
            foreach (XmlNode N1 in ReadXML.DocumentElement.ChildNodes)
            {
                if (N1.Name == "cac:InvoiceLine")
                {
                    foreach (XmlNode N2 in N1.ChildNodes)
                    {
                        switch (N2.Name)
                        {
                            case "cbc:InvoicedQuantity":
                                Newelemento.Cantidad = N2.InnerText;
                                SalidaXML = SalidaXML + 1;
                                break;

                            case "cac:TaxTotal":
                                foreach (XmlNode N3 in N2.ChildNodes)
                                {
                                    if (N3.Name == "cac:TaxSubtotal")
                                    {
                                        foreach (XmlNode N4 in N3.ChildNodes)
                                        {
                                            if (N4.Name == "cac:TaxCategory")
                                            {
                                                foreach (XmlNode N5 in N4.ChildNodes)
                                                {
                                                    if (N5.Name == "cbc:Percent")
                                                    {
                                                        Newelemento.PorcentajeIVA = N5.InnerText;
                                                        SalidaXML = SalidaXML + 1;
                                                    }
                                                    if(SalidaXML == 2) { break;}
                                                }
                                            }
                                        }
                                    }
                                }
                                break;

                            case "cac:Item":
                                foreach (XmlNode N3 in N2.ChildNodes)
                                {
                                    switch (N3.Name)
                                    {
                                        case "cbc:Description":
                                            Newelemento.NombreProducto = N3.InnerText;
                                            SalidaXML = SalidaXML + 1;
                                            break;

                                        case "cac:SellersItemIdentification":
                                            foreach (XmlNode N4 in N3.ChildNodes)
                                            {
                                                if (N4.Name == "cbc:ID")
                                                {
                                                     
                                                        Newelemento.CodigoProducto = N4.InnerText;
                                                    
                                                        
                                                    SalidaXML = SalidaXML + 1;
                                                }
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                break;

                            case "cac:Price":
                                foreach (XmlNode N3 in N2.ChildNodes)
                                {
                                    switch (N3.Name)
                                    {
                                        case "cbc:PriceAmount":
                                            Newelemento.ValorUnit = N3.InnerText;
                                            SalidaXML = SalidaXML + 1;
                                            break;

                                        case "cbc:BaseQuantity":
                                            Newelemento.Unidad = N3.InnerText;
                                            SalidaXML = SalidaXML + 1;
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
                    //Se agregan valores a lista 
                    Lista.Add(new Elemento
                    {
                        NombreProducto  = Newelemento.NombreProducto,
                        CodigoProducto  = Newelemento.CodigoProducto,
                        Unidad          = Newelemento.Unidad,
                        Cantidad        = Newelemento.Cantidad,
                        ValorUnit       = Newelemento.ValorUnit,
                        PorcentajeIVA   = Newelemento.PorcentajeIVA
                    });
                }    
            }
            return Lista;
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
        public int LectorAdquiriente(string RutaXML, Adquiriente NewAdquiriente, ref string NewNodo)
        {
            LogSeguimientoLectorCompra("--Inicia validacion del documento del Adquiriente.");

            //Se inicializa variable para la salida de los ciclos.
            SalidaXML = 0;

            //Se llama el metodo, que leera el XML.
            XmlDocument ReadXML = new XmlDocument();
            ReadXML.Load(RutaXML);
            
            
            try
            {
                //Se comienza a rrecorrer los nodo del XML.
                LogSeguimientoLectorCompra("--Inicia recorrido por los nodos del XML.");
                foreach (XmlNode N1 in ReadXML.DocumentElement.ChildNodes)
                {
                    switch (N1.Name)
                    {
                        case "cac:ReceiverParty":
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
                                if (SalidaXML == 1) { break; }
                            }
                            break;
                        case "cac:Attachment":
                            foreach (XmlNode N2 in N1.ChildNodes)
                            {
                                foreach (XmlNode N3 in N2.ChildNodes)
                                {
                                    if (N3.Name == "cbc:Description")
                                    {
                                        //Se captura el nodo y se asigna a una variable
                                        LogSeguimientoLectorCompra("--Se captura la informacion del nodo resultante.");
                                        Nodo = N3.InnerText;;

                                        //Se invoca metodo que remplaza los valores no necesarios en el XML nuevo
                                        NewNodo = Replace(Nodo);

                                        SalidaXML = SalidaXML + 1;
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                    if (SalidaXML == 2) { break; }
                }

                //Se guarda mensaje en log.
                LogSeguimientoLectorCompra("--Se captura documento del adquiriente: (" + NewAdquiriente.Nit + ").");

                //Se retorna el objeto adquiriente.
                return NewAdquiriente.Nit;
            }
            catch (Exception error)
            {

                //Se guarda mensaje en log.
                LogSeguimientoLectorCompra("--No se logra capturar documento del adquiriente: (" + NewAdquiriente.Nit + ", Error : " + error + ").");

                //Se retorna un cero, por que no se capturo nit del adquiriente.
                return 0;
            }
        }

        //Metodo para obtener los datos del Emisor(proveedor)
        public dynamic LectorEmisor(string RutaXML,Emisor NewEmisor)
        {
            //Se comienza a llenar la informacion por archivo
            LogSeguimientoLectorCompra("--Inicia proceso de extraer informacion del XML, para el EMISOR.");
            try
            {
                //Se invoca el LectorEmisorAuxiliar, para que lea los valores del nuevo XML
                LectorEmisorAuxiliar(RutaXML, NewEmisor);

                //Se retorna objeto Emisor con la informacion encontrada en el XML
                return NewEmisor;
            }
            catch (Exception err)
            {
                LogSeguimientoLectorCompra("--No se logra capturar la informacion del EMISOR." + err);

                return string.Empty;
            }
        }

        //Metodo para obtener los datos del Detalle del Documento.
        public dynamic LectorDetalle(string RutaXML, DetalleDocumento NewDetalle)
        {
            //Se comienza a llenar la informacion por archivo
            LogSeguimientoLectorCompra("--Inicia proceso de extraer informacion del detalle del XML.");

            try
            {
                //Se invoca el LectorEmisorAuxiliar, para que lea los valores del nuevo XML
                LectorDetalleAuxiliar(RutaXML, NewDetalle);

                //Se indica que los datos se pueden actualizar
                NewDetalle.Actualizar = true;

                return NewDetalle;
            }
            catch (Exception Err)
            {
                LogSeguimientoLectorCompra("--No se logra extraer informacion del detalle del XML." + Err);

                return string.Empty;
            }
        }

        //Se captura la informacion del emisor en el XML
        public Emisor LectorEmisorAuxiliar(string NewXML, Emisor emisor)
        {
            //Se invoca el metodo para abrir el XML
            XmlDocument ReadXML = new XmlDocument();
            ReadXML.LoadXml(NewXML);

            //Se comienza a llenar la informacion por archivo
            LogSeguimientoLectorCompra("--Inicia proceso de extraer informacion del EMISOR.");

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
                                                            emisor.Clase = N4.Attributes["schemeName"].Value;
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

                    case "cac:Delivery":
                        foreach (XmlNode N2 in N1.ChildNodes)
                        {
                            if (N2.Name == "cac:DeliveryParty")
                            {
                                foreach (XmlNode N3 in N2.ChildNodes)
                                {
                                    if (N3.Name == "cac:PartyTaxScheme")
                                    {
                                        foreach (XmlNode N4 in N3.ChildNodes)
                                        {
                                            switch (N4.Name)
                                            {
                                                case "cbc:CompanyID":
                                                    emisor.Clase = N4.Attributes["schemeName"].Value;
                                                    break;

                                                case "cac:TaxScheme":
                                                    foreach (XmlNode N5 in N4.ChildNodes)
                                                    {
                                                        if (N5.Name == "cbc:ID")
                                                        {
                                                            emisor.RegimenFis = N5.InnerText;
                                                        }
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }

                                            if (N4.Name == "cbc:CompanyID")
                                            {
                                                
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                    
                                }
                                break;
                            }
                        }
                            break;
                    default:
                        break;

                }
            }

            //Se comienza a llenar la informacion por archivo
            LogSeguimientoLectorCompra("--Finaliza proceso de extraer informacion del EMISOR.");

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
                        detalle.FechaExpedicion = N1.InnerText;
                        SalidaXML = SalidaXML + 1;
                        break;

                    case "cbc:IssueTime":
                        detalle.HoraExpedicion = N1.InnerText;
                        break;

                    case "cbc:DocumentCurrencyCode":
                        detalle.Currency = N1.InnerText;
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
                                                    detalle.NitProveedor = N4.InnerText + "-" + N4.Attributes["schemeID"].Value;
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
                LogSeguimientoLectorCompra("--Inicia adecuacion del Nodo resultante ");

                //Se remplaza  informacion en el Nodo resultante
                string Replace1 = Nodo.Replace("<![CDATA[", "");

                //Se remplaza  informacion en el Nodo resultante
                string Nodo2 = Replace1.Replace("]]>", "");

                LogSeguimientoLectorCompra("--Finaliza adecuacion del Nodo resultante.");
                return Nodo2;
            }
            catch (Exception Error)
            {
                //Se guarda el estado del proceso actual 
                LogSeguimientoLectorCompra("--Se declina adecuacion del Nodo resultante, error : " + Error.Message);

                return "Error";
            }
        }
        
        //Metodo para guardar el log del proceso del lector de compras
        public void LogSeguimientoLectorCompra(string Texto, string NombreArchivo = "LogSeguimientoLectorCompra.txt")
        {
            string Path = Directory.GetCurrentDirectory();
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

        //Metodo para mover los archivos segun el estado
        public void MoverArchivo(string origen, string destino, string archivo, bool tipo)
        {
            //Variable que define para donde se mueve el XML.
            string RutaDestino;

            //Variable que define donde se encuentra el XML.
            string RutaOrigen = origen + @"\" + archivo + ".xml";
            
            //Se valida si el archivo se mueve a la cvarpeta actualizados o rechazados
            if (tipo is true)
            {
                string Path = destino + @"\Actualizados";

                //Se valida si la carpeta existe si no la crea.
                if (!Directory.Exists(Path))
                {
                    Directory.CreateDirectory(Path);
                }
            
                RutaDestino = Path + @"\" + archivo + ".xml";
            }
            else
            {
                string Path = destino + @"\Rechazados";

                //Se valida si la carpeta existe si no la crea.
                if (!Directory.Exists(Path))
                {
                    Directory.CreateDirectory(Path);
                }

                RutaDestino = Path + @"\" + archivo + ".xml";
            }

            //Variable para saber si el archivo ya se encuentra en las carpetas
            bool Result = File.Exists(RutaDestino);

            //Si el archivo se encuentra, procede a eliminarlo de la ruta
            if (Result == true)
            {
                File.Delete(RutaDestino);
            }

            //se copia el archivo de la ruta de origen a la nueva ruta
            System.IO.File.Move(RutaOrigen, RutaDestino);

            //Se almacena en el log que el archivo fue guardado.
            LogSeguimientoLectorCompra("--El archivo : " + archivo + ", fue movido a la carpeta:" + RutaDestino);
        }

    }
}
