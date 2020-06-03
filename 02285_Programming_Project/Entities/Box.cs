using _02285_Programming_Project.AI;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace _02285_Programming_Project.Entities
{
    class Box : Entity, IEquatable<Box>
    {
        //public string UniqueID { get; set; } // For fetching a specific box, regardless of position


        public Box(char name, string colour)
        {
            Name = name;
            Colour = colour;
        }

        public Box(Box box)
        {
            Name = box.Name;
            Colour = box.Colour;
        }


        #region Hash and comparison tools
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Name, this.Colour);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Box);
        }

        public bool Equals([AllowNull] Box box)
        {
            return box != null && box.Name.Equals(this.Name) && box.Colour.Equals(this.Colour);
        }
        #endregion
    }
}
