using System;
using System.Collections.Generic;
using System.Text;

namespace SJKP.Azure.WebJobs.Extensions.ACI
{
    public class Container
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public Dictionary<string, string> EnvironmentVariables { get; set; }

        public float Cpu { get; set; }

        public float Memory { get; set; }

        public List<Port> Ports { get; set; }

    }

    public class Port
    {
        public Protocol Protocol { get; set; }

        public bool Public { get; set; }

        public int PortNumber { get; set; }
    }

    public enum Protocol
    {
        Udp,
        Tcp
    }
}

