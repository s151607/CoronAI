using _02285_Programming_Project.Actions;
using _02285_Programming_Project.AI;
using _02285_Programming_Project.Planning;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace _02285_Programming_Project.Entities
{
    /// <summary>
    /// The default actor in the Sokoban game, as detailed in the assignment description
    /// </summary>
    class Agent : Entity, IEquatable<Agent>
    {

        public Planner SearchClient;
        public List<WorldState> plan { get; set; }


        public Agent(char name, string colour)
        {
            this.Name = name;
            this.Colour = colour;
            this.SearchClient = new AStar(this);
        }

        #region Hash and comparison tools
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Name, this.Colour);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Agent);
        }

        public bool Equals([AllowNull] Agent agent)
        {
            return agent != null && agent.Name == this.Name && agent.Colour.Equals(this.Colour);
        }
        #endregion
    }
}
