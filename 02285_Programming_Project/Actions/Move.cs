using System;
using _02285_Programming_Project.AI;
using _02285_Programming_Project.Entities;
using System.Collections.Generic;
using System.Text;
using static _02285_Programming_Project.Planning.ConstraintState;

namespace _02285_Programming_Project.Actions
{
    class Move : Action
    {
        public Move()
        {
            this.Cost = 1;
        }

        private bool isPossible(WorldState worldState, Directions agentDir)
        {
            Location newAgentLocation = Location.RelativeLocation(worldState.agentLocation, agentDir);

            if(!(worldState.assignedBoxes.TryGetValue(newAgentLocation, out Box box) && box.Colour.Equals(worldState.agent.Colour)) &&
                !worldState.Walls.TryGetValue(newAgentLocation, out Location wallLocation))
            {
                return true;
            }
            return false;
        }

        public override bool TryPerformAction(WorldState worldState, Directions agentDir, Directions boxDir = Directions.None)
        {
            if(this.isPossible(worldState, agentDir))
            {
                Location newAgentLocation = Location.RelativeLocation(worldState.agentLocation, agentDir);
                worldState.agentLocation = newAgentLocation;
                return true;
            }
            return false;
        }
    }
}
