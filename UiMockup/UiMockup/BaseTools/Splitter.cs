using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinalYearProject.BaseTools
{
    class Splitter:GeneralTool
    {
        private int numOfOutput;

        public Splitter(int numOfOutput)
        {
            this.numOfOutput = numOfOutput;
        }

        public override Object createMe()
        {
            return new Splitter(this.numOfOutput);
        }
    }
}
