using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Serialization;
using _02285_Programming_Project.Actions;
using _02285_Programming_Project.AI;
using _02285_Programming_Project.Entities;
using Action = _02285_Programming_Project.Actions.Action;

namespace _02285_Programming_Project.Planning
{
    abstract class Heuristic
    {
        public abstract int H(WorldState state);
    }

    class BFSHeruistic : Heuristic
    {
        private static bool SimulateMove(WorldState state, Action.Directions direction)
        {
            Location newAgentLocation = Location.RelativeLocation(state.agentLocation, direction);
            if (state.Walls.TryGetValue(newAgentLocation, out Location wallLocation)) return false;
            else
            {
                state.agentLocation = newAgentLocation;
                return true;
            }
        }

        public static float CalcH(WorldState state, List<(EntityLocation, int[,], float priority)> hMatrix)
        {
            var locations = state.assignedBoxes.Keys;
            float total = 0;

            foreach ((EntityLocation, int[,], float priority) m in hMatrix)
            {
                int shortest = int.MaxValue;
                Location shortestBox = new Location();

                foreach (Location loc in locations)
                {
                    if (m.Item2[loc.x, loc.y] < shortest && state.assignedBoxes.TryGetValue(loc, out Box box) && box.Name.Equals(m.Item1.Entity.Name))
                    {
                        shortest = m.Item2[loc.x, loc.y];
                        shortestBox = loc;
                    }
                }
                total += (float)(shortest * m.Item3);

                /*
                if(shortestBox.ManhattanDistanceTo(m.Item2.Location) == 0)
                {
                    total += (float)(shortest * m.Item3);
                }
                else
                {
                    total += (float)((shortest + shortestBox.ManhattanDistanceTo(state.agentLocation)) * m.Item3);
                }*/


            }
            return total;
        }

        public static List<(WorldState, List<(EntityLocation, int[,], float priority)>)> PrecalcH(List<WorldState> states)
        {
            List<(WorldState, List<(EntityLocation, int[,], float priority)>)> initialStatesWithHeuristics = new List<(WorldState, List<(EntityLocation, int[,], float priority)>)>();
            foreach (WorldState state in states)
            {
                WorldState initialState = new WorldState(WorldState.CloneBoxes(state.assignedBoxes), state);
                WorldState current;
                WorldState childState;
                List<Action.Directions> directions = Enum.GetValues(typeof(Action.Directions)).OfType<Action.Directions>().ToList().Where(d => d != Action.Directions.None).ToList();
                List<(EntityLocation, int[,], float priority)> goalHeuristics = new List<(EntityLocation, int[,], float priority)>();

                foreach (EntityLocation goal in initialState.boxGoals)
                {
                    int[,] hMatrix = new int[50, 50];
                    for (int row = 0; row < 50; row++)
                    {
                        for (int column = 0; column < 50; column++)
                        {
                            hMatrix[row, column] = int.MaxValue;
                        }
                    }

                    initialState.agentLocation = goal.Location;
                    initialState.G = 0;
                    HashSet<WorldState> explored = new HashSet<WorldState>();
                    Queue<WorldState> Q = new Queue<WorldState>();
                    Q.Enqueue(initialState);
                    explored.Add(initialState);

                    //Assign value to each tile
                    while (Q.Count > 0)
                    {
                        current = Q.Dequeue();
   

                        hMatrix[current.agentLocation.x, current.agentLocation.y] = current.G;

                        foreach (Action.Directions agentDirection in directions)
                        {
                            childState = new WorldState(WorldState.CloneBoxes(current.assignedBoxes), current);
                            if (SimulateMove(childState, agentDirection) && !explored.Contains(childState))
                            {
                                childState.Parent = current;
                                childState.G = childState.Parent.G + 1;
                                Q.Enqueue(childState);
                                explored.Add(childState);
                            }
                        }
                    }

                    float priority = 1;
                    foreach (Action.Directions agentDirection in directions)
                    {
                        if (initialState.Walls.TryGetValue(Location.RelativeLocation(goal.Location, agentDirection), out Location loc))
                        {
                            priority += (float)2;
                        }
                        else if (initialState.boxGoals.Any(bG => bG.Location.Equals(Location.RelativeLocation(goal.Location, agentDirection))))
                        {
                            priority += (float)1;
                        }
                    }
                    goalHeuristics.Add((goal, hMatrix, priority));
                }
                initialStatesWithHeuristics.Add((state ,goalHeuristics));
            }
            return initialStatesWithHeuristics;
        }


        private static void Print2DArray(int[,] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    Console.Write(matrix[i, j] + " ");
                    if ((int)matrix[i, j] < 10) Console.Write(" ");
                }
                Console.WriteLine();
            }
        }

        public override int H(WorldState state)
        {
            WorldState initialState = new WorldState(WorldState.CloneBoxes(state.assignedBoxes), state);
            WorldState current;
            WorldState childState;
            List<Action.Directions> directions = Enum.GetValues(typeof(Action.Directions)).OfType<Action.Directions>().ToList().Where(d => d != Action.Directions.None).ToList();
            int hValue = 0;

            foreach (EntityLocation goal in initialState.boxGoals)
            {
                initialState.agentLocation = goal.Location;
                initialState.G = 0;

                HashSet<WorldState> explored = new HashSet<WorldState>();
                Queue<WorldState> Q = new Queue<WorldState>();
                Q.Enqueue(initialState);

                while (Q.Count > 0)
                {
                    current = Q.Dequeue();
                    explored.Add(current);
                    if (current.assignedBoxes.TryGetValue(current.agentLocation, out Box box) && box.Name.Equals(goal.Entity.Name))
                    {
                        hValue += current.G;
                        break;
                    }

                    foreach (Action.Directions agentDirection in directions)
                    {
                        childState = new WorldState(WorldState.CloneBoxes(current.assignedBoxes), current);
                        if (SimulateMove(childState, agentDirection) && !explored.Contains(childState))
                        {
                            childState.Parent = current;
                            childState.G = childState.Parent.G + 1;
                            Q.Enqueue(childState);
                        }
                    }
                }
            }
            return hValue;
        }
    }


    class BasicHeuristic : Heuristic
    {
        public override int H(WorldState state)
        {
            var agentLocation = state.agentLocation;
            var assignedBoxes = state.assignedBoxes;
            var goals = state.boxGoals;
            int agentToBoxDistance;
            if (assignedBoxes.Count > 0) agentToBoxDistance = int.MaxValue;
            else agentToBoxDistance = 0;

            /*
            //agent to closest box not in goal + boxes to closest goal
            foreach (KeyValuePair<Location, Box> boxLocation in assignedBoxes)
            {
                if(!(goals.Any(g => g.Location.Equals(boxLocation.Key) && g.Entity.Name.Equals(boxLocation.Value.Name))))
                {
                    if (agentLocation.ManhattanDistanceTo(boxLocation.Key) < agentToBoxDistance) agentToBoxDistance = agentLocation.ManhattanDistanceTo(boxLocation.Key);
                }

            }*/

            int boxToGoalDistance = int.MaxValue;
            int totalBoxesToGoalDistance = 0;
            foreach (EntityLocation goal in goals)
            {
                foreach (KeyValuePair<Location, Box> boxLocation in assignedBoxes.Where(b => b.Value.Name.Equals(goal.Entity.Name)))
                {

                    if (boxLocation.Key.ManhattanDistanceTo(goal.Location) < boxToGoalDistance) boxToGoalDistance = boxLocation.Key.ManhattanDistanceTo(goal.Location);
                }

                totalBoxesToGoalDistance += boxToGoalDistance;
                boxToGoalDistance = int.MaxValue;
            }

            if (totalBoxesToGoalDistance < 0) throw new Exception("waat");
            return totalBoxesToGoalDistance;


            /*
            foreach (KeyValuePair<Location, Box> boxLocation in assignedBoxes)
            {
                if (agentLocation.ManhattanDistanceTo(boxLocation.Key) < agentToBoxDistance) agentToBoxDistance = agentLocation.ManhattanDistanceTo(boxLocation.Key);
            }

            int boxToGoalDistance = int.MaxValue;
            int totalBoxesToGoalDistance = 0;
            foreach (EntityLocation goal in goals)
            {
                foreach(KeyValuePair<Location, Box> boxLocation in assignedBoxes.Where(b => b.Value.Name.Equals(goal.Entity.Name)))
                {
                    if(!goal.Location.Equals(boxLocation.Key) && goals.Any(g => g.Location.Equals(boxLocation.Key)))
                    {
                        continue;
                    }

                    if (boxLocation.Key.ManhattanDistanceTo(goal.Location) < boxToGoalDistance) boxToGoalDistance = boxLocation.Key.ManhattanDistanceTo(goal.Location);
                }

                totalBoxesToGoalDistance += boxToGoalDistance;
                boxToGoalDistance = int.MaxValue;
            }

            int agentToAgentGoal = 0;
            if(state.isAgentGoal)
            {
                agentToAgentGoal = state.agentLocation.ManhattanDistanceTo(state.agentGoal);
            }*/


            //return totalBoxesToGoalDistance;
            //return totalBoxesToGoalDistance + agentToBoxDistance + agentToAgentGoal;
        }
    }
}