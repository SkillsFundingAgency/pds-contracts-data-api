using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.Audit.Api.Client.Registrations;
using Pds.Contracts.Data.Services.DependencyInjection;
using Pds.Contracts.Data.Services.DocumentServices;
using Pds.Contracts.Data.Services.Interfaces;
using Pds.Contracts.Data.Services.Models;
using Pds.Contracts.Data.Services.Tests.Integration.DocumentServices;
using Pds.Core.Logging;
using System.IO;
using System.Threading.Tasks;
using DataModels = Pds.Contracts.Data.Repository.DataModels;

namespace Pds.Contracts.Data.Services.Tests.Integration
{
    [TestClass]
    [TestCategory("Integration")]
    public class ContractDocumentServiceTests
    {
        [TestInitialize]
        public void TestInitiaize()
        {
            BlobHelper.CreateSampleBlobFile();
        }

        [TestMethod]
        public async Task UpsertOriginalContractXmlAsync_ExpectedResult_Test()
        {
            //Arrange
            var sampleContent = "<contract xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='urn:sfa:schemas:contract'></contract>";
            var contractDocumentService = BuildContractDocumentService();
            var request = new UpdateConfirmApprovalRequest() { ContractNumber = "Main-0002", ContractVersion = 2, FileName = "sample-blob-file.xml", Id = 7 };
            var contract = new DataModels.Contract()
            {
                Id = 7,
                ContractVersion = 2,
                ContractNumber = "Main-0002",
                ContractData = new DataModels.ContractData()
                {
                    Id = 7,
                    OriginalContractXml = "<contract>sample.xml.data</contract>"
                }
            };

            //Act
            await contractDocumentService.UpsertOriginalContractXmlAsync(contract, request);

            //Assert
            contractDocumentService.Should().NotBeNull();
            contract.ContractData.OriginalContractXml.Should().Be(sampleContent);
        }

        private IContractDocumentService BuildContractDocumentService()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.development.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();
            services.AddLoggerAdapter();
            services.AddAutoMapper(typeof(FeatureServiceCollectionExtensions).Assembly);
            var policyRegistry = services.AddPolicyRegistry();
            services.AddAuditApiClient(configuration, policyRegistry);

            services.AddSingleton(s => Helpers.BlobHelper.GetBlobContainerClient(configuration));
            services.AddSingleton<IContractDocumentService, ContractDocumentService>();

            var provider = services.BuildServiceProvider();

            return provider.GetRequiredService<IContractDocumentService>();
        }
    }
}