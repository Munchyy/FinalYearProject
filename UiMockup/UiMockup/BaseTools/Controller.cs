using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FinalYearProject.BaseTools
{
    public enum ControllerMode { ISTRING, SINGLELIGHT }     //ADD OTHER MODES FOR CONTROLLER

    public class Controller:GeneralTool
    {
                
        private TreeNode reference;
        public TreeNode Reference
        {
            get { return reference; }
            set { reference = value; }
        }

        private Router router; //reference to parent router
        public Router Router
        {
            get { return router; }
            set { router = value; }
        }

        private UInt16 id;
        public UInt16 IDNum
        {
            get { return id; }
            set { id = value; }
        }

        private int ledIDs = 1;
        public int LEDIDs
        {
            get { return ledIDs; }
            set { ledIDs = value; }
        }

        private ControllerMode mode;
        public ControllerMode Mode
        {
            get { return mode; }
        }

        private List<LedString> childString;
        public List<LedString> Connections
        {
            get { return childString; }
            set { childString = value; }
        }

        private int stringCount = 1;
        public int StringCount
        {
            get { return stringCount; }
            set { stringCount = value; }
        }

        public Controller(ControllerMode mode = ControllerMode.ISTRING)
        {
            this.mode = mode;
            childString = new List<LedString>();
        }

        public override Object createMe()
        {
            return new Controller(this.mode);
        }
    }
}
