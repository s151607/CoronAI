using System;
using System.Collections.Generic;
using System.Text;

namespace _02285_Programming_Project.Entities
{

    namespace _02285_Programming_Project.Entities
    {
        class Assignment
        {
            public Agent Agent { get; set; }
            public EntityLocation EntityLocation { get; set; }

            public Assignment(Agent agent, EntityLocation entityLocation)
            {
                Agent = agent;
                EntityLocation = entityLocation;
            }


        }
    }

}
