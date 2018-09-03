using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SJKP.Azure.WebJobs.Extensions.ACI
{
    internal class ContainerGroupDeleteAsyncCollector : ContainerAsyncCollectorBase<DeleteContainerGroupAttribute>, IAsyncCollector<ContainerGroupDelete>
    {
        public ContainerGroupDeleteAsyncCollector(ContainerGroupConfiguration aciConfiguration, DeleteContainerGroupAttribute attribute) : base(aciConfiguration, attribute)
        {

        }
        public async Task AddAsync(ContainerGroupDelete item, CancellationToken cancellationToken = default(CancellationToken))
        {
            var mgr = await Authenticate();
            await mgr.ContainerGroups.DeleteByResourceGroupAsync(attribute.AciResourceGroupName, item.GroupName);
        }

        public Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }
    }
}
