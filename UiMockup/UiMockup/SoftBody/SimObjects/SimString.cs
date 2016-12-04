//DERIVED FROM http://cg.skeelogy.com/introduction-to-soft-body-physics-in-xna/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using Microsoft.Xna.Framework;


namespace SoftBody.SimObjects
{
    public class SimString
    {
        private int id;
        [DisplayName("String ID")]
        [Description("ID of string shown in XML file")]
        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        private int numOfMasses;
        [DisplayName("Number of masses")]
        [Description("Number of leds on the led string")]
        public int NumOfMasses
        {
            get { return numOfMasses; }
        }

        private Vector2 firstMassLocation;
        [DisplayName("First mass location X")]
        [Description("The X coordinate of the first led in the string. Anchored by default")]
        public float FirstMassLocationX
        {
            get { return MassList.ElementAt(0).CurrPositionX; }
            set { massList.ElementAt(0).CurrPositionX = value; }
        }

        [DisplayName("First mass location Y")]
        [Description("The Y coordinate of the first led in the string. Anchored by default")]
        public float FirstMassLocationY
        {
            get { return MassList.ElementAt(0).CurrPositionY; }
            set { massList.ElementAt(0).CurrPositionY = value; }
        }

        private List<SimMass> massList;
        [DisplayName("Light Points")]
        [Description("The collection of light points that make up the string")]        
        public List<SimMass> MassList
        {
            get { return massList; }
            set { massList = value;
                    numOfMasses = massList.Count;
            }
        }

        public SimString()
        {
            massList = new List<SimMass>();
        }

        public SimString(List<SimMass> massList)
        {
            this.massList = massList;
            this.numOfMasses = this.massList.Count;
        }

        public void AddMass(SimMass mass){
            massList.Add(mass);
            this.firstMassLocation = massList.ElementAt(0).CurrPosition;
            this.numOfMasses = this.massList.Count;
        }

        public void AddMass(List<SimMass> masses){
            massList.AddRange(masses);
            firstMassLocation = massList.ElementAt(0).CurrPosition;
            this.numOfMasses = this.massList.Count;
        }

        public SimString FindMassOwner(SimMass mass)
        {
            if (massList.Contains(mass))
            {
                return this;
            }
            else
            {
                return null;
            }
        }
    }
}
