using System;
using _02285_Programming_Project.AI;
using _02285_Programming_Project.Entities;
using _02285_Programming_Project.Properties;
using System.Collections.Generic;
using static _02285_Programming_Project.Planning.ConstraintState;
using System.Diagnostics;
using System.Threading;
using _02285_Programming_Project.Planning;
using System.Linq;

namespace _02285_Programming_Project.AI
{
    class Program
    {

        static void Main(string[] args)
        {
            bool isDebug = false;

            if (isDebug == false)
            {
                List<WorldState> initialStates = ServerCommunicator.readLevel();
                List<(WorldState, List<(EntityLocation, int[,], float priority)>)> hMatrix = BFSHeruistic.PrecalcH(initialStates);
                var testCBS = new CBS(hMatrix);
                var testSolution = testCBS.findSolution();
                List<string> lines = ServerCommunicator.printSolutionToFile(testSolution);

                int counter = 0;
                foreach (string line in lines)
                {
                    Console.WriteLine(line);
                    string result = Console.ReadLine();

                    counter++;
                    if (!result.Contains("false")) continue;
                    else throw new Exception(line + result + "counter = " + counter );
                    
                }
            }
            else
            { 
                var initialStates = ServerCommunicator.readLevelFromFile();
                List<(WorldState, List<(EntityLocation, int[,], float priority)>)> hMatrix = BFSHeruistic.PrecalcH(initialStates);
                var testCBS = new CBS(hMatrix);
                var testSolution = testCBS.findSolution();
                List<string> lines = ServerCommunicator.printSolutionToFile(testSolution);
                throw new Exception("at end");
            }


            /*
            Box box1 = new Box('a', "red");
            Box box2 = new Box('a', "red");

            Location location1 = new Location(3, 3);
            Location location2 = new Location(3, 3);

            var hash1 = HashCode.Combine(box1, location1);
            var hash2 = HashCode.Combine(box2, location2);
            */

            //var test = ServerCommunicator.readLevel();
            /// SINGLE AGENT TESTING//
            /*
            Console.WriteLine("TESTAI");
            var initialStates = ServerCommunicator.readLevelFromFile();
            if (initialStates.Count > 1) throw new Exception();

            var watch1 = System.Diagnostics.Stopwatch.StartNew();
            //List<WorldState> plan1 = initialStates[0].agent.SearchClient.MakePlan(initialStates[0], new HashSet<Constraint>());
            watch1.Stop();
            var elapsedMs1 = watch1.ElapsedMilliseconds;

            var watch2 = System.Diagnostics.Stopwatch.StartNew();
            List<WorldState> plan2 = Astar.MakePlan(initialStates[0].agent, initialStates[0], new HashSet<Constraint>());
            watch2.Stop();
            var elapsedMs2 = watch2.ElapsedMilliseconds;

            List<List<WorldState>> solution = new List<List<WorldState>>();
            solution.Add(plan2);
            List<string> lines = ServerCommunicator.printSolutionToFile(solution);

            foreach (string line in lines)
            {
                Console.WriteLine(line);
                string result = Console.ReadLine();

                if (!result.Contains("false")) continue;
                else throw new Exception(line + result);
            }

            throw new Exception("at end");
         
            */

            //////////////////////



            /*
            bool runDebug = true;
            if (runDebug)
            {
                var test = ServerCommunicator.readLevelFromFile();

                var testCBS = new CBS(test);

                var testSolution = testCBS.findSolution();
                List<string> lines = ServerCommunicator.printSolutionToFile(testSolution);


                throw new Exception("We are at the end");
            }
            else
            {
                var test = ServerCommunicator.readLevel();
                var testCBS = new CBS(test);
                var testSolution = testCBS.findSolution();
                List<string> lines = ServerCommunicator.printSolutionToFile(testSolution);

                foreach (string line in lines)
                {
                    Console.WriteLine(line);
                    string result = Console.ReadLine();

                    if (!result.Contains("false")) continue;
                    else throw new Exception(line + result);
                }
            }*/
        }
    }
}
