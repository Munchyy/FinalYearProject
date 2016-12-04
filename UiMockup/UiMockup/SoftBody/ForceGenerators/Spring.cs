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
    public class Spring:ForceGenerator
    {
        private float stiffness;
        private float damping;
        private float restLength;
        private SimObject simObjectA;
        private SimObject simObjectB;

        public float Stiffness
        {
            get { return stiffness; }
            set { stiffness = value; }
        }

        public float Damping
        {
            get { return damping; }
            set { damping = value; }
        }

        public SimObject SimObjectA
        {
            get { return simObjectA; }
            set { simObjectA = value; }
        }

        public SimObject SimObjectB
        {
            get { return simObjectB; }
            set { simObjectB = value; }
        }

        public Spring(float stiffness, float damping, SimObject simObjectA, SimObject simObjectB)
            : this(stiffness, damping, simObjectA, simObjectB, (simObjectA.CurrPosition - simObjectB.CurrPosition).Length()) { }

        public Spring(float stiffness, float damping, SimObject simObjectA, SimObject simObjectB, float restlength)
            : base()
        {
            this.stiffness = stiffness;
            this.damping = damping;
            this.simObjectA = simObjectA;
            this.simObjectB = simObjectB;
            this.restLength = restlength;
        }

        private Vector2 direction;
        private float currLength;
        private Vector2 force;

        public void ApplyForce(SimObject simObject)
        {
            //get direction vector
            direction = simObjectA.CurrPosition - simObjectB.CurrPosition;

            //check for zero vector
            if (direction != Vector2.Zero)
            {
                //get length
                currLength = direction.Length();

                //normalize
                direction.Normalize();

                //add spring force
                force = -stiffness * ((currLength - restLength) * direction);

                //add spring damping
                force += -damping * Vector2.Dot(simObjectA.CurrVelocity - simObjectB.CurrVelocity,direction) * direction;


                //apply equal and opposite forces
                simObjectA.ResultantForce += force;
                simObjectB.ResultantForce += -force;
            }
        }
    }
}
