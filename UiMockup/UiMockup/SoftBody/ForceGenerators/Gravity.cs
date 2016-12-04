
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
using SoftBody.SimObjects;

namespace SoftBody.ForceGenerators
{
    public class Gravity:ForceGenerator
    {
        private Vector2 acceleration;

        public Vector2 Acceleration
        {
            get { return acceleration; }
            set { acceleration = value; }
        }

        public float AccelerationX
        {
            get { return acceleration.X; }
            set { acceleration.X = value; }
        }

        public float AccelerationY
        {
            get { return acceleration.Y; }
            set { acceleration.Y = value; }
        }

        public Gravity() 
            : this(new Vector2(0, 9.81f)) { }

        public Gravity(Vector2 acceleration)
            : base()
        {
            this.acceleration = acceleration;
        }

        public void ApplyForce(SimObject simObject)
        {
            simObject.ResultantForce += simObject.Mass * acceleration;
        }
    }
}
