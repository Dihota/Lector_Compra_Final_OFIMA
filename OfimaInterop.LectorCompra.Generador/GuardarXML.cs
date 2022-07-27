using OfimaInteropLectorCompra.Entidades;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace OfimaInteropLectorCompra
{
    public class GuardarXML
    {
        //Se encarga de guardar la informacion del emisor en la Base de datos.
        public bool GuardarEmisor(string conex, Emisor emisor)
        {
                //Query SQL a ejecutar.  
                string query = "exec Ofsp_ActualizarProveedordesdeXML @pNit,@pNombre,@pDireccion,@pCiudad,@pPais,@pEmail,@pTelefono,@pClase,@pRegimen";
                
                LectorXML lector = new LectorXML();

                using( SqlConnection conexion = new SqlConnection(conex))
                {
                    
                    SqlCommand cmd = new SqlCommand(query, conexion);

                    cmd.Parameters.AddWithValue("@pNit", emisor.Nit);
                    cmd.Parameters.AddWithValue("@pNombre", emisor.Nombre);
                    cmd.Parameters.AddWithValue("@pDireccion", emisor.Direccion);
                    cmd.Parameters.AddWithValue("@pCiudad", emisor.Ciudad);
                    cmd.Parameters.AddWithValue("@pPais", emisor.Pais);
                    cmd.Parameters.AddWithValue("@pEmail", emisor.Email);
                    cmd.Parameters.AddWithValue("@pTelefono", emisor.TeLefono!= null? emisor.TeLefono:"");
                    cmd.Parameters.AddWithValue("@pClase", emisor.Clase);
                    cmd.Parameters.AddWithValue("@pRegimen", emisor.RegimenFis!= null ? emisor.RegimenFis : "ZZ");
                try
                    {
                        conexion.Open();
                        cmd.ExecuteNonQuery();
                        conexion.Close();
                        return true;
                    }
                    catch (Exception Er)
                    {
                    lector.LogSeguimientoLectorCompra("String conexion:" + conex + " SP: Ofsp_ActualizarProveedordesdeXML" + ", parametros(pNit =" + emisor.Nit + ",pNombre" + emisor.Nombre + ",pDireccion"+ emisor.Direccion + ",pCiudad" + emisor.Ciudad + ",pPais"+ emisor.Pais + ",pEmail"+  emisor.Email +",pTelefono" + emisor.TeLefono+")" ) ;
                    lector.LogSeguimientoLectorCompra("Error en el metodo GuardarEmisor: " + Er);
                        return false;
                    }    
                }
        }

        //Se encarga de guardar la informacion del detalle del XML en la Base de datos.
        public bool GuardarDetalle(string conex,DetalleDocumento detalle, List<Elemento> ListaElemento)
        {
            string query = "exec Ofsp_ActualizarCargarXMLdesdeXML @pNitProveedor,@pNomProveedor,@pTipoDcto,@pNroDcto,@pCUDE,@pFechaExped,@pHoraEped,@pNomProducto,@pCodigoProducto,@pValorUnit,@pCantidad,@pPorcentajeIVA,@pMoneda";

            LectorXML lector = new LectorXML();

            bool estado = false;

            foreach ( var Producto in ListaElemento)
            {
                using (SqlConnection conexion = new SqlConnection(conex))
                {


                    SqlCommand cmd = new SqlCommand(query, conexion);

                    cmd.Parameters.AddWithValue("@pNitProveedor", detalle.NitProveedor);
                    cmd.Parameters.AddWithValue("@pNomProveedor", detalle.NombreProveedor);
                    cmd.Parameters.AddWithValue("@pTipoDcto", detalle.TipoDcto);
                    cmd.Parameters.AddWithValue("@pNroDcto", detalle.NroDcto);
                    cmd.Parameters.AddWithValue("@pCUDE", detalle.CUDE);
                    cmd.Parameters.AddWithValue("@pFechaExped", detalle.FechaExpedicion);
                    cmd.Parameters.AddWithValue("@pHoraEped", detalle.HoraExpedicion);
                    cmd.Parameters.AddWithValue("@pMoneda", detalle.Currency);
                    cmd.Parameters.AddWithValue("@pNomProducto", Producto.NombreProducto);
                    cmd.Parameters.AddWithValue("@pCodigoProducto", Producto.CodigoProducto);
                    cmd.Parameters.AddWithValue("@pValorUnit", Producto.ValorUnit);
                    cmd.Parameters.AddWithValue("@pCantidad", Producto.Cantidad);
                    cmd.Parameters.AddWithValue("@pPorcentajeIVA", Producto.PorcentajeIVA!= null? Producto.PorcentajeIVA : "0");
                

                    try
                    {
                        conexion.Open();
                        cmd.ExecuteNonQuery();
                        conexion.Close();
                        estado = true;
                        
                    }
                    catch (Exception Er )
                    {
                        lector.LogSeguimientoLectorCompra("String conexion:" + conex + " SP: Ofsp_ActualizarCargarXMLdesdeXML" + ", parametros(pNitProveedor =" +
                            detalle.NitProveedor + ",pNomProveedor" + detalle.NombreProveedor + ",pTipoDcto" + detalle.TipoDcto + ",pNroDcto" + detalle.NroDcto +
                            ",pCUDE" + detalle.CUDE + ",pFechaExped" + detalle.FechaExpedicion + ",pHoraEped" + detalle.HoraExpedicion +
                            ",pMoneda" + detalle.Currency + ",pNomProducto" + Producto.NombreProducto + ",pCodigoProducto" + Producto.CodigoProducto +
                            ",pValorUnit" + Producto.ValorUnit + ",pCantidad" + Producto.Cantidad + ",pPorcentajeIVA" + Producto.PorcentajeIVA + ")");

                        lector.LogSeguimientoLectorCompra("Error en el metodo GuardarDetalle: " + Er);
                        estado = false;
                        return false;   
                    }
                }
            }

            if (estado == true)
            {
                return true;
            }
            else 
            {
                return false;
            }

        }

    }
}
