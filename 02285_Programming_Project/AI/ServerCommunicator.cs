using _02285_Programming_Project.Entities;
using _02285_Programming_Project.Entities._02285_Programming_Project.Entities;
using _02285_Programming_Project.Planning;
using System;
using System.Collections.Generic;
//using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Action = _02285_Programming_Project.Actions.Action;

namespace _02285_Programming_Project.AI
{
    class ServerCommunicator
    {
        private const string readError = "Error reading server communication";


        public static List<string> printSolutionToFile(List<List<WorldState>> solution)
        {
            StringBuilder currentLine = new StringBuilder("");
            List<string> lines = new List<string>();

            int longestPlanLength = 0;
            foreach (List<WorldState> plan in solution)
            {
                if (plan.Count > longestPlanLength) longestPlanLength = plan.Count;
            }

            for (int j = 1; j < longestPlanLength; j++)
            {
                currentLine = new StringBuilder("");
                for (int i = 0; i < 10; i++)
                {
                    List<WorldState> plan = solution.FirstOrDefault(p => char.GetNumericValue(p.First().agent.Name) == i);

                    if (plan == null)
                    {

                    }
                    else if (plan.Count <= j)
                    {
                        currentLine.Append("NoOp;");
                    }
                    else
                    {
                        string action = Action.actionToString(plan[j].ActionToGetHere.Item1);
                        string agentDir = Action.directionToString(plan[j].ActionToGetHere.Item2);

                        if (action.Equals("NoOp"))
                        {
                            currentLine.Append(action + ";");
                        }
                        else
                        {
                            currentLine.Append(action + "(");
                            currentLine.Append(agentDir);
                            if (!action.Equals("Move"))
                            {
                                string boxDir = Action.directionToString(plan[j].ActionToGetHere.Item3);
                                currentLine.Append("," + boxDir);
                            }
                            currentLine.Append(");");
                        }
                    }
                }
                currentLine.Remove(currentLine.Length - 1, 1);
                lines.Add(currentLine.ToString());
            }
            return lines;
        }

        private static List<WorldState> SimpleAssignBoxesToAgents(List<EntityLocation> agents, List<EntityLocation> boxes, List<EntityLocation> agentGoals, List<EntityLocation> boxGoals, HashSet<Location> walls)
        {
            if (agents.Count > 1) throw new Exception("this is only for SA");
            List<WorldState> initialStates = new List<WorldState>();
            Location agentGoal = null;

            if (agentGoals.Count > 0)
            {
                agentGoal = agentGoals[0].Location;
            }

            Dictionary<Location, Box> boxes1 = new Dictionary<Location, Box>();
            foreach (var b in boxes)
            {
                boxes1.Add(b.Location, (Box)b.Entity);
            }

            initialStates.Add(new WorldState(agents[0].Location, agentGoal, boxes1, boxGoals, walls, (Agent)agents[0].Entity));
            return initialStates;

        }



        private static List<WorldState> AssignBoxesToAgents(List<EntityLocation> agents, List<EntityLocation> boxes, List<EntityLocation> agentGoals, List<EntityLocation> boxGoals, HashSet<Location> walls)
        {
            List<WorldState> initialStates = new List<WorldState>();
            List<Assignment> boxAssignments = new List<Assignment>();
            List<Assignment> goalAssignments = new List<Assignment>();
            bool isAgentGoal = false;

            List<WorldState> completeInitStates = new List<WorldState>();
            foreach (EntityLocation agentEntity in agents)
            {
                WorldState completeInitState = new WorldState();
                completeInitState.agent = (Agent)agentEntity.Entity;
                completeInitState.agentLocation = agentEntity.Location;
                completeInitState.Walls = walls;
                completeInitState.assignedBoxes = new Dictionary<Location, Box>();
                completeInitState.boxGoals = new List<EntityLocation>();
                completeInitState.boxGoals.Add(new EntityLocation(completeInitState.agent, completeInitState.agentLocation));
                completeInitStates.Add(completeInitState);
            }

            List<(WorldState, List<(EntityLocation, int[,], float priority)>)> completeInitStatesWithH =  BFSHeruistic.PrecalcH(completeInitStates);

            while (boxGoals.Count > 0)
            {
                foreach (EntityLocation agentEntity in agents)
                {
                    double shortestDistance = double.PositiveInfinity;
                    EntityLocation bestBoxGoalLocation = null;
                    EntityLocation bestInitialBoxGoalLocation = null;

                    Agent agent = (Agent)agentEntity.Entity;
                    EntityLocation agentGoal = null;

                    if (agentGoals.Count > 0)
                    {
                        isAgentGoal = agentGoals.Any(a => a.Entity.Name.Equals(agentEntity.Entity.Name));
                        if (isAgentGoal) agentGoal = agentGoals.First(a => a.Entity.Name.Equals(agentEntity.Entity.Name));
                    }

                    foreach (EntityLocation boxGoalLocation in boxGoals.Where(b => b.Entity.Colour.Equals(agent.Colour)))
                    {
                        // Assign a goal and a box
                        foreach (EntityLocation boxInitialLocation in boxes.Where(b => b.Entity.Name.Equals(boxGoalLocation.Entity.Name)))
                        {
                            double total_dist = 0.0;
                            total_dist += completeInitStatesWithH.First(c => c.Item1.agent.Name.Equals(agentEntity.Entity.Name)).Item2.First()
                                .Item2[boxInitialLocation.Location.x, boxInitialLocation.Location.y];               //agentEntity.ManhattanDistance(boxInitialLocation);
                            total_dist += completeInitStatesWithH.First(c => c.Item1.agent.Name.Equals(agentEntity.Entity.Name)).Item2.First()
                                .Item2[boxGoalLocation.Location.x, boxGoalLocation.Location.y];                             //boxInitialLocation.ManhattanDistance(boxGoalLocation);
                            if (isAgentGoal) total_dist += completeInitStatesWithH.First(c => c.Item1.agent.Name.Equals(agentEntity.Entity.Name)).Item2.First()
                                .Item2[agentGoal.Location.x, agentGoal.Location.y];  //boxGoalLocation.ManhattanDistance(agentGoal);

                            if (total_dist < shortestDistance)
                            {
                                shortestDistance = total_dist;
                                bestBoxGoalLocation = boxGoalLocation;
                                bestInitialBoxGoalLocation = boxInitialLocation;
                            }
                        }
                    }

                    if (bestBoxGoalLocation != null && bestInitialBoxGoalLocation != null && shortestDistance < int.MaxValue)
                    {
                        boxAssignments.Add(new Assignment(agent, bestInitialBoxGoalLocation));
                        goalAssignments.Add(new Assignment(agent, bestBoxGoalLocation));
                        EntityLocation boxToRemove = null;
                        EntityLocation boxGoalToRemove = null;


                        foreach (EntityLocation b in boxes)
                        {
                            if (b.Location.Equals(bestInitialBoxGoalLocation.Location))
                            {
                                boxToRemove = b;
                            }
                        }

                        foreach (EntityLocation b in boxGoals)
                        {
                            if (b.Location.Equals(bestBoxGoalLocation.Location))
                            {
                                boxGoalToRemove = b;
                            }
                        }

                        boxes.Remove(boxToRemove);
                        boxGoals.Remove(boxGoalToRemove);
                    }
                }
            }

            foreach (EntityLocation b in boxes)
            {
                int minBoxCount = int.MaxValue;
                Agent minAgent = null;

                foreach (EntityLocation agent in agents.Where(ass => ass.Entity.Colour.Equals(b.Entity.Colour)))
                {
                    int thisAgentCount = 0;
                    foreach (Assignment a in boxAssignments.Where(ass => ass.Agent.Colour.Equals(b.Entity.Colour)))
                    {
                        if (a.Agent.Equals(agent.Entity))
                        {
                            thisAgentCount++;
                        }
                    }
                    if (thisAgentCount < minBoxCount)
                    {
                        minBoxCount = thisAgentCount;
                        minAgent = (Agent)agent.Entity;
                    }
                }

                if (minAgent == null) throw new Exception("");
                boxAssignments.Add(new Assignment(minAgent, b));
            }




            foreach (EntityLocation agentEntity in agents)
            {

                Location agentLocation = agentEntity.Location;
                Location agentGoal;

                List<EntityLocation> test = agentGoals.Where(a => a.Entity.Name.Equals(agentEntity.Entity.Name)).ToList();
                if (test.Count > 0)
                {
                    agentGoal = test.ElementAt(0).Location;
                }
                else agentGoal = null;

                Dictionary<Location, Box> agentBoxes = new Dictionary<Location, Box>();
                List<EntityLocation> boxGoalsforThisAgent = new List<EntityLocation>();

                foreach (Assignment boxAssignment in boxAssignments)
                {
                    if (boxAssignment.Agent.Name.Equals(agentEntity.Entity.Name))
                    {
                        agentBoxes.Add(boxAssignment.EntityLocation.Location, (Box)boxAssignment.EntityLocation.Entity);
                    }
                }

                foreach (Assignment goalAssignment in goalAssignments)
                {
                    if (goalAssignment.Agent.Name.Equals(agentEntity.Entity.Name))
                    {
                        boxGoalsforThisAgent.Add(goalAssignment.EntityLocation);
                    }
                }

                initialStates.Add(new WorldState(agentLocation, agentGoal, agentBoxes, boxGoalsforThisAgent, walls, (Agent)agentEntity.Entity));
            }
            return initialStates;
        }

        public static List<WorldState> readLevelFromFile()
        {
            List<EntityLocation> agents = new List<EntityLocation>();
            List<EntityLocation> boxes = new List<EntityLocation>();
            List<EntityLocation> agentGoals = new List<EntityLocation>();
            List<EntityLocation> boxGoals = new List<EntityLocation>();
            HashSet<Location> wallLocations = new HashSet<Location>();
            Regex colourPrefix = new Regex(@"^[a-zA-Z]+:");
            List<(string, char)> boxColours = new List<(string, char)>();
            List<(string, char)> agentColours = new List<(string, char)>();

            //string[] fileLines = System.IO.File.ReadAllLines(@"C:\Users\gustavfjorder\source\repos\02285_Programming_Project\02285_Programming_Project\Levels\MASimple.lvl");
            string[] fileLines = System.IO.File.ReadAllLines(@"C:\Users\asger\Desktop\02285_Programming_Project-restructuring\02285_Programming_Project\Levels\comp20MA\MAdeepChaos.lvl");

            int index = 5;

            // Read all colour lines
            string colourLine = fileLines[index];
            while (colourPrefix.IsMatch(colourLine))
            {
                string colour = colourLine.Substring(0, colourLine.IndexOf(":"));
                string entityString = colourLine.Remove(0, colour.Length + 1);
                string[] entities = entityString.Split(",");

                for (int i = 0; i < entities.Count(); i++)
                {
                    entities[i] = entities[i].Replace(" ", String.Empty);
                }

                // Iterate through each char after the "<colour>:" prefix.
                foreach (string entity in entities)
                {
                    if (entity.Length > 1) throw new Exception();
                    char cEntity = Convert.ToChar(entity);
                    if (Char.IsDigit(cEntity)) //Agent
                    {
                        agentColours.Add((colour, cEntity));
                    }
                    else //Box
                    {
                        boxColours.Add((colour, cEntity));
                    }
                }
                index++;
                colourLine = fileLines[index];
            }

            // colourLine now contains the one after
            if (!colourLine.Equals("#initial")) throw new Exception(readError);

            int xCount = 0;
            int yCount = 0;
            index++;
            string levelLine = fileLines[index];
            while (!levelLine.Equals("#goal"))
            {
                xCount = 0;
                foreach (char entity in levelLine.ToCharArray())
                {
                    if (entity.Equals(' '))
                    {
                        xCount++;
                        continue; //Space
                    }
                    else if (entity.Equals('+')) //Wall
                    {
                        wallLocations.Add(new Location(xCount, yCount));
                    }
                    else if (Char.IsDigit(entity)) //Agent
                    {
                        string colour = null;
                        foreach (var agent in agentColours)
                        {
                            if (agent.Item2.Equals(entity))
                            {
                                colour = agent.Item1;
                            }
                        }
                        agents.Add(new EntityLocation(new Agent(entity, colour), new Location(xCount, yCount)));
                    }
                    else //Box
                    {
                        string colour = null;
                        foreach (var box in boxColours)
                        {
                            if (box.Item2.Equals(entity))
                            {
                                colour = box.Item1;
                            }
                        }

                        //hvis der ikke er en agent med farven colour
                        bool hasAgent = false;
                        foreach (var ac in agentColours)
                        {
                            if (ac.Item1 == colour) hasAgent = true;
                        }

                        if (hasAgent) boxes.Add(new EntityLocation(new Box(entity, colour), new Location(xCount, yCount)));
                        else wallLocations.Add(new Location(xCount, yCount));
                    }

                    xCount++;
                }
                index++;
                levelLine = fileLines[index];
                yCount++;
            }



            yCount = 0;
            index++;
            levelLine = fileLines[index];
            while (!levelLine.Equals("#end"))
            {
                xCount = 0;
                foreach (char entity in levelLine.ToCharArray())
                {
                    if (Char.IsDigit(entity)) // If agent goal
                    {
                        Agent agent = (Agent)agents.First(a => a.Entity.Name.Equals(entity)).Entity;
                        agentGoals.Add(new EntityLocation(agent, new Location(xCount, yCount)));
                    }
                    else if (boxes.Any(b => b.Entity.Name.Equals(entity))) //(!entity.Equals(' ') && !entity.Equals('+')) //Then it must be a box
                    {
                        Box box = (Box)boxes.First(b => b.Entity.Name.Equals(entity)).Entity;
                        boxGoals.Add(new EntityLocation(box, new Location(xCount, yCount)));
                    }
                    xCount++;
                }
                index++;
                levelLine = fileLines[index];
                yCount++;
            }
            //List<WorldState> initialWorldstates = SimpleAssignBoxesToAgents(agents, boxes, agentGoals, boxGoals, wallLocations);
            List<WorldState> initialWorldstates = AssignBoxesToAgents(agents, boxes, agentGoals, boxGoals, wallLocations);



            // create initial state, goal state and return 
            // Construct initial states
            // Agent's own goals
            return initialWorldstates;

        }


        // returnerer et initial state for hver agent hvor bokse og mål er fordelt
        private static List<WorldState> AssignBoxesToAgentsNew(List<EntityLocation> agents, List<EntityLocation> boxes, List<EntityLocation> agentGoals, List<EntityLocation> boxGoals, HashSet<Location> walls)
        {
            List<WorldState> initialSates = new List<WorldState>();
            WorldState newState;
            foreach (var agent in agents)
            {
                newState = new WorldState();
                newState.agent = (Agent)agent.Entity;
                newState.Walls = walls;
                newState.timeStep = 0;
                newState.Parent = null;

                //If there is an agentgoal for this agent set it..
                if (agentGoals.Any(a => a.Entity.Equals(newState.agent)))
                {
                    EntityLocation agentGoal = agentGoals.First(a => a.Entity.Equals(newState.agent));
                    newState.agentGoal = agentGoal.Location;
                    newState.isAgentGoal = true;
                }
                else
                {
                    newState.agentGoal = null;
                    newState.isAgentGoal = false;
                }
                initialSates.Add(newState);
            }

            //foreach box
            //assign it to the agent of same colour with smallest amount of assigned boxes so far.



            return initialSates;
        }





        public static List<WorldState> readLevel()
        {
            Console.WriteLine("CoronAI");

            List<EntityLocation> agents = new List<EntityLocation>();
            List<EntityLocation> boxes = new List<EntityLocation>();
            List<EntityLocation> agentGoals = new List<EntityLocation>();
            List<EntityLocation> boxGoals = new List<EntityLocation>();
            HashSet<Location> wallLocations = new HashSet<Location>();
            Regex colourPrefix = new Regex(@"^[a-zA-Z]+:");
            List<(string, char)> boxColours = new List<(string, char)>();
            List<(string, char)> agentColours = new List<(string, char)>();

            if (!Console.ReadLine().Equals("#domain")) throw new Exception(readError);
            if (!Console.ReadLine().Equals("hospital")) throw new Exception(readError);
            if (!Console.ReadLine().Equals("#levelname")) throw new Exception(readError);
            Console.ReadLine(); //Level name. We don't care about this.
            if (!Console.ReadLine().Equals("#colors")) throw new Exception(readError);

            // Read all colour lines
            string colourLine = Console.ReadLine();
            while (colourPrefix.IsMatch(colourLine))
            {
                string colour = colourLine.Substring(0, colourLine.IndexOf(":"));
                string entityString = colourLine.Remove(0, colour.Length + 1);
                string[] entities = entityString.Split(",");

                for (int i = 0; i < entities.Count(); i++)
                {
                    entities[i] = entities[i].Replace(" ", String.Empty);
                }
                // Iterate through each char after the "<colour>:" prefix.
                foreach (string entity in entities)
                {
                    if (entity.Length > 1) throw new Exception();
                    char cEntity = Convert.ToChar(entity);
                    if (Char.IsDigit(cEntity)) //Agent
                    {
                        agentColours.Add((colour, cEntity));
                    }
                    else //Box
                    {
                        boxColours.Add((colour, cEntity));
                    }
                }
                colourLine = Console.ReadLine();
            }

            // colourLine now contains the one after
            if (!colourLine.Equals("#initial")) throw new Exception(readError);

            int xCount = 0;
            int yCount = 0;
            string levelLine = Console.ReadLine();
            while (!levelLine.Equals("#goal"))
            {
                xCount = 0;
                foreach (char entity in levelLine.ToCharArray())
                {
                    if (entity.Equals(' '))
                    {
                        xCount++;
                        continue; //Space
                    }
                    else if (entity.Equals('+')) //Wall
                    {
                        wallLocations.Add(new Location(xCount, yCount));
                    }
                    else if (Char.IsDigit(entity)) //Agent
                    {
                        string colour = null;
                        foreach (var agent in agentColours)
                        {
                            if (agent.Item2.Equals(entity))
                            {
                                colour = agent.Item1;
                            }
                        }
                        agents.Add(new EntityLocation(new Agent(entity, colour), new Location(xCount, yCount)));
                    }
                    else //Box
                    {
                        string colour = null;
                        foreach (var box in boxColours)
                        {
                            if (box.Item2.Equals(entity))
                            {
                                colour = box.Item1;
                            }
                        }

                        //hvis der ikke er en agent med farven colour
                        bool hasAgent = false;
                        foreach (var ac in agentColours)
                        {
                            if (ac.Item1 == colour) hasAgent = true;
                        }

                        if (hasAgent) boxes.Add(new EntityLocation(new Box(entity, colour), new Location(xCount, yCount)));
                        else wallLocations.Add(new Location(xCount, yCount));
                    }

                    xCount++;
                }
                levelLine = Console.ReadLine();
                yCount++;
            }



            yCount = 0;
            levelLine = Console.ReadLine();
            while (!levelLine.Equals("#end"))
            {
                xCount = 0;
                foreach (char entity in levelLine.ToCharArray())
                {
                    if (Char.IsDigit(entity)) // If agent goal
                    {
                        Agent agent = (Agent)agents.First(a => a.Entity.Name.Equals(entity)).Entity;
                        agentGoals.Add(new EntityLocation(agent, new Location(xCount, yCount)));
                    }
                    else if (boxes.Any(b => b.Entity.Name.Equals(entity))) //(!entity.Equals(' ') && !entity.Equals('+')) //Then it must be a box //else if (!entity.Equals(' ') && !entity.Equals('+')) //Then it must be a box
                    {
                        Box box = (Box)boxes.First(b => b.Entity.Name.Equals(entity)).Entity;
                        boxGoals.Add(new EntityLocation(box, new Location(xCount, yCount)));
                    }
                    xCount++;
                }
                levelLine = Console.ReadLine();
                yCount++;
            }

            List<WorldState> initialWorldstates = AssignBoxesToAgents(agents, boxes, agentGoals, boxGoals, wallLocations);
            //List<WorldState> initialWorldstates = SimpleAssignBoxesToAgents(agents, boxes, agentGoals, boxGoals, wallLocations);

            return initialWorldstates;
        }
    }
}

