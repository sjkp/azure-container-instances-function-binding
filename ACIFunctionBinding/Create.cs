
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using SJKP.Azure.WebJobs.Extensions.ACI;
using System.Collections.Generic;

namespace ACIFunctionBinding
{
    public static class Create
    {
        [FunctionName("Create")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", 
            Route = null)]HttpRequest req, TraceWriter log, 
            [ContainerGroup()] ICollector<Container> container)
        {
            log.Info("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            container.Add(new Container()
            {
                Cpu = 1,
                Memory = 1,
                Image = "dazzlejim/mariohtml5",
                Name = "acihello1",
                Ports = new List<Port>()
                {
                   new Port()
                   {
                       PortNumber = 8080,
                       Protocol = Protocol.Tcp,
                       Public = true
                   }
                }
            });
            container.Add(new Container()
            {
                Cpu = 1,
                Memory = 1,
                Image = "microsoft/aci-helloworld",
                Name = "acihello2",
                Ports = new List<Port>()
                {                   
                   new Port()
                   {
                       PortNumber = 81,
                       Protocol = Protocol.Tcp,
                       Public = false
                   }
                }
            });


            container.Add(new Container()
            {
                Cpu = 1,
                Memory = 1,
                Image = "dorowu/ubuntu-desktop-lxde-vnc",
                Name = "acihello3",
                Ports = new List<Port>()
                {
                   new Port()
                   {
                       PortNumber = 80,
                       Protocol = Protocol.Tcp,
                       Public = true
                   },
                    new Port()
                   {
                       PortNumber = 5900,
                       Protocol = Protocol.Tcp,
                       Public = true
                   }
                },
                EnvironmentVariables = new Dictionary<string, string>()
                {
                    {"VNC_PASSWORD", "mypassword" }
                }
            });            


            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}
