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
            lector.LectorCompras(TxtRuta.Text);
        }
    }
}
