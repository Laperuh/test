using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace AprioriAllLib
{
    public partial class Main_window : Form
    {
        String data_file = "";
        bool data_loaded = false;
        double support = new double();
        List<ICustomer> data = new List<ICustomer>();
        List<ICustomer> customerList = new List<ICustomer>();
        List<ICustomer> aprioriAllResult  = new List<ICustomer>();
        List<double> supp_l = new List<double>();
        TimeSpan ts;

        public Main_window()
        {
            InitializeComponent();
        }

        private void data_browse_button_Click(object sender, EventArgs e)
        {
            data_loaded = false;
            data_FileDialog.Filter = "Текстовые файлы (.txt)|*.txt|Все файлы (*.*)|*.*";
            data_FileDialog.FilterIndex = 1;



            if (data_FileDialog.ShowDialog() == DialogResult.OK)
            {
                data_file = data_FileDialog.FileName;
                textBox_data.Text = data_file;
                Data_loader loader = new Data_loader(data_file);
                data = loader.load_data();
                data_loaded = false;
                customerList.Clear();
                customerList = XmlReader.ReadFromXmlFile("D:\\XmlFile.xml"); //xml name

                aprioriAllResult.Clear();
                supp_l.Clear();
                start_apriori_button.Enabled = true;
            }
        }

        private void textBox_data_TextChanged(object sender, EventArgs e)
        {

        }

        private void start_apriori_button_Click(object sender, EventArgs e)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            AprioriAll aprioriAll = new AprioriAll(customerList);
            aprioriAllResult = aprioriAll.RunAprioriAll(support, true,out supp_l);
            stopWatch.Stop();
            ts = stopWatch.Elapsed;
            button_show_litemsetsApriori.Enabled = true;
            time1.Enabled = true;
        }

        private void button_show_litemsetsApriori_Click(object sender, EventArgs e)
        {
            customerList = XmlReader.ReadFromXmlFile("dataset1.xml"); //xml name

            //Itemsets_display display = new Itemsets_display(customerList, "Apriori");
            Itemsets_display display = new Itemsets_display(aprioriAllResult,supp_l, "AprioriAll");
            display.show();
        }

        private void support_setbox_ValueChanged(object sender, EventArgs e)
        {
            support = (double)support_setbox.Value / 100;
        }

        private void time1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("           " + ts.Minutes + " м " + ts.Seconds.ToString() + " с " + ts.Milliseconds.ToString() + " мс           ", "время выполнения");
        }
    }
}
