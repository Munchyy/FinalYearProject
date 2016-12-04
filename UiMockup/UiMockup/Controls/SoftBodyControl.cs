//DERIVED FROM http://cg.skeelogy.com/introduction-to-soft-body-physics-in-xna/

using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using SoftBody.SimObjects;
using SoftBody.ForceGenerators;
using SoftBody.Integrators;
using SoftBody.Simulations;
using SoftBody.Constraints;
using System.Collections.Generic;
using WinFormsGraphicsDevice;

using System;
using System.Reflection;
using System.ComponentModel;

using C3.XNA;



namespace FinalYearProject.Controls
{
    public class SoftBodyControl:WinFormsGraphicsDevice.GraphicsDeviceControl
    {
        


        #region vars
        Timer timer;
        ServiceContainer services;
        SpriteBatch spriteBatch;
        MainForm parentForm;
        
        //simulation related
        public Simulation springSim;
        Gravity gravity;
        Medium air;
        ControlSide barrier;
        ForwardEulerIntegrator integrator;
        int worldSizeMultiplier = 5;

        //mass related variables
        List<SimString> stringLists;
        Texture2D massTexture;
        Texture2D selectedTexture;
        int numOfMasses;
        float massDistance = 1f;
        float mass = 0.7f;

        public SimMass lastSelectedMass;
        public SimString lastSelectedString;
        SimMass selectedMass;
        SimString selectedString;

        
        //spring related variables
        float stiffness = 20f;
        float damping = 10f;

        //mouse detection variables
        bool rMouseDown;
        bool lMouseDown;
        bool rMouseUp;
        bool lMouseUp;
        Microsoft.Xna.Framework.Input.ButtonState lastLMouse;//last state of mousebuttons
        Microsoft.Xna.Framework.Input.ButtonState lastRMouse;//last state of mousebuttons

        //keyboard detection
        KeyboardState kbState;
        KeyboardState oldKbState;

        //Camera vars
        Camera camera;
        int previousScroll;
        float zoomIncrement;
        Rectangle worldDimensions;

        //view related vars
        private bool lines = true; //select if lines are on/off
        private bool grids = true;
        public float gridSplit = 12.5f;

        //area select vars
        Vector2 startVector;
        Rectangle selectionRectangle;

        //group vars
        List<ObjectGroup> objectGroupList;

        #endregion

        #region properties
        public List<SimString> StringLists
        {
            get { return stringLists; }
        }
        
        public MainForm ParentForm
        {
            get { return parentForm; }
            set { parentForm = value; }
        }

        public int NumOfMasses{
            get { return numOfMasses; }
            set { numOfMasses = value; }
        }
        #endregion

        protected override void Initialize()
        {
            this.Dock = DockStyle.Fill;

            //Init camera and dimensions
            this.worldDimensions = new Rectangle(0,0,this.Width, this.Height);
            previousScroll = Mouse.GetState().ScrollWheelValue;
            zoomIncrement = 0.01f;
            camera = new Camera(GraphicsDevice.Viewport,(int)worldDimensions.Width * worldSizeMultiplier, (int)worldDimensions.Height*worldSizeMultiplier,1f);
            
            numOfMasses = 80;
            
            ContentManager content = new ContentManager(Services);
            content.RootDirectory = "Content";
            massTexture = content.Load<Texture2D>("Image1");
            selectedTexture = content.Load<Texture2D>("selectedRing");
            spriteBatch = new SpriteBatch(GraphicsDevice);
            selectedMass = null;

            lastLMouse = Microsoft.Xna.Framework.Input.ButtonState.Released;
            lastRMouse = Microsoft.Xna.Framework.Input.ButtonState.Released;

            //area selection
            startVector = new Vector2();

            //group list
            objectGroupList = new List<ObjectGroup>();


            // Start the animation timer.
            timer = new Timer();
            timer.Interval = 1;
            timer.Tick += Tick;
            timer.Start();

            //create simulation
            springSim = new Simulation(this);
            springSim.Stiffness = stiffness;
            springSim.Damping = damping;
            springSim.Length = 10f;
            springSim.MassPerString = numOfMasses;
            stringLists = new List<SimString>();
            
            //add forces
            gravity = new Gravity(new Vector2(0f, 9.81f));
            springSim.AddGlobalForceGenerator(gravity);

            air = new Medium(0.5f);
            springSim.AddGlobalForceGenerator(air);

            barrier = new ControlSide(new Rectangle(worldDimensions.X,worldDimensions.Y,
                worldDimensions.Width * worldSizeMultiplier,worldDimensions.Height*worldSizeMultiplier));
            springSim.AddGlobalForceGenerator(barrier);
            
            //add integrator
            integrator = new ForwardEulerIntegrator(this);
            springSim.Integrator = integrator;

            parentForm.saveState();
        }

        //reset to as new state (removes masses and springs)
        public void ResetToNew()
        {
            //remove masses and springs from sim
            springSim.SimObjects.Clear();
            springSim.SpringList.Clear();
            
            //remove masses and springs from control
            stringLists.Clear();
        }

        //load a string passed from the xml
        public void loadMasses(List<SimString> simStrings)
        {
            foreach (SimString sString in simStrings)
            {
                springSim.AddSimString(sString);
            }
            stringLists.AddRange(simStrings);
        }
        
        public void AddNewMasses(Vector2 position)
        {
            //check combo box for number of masses to add
            string val = parentForm.combo_numOfMasses.Text;

           // int num = (int)parentForm.combo_numOfMasses.SelectedItem;
            if (val == "80" || val == "120" || val == "160" || val == "200" || val == "240")
            {
                //initialise mass list
                int num = int.Parse(val);
                List<SimMass> masses = new List<SimMass>();
                masses.Add(new SimMass(mass, position, SimObjectType.PASSIVE));
                for (int i = 0; i < num - 1; i++)
                {
                    masses.Add(new SimMass(mass, new Vector2(position.X, position.Y + (massDistance * (massTexture.Height * (masses.Count))) / 2), SimObjectType.ACTIVE));
                }
                SimString massString = new SimString(masses);
                stringLists.Add(massString);

                parentForm.stringAdded(masses);
                //add masses to simulation
                springSim.AddSimString(massString);
            }
            else
            {
                MessageBox.Show("Please enter a valid number of masses in the combo-box");
            }

        }

        private void Tick(object sender, EventArgs e)
        {
            Invalidate();
        }

        protected override void Draw()
        {

            #region Combo Boxes
            Color backColor = Color.DimGray;
            string backColorVal = parentForm.combo_backColor.Text;
            switch (backColorVal)
            {
                case "White": backColor = Color.White; break;
                case "Black": backColor = Color.Black; break;
                case "Red": backColor = Color.Red; break;
                case "Blue": backColor = Color.Blue; break;
                case "Green": backColor = Color.Green; break;
                case "Light Green": backColor = Color.LightGreen; break;
                case "Pink": backColor = Color.Pink; break;
                case "Cyan": backColor = Color.Cyan; break;
                case "Grey": backColor = Color.DimGray; break;
            }
            GraphicsDevice.Clear(backColor);


            
            string gridSplitVal = parentForm.combo_gridLines.Text;

            switch (gridSplitVal)
            {
                case "12.5": gridSplit = 12.5f; break;
                case "25": gridSplit = 25; break;
                case "50": gridSplit = 50; break;
                case "100": gridSplit = 100; break;
                case "200": gridSplit = 200; break;
            }


            Color gridColor = Color.White;
            string gridColorVal = parentForm.combo_gridColor.Text;
            switch (gridColorVal)
            {
                case "White": gridColor = Color.White ; break;
                case "Black": gridColor = Color.Black ; break;
                case "Red": gridColor = Color.Red ; break;
                case "Blue": gridColor = Color.Blue ; break;
                case "Green": gridColor = Color.Green ; break;
                case "Light Green": gridColor = Color.LightGreen ; break;
                case "Pink": gridColor = Color.Pink ; break;
                case "Cyan": gridColor = Color.Cyan ; break;
            }
            #endregion

            if (grids)
            {
                float gridSplitDist =  ((worldDimensions.Width * worldSizeMultiplier)/ gridSplit);
                for (float i = 0f; i < worldDimensions.Width * worldSizeMultiplier; i += gridSplitDist)
                {
                    spriteBatch.Begin(SpriteSortMode.BackToFront,
                        null, null, null, null, null,
                        camera.GetTransformation());
                    C3.XNA.Primitives2D.DrawLine(spriteBatch, new Vector2(i, 0),
                        new Vector2(i, worldDimensions.Height * worldSizeMultiplier), gridColor,2f);
                    spriteBatch.End();
                }

                
                for (float i = 0; i < worldDimensions.Height * worldSizeMultiplier; i += gridSplitDist)
                {
                    spriteBatch.Begin(SpriteSortMode.BackToFront,
                        null, null, null, null, null,
                        camera.GetTransformation());
                    C3.XNA.Primitives2D.DrawLine(spriteBatch, new Vector2(0, i), new Vector2(worldDimensions.Width * worldSizeMultiplier, i), gridColor, 2f);
                    spriteBatch.End();
                }
            }

            List<List<SimMass>> simObjects = springSim.SimObjects;

            foreach (List<SimMass> objList in simObjects) {
                foreach (SimMass simObject in objList)
                {
                    spriteBatch.Begin(SpriteSortMode.BackToFront,
                    null, null, null, null, null,
                    camera.GetTransformation());
                    simObject.Draw(spriteBatch, massTexture);
                    if (simObject.Selected)
                        simObject.Draw(spriteBatch, selectedTexture);
                    spriteBatch.End();
                }

                //Draw lines between each led point
                if(lines)
                for (int i = 0; i < objList.Count - 1; i++)
                {
                    spriteBatch.Begin(SpriteSortMode.BackToFront,
                    null, null, null, null, null,
                    camera.GetTransformation());
                    spriteBatch.DrawLine(objList.ElementAt(i).CurrPosition, objList.ElementAt(i + 1).CurrPosition, Color.Black);
                    spriteBatch.End();
                }
            }   

            int speed = 3;
            for (int i = 0; i < speed;i++ )
                UpdateMe();
        }

        //update logic; pan and zoom
        protected void UpdateMe()
        {
            springSim.Update();
            HandleInput();

            #region ZoomControl
            // Adjust zoom if the mouse wheel has moved
            MouseState mouseStateCurrent = Mouse.GetState();
            if (mouseStateCurrent.ScrollWheelValue > previousScroll)
                camera.Zoom += zoomIncrement;
            else if (mouseStateCurrent.ScrollWheelValue < previousScroll)
                camera.Zoom -= zoomIncrement;

            previousScroll = mouseStateCurrent.ScrollWheelValue;
            kbState = Keyboard.GetState();
            if (kbState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W))
                camera.Zoom += zoomIncrement;
            if (kbState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
                camera.Zoom -= zoomIncrement;

            #endregion

            #region MoveControl
            // Move the camera when the arrow keys are pressed
            Vector2 movement = Vector2.Zero;
            Viewport vp = GraphicsDevice.Viewport;
            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
                movement.X--;
            if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
                movement.X++;
            if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
                movement.Y--;
            if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
                movement.Y++;
            
            camera.Pos += movement * 10;
            #endregion

            base.Update();

        }


        //any controls other than pan and zoom
        private void HandleInput()
        {
            if (MainForm.ApplicationIsActivated())
            {
                MouseState mouseState = Mouse.GetState();
                //DETECT MOUSE UP AND DOWN
                rMouseDown = false;
                rMouseUp = false;
                lMouseDown = false;
                lMouseUp = false;

                bool spaceDown = false;
                bool delDown = false;
                bool dDown = false;


                #region button up/down checks;

                if (kbState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D) &&
                        oldKbState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.D))
                    dDown = true;
                else
                    dDown = false;

                if (kbState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space) &&
                        oldKbState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.Space))
                    spaceDown = true;
                else
                    spaceDown = false;

                if (kbState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Delete) &&
                        oldKbState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.Delete))
                    delDown = true;
                else
                    delDown = false;

                if (lastRMouse != mouseState.RightButton)
                {
                    if (lastRMouse == Microsoft.Xna.Framework.Input.ButtonState.Released)
                        rMouseDown = true;
                    if (lastRMouse == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                        rMouseUp = true;
                }

                if (lastLMouse != mouseState.LeftButton)
                {
                    if (lastLMouse == Microsoft.Xna.Framework.Input.ButtonState.Released)
                        lMouseDown = true;
                    if (lastLMouse == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                        lMouseUp = true;
                }
                #endregion

                #region mouse input handling

                if (ClientRectangle.Contains(PointToClient(Control.MousePosition))) //if mouse position is over the control
                {
                    if (!this.Focused && mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                        this.Focus(); //if not in focus, and left click is on control: give controlfocus
                    else //control is in focus
                    {
                        if (delDown)
                        {
                            parentForm.DeleteSelected();
                        }

                        if (parentForm.selectedToolTreeNode.Index == 0)//led is selected
                        {
                            #region ledMode

                            if (lMouseDown)
                            {
                                AddNewMasses(new Vector2(GetMousePos().X, GetMousePos().Y));
                            }

                            #endregion
                        }

                        else if (parentForm.selectedToolTreeNode.Index == 1) //select is selected
                        {
                            #region selectMode

                            bool offMass = false;
                            if (lMouseDown && selectedMass != null)
                            {
                                foreach (SimString massString in stringLists)
                                    foreach (SimMass mass in massString.MassList)
                                        if (!(GetMousePos().X > mass.CurrPositionX - (massTexture.Width / 2) - 5f
                                            && GetMousePos().X < mass.CurrPositionX + (massTexture.Width / 2) + 5f
                                                && GetMousePos().Y > mass.CurrPositionY - (massTexture.Height / 2) - 5f
                                                    && GetMousePos().Y < mass.CurrPositionY + (massTexture.Height / 2) + 5f)) // deteect of mouse down over a mass
                                        {
                                            offMass = true;
                                        }
                                if (offMass) //click was not on a mass
                                {
                                    if (selectedMass.SimObjectType == SimObjectType.PASSIVETEMP)
                                        selectedMass.SimObjectType = SimObjectType.ACTIVE;
                                    ClearSelection();
                                    offMass = false;

                                }


                            }

                            if (lMouseDown && selectedMass == null)
                            {
                                SimMass newMass = null;
                                SimString newString = null;
                                foreach (SimString massString in stringLists)
                                    foreach (SimMass mass in massString.MassList)
                                        if (GetMousePos().X > mass.CurrPositionX - (massTexture.Width / 2) - 5f
                                            && GetMousePos().X < mass.CurrPositionX + (massTexture.Width / 2) + 5f
                                                && GetMousePos().Y > mass.CurrPositionY - (massTexture.Height / 2) - 5f
                                                    && GetMousePos().Y < mass.CurrPositionY + (massTexture.Height / 2) + 5f) // deteect of mouse down over a mass
                                        {
                                            //mouse click was on a mass
                                            newMass = mass;
                                            newString = massString;
                                        }


                                if (newMass != null && newString != null)
                                {
                                    MakeSelection(newMass, newString);
                                }
                            }


                            if (dDown)
                            {
                                if (selectedMass != null)
                                {
                                    if (selectedMass.SimObjectType == SimObjectType.PASSIVETEMP)
                                        selectedMass.SimObjectType = SimObjectType.ACTIVE;
                                }
                                ClearSelection();

                                parentForm.saveState();
                            }

                            if ((spaceDown || rMouseDown) && selectedMass != null)
                            {
                                if (selectedMass.SimObjectType == SimObjectType.PASSIVETEMP) //toggle (temp)passive/active
                                    selectedMass.SimObjectType = SimObjectType.PASSIVE;
                                else
                                    selectedMass.SimObjectType = SimObjectType.PASSIVETEMP;

                                parentForm.saveState();
                            }


                            if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                            {
                                if (selectedMass != null) //if left mouse held and mass is selected, mass is being moved
                                {
                                    selectedMass.CurrPosition = GetMousePos();

                                    if (selectedMass.SimObjectType == SimObjectType.ACTIVE)
                                        selectedMass.SimObjectType = SimObjectType.PASSIVETEMP;


                                }
                            }
                            #endregion
                        }

                        else if (parentForm.selectedToolTreeNode.Index == 2) //rectangle select is selected
                        {
                            #region rectSelectMode

                            if (lMouseDown)
                                startVector = new Vector2(this.PointToClient(Control.MousePosition).X, this.PointToClient(Control.MousePosition).Y);

                            if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                            {
                                Vector2 endVector = new Vector2(this.PointToClient(Control.MousePosition).X, this.PointToClient(Control.MousePosition).Y);
                                int rectWidth = (int)(this.PointToClient(Control.MousePosition).X - startVector.X);
                                int rectHeight = (int)(this.PointToClient(Control.MousePosition).Y - startVector.Y);

                                Vector2 startVectorTrans = TransformPos(startVector);
                                Vector2 endVectorTrans = TransformPos(endVector);
                                Vector2 rectVector;
                                if (endVectorTrans.Length() > startVectorTrans.Length())
                                {
                                    rectVector = endVectorTrans - startVectorTrans;
                                }
                                else
                                    rectVector = startVectorTrans - endVectorTrans;

                                selectionRectangle = new Rectangle((int)startVectorTrans.X, (int)startVectorTrans.Y, (int)rectVector.X, (int)rectVector.Y);
                                spriteBatch.Begin();

                                spriteBatch.DrawRectangle(new Rectangle((int)startVector.X, (int)startVector.Y, rectWidth, rectHeight), Color.Black);

                                spriteBatch.End();
                            }

                            if (lMouseUp)
                            {
                                //check if any masses are inside rectangle
                                ObjectGroup currentGroup = new ObjectGroup();
                                foreach (SimString massString in stringLists)
                                {
                                    foreach (SimMass mass in massString.MassList)
                                    {
                                        if (selectionRectangle.Contains(new Point((int)mass.CurrPositionX, (int)mass.CurrPositionY)))
                                        {
                                            mass.Selected = true;
                                            foreach (SimMass sMass in massString.MassList)
                                            {
                                                sMass.Selected = true;
                                            }
                                        }
                                    }
                                }
                            }

                            if (dDown)
                            {
                                foreach (SimString massString in stringLists)
                                    foreach (SimMass mass in massString.MassList)
                                        mass.Selected = false;
                            }

                            if (spaceDown || rMouseDown)
                            {
                                foreach (SimString massString in stringLists)
                                    foreach (SimMass mass in massString.MassList)
                                    {
                                        if (mass.Selected)
                                            if (mass.SimObjectType == SimObjectType.ACTIVE || mass.SimObjectType == SimObjectType.PASSIVETEMP)
                                            {
                                                mass.SimObjectType = SimObjectType.GROUPPASSIVE;
                                            }
                                            else if (mass.SimObjectType == SimObjectType.GROUPPASSIVE)
                                            {
                                                mass.SimObjectType = SimObjectType.ACTIVE;
                                            }
                                    }
                            }
                            #endregion
                        }
                    }

                }
                #endregion

                oldKbState = kbState;
                lastLMouse = mouseState.LeftButton;
                lastRMouse = mouseState.RightButton;
                spaceDown = false;
            }
        }

        private void MakeSelection(SimMass mass,SimString massString)
        {
            selectedMass = mass;
            selectedMass.Selected = true;
            selectedString = massString;
            lastSelectedMass = mass; //for use in parent form
            lastSelectedString = massString; //for use in parent form
            parentForm.setPropertyView(selectedString);
        }
        
        //deselects all selected masses
        public void ClearSelection()
        {
            if (selectedMass != null)
            {
                selectedMass.Selected = false;
                selectedMass = null;

                selectedString = null;
            }
            foreach (SimString massString in stringLists)
                foreach (SimMass mass in massString.MassList)
                    if (mass.Selected)
                    {
                        mass.Selected = false;
                    }
        }

        private Vector2 GetMousePos()
        {
            System.Drawing.Point point = this.PointToClient(Control.MousePosition);
            Vector2 mouse = new Vector2(point.X,point.Y);
            Matrix inverse = Matrix.Invert(camera.GetTransformation());
            Vector2 mousePos = Vector2.Transform(mouse, inverse);
            return mousePos;
        }
        private Vector2 TransformPos(Vector2 oldPos)
        {
            Matrix inverse = Matrix.Invert(camera.GetTransformation());
            Vector2 newPos = Vector2.Transform(oldPos, inverse);
            return newPos;
        }

        public List<SimMass> FindSelected()
        {
            List<SimMass> returnList = new List<SimMass>();
            foreach (SimString massString in stringLists)
                foreach (SimMass mass in massString.MassList)
                    if (mass.Selected)
                    {
                        returnList.Add(mass);
                    }
            return returnList;
        }

        public List<SimString> FindSelectedStrings()
        {
            List<SimString> returnList = new List<SimString>();
            foreach (SimString massString in stringLists)
            {
                foreach (SimMass mass in massString.MassList)
                {
                    if (mass.Selected)
                    {
                        returnList.Add(massString);
                        break;
                    }
                }
            }
            return returnList;
        }

        public List<SimMass> SelectAll()
        {
            List<SimMass> returnList = new List<SimMass>();
            if (stringLists != null)
            {
                foreach (SimString massString in stringLists)
                    foreach (SimMass mass in massString.MassList)
                    {
                        returnList.Add(mass);
                    }
            }
            return returnList;
        }

        public void toggleLines(){
            if (lines)
                lines = false;
            else lines = true;
        }

        public void toggleGrid()
        {
            if (grids)
                grids = false;
            else grids = true;
        }

        public void AlignTop()
        {
            //find all selected strings
            List<SimString> strings = new List<SimString>();
            foreach (SimString massString in stringLists)
            {
                if (massString.MassList.ElementAt(0).Selected)
                {
                    strings.Add(massString);
                }
            }
            if (strings.Count != 0)
            {
                //find leftmost first mass string
                SimString leftString = strings.ElementAt(0);
                foreach (SimString massString in strings)
                {
                    if (massString.MassList.ElementAt(0).CurrPositionX < leftString.MassList.ElementAt(0).CurrPositionX)
                    {
                        leftString = massString;
                    }
                }

                //align the strings to match the topstring
                foreach (SimString massString in strings)
                {
                    if (massString != leftString)
                    {
                        float difference = massString.FirstMassLocationY - leftString.FirstMassLocationY;
                        foreach (SimMass mass in massString.MassList)
                        {
                            mass.CurrPositionY = mass.CurrPositionY - difference;
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Could not align, no full strings selected.\nPlease select strings using rectangle select and try again","Align Error");
            }
        }

        public void AlignLeft()
        {
            //find all selected strings
            List<SimString> strings = new List<SimString>();
            foreach (SimString massString in stringLists)
            {
                if (massString.MassList.ElementAt(0).Selected)
                {
                    strings.Add(massString);
                }
            }
            if (strings.Count != 0)
            {
                //find leftmost first mass string
                SimString leftString = strings.ElementAt(0);
                foreach (SimString massString in strings)
                {
                    if (massString.MassList.ElementAt(0).CurrPositionX < leftString.MassList.ElementAt(0).CurrPositionX)
                    {
                        leftString = massString;
                    }
                }

                //align the strings to match the topstring
                foreach (SimString massString in strings)
                {
                    if (massString != leftString)
                    {
                        float difference = massString.FirstMassLocationX - leftString.FirstMassLocationX;
                        foreach (SimMass mass in massString.MassList)
                        {
                            mass.CurrPositionX = mass.CurrPositionX - difference;
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Could not align, no full strings selected.\nPlease select strings using rectangle select and try again", "Align Error");
            }
        }

        public void RemoveString(SimString simString)
        {
            List<List<Spring>> springsToDelete = new List<List<Spring>>();
            foreach (List<Spring> springs in springSim.SpringList)
            {
                if (springs.ElementAt(0).SimObjectA == simString.MassList.ElementAt(0))
                {
                    springsToDelete.Add(springs);
                }
            }

            foreach (List<Spring> springs in springsToDelete)
            {
                springSim.SpringList.Remove(springs);
            }

            springSim.SimObjects.Remove(simString.MassList);



        }
    
    }
}
