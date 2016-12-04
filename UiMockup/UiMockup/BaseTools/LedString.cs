using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SoftBody.SimObjects;
using System.Windows.Forms;

namespace FinalYearProject.BaseTools
{
    public enum LedStringType { RGB, SINGLECOLOR }

    public class LedString:GeneralTool
    {
        private TreeNode reference;
        private int numOfLeds;
        private LedStringType stringType;
        private int powerIn;
        private int id; //not put into xml, just for naming/reference
        private Controller controller;
        private List<SimMass> massList;

        public TreeNode Reference
        {
            get { return reference; }
            set { reference = value; }
        }

        public List<SimMass> MassList
        {
            get { return massList; }
            set { massList = value; }
        }

        public int NumOfLeds
        {
            get { return numOfLeds; }
            set { numOfLeds = value; }
        }

        public LedStringType StringType
        {
            get { return stringType; }
            set { stringType = value; }
        }

        public int PowerIn
        {
            get { return powerIn; }
            set { powerIn = value; }
        }

        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        public Controller Controller
        {
            get { return controller; }
            set { controller = value; }
        }

        public LedString(int numOfLeds = 50, LedStringType stringType = LedStringType.SINGLECOLOR, int powerIn = 10)
        {
            this.numOfLeds = numOfLeds;
            this.stringType = stringType;
            this.powerIn = powerIn;
            massList = new List<SimMass>();
        }

        public override Object createMe()
        {
            return new LedString(this.numOfLeds, this.stringType, this.powerIn);
        }
    }
}
