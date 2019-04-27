using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compute
{
    internal class Role
    {
        public string Name { get; }
        public IEnumerable<RoleInstance> Instances { get; }

        public Role(string name)
        {
            this.Name = name;
            Instances = new List<RoleInstance>();
        }
    }
}