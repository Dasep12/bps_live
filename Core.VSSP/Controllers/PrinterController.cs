using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO.Ports;
using System.Linq;
using System.Web.Mvc;

namespace Core.VSSP.Controllers
{
    public class PrinterController : Controller
    {
        // GET: Printer
        public class PrinterListClass
        {
            private void PrinterViews()
            {
                List<PortClass> ports = new List<PortClass>();
                List<Printersclass> Printersfor = new List<Printersclass>();
                string[] portnames = SerialPort.GetPortNames();
                /*PORTS*/
                for (int i = 0; i < portnames.Count(); i++)
                {
                    ports.Add(new PortClass() { Name = portnames[i].Trim(), Desc = portnames[i].Trim() });
                }
                /*PRINTER*/
                for (int i = 0; i < PrinterSettings.InstalledPrinters.Count; i++)
                {
                    Printersfor.Add(new Printersclass() { Name = PrinterSettings.InstalledPrinters[i].Trim(), Desc = PrinterSettings.InstalledPrinters[i].Trim() });
                }
            }
        }
        public class PortClass
        {
            public string Name { get; set; }
            public string Desc { get; set; }

            public override string ToString()
            {
                return string.Format("{0} ({1})", Name, Desc);
            }
        }
        public class Printersclass
        {
            public string Name { get; set; }
            public string Desc { get; set; }

            public override string ToString()
            {
                return string.Format("{0} ({1})", Name, Desc);
            }
        }
    }
}