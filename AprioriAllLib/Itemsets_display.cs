using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AprioriAllLib
{
    class Itemsets_display
    {
        List<ICustomer> itemsets = new List<ICustomer>();
        string title = "";
        int iter = 0;
        List<double> supp_l = new List<double>();

        public Itemsets_display(List<ICustomer> list,List<double> supp_l, string title)
        {
            this.itemsets = list;
            this.title = title;
            this.supp_l = supp_l;
        }

        public void show()
        {
            Show_itemsets window = new Show_itemsets();
            window.Show();
            //if(itemsets.Count==0)
            //{
            //    return;
            //}
            foreach(ICustomer i in itemsets)
            {
                
                DataGridViewRow row = new DataGridViewRow();
                //row.CreateCells(window.dataGridView1);
              //  string items = "{";
                //foreach(string element in i.items)
                //{
                //    items += element + ", ";
                //}

             //   items = items.Substring(0, items.Length - 2) + "}";

                window.dataGridView1.Rows.Add(i, string.Format("{0:0.000}", supp_l[iter]));
                iter++;
            }
            window.Text += "-" + title;
           
        }
    }
}
