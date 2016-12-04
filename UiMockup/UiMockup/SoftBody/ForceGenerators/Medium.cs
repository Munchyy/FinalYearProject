//DERIVED FROM http://cg.skeelogy.com/introduction-to-soft-body-physics-in-xna/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SoftBody.SimObjects;

namespace SoftBody.ForceGenerators
{
    class Medium:ForceGenerator
    {
        private float dragCoefficient;

        public float DragCoefficient
        {
            get { return dragCoefficient; }
            set { dragCoefficient = value; }
        }

        public Medium(float dragCoefficient)
            :base()
        {
            this.dragCoefficient = dragCoefficient;
        }

        public void ApplyForce(SimObject simObject)
        {
            simObject.ResultantForce += -dragCoefficient * simObject.CurrVelocity;
        }
    }
}
