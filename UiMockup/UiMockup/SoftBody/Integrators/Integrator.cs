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
    public abstract class Integrator
    {
        private GraphicsDeviceControl control;

        protected float fixedTimeStep;

        public float FixedTimeStep
        {
            get { return fixedTimeStep; }
            set { fixedTimeStep = value;  }
        }

        public Integrator(GraphicsDeviceControl control)
        {
            this.control = control;

            //fixedTimeStep = (float)game.TargetElapsedTime.TotalSeconds;
            fixedTimeStep = 0.01f;
        }

        public abstract void Integrate(Vector2 acceleration, SimObject simObject);
    }
}
