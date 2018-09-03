using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.ContainerInstance.Fluent.ContainerGroup.Definition;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SJKP.Azure.WebJobs.Extensions.ACI
{
    internal abstract class ContainerAsyncCollectorBase<T> where T : ContainerGroupBaseAttribute
    {
        private static readonly AzureServiceTokenProvider _tokenProvider = new AzureServiceTokenProvider();
        protected readonly T attribute;
        protected readonly ContainerGroupConfiguration aciConfiguration;
        public ContainerAsyncCollectorBase(ContainerGroupConfiguration aciConfiguration, T attribute)
        {
            this.attribute = attribute;

            this.aciConfiguration = aciConfiguration;
        }
        protected async Task<IContainerInstanceManager> Authenticate()
        {
            var env = AzureEnvironment.FromName(this.attribute.AzureEnvironment);
            string accessToken = await _tokenProvider.GetAccessTokenAsync(env.ResourceManagerEndpoint).ConfigureAwait(false);
            var creds = new AzureCredentials(new TokenCredentials(accessToken), new TokenCredentials(accessToken), _tokenProvider.PrincipalUsed.TenantId, env);
            return ContainerInstanceManager.Authenticate(creds, this.attribute.SubscriptionId);
        }
    }
    internal class ContainerCreateAsyncCollector : ContainerAsyncCollectorBase<ContainerGroupAttribute>, IAsyncCollector<Container>
    {
       
        
        private IWithNextContainerInstance containerInstances;

        IWithPrivateImageRegistryOrVolume withPublicOrPrivateImageRegistry = null;

        public ContainerCreateAsyncCollector(ContainerGroupConfiguration aciConfiguration, ContainerGroupAttribute attribute) : base(aciConfiguration, attribute)
        {
           
        }

        public ContainerCreateAsyncCollector(ContainerGroupConfiguration aciConfiguration, ContainerGroupWithPrivateRegistryAttribute attribute) : base(aciConfiguration, attribute)
        {

        }

        public async Task AddAsync(Container item, CancellationToken cancellationToken = default(CancellationToken))
        {
            IWithPrivateImageRegistryOrVolume o = await Setup();
            var image = o.WithoutVolume().DefineContainerInstance(item.Name).WithImage(item.Image);
            
            if (item.Ports.Count == 0)
            {
                this.containerInstances = image.WithoutPorts().WithCpuCoreCount(item.Cpu).WithMemorySizeInGB(item.Memory).Attach();
            }
            else
            {
                IWithPortsOrContainerInstanceAttach<IWithNextContainerInstance> a = null;
                foreach (var p in item.Ports.GroupBy(s => new { s.Protocol, s.Public }))
                {
                    if (p.Key.Public)
                    {
                        if (p.Key.Protocol == Protocol.Tcp)
                        {
                            a = (a ?? image as IWithPorts<IWithNextContainerInstance>).WithExternalTcpPorts(p.Select(s => s.PortNumber).ToArray());
                        }
                        if (p.Key.Protocol == Protocol.Udp)
                        {
                            a = (a ?? image as IWithPorts<IWithNextContainerInstance>).WithExternalUdpPorts(p.Select(s => s.PortNumber).ToArray());
                        }
                    }
                    else
                    {
                        if (p.Key.Protocol == Protocol.Tcp)
                        {
                            a = (a ?? image as IWithPorts<IWithNextContainerInstance>).WithInternalTcpPorts(p.Select(s => s.PortNumber).ToArray());
                        }
                        if (p.Key.Protocol == Protocol.Udp)
                        {
                            a = (a ?? image as IWithPorts<IWithNextContainerInstance>).WithInternalTcpPorts(p.Select(s => s.PortNumber).ToArray());
                        }
                    }
                }
                this.containerInstances = a.WithCpuCoreCount(item.Cpu).WithMemorySizeInGB(item.Memory).WithEnvironmentVariables(item.EnvironmentVariables ?? new Dictionary<string, string>()).Attach();
            }
            
        }

        private async Task<IWithPrivateImageRegistryOrVolume> Setup()
        {
            if (withPublicOrPrivateImageRegistry == null)
            {
                var mgr = await Authenticate();
                var a = mgr.ContainerGroups.Define(attribute.GroupName).WithRegion(Region.Create(this.attribute.AzureRegion)).WithExistingResourceGroup(attribute.AciResourceGroupName);
                IWithPublicOrPrivateImageRegistry o = null;
                if (attribute.OsType == "linux")
                {
                    o = a.WithLinux();
                }
                else
                {
                    o = a.WithWindows();
                }              
                if (attribute is ContainerGroupWithPrivateRegistryAttribute)
                {
                    var registry = attribute as ContainerGroupWithPrivateRegistryAttribute;
                    withPublicOrPrivateImageRegistry = o.WithPrivateImageRegistry(registry.ImageRegistryServer, registry.ImageRegistryUser, registry.ImageRegistryPassword);
                }
                else
                {
                    withPublicOrPrivateImageRegistry = o.WithPublicImageRegistryOnly();
                }                
            }
            return withPublicOrPrivateImageRegistry;
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await this.containerInstances.WithDnsPrefix(attribute.DnsPrefix ?? attribute.GroupName).CreateAsync(cancellationToken);
        }
    }
}
