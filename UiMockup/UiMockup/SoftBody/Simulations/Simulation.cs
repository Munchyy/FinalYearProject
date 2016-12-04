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
using SoftBody.ForceGenerators;
using SoftBody.Integrators;
using SoftBody.SimObjects;
using SoftBody.Constraints;
using FinalYearProject.Controls;
using WinFormsGraphicsDevice;

namespace SoftBody.Simulations
{
    public class Simulation
    {
        private SoftBodyControl control;
        private List<List<SimMass>> simObjects;
        protected List<ForceGenerator> globalForceGenerators;
        protected List<List<Spring>> springList;
        protected List<List<LengthConstraint>> constraintsList;
        protected Integrator integrator;
        private float stiffness;
        public float Stiffness{ set { stiffness = value; } }
        private float damping;
        public float Damping { set { damping = value; } }
        private float length;
        public float Length { set { length = value; } }
        private int massPerString;
        public int MassPerString { set { massPerString = value; } }

        public List<List<Spring>> SpringList
        {
            get { return springList; }
            set { springList = value; }
        }

        public List<List<SimMass>> SimObjects
        {
            get { return simObjects; }
            set { simObjects = value; }
        }

        public Integrator Integrator
        {
            get { return integrator; }
            set { integrator = value; }
        }


        public Simulation(SoftBodyControl control)
        {
            this.control = control;
            integrator = new ForwardEulerIntegrator(control);
            simObjects = new List<List<SimMass>>();
            globalForceGenerators = new List<ForceGenerator>();
            springList = new List<List<Spring>>();
            constraintsList = new List<List<LengthConstraint>>();
        }


        public void AddSimString(SimString list)
        {
            simObjects.Add(list.MassList);
             //add springs and constraints
            List<Spring> springs = new List<Spring>();
            List<LengthConstraint> constraints = new List<LengthConstraint>();
            for (int i = 0; i < list.NumOfMasses-1; i++)
            {                 
             //   constraints.Add(new LengthConstraint(length, list.MassList.ElementAt(i), list.MassList.ElementAt(i + 1)));
                springs.Add(new Spring(stiffness, damping, list.MassList.ElementAt(i), list.MassList.ElementAt(i + 1)));
            }
            springList.Add(springs);
            //constraintsList.Add(constraints);
        }

        public void RemoveLastSimObject()
        {
            simObjects.RemoveAt(simObjects.Count-1);
        }

        public void AddGlobalForceGenerator(ForceGenerator forceGenerator)
        {
            globalForceGenerators.Add(forceGenerator);
        }


        Vector2 acceleration;

        public virtual void Update()
        {
            //sum spring forces
            foreach(List<Spring> sprList in springList)
                foreach(Spring spring in sprList){
                    spring.ApplyForce(null);
                }

            foreach (List<SimMass> objList in simObjects){
                //sum global forces on all objects
                foreach (SimMass simObject in objList)
                {
                    if (simObject.SimObjectType == SimObjectType.ACTIVE)
                    {
                        foreach (ForceGenerator forceGenerator in globalForceGenerators)
                        {
                            forceGenerator.ApplyForce(simObject);
                        }
                    }
                }

                foreach (SimMass simObject in objList)
                {
                    if (simObject.SimObjectType == SimObjectType.ACTIVE)
                    {
                        //find acceleration
                        acceleration = simObject.ResultantForce / simObject.Mass;

                        //integrate
                        integrator.Integrate(acceleration, simObject);
                    }
                }

                //do constraint calculations
                int constraintInterations = 1;

                for (int i = 0; i < constraintInterations; i++)
                {
                    foreach (List<LengthConstraint> constraints in constraintsList)
                        foreach (LengthConstraint constraint in constraints)
                        {
                            constraint.SatisfyConstraint();
                        }
                }

                    //update objects
                    foreach (SimMass simObject in objList)
                    {
                        simObject.Update();
                    }

                //reset forces on each object
                foreach (SimMass simObject in objList)
                {
                    simObject.ResetForces();
                }
            }
        }
    }
}
