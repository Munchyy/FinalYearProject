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
using FinalYearProject.Controls;
using WinFormsGraphicsDevice;

namespace SoftBody.Integrators
{
    public class ForwardEulerIntegrator:Integrator
    {
        public ForwardEulerIntegrator(SoftBodyControl control)
            : base(control) { }

        public override void Integrate(Vector2 acceleration, SimObject simObject)
        {
            //calculate new position
            simObject.CurrPosition += simObject.CurrVelocity * fixedTimeStep;

            //calculate new velocity
            simObject.CurrVelocity += acceleration * fixedTimeStep;
        }
    }
}
