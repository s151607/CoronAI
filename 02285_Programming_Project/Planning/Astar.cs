using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using _02285_Programming_Project.Actions;
using _02285_Programming_Project.Entities;
using _02285_Programming_Project.AI;
using Priority_Queue;
using Action = _02285_Programming_Project.Actions.Action;
using static _02285_Programming_Project.Planning.ConstraintState;
using System.Linq;
using System.Runtime.InteropServices;

namespace _02285_Programming_Project.Planning
{
    class Astar
    {
        public static List<WorldState> MakePlan(Agent agent, WorldState initialState, HashSet<Constraint> constraints, List<(EntityLocation, int[,], float priority)> hMatrix)
        {
            long nodesChecked = 0;
            #region Initialization
            const int conflictTimeRange = 2;
            const int conflictDistRange = 2;
            const int searchNodeLimit = 1000000;
            BFSHeruistic heuristic = new BFSHeruistic(); //new BFSHeruistic();
            Move move = new Move();
            Pull pull = new Pull();
            Push push = new Push();
            NoOp noOp = new NoOp();
            HashSet<WorldState> explored = new HashSet<WorldState>();
            FastPriorityQueue<WorldState> frontier = new FastPriorityQueue<WorldState>(searchNodeLimit);
            HashSet<WorldState> frontierSet = new HashSet<WorldState>();
            WorldState currentNode;
            WorldState childNode;
            List<Action.Actions> actions = Enum.GetValues(typeof(Action.Actions)).OfType<Action.Actions>().ToList();
            List<Action.Directions> directions = Enum.GetValues(typeof(Action.Directions)).OfType<Action.Directions>().ToList().Where(d => d != Action.Directions.None).ToList();
            List<Constraint> listOfConstraints = constraints.ToList().Where(c => c.agentUnderConstraint.Equals(initialState.agent)).ToList();

            initialState.G = 0;
            initialState.H = BFSHeruistic.CalcH(initialState, hMatrix);
            frontier.Enqueue(initialState, initialState.H + initialState.G);
            #endregion

            while (frontier.Count > 0)
            {
                #region Get current Node
                currentNode = frontier.Dequeue();
                nodesChecked++;
                if (currentNode.isGoal()) 
                {
                    frontier.ResetNode(initialState);
                    frontier = null;
                    explored = null;
                    return constructPath(currentNode); 
                }

                explored.Add(currentNode);

                bool conflictNearby = false;
                if (listOfConstraints.Any(a => (a.timeStamp > currentNode.timeStep - conflictTimeRange) && (a.timeStamp < currentNode.timeStep + conflictTimeRange) &&
                     a.constraintLocation.ManhattanDistanceTo(currentNode.agentLocation) < conflictDistRange))
                {
                    conflictNearby = true;
                }
                #endregion

                // Foreach childnode of current
                foreach (Action.Actions action in actions)
                {
                    #region NoOp
                    childNode = new WorldState(WorldState.CloneBoxes(currentNode.assignedBoxes), currentNode);
                    //NoOp doesn't take directions. No need to loop over them. Can only be used if there is a conflict nearby (in time and coordinates)
                    if (action.Equals(Action.Actions.NoOp) && conflictNearby && childNode.Validate(constraints))
                    {
                        childNode.ActionToGetHere = (Action.Actions.NoOp, Action.Directions.None, Action.Directions.None);
                        childNode.H = heuristic.H(childNode); //NoOP doesn't affect our heuristic
                        childNode.G = currentNode.G + noOp.Cost;
                        AddIfOptimal(frontierSet, childNode, explored, conflictNearby, frontier);
                        continue;
                    }
                    #endregion

                    foreach (Action.Directions agentDirection in directions)
                    {
                        #region Move
                        //Move doesn't take a box direction. No need to loop over them.
                        if (action.Equals(Action.Actions.Move))
                        {
                            childNode = new WorldState(WorldState.CloneBoxes(currentNode.assignedBoxes), currentNode);
                            if(move.TryPerformAction(childNode, agentDirection) && childNode.Validate(constraints))
                            {
                                childNode.ActionToGetHere = (Action.Actions.Move, agentDirection, Action.Directions.None);
                                childNode.H = currentNode.H; //Move doesn't affect our heuristic
                                childNode.G = currentNode.G + move.Cost;
                                AddIfOptimal(frontierSet, childNode, explored, conflictNearby, frontier);
                            }
                            continue;
                        }
                        #endregion

                        foreach (Action.Directions boxDirection in directions)
                        {
                            #region Push & Pull
                            if (action.Equals(Action.Actions.Push))
                            {
                                childNode = new WorldState(WorldState.CloneBoxes(currentNode.assignedBoxes), currentNode);
                                if (push.TryPerformAction(childNode, agentDirection,boxDirection) && childNode.Validate(constraints))
                                {
                                    //Console.WriteLine(nodesChecked + " push " + childNode.agentLocation.x + " " + childNode.agentLocation.y);
                                    childNode.ActionToGetHere = (Action.Actions.Push, agentDirection, boxDirection);
                                    childNode.G = currentNode.G + push.Cost;

                                    childNode.H = BFSHeruistic.CalcH(childNode, hMatrix);


                                    AddIfOptimal(frontierSet, childNode, explored, conflictNearby, frontier);
                                }
                            }
                            else if (action.Equals(Action.Actions.Pull))
                            {
                                childNode = new WorldState(WorldState.CloneBoxes(currentNode.assignedBoxes), currentNode);
                                if (pull.TryPerformAction(childNode, agentDirection, boxDirection) && childNode.Validate(constraints))
                                {
                                    //Console.WriteLine(nodesChecked + " pull " + childNode.agentLocation.x + " " + childNode.agentLocation.y);
                                    childNode.ActionToGetHere = (Action.Actions.Pull, agentDirection, boxDirection);
                                    childNode.G = currentNode.G + pull.Cost;

                                    childNode.H = BFSHeruistic.CalcH(childNode, hMatrix);

                                    AddIfOptimal(frontierSet, childNode, explored, conflictNearby, frontier);
                                }
                            }
                            #endregion
                        }
                    }
                }
            }
            frontier.ResetNode(initialState);
            frontier = null;
            explored = null;
            return new List<WorldState>();
        }

        private static void AddIfOptimal(HashSet<WorldState> frontierSet, WorldState childNode, HashSet<WorldState> explored, bool conflictNearby, FastPriorityQueue<WorldState> frontier)
        {
            WorldState tmp;

            if (frontierSet.TryGetValue(childNode, out tmp) && (tmp.G <= childNode.G) && !conflictNearby)
            {
                return;
            }
            else if (frontierSet.TryGetValue(childNode, out tmp) && (tmp.G > childNode.G) && !conflictNearby)
            {
                tmp = childNode; //TODO check om det her virker i forhold til frontier og frontierset
            }
            else if (explored.TryGetValue(childNode, out tmp) && (tmp.G <= childNode.G) && !conflictNearby)
            {
                return;
            }
            else if (explored.TryGetValue(childNode, out tmp) && (tmp.G > childNode.G) && !conflictNearby)
            {
                tmp = childNode;
            }
            else if (!conflictNearby)
            {
                frontierSet.Add(childNode);
                frontier.Enqueue(childNode, childNode.G + 10*childNode.H);
            }
            else
            {
                frontier.Enqueue(childNode, childNode.G + 10*childNode.H);
            }
            //Console.WriteLine("H = " + childNode.H + " G = " + childNode.G + " agentcoord = " + childNode.agentLocation.x + " " +  childNode.agentLocation.y);
        }

        private static List<WorldState> constructPath(WorldState goalState)
        {
            List<WorldState> plan = new List<WorldState>();
            WorldState currentState = goalState;

            do
            {
                plan.Add(currentState);
                currentState = currentState.Parent;

            } while (currentState != null);

            plan.Reverse();
            return plan;
        }
    }
}
