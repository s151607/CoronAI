using System;
using _02285_Programming_Project.AI;
using _02285_Programming_Project.Entities;
using System.Collections.Generic;
using System.Text;
using static _02285_Programming_Project.Planning.ConstraintState;

namespace _02285_Programming_Project.Actions
{
    class Push : Action
    {
        public Push()
        {
            this.Cost = 1;
        }

        private bool isPossible(WorldState worldState, Directions agentDir, Directions boxDir)
        {
            Location newAgentLocation = Location.RelativeLocation(worldState.agentLocation, agentDir);
            Location newBoxLocation = Location.RelativeLocation(newAgentLocation, boxDir); //Because current box location = new agent location

            if (worldState.assignedBoxes.TryGetValue(newAgentLocation, out Box box) && box.Colour.Equals(worldState.agent.Colour) &&
                !(worldState.assignedBoxes.TryGetValue(newBoxLocation, out box) && box.Colour.Equals(worldState.agent.Colour)) &&
                !worldState.Walls.TryGetValue(newBoxLocation, out Location boxLocation) &&
                !newBoxLocation.Equals(worldState.agentLocation))
            {
                return true;
            }
            return false;
        }
        //bliver ikke checket korrekt om der er en væg på nye boxlocation

        public override bool TryPerformAction(WorldState worldState, Directions agentDir, Directions boxDir)
        {
            Location newAgentLocation = Location.RelativeLocation(worldState.agentLocation, agentDir);
            Location currentBoxLocation = newAgentLocation;
            Location newBoxLocation = Location.RelativeLocation(currentBoxLocation, boxDir);

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
