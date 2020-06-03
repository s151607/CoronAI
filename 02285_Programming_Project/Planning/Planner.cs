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
    abstract class Planner
    {
        protected const int searchNodeLimit = 1000000;
        protected Agent agent;
        protected Heuristic heuristic;
        protected Move move;
        protected Pull pull;
        protected Push push;
        protected NoOp noOp;
        protected const int conflictTimeRange = 0;
        protected const int conflictDistRange = 0;

        protected Planner()
        {
            this.move = new Move();
            this.pull = new Pull();
            this.push = new Push();
            this.noOp = new NoOp();
        }

        public abstract List<WorldState> MakePlan(WorldState initialState, HashSet<Constraint> constraints);
    }

    class AStar : Planner
    {
        private HashSet<WorldState> explored;
        private FastPriorityQueue<WorldState> frontier;

        public AStar(Agent agent) : base()
        {
            this.agent = agent;
            this.heuristic = new BasicHeuristic();
        }

        public override List<WorldState> MakePlan(WorldState initialState, HashSet<Constraint> constraints)
        {
            long nodesChecked = 0;
            long amountOfStatesAddedToFrontier = 0;
            this.frontier = new FastPriorityQueue<WorldState>(searchNodeLimit);
            this.explored = new HashSet<WorldState>();

            List<Constraint> listOfConstraints = constraints.ToList().Where(c => c.agentUnderConstraint.Equals(initialState.agent)).ToList();
            initialState.H = heuristic.H(initialState);
            initialState.G = 0;
            frontier.Enqueue(initialState, initialState.H + initialState.G);
            WorldState currentNode;
            var Directions = Enum.GetValues(typeof(Action.Directions));
            List<Action.Directions> directions = Directions.OfType<Action.Directions>().ToList().Where(d => d != Action.Directions.None).ToList();

            while (frontier.Count > 0)
            {
                
                currentNode = frontier.Dequeue();
                nodesChecked++;
                if (currentNode.isGoal())
                {
                    Console.WriteLine("old visited " + nodesChecked + " states");
                    this.frontier.ResetNode(initialState);
                    this.frontier = null;
                    this.explored = null;
                    return constructPath(currentNode);
                }

                bool conflictNearby = false;
                if(listOfConstraints.Any(a => (a.timeStamp > currentNode.timeStep - conflictTimeRange) && (a.timeStamp < currentNode.timeStep + conflictTimeRange) && 
                    a.constraintLocation.ManhattanDistanceTo(currentNode.agentLocation) < conflictDistRange))
                {
                    conflictNearby = true;
                }

                // Expand for all possible action, direction combination.
                foreach (Action.Actions action in Enum.GetValues(typeof(Action.Actions)))
                {
                    if(action.Equals(Action.Actions.Move))
                    {
                        foreach(Action.Directions agentDirection in directions)
                        {
                            WorldState childNode = new WorldState(WorldState.CloneBoxes(currentNode.assignedBoxes), currentNode);

                            if(this.move.TryPerformAction(childNode, agentDirection) && (!explored.Contains(childNode) || conflictNearby) && childNode.Validate(constraints))
                            {
                                childNode.ActionToGetHere = (Action.Actions.Move, agentDirection, Action.Directions.None);
                                childNode.H = heuristic.H(childNode);
                                childNode.G = currentNode.G + this.move.Cost;
                                frontier.Enqueue(childNode, childNode.H + childNode.G);
                                
                                //Console.WriteLine("Added node with H = " + childNode.H + " G = " + childNode.G + ". At " + amountOfStatesAddedToFrontier + " explored states.");
                            }
                        }
                    }
                    else if(action.Equals(Action.Actions.Pull))
                    {
                        foreach (Action.Directions agentDirection in directions)
                        {
                            foreach (Action.Directions boxDirection in directions)
                            {
                                WorldState childNode = new WorldState(WorldState.CloneBoxes(currentNode.assignedBoxes), currentNode);

                                if (this.pull.TryPerformAction(childNode, agentDirection, boxDirection) && (!explored.Contains(childNode) || conflictNearby) && childNode.Validate(constraints))
                                {
                                    childNode.ActionToGetHere = (Action.Actions.Pull, agentDirection, boxDirection);
                                    childNode.H = heuristic.H(childNode);
                                    childNode.G = currentNode.G + this.pull.Cost;
                                    frontier.Enqueue(childNode, childNode.H + childNode.G);
                                    
                                    //Console.WriteLine("Added node with H = " + childNode.H + " G = " + childNode.G + ". At " + amountOfStatesAddedToFrontier + " explored states.");
                                }
                            }
                        }
                    }
                    else if (action.Equals(Action.Actions.Push))
                    {
                        foreach (Action.Directions agentDirection in directions)
                        {
                            foreach (Action.Directions boxDirection in directions)
                            {
                                WorldState childNode = new WorldState(WorldState.CloneBoxes(currentNode.assignedBoxes), currentNode);

                                if (this.push.TryPerformAction(childNode, agentDirection, boxDirection) && (!explored.Contains(childNode) || conflictNearby) && childNode.Validate(constraints))
                                {
                                    childNode.ActionToGetHere = (Action.Actions.Push, agentDirection, boxDirection);
                                    childNode.H = heuristic.H(childNode);
                                    childNode.G = currentNode.G + this.push.Cost;
                                    frontier.Enqueue(childNode, childNode.H + childNode.G);
                                    
                                    //Console.WriteLine("Added node with H = " + childNode.H + " G = " + childNode.G + ". At " + amountOfStatesAddedToFrontier + " explored states.");
                                }
                            }
                        }
                    }
                    else if (action.Equals(Action.Actions.NoOp))
                    {
                        WorldState childNode = new WorldState(WorldState.CloneBoxes(currentNode.assignedBoxes), currentNode);

                        if (this.noOp.TryPerformAction(childNode) && (!explored.Contains(childNode) || conflictNearby) && childNode.Validate(constraints))
                        {
                            childNode.ActionToGetHere = (Action.Actions.NoOp, Action.Directions.None, Action.Directions.None);
                            childNode.H = heuristic.H(childNode);
                            childNode.G = currentNode.G + this.noOp.Cost;
                            frontier.Enqueue(childNode, childNode.H + childNode.G);
                            
                            //Console.WriteLine("Added node with H = " + childNode.H + " G = " + childNode.G + ". At " + amountOfStatesAddedToFrontier + " explored states.");
                        }
                    }
                }
                explored.Add(currentNode);
                amountOfStatesAddedToFrontier++;
            }
            this.frontier.ResetNode(initialState);
            this.frontier = null;
            this.explored = null;
            return null;
        }

        private List<WorldState> constructPath(WorldState goalState)
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
