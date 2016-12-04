using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FinalYearProject.BaseTools
{
    public class Router:GeneralTool
    {
        private int index;
        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        private int powerIn;
        public int PowerIn
        {
            get { return powerIn; }
            set { powerIn = value; }
        }
        
        private List<int> ethPorts;        
        
        private UInt16 controllerIDs = 1;
        public UInt16 ControllerIDs
        {
            get { return controllerIDs; }
            set { controllerIDs = value; }
        }

        private UInt16 ledIDs = 1;
        public UInt16 LEDIDs
        {
            get { return ledIDs; }
            set { ledIDs = value; }
        }

        private TreeNode reference;
        public TreeNode Reference
        {
            get { return reference; }
            set { reference = value; }
        }

        private List<Controller> controllers;
        public List<Controller> Controllers
        {
            get { return controllers; }
            set { controllers = value; }
        }

        private IPAddress ip;
        public IPAddress Ip
        {
            get { return ip; }
            set { ip = value; }
        }

        public Router(int powerIn = 10,List<int> ethPorts = null)
        {
            this.powerIn = powerIn;
            if (ethPorts == null)
                this.ethPorts = new List<int>();
            else
                this.ethPorts = ethPorts;

            controllers = new List<Controller>();
        }


        public override Object createMe()
        {
            Router newRouter = new Router(this.powerIn, this.ethPorts);
            return newRouter;
        }
    }
}
