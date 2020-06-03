using System;
using System.Collections.Generic;
using _02285_Programming_Project.AI;
using _02285_Programming_Project.Entities;
using System.Text;
using static _02285_Programming_Project.Planning.ConstraintState;
using System.Runtime.CompilerServices;

namespace _02285_Programming_Project.Actions
{
    /// <summary>
    /// Abstract class for the definitions of any action type
    /// </summary>
    abstract class Action
    {
        public enum Directions
        {
            None,
            North,
            West,
            South,
            East
        }

        public enum Actions
        {
            NoOp,
            Move,
            Pull,
            Push
        }

        public static string directionToString(Directions dir)
        {
            if (dir.Equals(Directions.East)) return "E";
            else if(dir.Equals(Directions.South)) return "S";
            else if (dir.Equals(Directions.West)) return "W";
            else if (dir.Equals(Directions.North)) return "N";
            else return "None";
        }

        public static string actionToString(Actions act)
        {
            if (act.Equals(Actions.Move)) return "Move";
            else if (act.Equals(Actions.Pull)) return "Pull";
            else if (act.Equals(Actions.Push)) return "Push";
            else return "NoOp";
        }

        // The cost of performing an action
        public int Cost { get; set; }

        public abstract bool TryPerformAction(WorldState worldState, Directions agentDir, Directions boxDir);
    }
}
