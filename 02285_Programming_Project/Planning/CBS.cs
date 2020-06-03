using _02285_Programming_Project.AI;
using _02285_Programming_Project.Entities;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;

namespace _02285_Programming_Project.Planning
{
    class CBS
    {
        private FastPriorityQueue<ConstraintState> frontier;
        //private List<Agent> agents;
        private List<(WorldState, List<(EntityLocation, int[,], float priority)>)> initialStatesWithHeuristics;

        public CBS(List<(WorldState, List<(EntityLocation, int[,], float priority)>)> initialStatesWithHeuristics)
        {
            const int searchNodeLimit = 100000;
            this.frontier = new FastPriorityQueue<ConstraintState>(searchNodeLimit);
            //this.agents = agents;
            this.initialStatesWithHeuristics = initialStatesWithHeuristics;
        }

        public List<List<WorldState>> findSolution()
        {
            ConstraintState initialNode = new ConstraintState();
            foreach((WorldState, List<(EntityLocation, int[,], float priority)>) initialStatesWithHeuristic in initialStatesWithHeuristics)
            {
                initialNode.solution.Add(Astar.MakePlan(initialStatesWithHeuristic.Item1.agent, initialStatesWithHeuristic.Item1, initialNode.constraints, initialStatesWithHeuristic.Item2));


                //initialNode.solution.Add(initialState.agent.SearchClient.MakePlan(initialState, initialNode.constraints));
            }

            initialNode.totalCost = calcSolutionCost(initialNode.solution);

            this.frontier.Enqueue(initialNode, initialNode.totalCost);

            while(frontier.Count > 0)
            {
                ConstraintState currentNode = frontier.Dequeue();
                (List<Agent>, int, Location) conflict = currentNode.Validate();
                if(conflict.Item1.Count == 0)
                {
                    return currentNode.solution;
                }

                foreach(Agent agent in conflict.Item1)
                {
                    ConstraintState childNode = new ConstraintState(currentNode, new ConstraintState.Constraint(conflict.Item2, conflict.Item3, agent));

                    int counter = 0;
                    int agentIndex = -1;
                    foreach(List<WorldState> childPlan in childNode.solution)
                    {
                        if(childPlan.First().agent.Equals(agent))
                        {
                            agentIndex = counter;
                        }
                        counter++;
                    }


                    (WorldState, List<(EntityLocation, int[,], float priority)>) relevantinitialStatesWithHeuristic = initialStatesWithHeuristics.First(m => m.Item1.agent.Name.Equals(agent.Name));
                    //initialNode.solution.Add(Astar.MakePlan(hMatrix.First().Item4.agent, hMatrix.First().Item4, initialNode.constraints, hMatrix));
                    childNode.solution[agentIndex] = Astar.MakePlan(relevantinitialStatesWithHeuristic.Item1.agent, relevantinitialStatesWithHeuristic.Item1, 
                        currentNode.constraints, relevantinitialStatesWithHeuristic.Item2);


                    //childNode.solution[agentIndex] = agent.SearchClient.MakePlan(this.initialStates.First(a => a.agent.Equals(agent)), childNode.constraints);
                    if(childNode.solution[agentIndex].Count > 0)
                    {
                        childNode.totalCost = calcSolutionCost(childNode.solution);
                        frontier.Enqueue(childNode, childNode.totalCost);
                    }
                }
            }
            return null; //Unsolvable
        }

        private int calcSolutionCost(List<List<WorldState>> solution)
        {
            int longestPlanLength = 0;
            foreach (List<WorldState> plan in solution)
            {
                if (plan.Count > longestPlanLength) longestPlanLength = plan.Count;
            }
            return longestPlanLength;
        }
        /*
         * lav tom constraintstate: R
         * lad hver agent find en løsning og placer dem i R og sæt R.cost
         * sæt R i frontier (minheap)
         * while frontier is not empty:
         *  find min node from frontier
         *  validate solution (each path) until a conflict occurs... if no conflict return solution
         *  C <- first conflict found (list of agents, location, time)
         *  foreach agent a in C:
         *      make new node A
         *      A.constraint = parent.constraint.deepcopy() + new constraint(a, C)
         *      A.solution = parent.solution.deepcopy()
         *      A.solution.a = replan for a
         *      A.cost = calc cost of solution
         *      add A to frontier
         *      
         *  private HashSet<WorldState> explored;
        private FastPriorityQueue<WorldState> frontier;

        public AStar(Agent agent) : base()
        {
            this.agent = agent;
            this.heuristic = new BasicHeuristic();
            this.explored = new HashSet<WorldState>();
            this.frontier = new FastPriorityQueue<WorldState>(searchNodeLimit);
        }

         *  
         */
    }
}
