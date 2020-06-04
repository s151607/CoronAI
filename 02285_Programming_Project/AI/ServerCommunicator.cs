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

            // Prepare output states
            List<WorldState> resultingInitStates = new List<WorldState>();
            foreach (EntityLocation agentEntity in agents)
            {
                WorldState completeInitState = new WorldState();
                completeInitState.agent = (Agent)agentEntity.Entity;
                completeInitState.agentLocation = agentEntity.Location;
                completeInitState.Walls = walls;
                completeInitState.assignedBoxes = new Dictionary<Location, Box>();
                completeInitState.boxGoals = new List<EntityLocation>();
                resultingInitStates.Add(completeInitState);
            }

            // Prepare agent-box dummies
            List<WorldState> agentBoxDummies = new List<WorldState>();
            foreach (EntityLocation agentEntity in agents)
            {
                WorldState completeInitState = new WorldState();
                completeInitState.agent = (Agent)agentEntity.Entity;
                completeInitState.agentLocation = agentEntity.Location;
                completeInitState.Walls = walls;
                completeInitState.assignedBoxes = new Dictionary<Location, Box>();
                completeInitState.boxGoals = new List<EntityLocation>();
                completeInitState.boxGoals.Add(new EntityLocation(agentEntity.Entity, agentEntity.Location));
                agentBoxDummies.Add(completeInitState);
            }
            List<(WorldState, List<(EntityLocation, int[,], float priority)>)> agentBoxDummiesWithH = BFSHeruistic.PrecalcH(agentBoxDummies);

            // Prepare goal-box dummies
            List<WorldState> boxGoalDummies = new List<WorldState>();
            foreach (EntityLocation agentEntity in agents.Distinct())
            {
                WorldState completeInitState = new WorldState();
                completeInitState.agent = (Agent)agentEntity.Entity;
                completeInitState.agentLocation = agentEntity.Location;
                completeInitState.Walls = walls;
                completeInitState.assignedBoxes = new Dictionary<Location, Box>();
                foreach (EntityLocation boxGoal in boxes.Where(box => box.Entity.Colour.Equals(completeInitState.agent.Colour)))
                {
                    completeInitState.assignedBoxes.Add(boxGoal.Location, (Box)boxGoal.Entity);
                }
                completeInitState.boxGoals = new List<EntityLocation>();
                foreach (EntityLocation boxGoal in boxGoals.Where(boxG => boxG.Entity.Colour.Equals(completeInitState.agent.Colour)))
                {
                    completeInitState.boxGoals.Add(new EntityLocation(boxGoal.Entity, boxGoal.Location));
                }
                //completeInitState.boxGoals.Add(new EntityLocation(completeInitState.agent, completeInitState.agentLocation));
                boxGoalDummies.Add(completeInitState);
            }
            List<(WorldState, List<(EntityLocation, int[,], float priority)>)> boxGoalDummiesWithH = BFSHeruistic.PrecalcH(boxGoalDummies);

            // For each colour of agent
            foreach (EntityLocation agentEnt in agents.Distinct())
            {
                string currentColour = agentEnt.Entity.Colour;

                // All states sharing a colour are assumed to be the same; i.e. the specific agent does not matter
                var completeInitState = boxGoalDummiesWithH.Where(state => state.Item1.agent.Colour.Equals(currentColour)).First();

                // The relative distance between an agent and a box is the same for all goals
                var hMatrixBoxGoal = completeInitState.Item2.First().Item2;

                var agentsOfCurrentColour = agents.Where(a => a.Entity.Colour.Equals(currentColour)).ToList();
                int agentCount = agentsOfCurrentColour.Count;

                var goalsInState = boxGoals.Where(goal => goal.Entity.Colour.Equals(currentColour)).ToList();
                var boxesInState = boxes.Where(box => box.Entity.Colour.Equals(currentColour)).ToList();

                int boxCount = boxesInState.Count;

                // If there is only one agent of this colour, then they get all goals and boxes of this colour
                if (agentCount == 1)
                {
                    resultingInitStates.Find(initState => initState.agent.Colour.Equals(currentColour)).boxGoals
                         = goalsInState;

                    if (agentGoals.Any(ag => ag.Entity.Name.Equals(agentsOfCurrentColour.First().Entity.Name)))
                    {
                        resultingInitStates.Find(ws => ws.agent.Name.Equals(agentsOfCurrentColour.First().Entity.Name)).agentGoal
                        = agentsOfCurrentColour.First().Location;
                    }

                    // I feel like this can be streamlined
                    foreach (var box in boxesInState)
                    {
                        resultingInitStates.Find(initState => initState.agent.Colour.Equals(currentColour)).assignedBoxes.Add(box.Location, (Box)box.Entity);
                    }

                    // Go to next agent colouring
                    continue;
                }

                // Find all box-goal actual distances
                int[,] goalBoxDistances = new int[boxCount, boxCount];
                int i = 0; int j = 0;
                foreach (var T2 in completeInitState.Item2) // Equivalent to "For every goal..."
                {
                    foreach (var box in boxes.Where(box => box.Entity.Colour.Equals(currentColour)
                                        && T2.Item2[box.Location.x, box.Location.y] != int.MaxValue))
                    {
                        int len = boxes.Where(box => box.Entity.Colour.Equals(currentColour)
                        && T2.Item2[box.Location.x, box.Location.y] != int.MaxValue).ToList().Count;
                        goalBoxDistances[i, j] = T2.Item2[box.Location.x, box.Location.y];
                        j++;
                    }
                    i++;
                    j = 0;
                }

                // Calculate optimal box-goal assignments
                var goalBoxAssignmentByColour = HungarianAlgorithm.FindAssignments(goalBoxDistances);

                // Translate these into box-goal pairings
                List<(EntityLocation goal, EntityLocation box)> boxGoalAssignments = new List<(EntityLocation, EntityLocation)>();
                for (int assignment = 0; assignment < goalsInState.Count; assignment++)
                {
                    boxGoalAssignments.Add((goalsInState[assignment], boxesInState[goalBoxAssignmentByColour[assignment]]));
                }


                //TODO: We should use multiple copies of each agent, to allow for assigning multiple boxes to one agent
                int ass2Size = agentCount * boxCount; // >= boxCount ? agentCount : boxCount;

                int[,] agentBoxDistances = new int[ass2Size, ass2Size];
                i = 0;
                // For every box, perform an agent-box assignment, to get boxCount dummy agents

                // Get relative agent-box distances via BFS
                for (int age = 0; age < agentCount; age++)
                {
                    foreach (var T in agentBoxDummiesWithH.Where(abd => abd.Item1.agent.Colour.Equals(currentColour)))
                    {

                        // Not complete, but based on perfect assignments!!!
                        foreach (var box in boxesInState)
                        {
                            agentBoxDistances[i, j] = Math.Abs(
                            T.Item2.First().Item2[T.Item1.agentLocation.x, T.Item1.agentLocation.y]
                            - T.Item2.First().Item2[box.Location.x, box.Location.y]);
                            j++;
                        }
                        i++;
                        j = 0;
                    }
                }

                // Calculate optimal box-goal assignments
                var agentBoxAssignmentByColour = HungarianAlgorithm.FindAssignments(agentBoxDistances);

                // Translate these into agent-box pairings
                List<(EntityLocation agent, List<EntityLocation> boxes)> agentBoxAssignments = new List<(EntityLocation, List<EntityLocation>)>();
                for (int a = 0; a < agentCount; a++) agentBoxAssignments.Add((agentsOfCurrentColour[a], new List<EntityLocation>()));

                for (int assignment = agentBoxAssignmentByColour.Length-1; assignment > agentBoxAssignmentByColour.Length-1 - agentCount; assignment--)
                {
                    var box = boxesInState[agentBoxAssignmentByColour[assignment] % boxCount];
                    var agentBoxPair = agentBoxAssignments[assignment % agentCount];
                    agentBoxPair.boxes.Add(box);
                }

                // Fill initial states
                //TODO: Multiple goals and boxes can be added!
                foreach (var agentBoxPair in agentBoxAssignments)
                {
                    // Add agent goal if it exists
                    if (agentGoals.Any(ag => ag.Entity.Name.Equals(agentBoxPair.agent.Entity.Name)))
                    {
                        resultingInitStates.Find(ws => ws.agent.Name.Equals(agentBoxPair.agent.Entity.Name)).agentGoal
                        = agentBoxPair.agent.Location;
                    }

                    
                    foreach (var box in agentBoxPair.boxes)
                    {
                        // Add all optimal goals
                        resultingInitStates.Find(ws => ws.agent.Name.Equals(agentBoxPair.agent.Entity.Name)).boxGoals
                            .Add(boxGoalAssignments.Find(bg => bg.box.Location.Equals(box.Location)).goal);

                        // Add all boxes
                        resultingInitStates.Find(ws => ws.agent.Name.Equals(agentBoxPair.agent.Entity.Name)).assignedBoxes
                        .Add(box.Location,
                        (Box)box.Entity);
                    }
                }
            }
            return resultingInitStates;
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
            //string[] fileLines = System.IO.File.ReadAllLines(@"C:\Users\asger\Desktop\02285_Programming_Project-restructuring\02285_Programming_Project\Levels\comp20MA\MATheZoo.lvl");
            string[] fileLines = System.IO.File.ReadAllLines(@"C:\Users\Count\source\repos\CoronAI\02285_Programming_Project\Levels\Comp20\MACoronAI.lvl");

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

