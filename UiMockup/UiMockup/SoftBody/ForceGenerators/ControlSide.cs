using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SoftBody.SimObjects;

namespace SoftBody.ForceGenerators
{
    public class ControlSide:ForceGenerator
    {
        Rectangle dimensions;
        public ControlSide(Rectangle rect)
        {
            this.dimensions = rect;
        }

        public void ApplyForce(SimObject simObj)
        {
            float damping = 0f;
            float dampingDistance = 20f;
            Vector2 v2 = simObj.ResultantForce;

            //check bottom
            if (simObj.CurrPositionY >= dimensions.Bottom && v2.Y > 0)
            {                
                v2.Y *= damping;
                simObj.CurrPositionY -= dampingDistance;
            }
            
            //check left right
            if (simObj.CurrPositionY >= dimensions.Right && v2.X > 0)
            {
                v2.X *= damping;
                simObj.CurrPositionX -= dampingDistance;
            }
            else if (simObj.CurrPositionX <= dimensions.Left && v2.X < 0)
            {
                v2.X *= damping;
                simObj.CurrPositionX += dampingDistance;
            }

            simObj.ResultantForce = v2;
        }
    }
}
