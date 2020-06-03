using _02285_Programming_Project.AI;
using _02285_Programming_Project.Entities;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;

namespace _02285_Programming_Project.Planning
{
    class ConstraintState : FastPriorityQueueNode
    {
        public HashSet<Constraint> constraints;
        public List<List<WorldState>> solution;
        public int totalCost;

        public ConstraintState(ConstraintState parent, Constraint constraint)
        {
            //We don't need to create a deep copy of parent constraints since we never change a constraint (only add new ones)
            this.constraints = new HashSet<Constraint>(parent.constraints);
            this.constraints.Add(constraint);
            this.solution = new List<List<WorldState>>();

            foreach (List<WorldState> plan in parent.solution)
            {
                List<WorldState> newPlan = new List<WorldState>();
                int i = -1;
                foreach (WorldState state in plan)
                {
                    if(newPlan.Count < 1)
                    {
                        newPlan.Add(state.Clone(null));
                    }
                    else
                    {
                        newPlan.Add(state.Clone(newPlan.ElementAt(i)));
                    }
                    i++;
                }
                this.solution.Add(newPlan);
            }
        }

        public ConstraintState()
        {
            this.constraints = new HashSet<Constraint>();
            this.solution = new List<List<WorldState>>();
            this.totalCost = 0;
        }

        public (List<Agent>, int, Location) Validate()
        {
            int longestPlanLength = 0;
            foreach(List<WorldState> plan in solution)
            {
                if (plan.Count > longestPlanLength) longestPlanLength = plan.Count;
            }

            for(int currentTime = 1; currentTime < longestPlanLength; currentTime++)
            {
                for (int outerPlanIndex = 0; outerPlanIndex < this.solution.Count; outerPlanIndex++)
                {
                    for (int innerPlanIndex = 0; innerPlanIndex < this.solution.Count; innerPlanIndex++)
                    {
                        if (outerPlanIndex == innerPlanIndex) continue;

                        WorldState outerState;
                        if (currentTime >= solution[outerPlanIndex].Count)
                        {
                            outerState = solution[outerPlanIndex][solution[outerPlanIndex].Count - 1];
                        }
                        else
                        {
                            outerState = solution[outerPlanIndex][currentTime];
                        }

                        WorldState innerStateNow;
                        WorldState innerStateOld;
                        if (currentTime >= solution[innerPlanIndex].Count)
                        {
                            innerStateNow = solution[innerPlanIndex][solution[innerPlanIndex].Count -1];
                            innerStateOld = innerStateNow;
                        }
                        else
                        {
                            innerStateNow = solution[innerPlanIndex][currentTime];
                            innerStateOld = solution[innerPlanIndex][currentTime - 1];
                        }

                        if(outerState.agentLocation.Equals(innerStateNow.agentLocation) || outerState.agentLocation.Equals(innerStateOld.agentLocation))
                        {
                            return FindMatchingConflicts(currentTime, outerState.agentLocation);
                        }

                        if(innerStateNow.assignedBoxes.ContainsKey(outerState.agentLocation))
                        {
                            return FindMatchingConflicts(currentTime, outerState.agentLocation);
                        }

                        if (innerStateOld.assignedBoxes.ContainsKey(outerState.agentLocation))
                        {
                            return FindMatchingConflicts(currentTime, outerState.agentLocation);
                        }

                        foreach (Location outerBoxLocation in outerState.assignedBoxes.Keys)
                        {
                            if (innerStateNow.agentLocation.Equals(outerBoxLocation))
                            {
                                return FindMatchingConflicts(currentTime, outerBoxLocation);
                            }
                            
                            if (innerStateOld.agentLocation.Equals(outerBoxLocation))
                            {
                                return FindMatchingConflicts(currentTime, outerBoxLocation);
                            }
                            
                            if (innerStateNow.assignedBoxes.ContainsKey(outerBoxLocation))
                            {
                                return FindMatchingConflicts(currentTime, outerBoxLocation);
                            }

                            if (innerStateOld.assignedBoxes.ContainsKey(outerBoxLocation))
                            {
                                return FindMatchingConflicts(currentTime, outerBoxLocation);
                            }
                        }
                    }
                }
            }
            return (new List<Agent>(), 0, new Location(0, 0));
        }

        private (List<Agent>, int, Location) FindMatchingConflicts(int timeStep, Location location)
        {
            List<Agent> agentsInConflicts = new List<Agent>();
            foreach(List<WorldState> plan in solution)
            {
                if(plan.Count <= timeStep)
                {
                    if (plan[plan.Count - 1].agentLocation.Equals(location) || plan[plan.Count - 1].assignedBoxes.ContainsKey(location))
                    {
                        agentsInConflicts.Add(plan[plan.Count - 1].agent);
                    }

                }
                else
                {
                    if (plan[timeStep].agentLocation.Equals(location) || plan[timeStep].assignedBoxes.ContainsKey(location))
                    {
                        agentsInConflicts.Add(plan[timeStep].agent);
                    }
                }  
            }
            return (agentsInConflicts, timeStep, location);
        }

        public class Constraint : IEquatable<Constraint>, ICloneable
        {
            public int timeStamp;
            public Location constraintLocation;
            public Agent agentUnderConstraint;

            public Constraint(int timeStamp, Location constraintLocation, Agent agentUnderConstraint)
            {
                this.timeStamp = timeStamp;
                this.constraintLocation = new Location(constraintLocation);
                this.agentUnderConstraint = agentUnderConstraint;
            }


            #region Hash and comparison tools
            public override int GetHashCode()
            {
                return HashCode.Combine(this.timeStamp, this.constraintLocation, this.agentUnderConstraint);
            }

            public bool Equals([AllowNull] Constraint other)
            {
                return other != null &&
                    this.timeStamp == other.timeStamp && this.constraintLocation.Equals(other.constraintLocation) &&
                    this.agentUnderConstraint.Equals(other.agentUnderConstraint);

            }

            public override bool Equals(object obj)
            {
                return Equals(obj as Constraint);
            }

            public object Clone()
            {
                return new Constraint(this.timeStamp, (Location)this.constraintLocation.Clone(), this.agentUnderConstraint);
            }
            #endregion
        }
    }
}
