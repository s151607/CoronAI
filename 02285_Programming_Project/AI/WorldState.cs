using System;
using System.Collections.Generic;
using _02285_Programming_Project.Actions;
using _02285_Programming_Project.Entities;
using System.Text;
using Priority_Queue;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Action = _02285_Programming_Project.Actions.Action;
using static _02285_Programming_Project.Planning.ConstraintState;

namespace _02285_Programming_Project.AI
{
    /// <summary>
    /// The current state of the game.
    /// An example of a world state is the game's initial state. 
    /// A goal state is a simplified world state.
    /// </summary>
    class WorldState : FastPriorityQueueNode, IComparable<WorldState>, IEquatable<WorldState>
    {
        public int timeStep;
        public Agent agent { get; set; }
        public Location agentLocation { get; set; }
        public Location agentGoal { get; set; }
        public bool isAgentGoal { get; set; }
        public Dictionary<Location, Box> assignedBoxes { get; set; }
        public HashSet<Location> Walls { get; set; }
        public float H { get; set; }
        public int G { get; set; }
        public WorldState Parent { get; set; }
        public (Action.Actions, Action.Directions, Action.Directions) ActionToGetHere { get; set; } //Action, agentDir, boxDir
        public List<EntityLocation> boxGoals { get; set; }

        public WorldState()
        {

        }
        public WorldState(Dictionary<Location, Box> boxes, WorldState parent)
        {
            this.timeStep = parent.timeStep + 1;
            this.Parent = parent;
            this.Walls = Parent.Walls;
            this.agentLocation = new Location(Parent.agentLocation);
            this.assignedBoxes = boxes;
            this.isAgentGoal = parent.isAgentGoal;
            this.boxGoals = parent.boxGoals;
            this.agent = parent.agent;
            this.agentGoal = parent.agentGoal;

        }
        public WorldState(Location agentLocation, Location agentGoal, Dictionary<Location, Box> boxes, List<EntityLocation> boxGoals, HashSet<Location> walls, Agent agent)
        {
            this.agent = agent;
            this.timeStep = 0;
            this.agentLocation = agentLocation;
            this.agentGoal = agentGoal;
            this.assignedBoxes = boxes;
            this.Walls = walls;
            this.Parent = null;
            this.boxGoals = boxGoals;

            if (agentGoal == null) this.isAgentGoal = false;
            else this.isAgentGoal = true;
        }

        public WorldState Clone(WorldState parent)
        {
            WorldState newWorldState = new WorldState();
            newWorldState.timeStep = this.timeStep;
            newWorldState.agent = this.agent;
            newWorldState.agentLocation = new Location(this.agentLocation);
            newWorldState.agentGoal = this.agentGoal;
            newWorldState.isAgentGoal = this.isAgentGoal;

            var a = this.assignedBoxes.Keys;
            var b = this.assignedBoxes.Values;
            Dictionary<Location, Box> newAssignedBoxes = new Dictionary<Location, Box>();
            for (int i = 0; i < a.Count; i++)
            {
                newAssignedBoxes.Add(new Location(a.ElementAt(i)), b.ElementAt(i));
            }
            newWorldState.assignedBoxes = newAssignedBoxes;

            newWorldState.Walls = this.Walls;
            newWorldState.H = this.H;
            newWorldState.G = this.G;
            newWorldState.Parent = parent;
            newWorldState.ActionToGetHere = this.ActionToGetHere;
            newWorldState.boxGoals = this.boxGoals;

            return newWorldState;
        }


        public static Dictionary<Location, Box> CloneBoxes(Dictionary<Location, Box> boxes)
        {
            var keys = boxes.Keys;
            Box box;
            Dictionary<Location, Box> newBoxes = new Dictionary<Location, Box>();

            foreach (Location location in keys)
            {
                if(boxes.TryGetValue(location, out box))
                {
                    newBoxes.Add(new Location(location), new Box(box));
                }
                else
                {
                    throw new Exception("Something is wrong with 'Boxes'");
                }
            }
            return newBoxes;
        }

        public bool isGoal()
        {
            //TEEEST
            foreach (EntityLocation boxGoal in boxGoals)
            {
                if (assignedBoxes.TryGetValue(boxGoal.Location, out Box box1) && box1.Name.Equals(boxGoal.Entity.Name))
                {
                    //Console.Write(boxGoal.Entity.Name + "   ");
                }
            }
            //Console.WriteLine("----");
            /////

            foreach (EntityLocation boxGoal in boxGoals)
            {
                if (!(assignedBoxes.TryGetValue(boxGoal.Location, out Box box) && box.Name.Equals(boxGoal.Entity.Name)))
                {
                    return false;
                }
            }
            
            if (this.isAgentGoal && !this.agentLocation.Equals(this.agentGoal))
            {
                return false;
            }
            return true;
        }

        // Checks if this state is valid with respect to the constraints.
        public bool Validate(HashSet<Constraint> constraints)
        {
            if(constraints.TryGetValue(new Constraint(this.timeStep, this.agentLocation, agent), out Constraint constraint) ||
                constraints.TryGetValue(new Constraint(this.timeStep + 1, this.agentLocation, agent), out constraint))        //TODO skift til +1 igen !?!!=
            {
                return false;
            }

            foreach (Location location in this.assignedBoxes.Keys)
            {
                if (constraints.TryGetValue(new Constraint(this.timeStep, location, agent), out Constraint constraint1) ||
                    constraints.TryGetValue(new Constraint(this.timeStep + 1, location, agent), out constraint1))    //TODO skift til +1 igen !?!!=
                {
                    return false;
                }
            }
            return true;
        }


        #region Hash and comparison tools
        public override int GetHashCode()
        {
            int combinedBoxHash = 0;
            foreach (KeyValuePair<Location, Box> box in this.assignedBoxes)
            {
                combinedBoxHash += HashCode.Combine(box.Key, box.Value);
            }

            return this.agentLocation.GetHashCode() + combinedBoxHash;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as WorldState);
        }

        public bool Equals([AllowNull] WorldState obj)
        {
            return this.GetHashCode().Equals(obj.GetHashCode());
        }


        public int CompareTo([AllowNull] WorldState other)
        {
            if ((G + H) > (other.G + other.H)) return -1;
            if ((G + H) == (other.G + other.H)) return 0;
            return 1;
        }

        #endregion
    }
}
