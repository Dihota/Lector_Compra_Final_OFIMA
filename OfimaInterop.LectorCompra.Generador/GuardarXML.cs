using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using OfimaInterop.LectorCompra.Generador.Entidades;


namespace OfimaInterop.LectorCompra.Generador
{
    public class GuardarXML
    {
        //Se encarga de guardar la informacion del emisor en la Base de datos.
        public bool GuardarEmisor(string conex, Emisor emisor)
        {
           
                string query = "exec Ofsp_AlmacenarEmisorXMLSEEC @pNit,@pNombre,@pDireccion,@pCiudad,@pPais,@pEmail,@pTelefono";

                using( SqlConnection conexion = new SqlConnection(conex))
                {
                   LectorXML lector = new LectorXML();
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


    }
}
