//DERIVED FROM http://cg.skeelogy.com/introduction-to-soft-body-physics-in-xna/using System;
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
using System;

namespace SoftBody.SimObjects
{
        public enum SimObjectType {  PASSIVE, ACTIVE, PASSIVETEMP,GROUPPASSIVE }

        public abstract class SimObject
        {
            private float mass;
            private SimObjectType simObjectType;
            protected Vector2 currPosition;
            protected Vector2 prevPosition;
            protected Vector2 currVelocity;
            protected Vector2 resultantForce;

            public float Mass
            {
                get { return mass; }
                set { mass = value; }
            }

            public SimObjectType SimObjectType
            {
                get { return simObjectType; }
                set { simObjectType = value; }
            }

            public Vector2 CurrPosition
            {
                get { return currPosition; }
                set { currPosition = value; }
            }

            public float CurrPositionX
            {
                get { return currPosition.X; }
                set { currPosition.X = value; }
            }

            public float CurrPositionY
            {
                get { return currPosition.Y; }
                set { currPosition.Y = value; }
            }

            public Vector2 PrevPosition
            {
                get { return prevPosition; }
                set { prevPosition = value; }
            }
            
            public Vector2 CurrVelocity
            {
                get { return currVelocity; }
                set { currVelocity = value; }
            }

            public SimObject(float mass, SimObjectType simObjectType)
            {
                this.mass = mass;
                this.currPosition = Vector2.Zero;
                this.prevPosition = currPosition;
                this.simObjectType = simObjectType;
                this.resultantForce = Vector2.Zero;
            }

            public Vector2 ResultantForce
            {
                get { return resultantForce; }
                set { resultantForce = value; }
            }

            public void ResetForces()
            {
                this.resultantForce = Vector2.Zero;
            }

            public abstract void Update();

            public void Draw(SpriteBatch spriteBatch, Texture2D texture)
            {
                try
                {
                    spriteBatch.Draw(texture, new Vector2(CurrPosition.X-(texture.Width/2),currPosition.Y-(texture.Height/2)), Color.White);
                }
                catch (System.NullReferenceException e) { Console.WriteLine(e.StackTrace); }
            }
        }
}
