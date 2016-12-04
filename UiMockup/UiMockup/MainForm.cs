using FinalYearProject.BaseTools;
using FinalYearProject.Controls;
using SoftBody.SimObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//xml
using System.Xml.Linq;

using System.Runtime.InteropServices;
using System.Diagnostics;

namespace FinalYearProject
{
    [Serializable]
    public partial class MainForm : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
        

        public enum SelectedObject { LED_NODE, NONE }
        public SelectedObject selected;
        public TreeNode selectedToolTreeNode;
        public TreeNode selectedGroupTreeNode;
        public List<ObjectGroup> groups;
        private bool isNew;

        //for led id's
        public UInt16 globalID;


        //for topo
        public List<Router> topology;

        //for undo functionality. not implemented
        public List<String> actions;
        public Stack<SoftBodyControl> undoStack;

        public MainForm()
        {
            InitializeComponent();
            Initialize();
            
        }

        private void Initialize()
        {
            string currentDir = Environment.CurrentDirectory;

            softBodyControl1.ParentForm = this;
            selectedToolTreeNode = treeView1.Nodes[0]; //default tool is led
            selectedGroupTreeNode = null;

            topology = new List<Router>();
            TreeNode routerNode = new TreeNode("Router 1");
            Router tempRouter = new Router();
            tempRouter.Index = 1;
            topology.Add(tempRouter);
            tempRouter.Reference = routerNode;
            topo_treeView.Nodes.Add(routerNode);

            isNew = true;

            groups = new List<ObjectGroup>();
            //undoStack = new Stack<SoftBodyControl>();


            //add "all leds" group
            ObjectGroup newGroup = new ObjectGroup();
            newGroup.Name = "All LEDs";
            newGroup.addList(softBodyControl1.SelectAll());
            groups.Add(newGroup);
            newGroup.Index = groups.IndexOf(newGroup);
            
            //create new tree node
            TreeNode newNode = new TreeNode();
            newNode.Name = newGroup.Name;
            newNode.Text = newGroup.Name;
            //add tree node to tree view
            groups_treeView.Nodes.Add(newNode);

            combo_numOfMasses.SelectedIndex = 0;
            combo_gridLines.SelectedIndex = 3;
            combo_gridColor.SelectedIndex = 1;
            combo_backColor.SelectedIndex = 8;
        }

        public void stringAdded(List<SimMass> massList)
        {
            isNew = false;

            //add the masses to an ledString
            LedString ledString = new LedString();
            ledString.MassList = massList;
            Controller controller = new Controller();
            controller.Connections.Add(ledString);
            Router router = topology.ElementAt(topology.Count-1);
            controller.Router = router;
            ledString.Controller = controller;

            
        
            router.Controllers.Add(controller);
            controller.IDNum = router.ControllerIDs;
            router.ControllerIDs++;

            ledString.ID = controller.StringCount;
            controller.StringCount++;

            foreach (SimMass mass in ledString.MassList)
            {
                mass.MassID = controller.LEDIDs;
                controller.LEDIDs++;
            }

            TreeNode controllerNode = new TreeNode("Controller "+controller.IDNum);
            TreeNode ledStringNode = new TreeNode("String "+ledString.ID);
            controller.Reference = controllerNode;
            ledString.Reference = ledStringNode;
            controllerNode.Nodes.Add(ledStringNode);
            controller.Router.Reference.Nodes.Add(controllerNode);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit?", "Exit?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }


        public void setPropertyView(Object obj)
        {
            //apply to property grid
            ObjectPropertyViewer.SelectedObject = obj;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            selectedToolTreeNode = e.Node;
            if (e.Node.Index == 3) //if delete node is pressed
            {
                DeleteSelected();
            }

            foreach (SimMass mass in softBodyControl1.SelectAll())
            {
                softBodyControl1.ClearSelection();
            }

        }

        private void treeView2_AfterSelect(object sender, TreeViewEventArgs e)
        {
            selectedGroupTreeNode = e.Node;
        }


        private void toolStripButton1_Click(object sender, EventArgs e)
        {

        }

        private void groupAddBtn_Click(object sender, EventArgs e)
        {
            //find all masses that are selected
            List<SimMass> selectedMasses = softBodyControl1.FindSelected();
            if (selectedMasses.Count == 0) //check if any are selected
            {
                MessageBox.Show("No LEDs are currently selected. Cannot add to group", "Error", MessageBoxButtons.OK);
            }
            else
            {
                //create new group, add leds and add to group list
                ObjectGroup newGroup = new ObjectGroup();
                newGroup.addList(selectedMasses);
                groups.Add(newGroup);
                newGroup.Index = groups.IndexOf(newGroup);
                newGroup.Name = "Custom Group" + groups.IndexOf(newGroup);
                Console.WriteLine(newGroup.ToString());

                //create new tree node
                TreeNode newNode = new TreeNode();
                newNode.Name = newGroup.Name;
                newNode.Text = newGroup.Name;
                //addtreenode reference to objectgroup
                newGroup.Reference = newNode;
                //add tree node to tree view
                groups_treeView.Nodes.Add(newNode);
                // actions.Add("GroupCreated");
            }

        }

        private void groupRemoveBtn_Click(object sender, EventArgs e)
        {
            if (selectedGroupTreeNode != null)
            {
                var result = MessageBox.Show("Are you sure you want to remove the group?", "Removing Group", MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    ObjectGroup toRemove = null;
                    foreach (ObjectGroup objGroup in groups)
                    {
                        if (selectedGroupTreeNode == objGroup.Reference)
                        {
                            toRemove = objGroup;
                        }
                    }

                    if (toRemove != null)
                    {
                        groups.Remove(toRemove);
                        selectedGroupTreeNode.Remove();
                    }
                    
                }
            }
            else
            {
                MessageBox.Show("No group selected", "Error:", MessageBoxButtons.OK);
            }
        }

        private void treeView2_DoubleClick(object sender, EventArgs e)
        {
            softBodyControl1.ClearSelection();

            if (selectedGroupTreeNode != null)
            {
                foreach (ObjectGroup objGroup in groups)
                {
                    if (objGroup.Name.Equals("All LEDs"))
                    {
                        objGroup.resetList();
                        objGroup.addList(softBodyControl1.SelectAll());
                    }
                }

                foreach (ObjectGroup objGroup in groups)
                {
                    if (objGroup.Name.Equals(selectedGroupTreeNode.Name))
                    {
                        softBodyControl1.ClearSelection();
                        foreach (SimMass mass in objGroup.GroupList)
                        {
                            mass.Selected = true;
                        }
                    }
                }
            }
        }

        //tool bar save icon on_click: Save the project to an xml file
        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-GB");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-GB");

            if (softBodyControl1.StringLists.Count != 0)
            {
                saveFileDialog1.Filter = "XML File (*.XML)|*.xml|All Files (*.*)|*.*";
                saveFileDialog1.FilterIndex = 1;
                DialogResult diaRes = saveFileDialog1.ShowDialog();
                if (diaRes == DialogResult.OK)
                {
                    string file = saveFileDialog1.FileName;

                    XDocument doc = new XDocument(new XElement("Scene",new XAttribute("xmlns","")));
                    //doc.Element("Scene").Add(new XElement("Router_"+router.Index.ToString()));
                    List<XElement> routerElements = new List<XElement>();
                    foreach (Router router in topology)
                    {
                        XElement newEle = new XElement("FixedIPService",new XAttribute("name","Router_" + router.Index), new XAttribute("hostname","0.0.0.0"),new XAttribute("port","51966"));
                        foreach (Controller controller in router.Controllers)
                        {
                            foreach (LedString ledString in controller.Connections)
                            {
                                foreach (SimMass mass in ledString.MassList)
                                {
                                    newEle.Add(new XElement("Light",
                                            new XAttribute("id",(controller.IDNum << 8)+(mass.MassID & 0xFF)),
                                            new XAttribute("x",mass.CurrPositionX/1000),
                                            new XAttribute("y",mass.CurrPositionY/1000),
                                            new XAttribute("z",0)
                                           ));
                                }
                            }
                        }
                        routerElements.Add(newEle);
                    }
                    
                    //add router elements to doc
                    foreach (XElement routerEle in routerElements)
                    {
                        doc.Element("Scene").Add(routerEle);
                    }
                    doc.Save(file);
                }
                
            }
            else
            {
                MessageBox.Show("Could not save, no elements", "Error Saving", MessageBoxButtons.OK);
            }
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-GB");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-GB");

            //goto new button to clear current stuff
            if(!isNew)
                newToolStripButton_Click(sender, e);

            isNew = false;
           
            DialogResult diaRes = openFileDialog1.ShowDialog();
            if (diaRes == DialogResult.OK)
            {
                string file = openFileDialog1.FileName;

                #region XML Loading
                XDocument loadDoc = XDocument.Load(file);
                XNamespace ns = loadDoc.Root.GetDefaultNamespace();

                //running count for generating routers (starts at 1 for default router)
                int routerCount = 1;

                foreach (XElement element in loadDoc.Elements()) //scene element
                {
                    //each router
                    foreach(XElement routerEle in element.Elements())
                    {
                        routerCount++;
                        Router newRouter = new Router();
                        topology.Add(newRouter);
                        newRouter.Index = routerCount;
                        TreeNode newRouterNode = new TreeNode("Router " + routerCount);
                        newRouter.Reference = newRouterNode;
                        topo_treeView.Nodes.Add(newRouterNode);

                        List<SimString> stringList = new List<SimString>();
                        bool exist = false;
                        float xPos;
                        float yPos;
                        UInt32 id;
                        UInt16 id16;
                        byte controllerID;
                        byte lightID;

                        foreach (XElement ele in routerEle.Elements())
                        {
                            if (ele.Name.ToString().Contains("Light"))//if element is a lightpoint
                            {

                                id = (UInt32)Convert.ToInt32(ele.Attribute("id").Value);
                                id16 = (UInt16)(id & 0xFFFF);
                                lightID = (byte)(id16 & 0xFF);
                                controllerID = (byte)(id16 >> 8);

                                xPos = Math.Abs(Convert.ToSingle(ele.Attribute("x").Value) * 1000);
                                yPos = Math.Abs(Convert.ToSingle(ele.Attribute("y").Value) * 1000);

                                foreach (SimString sString in stringList)
                                {
                                    if (sString.ID == controllerID)
                                    { // a string has the id, add to it
                                        exist = true;
                                        sString.AddMass(new SimMass(new Microsoft.Xna.Framework.Vector2(xPos, yPos), lightID));
                                    }
                                }

                                if (!exist)
                                {
                                    SimString newString = new SimString();
                                    newString.ID = controllerID;
                                    newString.AddMass(new SimMass(new Microsoft.Xna.Framework.Vector2(xPos, yPos), lightID));
                                    stringList.Add(newString);
                                }
                                exist = false;
                            }
                        }

                        //add found lightpoints and controllers to the router
                        foreach (SimString sString in stringList)
                        {
                            //create controllers
                            Controller newController = new Controller();
                            TreeNode newControllerNode = new TreeNode("Controller " + newRouter.ControllerIDs);
                            newRouter.ControllerIDs++;
                            newController.IDNum = (ushort)sString.ID;
                            newController.Reference = newControllerNode;
                            newController.Router = newRouter;

                            newRouter.Controllers.Add(newController);
                            newRouterNode.Nodes.Add(newControllerNode);

                            LedString newString = new LedString();
                            newString.Controller = newController;
                            newString.ID = sString.ID;
                            newString.MassList = sString.MassList;
                            newString.NumOfLeds = sString.MassList.Count;
                            newController.Connections.Add(newString);

                            TreeNode newStringNode = new TreeNode("String " + newController.StringCount);
                            newController.StringCount++;

                            newString.Reference = newStringNode;

                            newControllerNode.Nodes.Add(newStringNode);

                        }
                        //add this router's masses to the simulation
                        softBodyControl1.loadMasses(stringList);
                    }
                } //parsing done
                #endregion
            }
        }

        //not implemented
        private void toggleLines_Click_1(object sender, EventArgs e)
        {
            softBodyControl1.toggleLines();
        }

        //save the state and put to undo list. not implemented
        public void saveState()
        {
            //undoStack.Push(ObjectCopier.Clone<SoftBodyControl >(softBodyControl1));
        }

        private void helpToolStripButton_Click(object sender, EventArgs e)
        {
            HelpForm help = new HelpForm();
            help.Show();
        }

        //not implemented
        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Do you want to save your current model?","New File",MessageBoxButtons.YesNoCancel);
            if (dr == DialogResult.Yes)
            {
                saveToolStripButton_Click(sender,e);
            }
            
            if (dr == DialogResult.Yes || dr == DialogResult.No)
            {
                //reset groups
                groups.Clear();
                //remove masses from simulation and xna control
                softBodyControl1.ResetToNew();

                //reset groups to start (only "all leds" group)
                groups.Clear();
                groups_treeView.Nodes.Clear();

                //add "all leds" group
                ObjectGroup newGroup = new ObjectGroup();
                newGroup.Name = "All LEDs";
                newGroup.addList(softBodyControl1.SelectAll());
                groups.Add(newGroup);
                newGroup.Index = groups.IndexOf(newGroup);

                //create new tree node
                TreeNode newNode = new TreeNode();
                newNode.Name = newGroup.Name;
                newNode.Text = newGroup.Name;
                //add tree node to tree view
                groups_treeView.Nodes.Add(newNode);


                //reset topology to start (one empty router)
                topology.Clear();
                topo_treeView.Nodes.Clear();

                TreeNode routerNode = new TreeNode("Router 1");
                Router tempRouter = new Router();
                tempRouter.Index = 1;
                topology.Add(tempRouter);
                tempRouter.Reference = routerNode;
                topo_treeView.Nodes.Add(routerNode);

                isNew = true;
            }
            
        }

        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
            if (split_tools_xna.Panel1Collapsed) split_tools_xna.Panel1Collapsed = false;
            else split_tools_xna.Panel1Collapsed = true;
        }


        private void groups_treeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
           // DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void topo_treeView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void topo_treeView_DragDrop(object sender, DragEventArgs e)
        {
            TreeNode newNode;

            if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false))
            {
                Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
                TreeNode destinationNode = ((TreeView)sender).GetNodeAt(pt);
                newNode = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");
                ///not implemented
               /*
                if (newNode.TreeView == groups_treeView)
                {
                    if (destinationNode == null)
                    {
                        DialogResult dr = MessageBox.Show("Do you want to create a new router?", "", MessageBoxButtons.YesNo);
                        if (dr == DialogResult.Yes)
                        {
                            int number = topology.Count + 1; //get number of new router
                            //create and configure new router, new controller and new string
                            TreeNode newRouterNode = new TreeNode("Router " + number);
                            Router newRouter = new Router();
                            newRouter.Index = number;
                            newRouter.Reference = newRouterNode;
                            topology.Add(newRouter);

                            Controller newController = new Controller();
                            TreeNode newControllerNode = new TreeNode("Controller " + newRouter.ControllerIDs);
                            newRouter.ControllerIDs++;
                            newRouter.Controllers.Add(newController);
                            newController.Reference = newControllerNode;
                            newControllerNode.Nodes.Add((TreeNode)newNode.Clone());
                            newRouterNode.Nodes.Add(newControllerNode);

                            topo_treeView.Nodes.Add(newRouterNode);
                        }
                    }
                    else
                        if (destinationNode.TreeView != newNode.TreeView) //was not dragged from topo - topo
                        {
                            destinationNode.Nodes.Add((TreeNode)newNode.Clone());
                            destinationNode.Expand();

                        }
                }
               
                else
                */
                #region topo_topo

                if (newNode.TreeView == topo_treeView)
                {
                    #region newRouter
                    if (destinationNode == null && newNode.Level == 2)
                    {
                        DialogResult dr = MessageBox.Show("Add the string to a new router?", "", MessageBoxButtons.YesNo);
                        if(dr == DialogResult.Yes)
                        {
                            int number = topology.Count+1;
                            LedString newString = null;
                            //remove old place in topology
                            foreach(Router router in topology)
                            {
                                foreach(Controller controller in router.Controllers){
                                    foreach(LedString ledString in controller.Connections)
                                    {
                                        if(ledString.Reference == newNode)
                                        {
                                            newString = ledString;
                                        }
                                    }
                                }
                            }
                            if(newString!=null)
                            {
                                //remove references of controller to topology
                                Controller toRemove = newString.Controller;
                                Router oldRouter = toRemove.Router;
                                toRemove.Router.Controllers.Remove(newString.Controller);// removes controller from router's list
                                newString.Controller = null;


                                TreeNode newRouterNode = new TreeNode("Router " + number);
                                Router newRouter = new Router();
                                newRouter.Index = number;

                                Controller newController = new Controller();
                                newController.IDNum = newRouter.ControllerIDs;
                                TreeNode newControllerNode = new TreeNode("Controller " + newRouter.ControllerIDs);
                                newRouter.ControllerIDs++;

                                //add to new place in tree and remove old place
                                newControllerNode.Nodes.Add((TreeNode)newNode.Clone());
                                newNode.Remove();
                                newString.Reference = newControllerNode.FirstNode;
                                newRouterNode.Nodes.Add(newControllerNode);
                                topo_treeView.Nodes.Add(newRouterNode);

                                topology.Add(newRouter);

                                //get right references for topology and treeview
                                newRouter.Reference = newRouterNode;
                                newRouter.Controllers.Add(newController);
                                newController.Reference = newControllerNode;
                                newController.Router = newRouter;
                                newController.Connections.Add(newString);
                                newString.Controller = newController;

                                   
                                //remove controller from treeview
                                toRemove.Reference.Remove();

                                //check if empty router is left
                                if (oldRouter.Controllers.Count == 0)
                                {
                                    oldRouter.Reference.Remove();
                                    topology.Remove(oldRouter);
                                }
                                    
                            }
                        }

                    }
                    #endregion
                    else

                        //
                        //is a string being dragged from topo to topo
                        //
                        if(destinationNode!=null)
                        if (destinationNode.TreeView == topo_treeView && newNode.Level == 2) 
                        {
                            LedString newString = null;
                            //remove old place in topology
                            foreach (Router router in topology)
                            {
                                foreach (Controller controller in router.Controllers)
                                {
                                    foreach (LedString ledString in controller.Connections)
                                    {
                                        if (ledString.Reference == newNode)
                                        {
                                            newString = ledString;
                                        }
                                    }
                                }
                            }

                            Controller oldController = newString.Controller;
                            Router oldRouter = oldController.Router;

                            if (destinationNode.Level == 0)//dragged onto router
                            {
                                //find router
                                Router newRouter = null;
                                foreach (Router router in topology)
                                {
                                    if (router.Reference == destinationNode)
                                    {
                                        newRouter = router;
                                    }
                                }
                                if (newRouter != null && oldRouter!=newRouter)
                                {
                                    //get new controller and node, add to topo and treeview
                                    Controller newController = new Controller();
                                    TreeNode newControllerNode = new TreeNode("Controller " + newRouter.ControllerIDs);
                                    newController.Reference = newControllerNode;

                                    newRouter.Controllers.Add(newController);
                                    newController.Router = newRouter;

                                    newController.IDNum = newRouter.ControllerIDs;
                                    newRouter.ControllerIDs++;

                                    newRouter.Reference.Nodes.Add(newControllerNode);

                                    //add node and remove last position
                                    newControllerNode.Nodes.Add((TreeNode)newNode.Clone());
                                    newNode.Remove();

                                    //Remove old string from old place
                                    oldController.Connections.Remove(newString);

                                    //add newstring to new place
                                    newString.Reference = newControllerNode.FirstNode;
                                    newString.Controller = newController;
                                    newController.Connections.Add(newString);

                                    

                                    //check if old controller is empty
                                    if (oldController.Connections.Count == 0)
                                    {
                                        oldController.Reference.Remove();
                                        oldRouter.Controllers.Remove(oldController);
                                        if (oldRouter.Controllers.Count == 0)
                                        {
                                            oldRouter.Reference.Remove();
                                            topology.Remove(oldRouter);
                                        }
                                    }



                                }
                                if (oldRouter == newRouter)
                                {
                                    MessageBox.Show("This string is already on this Router", "Error", MessageBoxButtons.OK);
                                }
                            }
                            if (destinationNode.Level == 1 || destinationNode.Level == 2)//dragged onto controller or string
                            {
                                //
                                //
                                //
                                // implement adding a string to a controller or string
                                //
                                //
                                //
                                //
                            }
                        }
                }

            #endregion
            }
        }

        private void topo_treeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void button_alignTop_Click(object sender, EventArgs e)
        {
            softBodyControl1.AlignTop();
        }

        private void button_alignLeft_Click(object sender, EventArgs e)
        {
            softBodyControl1.AlignLeft();
        }

        private void toggleGrid_Click(object sender, EventArgs e)
        {
            softBodyControl1.toggleGrid();
        }

        public void DeleteSelected()
        {
            List<SimString> stringList = softBodyControl1.FindSelectedStrings();

            foreach (SimString _string in stringList)
            {
                LedString stringToDel = null;
                Controller controllerToDel = null;
                bool delController = false;
                Router routerToDel = null;
                bool delRouter = false;

                foreach (Router router in topology)
                    foreach (Controller controller in router.Controllers)
                        foreach (LedString ledString in controller.Connections)
                            if (ledString.MassList == _string.MassList)
                            {
                                softBodyControl1.RemoveString(_string);
                                stringToDel = ledString;
                                controllerToDel = controller;
                                if (controller.Connections.Count == 1)
                                {
                                    delController = true;
                                    routerToDel = router;
                                    if (router.Controllers.Count == 1 && topology.Count > 1)
                                    {
                                        delRouter = true;
                                    }
                                }
                            }
                if (stringToDel != null)
                {
                    controllerToDel.Connections.Remove(stringToDel);
                    stringToDel.Reference.Remove();
                    if (delController)
                    {
                        routerToDel.Controllers.Remove(controllerToDel);
                        controllerToDel.Reference.Remove();
                        if (delRouter)
                        {
                            routerToDel.Reference.Remove();
                            topology.Remove(routerToDel);
                        }
                    }
                }
            }
        }

        public static bool ApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return false;       // No window is currently activated
            }

            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }
    }
}
