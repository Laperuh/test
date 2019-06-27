using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AprioriAllLib
{
    class Program_AprioriAll
    {
        [STAThread]
        static void Main(string[] args)
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main_window());

            List<double> supp_l = new List<double>();
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Console.Out.WriteLine("Реализация алгоритма AprioriAll в .NET.\n");
            Console.ReadKey();
            List<ICustomer> customerList = null;
            double support = -1;

            if (args.Count() == 0)
            {
                try
                {
                    support = 0.4;
                    if (support <= 0 || support > 1)
                    {
                        Console.WriteLine("Недопустимая поддержка: должна быть от 0 до 1");
                        return;
                    }
                    customerList = XmlReader.ReadFromXmlFile("dataset1.xml"); //xml name
                }
                catch (Exception)
                {
                    throw new Exception("Недопустимые параметры");
                }
            }
            else
            {
                Console.Out.WriteLine("Использование: aprioriall <имя_файла> <поддержка>");
                Console.Out.WriteLine("<filename>: путь к файлу XML, содержащему базу данных клиентов");
                Console.Out.WriteLine("  <support>: действительное число больше 0 и меньше или равно 1");
                Console.ReadKey();
                return;
            }

            Console.ReadKey();
            Console.Out.WriteLine("Входные данные:");
            foreach (ICustomer c in customerList)
                Console.Out.WriteLine(" - {0}", c);

            Console.Out.WriteLine("\n вычисление:");
            AprioriAll aprioriAll = new AprioriAll(customerList);
            var aprioriAllResult = aprioriAll.RunAprioriAll(support, true, out supp_l);

            Console.Out.WriteLine("\n Результаты:");
            foreach (ICustomer c in aprioriAllResult)
                Console.Out.WriteLine(" - {0}", c);

            Console.Write("\n Конец.");
            Console.ReadKey();
        }
    }
}
