using Microsoft.Azure.WebJobs.Description;
using System;
using System.Collections.Generic;

namespace SJKP.Azure.WebJobs.Extensions.ACI
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public class ContainerGroupAttribute : ContainerGroupBaseAttribute
    {
       

        [AppSetting(Default = @"AciOsType")]
        public string OsType { get; set; }

      


        /// <summary>
        /// <see cref="Microsoft.Azure.Management.ResourceManager.Fluent.Core.Region"></see>, e.g. westeurope
        /// </summary>
        [AppSetting(Default = @"AciAzureRegion")]
        public string AzureRegion { get; set; }

        [AppSetting(Default = @"AciGroupName")]
        public string GroupName { get; set; }
        public string DnsPrefix { get; set; }
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public sealed class ContainerGroupWithPrivateRegistryAttribute : ContainerGroupAttribute
    {
        public string ImageRegistryServer { get; set; }
        public string ImageRegistryUser { get; set; }

        public string ImageRegistryPassword { get; set; }
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public sealed class DeleteContainerGroupAttribute : ContainerGroupBaseAttribute
    {
       
    }

    public abstract class ContainerGroupBaseAttribute : Attribute
    {

        /// <summary>
        /// <see cref="Microsoft.Azure.Management.ResourceManager.Fluent.AzureEnvironment"/>, e.g. AzureGlobalCloud
        /// </summary>
        [AppSetting(Default = @"AciAzureEnvironment")]
        public string AzureEnvironment { get; set; }
        [AppSetting(Default = @"AciResourceGroupName")]
        public string AciResourceGroupName { get; set; }

        [AppSetting(Default = @"AciSubscriptionId")]
        public string SubscriptionId { get; set; }
    }
}
