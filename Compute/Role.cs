using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compute
{
    public class Role
    {
        public string Name { get; }
        public ConcurrentDictionary<string, RoleInstance> Instances { get; }

        public Role(string name)
        {
            this.Name = name;
            Instances = new ConcurrentDictionary<string, RoleInstance>();
        }
    }
}