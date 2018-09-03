using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Newtonsoft.Json.Linq;

namespace SJKP.Azure.WebJobs.Extensions.ACI
{
    public class ContainerGroupConfiguration : IExtensionConfigProvider
    {         

        public void Initialize(ExtensionConfigContext context)
        {
            // add converter between JObject and SlackMessage
            // Allows a user to bind to IAsyncCollector<JObject>, and the sdk will convert that to IAsyncCollector<SlackMessage>
            context.AddConverter<JObject, Container>(input => input.ToObject<Container>());

            context.AddConverter<JObject, ContainerGroupDelete>(input => input.ToObject<ContainerGroupDelete>());

            // Add a binding rule for Collector
            context.AddBindingRule<ContainerGroupAttribute>()
                .BindToCollector<Container>(attr => new ContainerCreateAsyncCollector(this, attr));

            // Add a binding rule for Collector
            context.AddBindingRule<ContainerGroupWithPrivateRegistryAttribute>()
                .BindToCollector<Container>(attr => new ContainerCreateAsyncCollector(this, attr));

            // Add a binding rule for Collector
            context.AddBindingRule<DeleteContainerGroupAttribute>()
                .BindToCollector<ContainerGroupDelete>(attr => new ContainerGroupDeleteAsyncCollector(this, attr));
        }
    }    
}