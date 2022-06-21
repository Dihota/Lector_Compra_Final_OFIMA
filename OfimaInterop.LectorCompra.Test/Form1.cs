using OfimaInterop.LectorCompra.Generador;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OfimaInterop.LectorCompra.Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void BtnObtener_Click(object sender, EventArgs e)
        {
            DgvVista.Refresh();
            LectorXML NewXML = new LectorXML();
            string ruta = TxtRuta.Text;
            DgvVista.DataSource = NewXML.ObtenerXML(ruta);
        }

        private void BtnAbrir_Click(object sender, EventArgs e)
        {
            using(var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string ruta = dialog.SelectedPath;
                    TxtRuta.Text = ruta;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LectorXML lector = new LectorXML();
            var Respuesta = lector.LectorCompras(TxtRuta.Text, 1017224525, @"Data Source=.\SQLEXPRESS;Initial Catalog=TODOTERRENO2017;User ID=Aplicacion_Ofimatica;Password=ofima");

            MessageBox.Show(Respuesta);
            //DgvVista.Columns.Add("nit", "Nit");
            //DgvVista.Columns.Add("nombre", "Nombre");
            //DgvVista.Columns.Add("direccion", "Direccion");
            //DgvVista.Columns.Add("ciudad", "Ciudad");
            //DgvVista.Columns.Add("pais", "Pais");
            //DgvVista.Columns.Add("email", "Email");
            //DgvVista.Columns.Add("tel", "Tel");

            //DgvVista.Rows.Add(Emisor.Nit, Emisor.Nombre, Emisor.Direccion, Emisor.Ciudad, Emisor.Pais, Emisor.Email, Emisor.TeLefono);



            
        }
    }
}
