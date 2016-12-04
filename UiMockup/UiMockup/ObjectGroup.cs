using SoftBody.SimObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FinalYearProject
{
    public class ObjectGroup
    {
        private List<SimMass> groupList;
        public List<SimMass> GroupList
        {
            get { return groupList; }
        }

        private TreeNode reference;
        public TreeNode Reference
        {
            get { return reference; }
            set { reference = value; }
        }

        private int index;
        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public ObjectGroup()
        {
            name = "Un-Named Group";
            index = -1;
            groupList = new List<SimMass>();
        }

        public void addList(List<SimMass> newList)
        {
            this.groupList.AddRange(newList);
        }

        public void addMass(SimMass newObj)
        {
            this.groupList.Add(newObj);
        }

        public void resetList()
        {
            this.groupList = new List<SimMass>();
        }
    }
}
