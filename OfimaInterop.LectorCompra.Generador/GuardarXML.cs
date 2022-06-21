using OfimaInterop.LectorCompra.Generador.Entidades;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;


namespace OfimaInterop.LectorCompra.Generador
{
    public class GuardarXML
    {
        //Se encarga de guardar la informacion del emisor en la Base de datos.
        public bool GuardarEmisor(string conex, Emisor emisor)
        {
                //Query SQL a ejecutar.  
                string query = "exec Ofsp_AlmacenarEmisorXMLSEEC @pNit,@pNombre,@pDireccion,@pCiudad,@pPais,@pEmail,@pTelefono";

                using( SqlConnection conexion = new SqlConnection(conex))
                {
                    
                    SqlCommand cmd = new SqlCommand(query, conexion);

                    cmd.Parameters.AddWithValue("@pNit", emisor.Nit);
                    cmd.Parameters.AddWithValue("@pNombre", emisor.Nombre);
                    cmd.Parameters.AddWithValue("@pDireccion", emisor.Direccion);
                    cmd.Parameters.AddWithValue("@pCiudad", emisor.Ciudad);
                    cmd.Parameters.AddWithValue("@pPais", emisor.Pais);
                    cmd.Parameters.AddWithValue("@pEmail", emisor.Email);
                    cmd.Parameters.AddWithValue("@pTelefono", emisor.TeLefono);
                  
                     try
                    {
                        conexion.Open();
                        cmd.ExecuteNonQuery();
                        conexion.Close();
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }    
                }
        }

        //Se encarga de guardar la informacion del detalle del XML en la Base de datos.
        public bool GuardarDetalle(string conex,DetalleDocumento detalle, List<Elemento> ListaElemento)
        {
            string query = "exec Ofsp_AlmacenarDetalleXMLSEEC @pNitProveedor,@pNomProveedor,@pFechaExped,@pHoraEped,@pCUDE,@pTipoDcto,@pNroDcto,@pNomProducto,@pCodigoProducto,@pUnidad,@pCantidad,@pValorUnit,@pPorcentajeIVA";

            bool estado = false;

            foreach ( var Producto in ListaElemento)
            {
                using (SqlConnection conexion = new SqlConnection(conex))
                {

                    SqlCommand cmd = new SqlCommand(query, conexion);

                    cmd.Parameters.AddWithValue("@pNitProveedor", detalle.NitProveedor);
                    cmd.Parameters.AddWithValue("@pNomProveedor", detalle.NombreProveedor);
                    cmd.Parameters.AddWithValue("@pFechaExped", detalle.FechaExpedicion);
                    cmd.Parameters.AddWithValue("@pHoraEped", detalle.HoraExpedicion);
                    cmd.Parameters.AddWithValue("@pCUDE", detalle.CUDE);
                    cmd.Parameters.AddWithValue("@pTipoDcto", detalle.TipoDcto);
                    cmd.Parameters.AddWithValue("@pNroDcto", detalle.NroDcto);
                    cmd.Parameters.AddWithValue("@pNomProducto", Producto.NombreProducto);
                    cmd.Parameters.AddWithValue("@pCodigoProducto", Producto.CodigoProducto);
                    cmd.Parameters.AddWithValue("@pUnidad", Producto.Unidad);
                    cmd.Parameters.AddWithValue("@pCantidad", Producto.Cantidad);
                    cmd.Parameters.AddWithValue("@pValorUnit", Producto.ValorUnit);
                    cmd.Parameters.AddWithValue("@pPorcentajeIVA", Producto.PorcentajeIVA);

                    try
                    {
                        conexion.Open();
                        cmd.ExecuteNonQuery();
                        conexion.Close();
                        estado = true;
                        
                    }
                    catch (Exception )
                    {
                        estado = false;
                        throw;   
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
