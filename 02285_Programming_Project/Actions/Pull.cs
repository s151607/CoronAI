using System;
using _02285_Programming_Project.AI;
using _02285_Programming_Project.Entities;
using System.Collections.Generic;
using System.Text;
using System.Reflection.Metadata.Ecma335;
using static _02285_Programming_Project.Planning.ConstraintState;

namespace _02285_Programming_Project.Actions
{
    class Pull : Action
    {
        public Pull()
        {
            this.Cost = 1;
        }

        private bool isPossible(WorldState worldState, Directions agentDir, Directions boxDir)
        {
            Location newAgentLocation = Location.RelativeLocation(worldState.agentLocation, agentDir);
            Location currentBoxLocation = Location.RelativeLocation(worldState.agentLocation, boxDir);

            if (!(worldState.assignedBoxes.TryGetValue(newAgentLocation, out Box box) && box.Colour.Equals(worldState.agent.Colour)) &&
                !worldState.Walls.TryGetValue(newAgentLocation, out Location wallLocation) &&
                worldState.assignedBoxes.TryGetValue(currentBoxLocation, out box) && box.Colour.Equals(worldState.agent.Colour))
            {
                return true;
            }
            return false;
        }

        public override bool TryPerformAction(WorldState worldState, Directions agentDir, Directions boxDir)
        {
            Location newAgentLocation = Location.RelativeLocation(worldState.agentLocation, agentDir);
            Location currentBoxLocation = Location.RelativeLocation(worldState.agentLocation, boxDir);
            Location newBoxLocation = worldState.agentLocation;

            if (this.isPossible(worldState, agentDir, boxDir))
            {
                worldState.assignedBoxes.TryGetValue(currentBoxLocation, out Box box); //Safe because isPossible
                worldState.assignedBoxes.Remove(currentBoxLocation);
                worldState.assignedBoxes.Add(newBoxLocation, box);

                worldState.agentLocation = newAgentLocation;

                return true;
            }
            return false;
        }
    }
}
