using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FinalYearProject.BaseTools
{
    class ActiveToolset
    {
        //Dictionaries of loaded tools. key = toolTypeId, value = object reference
        private Dictionary<int, Router> routerTypes;
        private Dictionary<int, Controller> controllerTypes;
        private Dictionary<int, LedString> ledStringTypes;
        private Dictionary<int, Splitter> splitterTypes;

        XDocument toolsetDocument;
    }
}
