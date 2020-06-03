using _02285_Programming_Project.AI;
using System;
using System.Collections.Generic;
using System.Text;

namespace _02285_Programming_Project.Entities
{
    class EntityLocation : IEquatable<EntityLocation>
    {
        public Entity Entity { get; set; }
        public Location Location { get; set; }

        public EntityLocation(Entity entity, Location location)
        {
            Entity = entity;
            Location = location;
        }


        public double ManhattanDistance(EntityLocation entity)
        {
            Location loc1 = Location;
            Location loc2 = entity.Location;

            double x_dif = Math.Abs(loc1.x - loc2.x);
            double y_dif = Math.Abs(loc1.y - loc2.y);

            return x_dif + y_dif;
        }

        // Indicates if same team, used for assignment and nowhere else
        public bool Equals(EntityLocation other)
        {

        //Check whether the compared object is null.
        if (Object.ReferenceEquals(other, null)) return false;

        //Check whether the compared object references the same data.
        if (Object.ReferenceEquals(this, other)) return true;

        //Check whether the products' properties are equal.
        return this.Entity.Colour.Equals(other.Entity.Colour);
        }
    }
}
