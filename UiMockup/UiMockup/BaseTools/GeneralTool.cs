using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FinalYearProject.BaseTools
{
    public enum ToolType { TOPO, GEO, BOTH, NONE }

    public abstract class GeneralTool
    {
        //fields that all tools will have

        private ToolType toolType;
        private TreeNode reference;
        public abstract Object createMe();
    }
}
