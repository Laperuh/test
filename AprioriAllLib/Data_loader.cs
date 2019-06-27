using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml;

namespace AprioriAllLib
{
    class Data_loader
    {
        public String file = "";
        public int tmp = 0;

        public Data_loader(string filename)
        {
            this.file = filename;
        }

        public List<ICustomer> load_data()
        {
            List<ICustomer> data = new List<ICustomer>();
            //открытие файла
            try
            {
                using (StreamReader sr = new StreamReader(file))
                {
                    int iter = 0;
                    XmlDocument XmlDoc = new XmlDocument();
                    XmlDeclaration XmlDec = XmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                   // XmlDoc.AppendChild(XmlDec); //??
                    XmlElement ElementDatabase = XmlDoc.CreateElement("Customers");
                    XmlDoc.AppendChild(ElementDatabase);               
                    string line;
                    line = sr.ReadLine();
                    string[] tp = line.Split(' ');
                    XmlElement Element_1 = XmlDoc.CreateElement("Customer");
                    Element_1.SetAttribute("Id", tp[0].ToString());
                    ElementDatabase.AppendChild(Element_1);
                    XmlElement Element_8 = XmlDoc.CreateElement("Transaction");
                    Element_1.AppendChild(Element_8);
                    foreach (string st in tp)
                    {
                        if (iter != 0)
                        {
                            XmlElement Element_3 = XmlDoc.CreateElement("Item");
                            Element_8.AppendChild(Element_3);
                            Element_3.InnerText = st;
                        }
                        iter++;
                    }
                    iter = 0;
                    int i = 0;
                    bool first_run = true;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] t = line.Split(' ');
                        i = Convert.ToInt32(t[0]);


                        if (first_run == true) {
                            string[] transaction_array = line.Split(' ');
                            i = Convert.ToInt32(transaction_array[0]);
                            tmp = i;
                             //Element_1 = XmlDoc.CreateElement("Customer");
                             //   Element_1.SetAttribute("Id", i.ToString());
                             //   ElementDatabase.AppendChild(Element_1);
                            XmlElement Element_2 = XmlDoc.CreateElement("Transaction");
                                Element_1.AppendChild(Element_2);
                                foreach (string st in transaction_array)
                                {
                                if (iter != 0)
                                {
                                    XmlElement Element_3 = XmlDoc.CreateElement("Item");
                                    Element_2.AppendChild(Element_3);
                                    Element_3.InnerText = st;
                                }
                                iter++;
                                }
                            iter = 0;
                           // XmlDoc.Save("D:\\XmlFile.Xml");
                            first_run = false;
                        }
                        else
                        {
                            string[] transaction_array = line.Split(' ');
                            i = Convert.ToInt32(transaction_array[0]);

                            if (i != tmp)
                            {
                                tmp = i;
                                Element_1 = XmlDoc.CreateElement("Customer");
                                Element_1.SetAttribute("Id", i.ToString());
                                ElementDatabase.AppendChild(Element_1);
                                XmlElement Element_2 = XmlDoc.CreateElement("Transaction");
                                Element_1.AppendChild(Element_2);
                                foreach (string st in transaction_array)
                                {
                                    if (iter != 0)
                                    {
                                        XmlElement Element_3 = XmlDoc.CreateElement("Item");
                                        Element_2.AppendChild(Element_3);
                                        Element_3.InnerText = st;
                                    }
                                    iter++;
                                }
                                iter = 0;
                                // XmlDoc.Save("D:\\XmlFile.Xml");
                            }
                            else
                            {
                                XmlElement Element_4 = XmlDoc.CreateElement("Transaction");
                                Element_1.AppendChild(Element_4);
                                foreach (string st in transaction_array)
                                {
                                    if (iter != 0)
                                    {
                                        XmlElement Element_3 = XmlDoc.CreateElement("Item");
                                    Element_4.AppendChild(Element_3);
                                    Element_3.InnerText = st;
                                    XmlDoc.Save("D:\\XmlFile.Xml");
                                    }
                                    iter++;
                                }
                                iter = 0;
                            }
                        }
                    }
                    XmlDoc.Save("D:\\XmlFile.Xml");
                    // root.Save("D:\\XmlFile.Xml");
                    sr.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Nie znaleziono pliku \n." + file, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return data;
            }
            return data;
            //throw new NotImplementedException();
        }

    }
}


//XElement root = new XElement("Customers",
//    new XElement("Customer", new XAttribute("id", "1"),
//    new XElement("Transaction", new XElement("Item", "item"))));