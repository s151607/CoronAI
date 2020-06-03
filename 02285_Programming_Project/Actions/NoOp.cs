using System;
using _02285_Programming_Project.AI;
using _02285_Programming_Project.Entities;
using System.Collections.Generic;
using System.Text;
using _02285_Programming_Project.Planning;

namespace _02285_Programming_Project.Actions
{

    class NoOp : Action
    {
        public NoOp()
        {
            Cost = 1;
        }

        public override bool TryPerformAction(WorldState worldState, Directions agentDir = Directions.None, Directions boxDir = Directions.None)
        {
            return true; //Always possible and doesn't change anything
        }
    }
}
