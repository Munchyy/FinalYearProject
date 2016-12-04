using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using SoftBody.SimObjects;

namespace SoftBody.Constraints
{
    public class LengthConstraint:Constraint
    {
        private float length;
        private SimObject simObj1;
        private SimObject simObj2;
        
        public float Length
        {
            get { return length; }
            set { length = value; }
        }

        public LengthConstraint(float length, SimObject simObj1, SimObject simObj2)
        {
            this.length = length;
            this.simObj1 = simObj1;
            this.simObj2 = simObj2;
        }

        Vector2 direction;
        float currentLength;
        Vector2 moveVector;
        public void SatisfyConstraint()
        {
            //calculate direction
            direction = simObj2.CurrPosition - simObj1.CurrPosition;
            //calculate current length
            currentLength = direction.Length();
            //check for zero vector
            if (direction != Vector2.Zero)
            {
                //normalize direction vector
                direction.Normalize();
                //move to goal positions
                moveVector =  (currentLength - length) * direction;
                simObj1.CurrPosition += moveVector;
                simObj2.CurrPosition += -moveVector;
            }
        }

    }
}
