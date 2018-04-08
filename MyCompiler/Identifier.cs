using System;
using System.Collections.Generic;
using System.Text;

namespace MyCompiler
{
    public class Identifier
    {
        public string name;
        public int type;
        public bool isArray;
        public int size;

        public Identifier(string name)
        {
            this.name = name;
        }

        public override bool Equals(object obj)
        {
            return name.Equals(((Identifier)obj).name);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return (isArray ? "id_array_" : "id_") + name;
        }
    }

}
