//DERIVED FROM http://cg.skeelogy.com/introduction-to-soft-body-physics-in-xna/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace SoftBody.SimObjects
{
    public class SimMass:SimObject
    {
        private int massID;
        public int MassID
        {
            get { return massID; }
            set { massID = value; }
        }

        private Texture2D massTexture;
        public Texture2D MassTexture
        {
            get { return massTexture; }
            set { massTexture = value; }
        }

        private Color ledColor;
        public Color LedColor
        {
            get { return ledColor; }
            set { ledColor = value; }
        }

        private bool selected;
        public bool Selected
        {
            get { return selected; }
            set { selected = value; }
        }

        public SimMass( float mass, Vector2 position, SimObjectType simObjectType):base(mass,simObjectType)
        {
            this.prevPosition = position;
            this.currPosition = position;
        }

        //for use in open-file function
        public SimMass(Vector2 position, int id):base(10f,SimObjectType.PASSIVE)
        {
            this.prevPosition = position;
            this.currPosition = position;
            this.massID = id;
        }

        public override void Update()
        {
            if (this.SimObjectType != SimObjectType.ACTIVE)
            {
                ResetForces();
                currVelocity = new Vector2(0f, 0f);
            }
                
            currPosition += currVelocity;
        }


    }
}
