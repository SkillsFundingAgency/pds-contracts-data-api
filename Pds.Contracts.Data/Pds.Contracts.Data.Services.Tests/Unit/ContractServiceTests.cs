using AutoMapper;
using Azure.Storage.Blobs;
using FluentAssertions;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Audit.Api.Client.Enumerations;
using Pds.Audit.Api.Client.Interfaces;
using Pds.Contracts.Data.Common.CustomExceptionHandlers;
using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Common.Responses;
using Pds.Contracts.Data.Repository.Implementations;
using Pds.Contracts.Data.Repository.Interfaces;
using Pds.Contracts.Data.Services.Implementations;
using Pds.Contracts.Data.Services.Interfaces;
using Pds.Contracts.Data.Services.Models;
using Pds.Contracts.Data.Services.Responses;
using Pds.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuditModels = Pds.Audit.Api.Client.Models;
using DataModels = Pds.Contracts.Data.Repository.DataModels;

namespace Pds.Contracts.Data.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class ContractServiceTests
    {
        private readonly IContractRepository _contractRepository
            = Mock.Of<IContractRepository>(MockBehavior.Strict);

        private readonly IMapper _mapper
            = Mock.Of<IMapper>(MockBehavior.Strict);

        private readonly Uri blobUri = new Uri("https://www.blob-container.com/");
        private readonly Mock<IAuditService> _mockAuditService
            = new Mock<IAuditService>(MockBehavior.Strict);

        private readonly Mock<ILoggerAdapter<ContractService>> _mockLogger
            = new Mock<ILoggerAdapter<ContractService>>(MockBehavior.Strict);

        private readonly ISemaphoreOnEntity<string> _semaphoreOnEntity
            = Mock.Of<ISemaphoreOnEntity<string>>(MockBehavior.Strict);

        private readonly IDocumentManagementContractService _mockDocumentService
            = Mock.Of<IDocumentManagementContractService>(MockBehavior.Strict);

        private readonly IContractValidationService _mockContractValidator
            = Mock.Of<IContractValidationService>(MockBehavior.Strict);

        private readonly IMediator _mockMediator
           = Mock.Of<IMediator>(MockBehavior.Strict);

        private readonly IContractDocumentService _contractDocumentService
           = Mock.Of<IContractDocumentService>(MockBehavior.Strict);

        private IUriService _mockUriService = Mock.Of<IUriService>(MockBehavior.Strict);

        #region CreateAsync


        #region AmendmentType - None

        [TestMethod]
        public async Task Create_AmendmentTypeNone_WhenContractDoesNotAlreadyExist_Then_ContractIsAddedToDatabase()
        {
            // Arrange
            DateTime timeCheckUTC = DateTime.UtcNow;
            CreateContractRequest createRequest = Generate_CreateContractRequest();
            createRequest.AmendmentType = ContractAmendmentType.None;

            SetupSemaphoreOnEntity();
            SetupLogger_LogInformationMethod();
            SetMockContractDocumentService();

            IEnumerable<DataModels.Contract> matchedRecords = new List<DataModels.Contract>();

            var contractRecord = new DataModels.Contract();
            contractRecord.Status = -1;

            Mock.Get(_mapper)
                .Setup(p => p.Map<DataModels.Contract>(createRequest))
                .Returns(contractRecord)
                .Verifiable();

            SetupRepository_GetByContractNumberAsyncMethod(createRequest, matchedRecords);
            SetupRepository_CreateAsyncMethod(contractRecord, Task.CompletedTask);
            Mock.Get(_mockContractValidator)
                .Setup(p => p.ValidateForNewContract(createRequest, matchedRecords));

            SetupMediator_Publish();

            var service = GetContractService();

            // Act
            await service.CreateAsync(createRequest);

            // Assert
            VerifyAll();
            contractRecord.Status.Should().Be((int)ContractStatus.PublishedToProvider);
            contractRecord.CreatedAt.Should().BeAfter(timeCheckUTC);
            contractRecord.LastUpdatedAt.Should().BeAfter(timeCheckUTC);
            Mock.Get(_mockMediator).Verify(x => x.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()), Times.Once());
            Mock.Get(_contractDocumentService)
                .Verify(d => d.UpsertOriginalContractXmlAsync(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()), Times.Once);
        }

        [TestMethod]
        public async Task Create_AmendmentTypeNone_WhenContractAlreadyExists_WithALowerVersion_Then_ContractIsAddedToDatabase_And_NoStatusChangeHappen()
        {
            // Arrange
            CreateContractRequest createRequest = Generate_CreateContractRequest();
            createRequest.AmendmentType = ContractAmendmentType.None;

            SetupSemaphoreOnEntity();
            SetupLogger_LogInformationMethod();
            SetMockContractDocumentService();

            IEnumerable<DataModels.Contract> matchedRecords = new List<DataModels.Contract>()
            {
                new DataModels.Contract()
                {
                    ContractNumber = createRequest.ContractNumber,
                    ContractVersion = createRequest.ContractVersion - 1,
                    Status = (int)ContractStatus.PublishedToProvider
                }
            };

            var contractRecord = new DataModels.Contract();
            contractRecord.Status = -1;

            Mock.Get(_mapper)
                .Setup(p => p.Map<DataModels.Contract>(createRequest))
                .Returns(contractRecord)
                .Verifiable();

            SetupRepository_GetByContractNumberAsyncMethod(createRequest, matchedRecords);
            SetupRepository_CreateAsyncMethod(contractRecord, Task.CompletedTask);
            Mock.Get(_mockContractValidator)
                .Setup(p => p.ValidateForNewContract(createRequest, matchedRecords));

            SetupMediator_Publish();

            var service = GetContractService();

            // Act
            await service.CreateAsync(createRequest);

            // Assert
            VerifyAll();
            contractRecord.Status.Should().Be((int)ContractStatus.PublishedToProvider);
            matchedRecords.First().Status.Should().Be((int)ContractStatus.PublishedToProvider);
            Mock.Get(_mockMediator).Verify(x => x.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()), Times.Once());
            Mock.Get(_contractDocumentService)
                .Verify(d => d.UpsertOriginalContractXmlAsync(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()), Times.Once);
        }

        [TestMethod]
        public void Create_AmendmentTypeNone_WhenContractAlreadyExists_WithTheSameContractVersion_Then_ExceptionIsRaised()
        {
            // Arrange
            CreateContractRequest createRequest = Generate_CreateContractRequest();
            createRequest.AmendmentType = ContractAmendmentType.None;

            SetupSemaphoreOnEntity();
            SetupLogger_LogInformationMethod();
            SetMockContractDocumentService();

            IEnumerable<DataModels.Contract> matchedRecords = new List<DataModels.Contract>()
            {
                new DataModels.Contract()
                {
                    ContractNumber = createRequest.ContractNumber,
                    ContractVersion = createRequest.ContractVersion
                }
            };

            SetupRepository_GetByContractNumberAsyncMethod(createRequest, matchedRecords);
            Mock.Get(_mockContractValidator)
                .Setup(p => p.ValidateForNewContract(createRequest, matchedRecords))
                .Throws(new DuplicateContractException(createRequest.ContractNumber, createRequest.ContractVersion));

            SetupMediator_Publish();

            var service = GetContractService();

            // Act
            Func<Task> act = async () => await service.CreateAsync(createRequest);

            // Assert
            act.Should().Throw<DuplicateContractException>();

            VerifyAll();
            Mock.Get(_mockMediator).Verify(x => x.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()), Times.Never);
            Mock.Get(_contractDocumentService)
                .Verify(d => d.UpsertOriginalContractXmlAsync(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()), Times.Never);
        }

        [TestMethod]
        public void Create_AmendmentTypeNone_WhenContractAlreadyExists_WithAHigherContractVersion_Then_ExceptionIsRaised()
        {
            // Arrange
            CreateContractRequest createRequest = Generate_CreateContractRequest();
            createRequest.AmendmentType = ContractAmendmentType.None;

            SetupSemaphoreOnEntity();
            SetupLogger_LogInformationMethod();
            SetMockContractDocumentService();

            IEnumerable<DataModels.Contract> matchedRecords = new List<DataModels.Contract>()
            {
                new DataModels.Contract()
                {
                    ContractNumber = createRequest.ContractNumber,
                    ContractVersion = createRequest.ContractVersion + 1
                }
            };

            SetupRepository_GetByContractNumberAsyncMethod(createRequest, matchedRecords);
            Mock.Get(_mockContractValidator)
                .Setup(p => p.ValidateForNewContract(createRequest, matchedRecords))
                .Throws(new ContractWithHigherVersionAlreadyExistsException(createRequest.ContractNumber, createRequest.ContractVersion));

            SetupMediator_Publish();

            var service = GetContractService();

            // Act
            Func<Task> act = async () => await service.CreateAsync(createRequest);

            // Assert
            act.Should().Throw<ContractWithHigherVersionAlreadyExistsException>();

            VerifyAll();
            Mock.Get(_mockMediator).Verify(x => x.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()), Times.Never);
            Mock.Get(_contractDocumentService)
                .Verify(d => d.UpsertOriginalContractXmlAsync(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()), Times.Never);
        }

        #endregion


        #region AmendmentType - Variation

        [TestMethod]
        public async Task Create_AmendmentTypeVariation_WhenNoOtherVersionOfTheContractExist_Then_ContractIsAddedToDatabase_But_NoStatusChangesHappen()
        {
            // Arrange
            CreateContractRequest createRequest = Generate_CreateContractRequest();
            createRequest.AmendmentType = ContractAmendmentType.Variation;

            SetupSemaphoreOnEntity();
            SetupLogger_LogInformationMethod();
            SetMockContractDocumentService();

            IEnumerable<DataModels.Contract> matchedRecords = new List<DataModels.Contract>();

            var contractRecord = new DataModels.Contract();
            contractRecord.Status = -1;

            Mock.Get(_mapper)
                .Setup(p => p.Map<DataModels.Contract>(createRequest))
                .Returns(contractRecord)
                .Verifiable();

            SetupRepository_GetByContractNumberAsyncMethod(createRequest, matchedRecords);
            SetupRepository_CreateAsyncMethod(contractRecord, Task.CompletedTask);
            Mock.Get(_mockContractValidator)
                .Setup(p => p.ValidateForNewContract(createRequest, matchedRecords));

            SetupMediator_Publish();

            var service = GetContractService();

            // Act
            await service.CreateAsync(createRequest);

            // Assert
            VerifyAll();
            contractRecord.Status.Should().Be((int)ContractStatus.PublishedToProvider);
            Mock.Get(_mockMediator).Verify(x => x.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()), Times.Once);
            Mock.Get(_contractDocumentService)
                .Verify(d => d.UpsertOriginalContractXmlAsync(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()), Times.Once);
        }

        [TestMethod]
        public async Task Create_AmendmentTypeVariation_WhenOtherVersionOfTheContractExist_ButNotAtStatus0_Then_ContractIsAddedToDatabase_But_NoStatusChangesHappen()
        {
            // Arrange
            int expectedStatus = (int)ContractStatus.Approved;
            CreateContractRequest createRequest = Generate_CreateContractRequest();
            createRequest.AmendmentType = ContractAmendmentType.Variation;

            SetupSemaphoreOnEntity();
            SetupLogger_LogInformationMethod();
            SetMockContractDocumentService();

            IEnumerable<DataModels.Contract> matchedRecords = new List<DataModels.Contract>()
            {
                new DataModels.Contract()
                {
                    ContractNumber = createRequest.ContractNumber,
                    ContractVersion = createRequest.ContractVersion - 1,
                    Status = expectedStatus
                }
            };

            var contractRecord = new DataModels.Contract();
            contractRecord.Status = -1;

            Mock.Get(_mapper)
                .Setup(p => p.Map<DataModels.Contract>(createRequest))
                .Returns(contractRecord)
                .Verifiable();

            SetupRepository_GetByContractNumberAsyncMethod(createRequest, matchedRecords);
            SetupRepository_CreateAsyncMethod(contractRecord, Task.CompletedTask);
            Mock.Get(_mockContractValidator)
                .Setup(p => p.ValidateForNewContract(createRequest, matchedRecords));

            SetupMediator_Publish();

            var service = GetContractService();

            // Act
            await service.CreateAsync(createRequest);

            // Assert
            VerifyAll();
            contractRecord.Status.Should().Be((int)ContractStatus.PublishedToProvider);
            matchedRecords.First().Status.Should().Be(expectedStatus);
            Mock.Get(_mockMediator).Verify(x => x.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()), Times.Once);
            Mock.Get(_contractDocumentService)
                .Verify(d => d.UpsertOriginalContractXmlAsync(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()), Times.Once);
        }

        [TestMethod]
        public async Task Create_AmendmentTypeVariation_WhenOtherVersionOfTheContractExist_WithStatus0_Then_ContractIsAddedToDatabase_And_ContractIsReplaced()
        {
            // Arrange
            int contractId = 123;
            int expectedAuditCount = 1;

            CreateContractRequest createRequest = Generate_CreateContractRequest();
            createRequest.AmendmentType = ContractAmendmentType.Variation;

            SetupSemaphoreOnEntity();
            SetupAuditService_TrySendAuditAsyncMethod(new ActionType[] { ActionType.ContractReplaced });
            SetupLogger_LogInformationMethod();
            SetMockContractDocumentService();

            IEnumerable<DataModels.Contract> matchedRecords = new List<DataModels.Contract>()
            {
                new DataModels.Contract()
                {
                    Id = contractId,
                    ContractNumber = createRequest.ContractNumber,
                    ContractVersion = createRequest.ContractVersion - 1,
                    Status = (int)ContractStatus.PublishedToProvider
                }
            };

            var contractRecord = new DataModels.Contract();
            contractRecord.Status = -1;

            Mock.Get(_mapper)
                .Setup(p => p.Map<DataModels.Contract>(createRequest))
                .Returns(contractRecord)
                .Verifiable();

            SetupRepository_GetByContractNumberAsyncMethod(createRequest, matchedRecords);
            SetupRepository_CreateAsyncMethod(contractRecord, Task.CompletedTask);

            Mock.Get(_mockContractValidator)
                .Setup(p => p.ValidateForNewContract(createRequest, matchedRecords));

            Mock.Get(_contractRepository)
                .Setup(p => p.UpdateContractStatusAsync(contractId, ContractStatus.PublishedToProvider, ContractStatus.Replaced))
                .Returns(Task.FromResult(new UpdatedContractStatusResponse()))
                .Verifiable();

            SetupMediator_Publish();

            var service = GetContractService();

            // Act
            await service.CreateAsync(createRequest);

            // Assert
            VerifyAll();
            contractRecord.Status.Should().Be((int)ContractStatus.PublishedToProvider);
            _mockAuditService.Verify(p => p.TrySendAuditAsync(It.IsAny<AuditModels.Audit>()), Times.Exactly(expectedAuditCount));
            Mock.Get(_mockMediator).Verify(x => x.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()), Times.Once);
            Mock.Get(_contractDocumentService)
                .Verify(d => d.UpsertOriginalContractXmlAsync(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()), Times.Once);
        }

        [TestMethod]
        public async Task Create_AmendmentTypeVariation_WhenMultipleOtherVersionOfTheContractExist_WithSomeAtStatus0_Then_ContractIsAddedToDatabase_And_ContractAllRelevantPreviousReplaced()
        {
            // Arrange
            int contractId = 123;
            int expectedAuditCount = 2;

            CreateContractRequest createRequest = Generate_CreateContractRequest();
            createRequest.AmendmentType = ContractAmendmentType.Variation;

            SetupSemaphoreOnEntity();
            SetupAuditService_TrySendAuditAsyncMethod(new ActionType[] { ActionType.ContractReplaced });
            SetupLogger_LogInformationMethod();
            SetMockContractDocumentService();

            IEnumerable<DataModels.Contract> matchedRecords = new List<DataModels.Contract>()
            {
                new DataModels.Contract()
                {
                    Id = contractId,
                    ContractNumber = createRequest.ContractNumber,
                    ContractVersion = createRequest.ContractVersion - 1,
                    Status = (int)ContractStatus.PublishedToProvider
                },
                new DataModels.Contract()
                {
                    Id = contractId - 1,
                    ContractNumber = createRequest.ContractNumber,
                    ContractVersion = createRequest.ContractVersion - 1,
                    Status = (int)ContractStatus.PublishedToProvider
                },
                new DataModels.Contract()
                {
                    Id = contractId - 2,
                    ContractNumber = createRequest.ContractNumber,
                    ContractVersion = createRequest.ContractVersion - 2,
                    Status = (int)ContractStatus.Approved
                }
            };

            var contractRecord = new DataModels.Contract();
            contractRecord.Status = -1;

            Mock.Get(_mapper)
                .Setup(p => p.Map<DataModels.Contract>(createRequest))
                .Returns(contractRecord)
                .Verifiable();

            SetupRepository_GetByContractNumberAsyncMethod(createRequest, matchedRecords);
            SetupRepository_CreateAsyncMethod(contractRecord, Task.CompletedTask);

            Mock.Get(_mockContractValidator)
                .Setup(p => p.ValidateForNewContract(createRequest, matchedRecords));

            Mock.Get(_contractRepository)
                .Setup(p => p.UpdateContractStatusAsync(contractId, ContractStatus.PublishedToProvider, ContractStatus.Replaced))
                .Returns(Task.FromResult(new UpdatedContractStatusResponse()))
                .Verifiable();
            Mock.Get(_contractRepository)
                .Setup(p => p.UpdateContractStatusAsync(contractId - 1, ContractStatus.PublishedToProvider, ContractStatus.Replaced))
                .Returns(Task.FromResult(new UpdatedContractStatusResponse()))
                .Verifiable();

            SetupMediator_Publish();

            var service = GetContractService();

            // Act
            await service.CreateAsync(createRequest);

            // Assert
            VerifyAll();
            contractRecord.Status.Should().Be((int)ContractStatus.PublishedToProvider);
            _mockAuditService.Verify(p => p.TrySendAuditAsync(It.IsAny<AuditModels.Audit>()), Times.Exactly(expectedAuditCount));
            Mock.Get(_mockMediator).Verify(x => x.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()), Times.Once);
            Mock.Get(_contractDocumentService)
                .Verify(d => d.UpsertOriginalContractXmlAsync(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()), Times.Once);
        }

        #endregion


        #region AmendmentType - Notification

        [TestMethod]
        public async Task Create_AmendmentTypeNotification_WhenContractAlreadyExists_WithALowerVersion_Then_ContractIsAddedToDatabase_And_NoStatusChangeHappen()
        {
            // Arrange
            int expectedStatus = (int)ContractStatus.Replaced;
            CreateContractRequest createRequest = Generate_CreateContractRequest();
            createRequest.AmendmentType = ContractAmendmentType.Notfication;

            SetupSemaphoreOnEntity();
            SetupLogger_LogInformationMethod();
            SetMockContractDocumentService();

            IEnumerable<DataModels.Contract> matchedRecords = new List<DataModels.Contract>()
            {
                new DataModels.Contract()
                {
                    ContractNumber = createRequest.ContractNumber,
                    ContractVersion = createRequest.ContractVersion - 1,
                    Status = expectedStatus
                }
            };

            var contractRecord = new DataModels.Contract();
            contractRecord.Status = -1;
            contractRecord.AmendmentType = (int)createRequest.AmendmentType;

            Mock.Get(_mapper)
                .Setup(p => p.Map<DataModels.Contract>(createRequest))
                .Returns(contractRecord)
                .Verifiable();

            SetupRepository_GetByContractNumberAsyncMethod(createRequest, matchedRecords);
            SetupRepository_CreateAsyncMethod(contractRecord, Task.CompletedTask);
            Mock.Get(_mockContractValidator)
                .Setup(p => p.ValidateForNewContract(createRequest, matchedRecords));

            SetupMediator_Publish();

            var service = GetContractService();

            // Act
            await service.CreateAsync(createRequest);

            // Assert
            VerifyAll();
            contractRecord.Status.Should().Be((int)ContractStatus.Approved);
            matchedRecords.First().Status.Should().Be(expectedStatus);
            Mock.Get(_mockMediator).Verify(x => x.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()), Times.Once);
            Mock.Get(_contractDocumentService)
                .Verify(d => d.UpsertOriginalContractXmlAsync(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()), Times.Once);
        }

        [TestMethod]
        public async Task Create_AmendmentTypeNotification_WhenNoOtherVersionOfTheContractExist_Then_ContractIsAddedToDatabase_But_NoStatusChangesHappen()
        {
            // Arrange
            CreateContractRequest createRequest = Generate_CreateContractRequest();
            createRequest.AmendmentType = ContractAmendmentType.Notfication;

            SetupSemaphoreOnEntity();
            SetupLogger_LogInformationMethod();
            SetMockContractDocumentService();

            IEnumerable<DataModels.Contract> matchedRecords = new List<DataModels.Contract>();

            var contractRecord = new DataModels.Contract();
            contractRecord.Status = -1;
            contractRecord.AmendmentType = (int)createRequest.AmendmentType;

            Mock.Get(_mapper)
                .Setup(p => p.Map<DataModels.Contract>(createRequest))
                .Returns(contractRecord)
                .Verifiable();

            SetupRepository_GetByContractNumberAsyncMethod(createRequest, matchedRecords);
            SetupRepository_CreateAsyncMethod(contractRecord, Task.CompletedTask);
            Mock.Get(_mockContractValidator)
                .Setup(p => p.ValidateForNewContract(createRequest, matchedRecords));

            SetupMediator_Publish();

            var service = GetContractService();

            // Act
            await service.CreateAsync(createRequest);

            // Assert
            VerifyAll();
            contractRecord.Status.Should().Be((int)ContractStatus.Approved);
            Mock.Get(_mockMediator).Verify(x => x.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()), Times.Once);
            Mock.Get(_contractDocumentService)
                .Verify(d => d.UpsertOriginalContractXmlAsync(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()), Times.Once);
        }

        [TestMethod]
        public async Task Create_AmendmentTypeNotification_WhenOtherVersionOfTheContractExist_WithStatus0_Then_ContractIsAddedToDatabase_And_ContractIsReplaced()
        {
            // Arrange
            int contractId = 123;
            int expectedAuditCount = 1;
            ContractStatus existingContractStatus = ContractStatus.PublishedToProvider;

            CreateContractRequest createRequest = Generate_CreateContractRequest();
            createRequest.AmendmentType = ContractAmendmentType.Notfication;

            SetupSemaphoreOnEntity();
            SetupAuditService_TrySendAuditAsyncMethod(new ActionType[] { ActionType.ContractReplaced });
            SetupLogger_LogInformationMethod();
            SetMockContractDocumentService();

            IEnumerable<DataModels.Contract> matchedRecords = new List<DataModels.Contract>()
            {
                new DataModels.Contract()
                {
                    Id = contractId,
                    ContractNumber = createRequest.ContractNumber,
                    ContractVersion = createRequest.ContractVersion - 1,
                    Status = (int)existingContractStatus
                }
            };

            var contractRecord = new DataModels.Contract();
            contractRecord.Status = -1;
            contractRecord.AmendmentType = (int)createRequest.AmendmentType;

            Mock.Get(_mapper)
                .Setup(p => p.Map<DataModels.Contract>(createRequest))
                .Returns(contractRecord)
                .Verifiable();

            SetupRepository_GetByContractNumberAsyncMethod(createRequest, matchedRecords);
            SetupRepository_CreateAsyncMethod(contractRecord, Task.CompletedTask);

            Mock.Get(_mockContractValidator)
                .Setup(p => p.ValidateForNewContract(createRequest, matchedRecords));

            Mock.Get(_contractRepository)
                .Setup(p => p.UpdateContractStatusAsync(contractId, existingContractStatus, ContractStatus.Replaced))
                .Returns(Task.FromResult(new UpdatedContractStatusResponse()))
                .Verifiable();

            SetupMediator_Publish();

            var service = GetContractService();

            // Act
            await service.CreateAsync(createRequest);

            // Assert
            VerifyAll();
            contractRecord.Status.Should().Be((int)ContractStatus.Approved);
            _mockAuditService.Verify(p => p.TrySendAuditAsync(It.IsAny<AuditModels.Audit>()), Times.Exactly(expectedAuditCount));
            Mock.Get(_mockMediator).Verify(x => x.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()), Times.Once);
            Mock.Get(_contractDocumentService)
                .Verify(d => d.UpsertOriginalContractXmlAsync(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()), Times.Once);
        }

        [TestMethod]
        public async Task Create_AmendmentTypeNotification_WhenOtherVersionOfTheContractExist_WithStatus4_Then_ContractIsAddedToDatabase_And_ContractIsReplaced()
        {
            // Arrange
            int contractId = 123;
            int expectedAuditCount = 1;
            ContractStatus existingContractStatus = ContractStatus.Approved;

            CreateContractRequest createRequest = Generate_CreateContractRequest();
            createRequest.AmendmentType = ContractAmendmentType.Notfication;

            SetupSemaphoreOnEntity();
            SetupAuditService_TrySendAuditAsyncMethod(new ActionType[] { ActionType.ContractReplaced });
            SetupLogger_LogInformationMethod();
            SetMockContractDocumentService();

            IEnumerable<DataModels.Contract> matchedRecords = new List<DataModels.Contract>()
            {
                new DataModels.Contract()
                {
                    Id = contractId,
                    ContractNumber = createRequest.ContractNumber,
                    ContractVersion = createRequest.ContractVersion - 1,
                    Status = (int)existingContractStatus
                }
            };

            var contractRecord = new DataModels.Contract();
            contractRecord.Status = -1;
            contractRecord.AmendmentType = (int)createRequest.AmendmentType;

            Mock.Get(_mapper)
                .Setup(p => p.Map<DataModels.Contract>(createRequest))
                .Returns(contractRecord)
                .Verifiable();

            SetupRepository_GetByContractNumberAsyncMethod(createRequest, matchedRecords);
            SetupRepository_CreateAsyncMethod(contractRecord, Task.CompletedTask);

            Mock.Get(_mockContractValidator)
                .Setup(p => p.ValidateForNewContract(createRequest, matchedRecords));

            Mock.Get(_contractRepository)
                .Setup(p => p.UpdateContractStatusAsync(contractId, existingContractStatus, ContractStatus.Replaced))
                .Returns(Task.FromResult(new UpdatedContractStatusResponse()))
                .Verifiable();

            SetupMediator_Publish();

            var service = GetContractService();

            // Act
            await service.CreateAsync(createRequest);

            // Assert
            VerifyAll();
            contractRecord.Status.Should().Be((int)ContractStatus.Approved);
            _mockAuditService.Verify(p => p.TrySendAuditAsync(It.IsAny<AuditModels.Audit>()), Times.Exactly(expectedAuditCount));
            Mock.Get(_mockMediator).Verify(x => x.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()), Times.Once);
            Mock.Get(_contractDocumentService)
                .Verify(d => d.UpsertOriginalContractXmlAsync(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()), Times.Once);
        }

        [TestMethod]
        public async Task Create_AmendmentTypeNotification_WhenOtherVersionOfTheContractExist_WithStatus5_Then_ContractIsAddedToDatabase_And_ContractIsReplaced()
        {
            // Arrange
            int contractId = 123;
            int expectedAuditCount = 1;
            ContractStatus existingContractStatus = ContractStatus.ApprovedWaitingConfirmation;

            CreateContractRequest createRequest = Generate_CreateContractRequest();
            createRequest.AmendmentType = ContractAmendmentType.Notfication;

            SetupSemaphoreOnEntity();
            SetupAuditService_TrySendAuditAsyncMethod(new ActionType[] { ActionType.ContractReplaced });
            SetupLogger_LogInformationMethod();
            SetMockContractDocumentService();

            IEnumerable<DataModels.Contract> matchedRecords = new List<DataModels.Contract>()
            {
                new DataModels.Contract()
                {
                    Id = contractId,
                    ContractNumber = createRequest.ContractNumber,
                    ContractVersion = createRequest.ContractVersion - 1,
                    Status = (int)existingContractStatus
                }
            };

            var contractRecord = new DataModels.Contract();
            contractRecord.Status = -1;
            contractRecord.AmendmentType = (int)createRequest.AmendmentType;

            Mock.Get(_mapper)
                .Setup(p => p.Map<DataModels.Contract>(createRequest))
                .Returns(contractRecord)
                .Verifiable();

            SetupRepository_GetByContractNumberAsyncMethod(createRequest, matchedRecords);
            SetupRepository_CreateAsyncMethod(contractRecord, Task.CompletedTask);

            Mock.Get(_mockContractValidator)
                .Setup(p => p.ValidateForNewContract(createRequest, matchedRecords));

            Mock.Get(_contractRepository)
                .Setup(p => p.UpdateContractStatusAsync(contractId, existingContractStatus, ContractStatus.Replaced))
                .Returns(Task.FromResult(new UpdatedContractStatusResponse()))
                .Verifiable();

            SetupMediator_Publish();

            var service = GetContractService();

            // Act
            await service.CreateAsync(createRequest);

            // Assert
            VerifyAll();
            contractRecord.Status.Should().Be((int)ContractStatus.Approved);
            _mockAuditService.Verify(p => p.TrySendAuditAsync(It.IsAny<AuditModels.Audit>()), Times.Exactly(expectedAuditCount));
            Mock.Get(_mockMediator).Verify(x => x.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()), Times.Once);
            Mock.Get(_contractDocumentService)
                .Verify(d => d.UpsertOriginalContractXmlAsync(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()), Times.Once);
        }

        #endregion


        #endregion


        [TestMethod]
        public async Task GetAsync_ReturnsExpectedResult()
        {
            // Arrange
            var expectedServiceModel = Mock.Of<Models.Contract>();
            var expectedDataModel = Mock.Of<DataModels.Contract>();
            var mockRepo = Mock.Of<IContractRepository>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetAsync(It.IsAny<int>()))
                .ReturnsAsync(expectedDataModel)
                .Verifiable();

            SetUpMockUriService("action");

            SetupLogger_LogInformationMethod();

            var mockMapper = Mock.Of<IMapper>();
            Mock.Get(mockMapper)
                .Setup(m => m.Map<Models.Contract>(It.IsAny<DataModels.Contract>()))
                .Returns(expectedServiceModel)
                .Verifiable();

            SetupAuditService_AuditAsyncMethod();

            var contractService = new ContractService(mockRepo, mockMapper, _mockUriService, _mockLogger.Object, _mockAuditService.Object, _semaphoreOnEntity, _mockDocumentService, _mockContractValidator, _mockMediator, _contractDocumentService);

            // Act
            var actual = await contractService.GetAsync(1);

            // Assert
            actual.Should().Be(expectedServiceModel);
            Mock.Get(mockRepo).Verify(r => r.GetAsync(It.IsAny<int>()), Times.Once);
            Mock.Get(mockMapper).Verify(m => m.Map<Models.Contract>(It.IsAny<DataModels.Contract>()), Times.Once);
        }

        [TestMethod]
        public async Task GetByContractNumber_ReturnsExpectedResultAsync()
        {
            // Arrange
            var expectedServiceModelCollection = Mock.Of<IList<Models.Contract>>();
            var expectedDataModelCollection = Mock.Of<IQueryable<DataModels.Contract>>();
            var mockRepo = Mock.Of<IContractRepository>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetByContractNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedDataModelCollection)
                .Verifiable();

            SetUpMockUriService("action");

            SetupLogger_LogInformationMethod();

            var mockMapper = Mock.Of<IMapper>();
            Mock.Get(mockMapper)
                .Setup(m => m.Map<IList<Models.Contract>>(It.IsAny<IQueryable<DataModels.Contract>>()))
                .Returns(expectedServiceModelCollection)
                .Verifiable();

            SetupAuditService_AuditAsyncMethod();

            var contractService = new ContractService(mockRepo, mockMapper, _mockUriService, _mockLogger.Object, _mockAuditService.Object, _semaphoreOnEntity, _mockDocumentService, _mockContractValidator, _mockMediator, _contractDocumentService);

            // Act
            var actual = await contractService.GetByContractNumberAsync("some-contract-number");

            // Assert
            actual.Should().BeEquivalentTo(expectedServiceModelCollection);
            Mock.Get(mockRepo).Verify(r => r.GetByContractNumberAsync("some-contract-number"), Times.Once);
            Mock.Get(mockMapper).Verify(m => m.Map<IList<Models.Contract>>(It.IsAny<IQueryable<DataModels.Contract>>()), Times.Once);
        }

        [TestMethod]
        public async Task GetByContractNumberAndVersion_ReturnsExpectedResultAsync()
        {
            // Arrange
            var expectedServiceModel = Mock.Of<Models.Contract>();
            var expectedDataModel = Mock.Of<DataModels.Contract>();
            var mockRepo = Mock.Of<IContractRepository>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetByContractNumberAndVersionAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(expectedDataModel)
                .Verifiable();

            SetUpMockUriService("action");

            SetupLogger_LogInformationMethod();

            var mockMapper = Mock.Of<IMapper>();
            Mock.Get(mockMapper)
                .Setup(m => m.Map<Models.Contract>(It.IsAny<DataModels.Contract>()))
                .Returns(expectedServiceModel)
                .Verifiable();

            SetupAuditService_AuditAsyncMethod();

            var contractService = new ContractService(mockRepo, mockMapper, _mockUriService, _mockLogger.Object, _mockAuditService.Object, _semaphoreOnEntity, _mockDocumentService, _mockContractValidator, _mockMediator, _contractDocumentService);

            // Act
            var actual = await contractService.GetByContractNumberAndVersionAsync("some-contract-number", 1);

            // Assert
            actual.Should().Be(expectedServiceModel);
            Mock.Get(mockRepo).Verify(r => r.GetByContractNumberAndVersionAsync("some-contract-number", 1), Times.Once);
            Mock.Get(mockMapper).Verify(m => m.Map<Models.Contract>(It.IsAny<DataModels.Contract>()), Times.Once);
        }

        [TestMethod]
        public async Task GetContractRemindersAsync_ReturnsExpectedResultAsync()
        {
            // Arrange
            int reminderInterval = 14;
            int pageNumber = 1;
            int pageSize = 1;
            ContractSortOptions sort = ContractSortOptions.LastUpdatedAt;
            SortDirection order = SortDirection.Asc;
            DateTime currentDateTimeMinusNumberOfDays = DateTime.UtcNow.Date.AddDays(-reminderInterval).AddHours(23).AddMinutes(59);
            string requestPath = "/api/contractReminder";
            string queryString = "?reminderInterval=14&page={page}&count=2";
            string routeTemplateURL = requestPath + queryString;

            var expectedDummyMetadata = new Metadata() { CurrentPage = 1, PageSize = pageSize, TotalCount = 1, TotalPages = 1, NextPageUrl = string.Empty, PreviousPageUrl = string.Empty };

            var expectedDummyReminderItems = new List<ContractReminderItem>();
            var expectedDummyServiceModel = new ContractReminderResponse<IEnumerable<ContractReminderItem>>(expectedDummyReminderItems)
            {
                Paging = expectedDummyMetadata
            };

            var expectedDummyDataModel = new PagedList<DataModels.Contract>(new List<DataModels.Contract>(), 1, 1, 1);

            var mockRepo = Mock.Of<IContractRepository>(MockBehavior.Strict);
            Mock.Get(mockRepo)
                .Setup(r => r.GetContractRemindersAsync(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ContractSortOptions>(), It.IsAny<SortDirection>()))
               .ReturnsAsync(expectedDummyDataModel)
               .Verifiable();

            SetUpMockUriService(routeTemplateURL);
            SetupLogger_LogInformationMethod();

            var mockMapper = Mock.Of<IMapper>();
            Mock.Get(mockMapper)
                .Setup(m => m.Map<IEnumerable<Models.ContractReminderItem>>(new List<DataModels.Contract>()))
                .Returns(expectedDummyReminderItems)
                .Verifiable();

            SetupAuditService_AuditAsyncMethod();

            var contractService = new ContractService(mockRepo, mockMapper, _mockUriService, _mockLogger.Object, _mockAuditService.Object, _semaphoreOnEntity, _mockDocumentService, _mockContractValidator, _mockMediator, _contractDocumentService);

            // Act
            var actual = await contractService.GetContractRemindersAsync(reminderInterval, pageNumber, pageSize, sort, order, routeTemplateURL);

            // Assert
            actual.Should().BeEquivalentTo(expectedDummyServiceModel);
            Mock.Get(mockRepo).Verify(r => r.GetContractRemindersAsync(currentDateTimeMinusNumberOfDays, pageNumber, pageSize, sort, order), Times.Once);
            Mock.Get(mockMapper).Verify(m => m.Map<IEnumerable<Models.ContractReminderItem>>(new List<DataModels.Contract>()), Times.Once);
            _mockLogger.Verify();
        }

        [TestMethod]
        public async Task UpdateLastEmailReminderSentAndLastUpdatedAt_TestAsync_SuccessResultExpected()
        {
            // Arrange
            Contract dummyServiceModel = new Contract() { Id = 1, ContractVersion = 1, ContractNumber = "abc" };
            var expectedDataModel = Mock.Of<DataModels.Contract>();
            var mockRepo = Mock.Of<IContractRepository>(MockBehavior.Strict);
            Mock.Get(mockRepo)
              .Setup(r => r.UpdateLastEmailReminderSentAndLastUpdatedAtAsync(It.IsAny<int>()))
              .ReturnsAsync(expectedDataModel)
              .Verifiable();

            string actionUrl = "action";
            var mockUriService = Mock.Of<IUriService>(MockBehavior.Strict);
            Mock.Get(mockUriService)
                .Setup(m => m.GetUri(actionUrl))
                .Returns(It.IsAny<Uri>())
                .Verifiable();

            var mockMapper = Mock.Of<IMapper>();
            Mock.Get(mockMapper)
                .Setup(m => m.Map<Models.Contract>(It.IsAny<DataModels.Contract>()))
                .Returns(dummyServiceModel)
                .Verifiable();

            SetupAuditService_AuditAsyncMethod();

            var contractService = new ContractService(mockRepo, mockMapper, mockUriService, _mockLogger.Object, _mockAuditService.Object, _semaphoreOnEntity, _mockDocumentService, _mockContractValidator, _mockMediator, _contractDocumentService);

            var reminder = GetASingleUpdateLastEmailReminderSentRequest();

            // Act
            var result = await contractService.UpdateLastEmailReminderSentAndLastUpdatedAtAsync(reminder);

            // Assert
            result.Should().NotBeNull();
            Mock.Get(mockRepo).Verify(r => r.UpdateLastEmailReminderSentAndLastUpdatedAtAsync(reminder.Id), Times.Exactly(1));
            Mock.Get(mockMapper).Verify(m => m.Map<Models.Contract>(It.IsAny<DataModels.Contract>()), Times.Once);
            _mockLogger.Verify();
        }

        #region ContractDocumentService



        #endregion


        #region UpdateContractConfirmApproval Tests

        [TestMethod]
        public async Task ConfirmApprovalAsync_TestAsync_SuccessResultExpected()
        {
            // Arrange
            var dummyData = GetDummyDataModelsContract_Datas();
            SetMockRepo_GetContractWithContractDataAsync_mockDataModel(dummyData);
            SetMockRepo_UpdateContractAsync();
            SetUpMockUriService("action");
            SetupLogger_LogInformationMethod();
            SetupAuditService_TrySendAuditAsyncMethod(new ActionType[] { ActionType.ContractConfirmApproval });
            SetMockContractDocumentService();
            SetMockContractValidator_Validate();
            SetupMediator_Publish();
            var contractService = GetContractService();
            var request = new UpdateConfirmApprovalRequest() { Id = 7, ContractNumber = "Main-0002", ContractVersion = 2, FileName = "Main-0002_v2_637503726040684393.xml" };

            // Act
            var result = await contractService.ConfirmApprovalAsync(request);

            // Assert
            result.Should().NotBeNull();
            Mock.Get(_contractRepository)
               .Verify(r => r.GetContractWithDatasAsync(It.IsAny<int>()), Times.Once);
            Mock.Get(_mockContractValidator)
               .Verify(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>(), c => c.ContractData != null), Times.Never);
            Mock.Get(_mockContractValidator)
              .Verify(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), true), Times.Never);
            Mock.Get(_contractDocumentService)
                .Verify(d => d.UpsertOriginalContractXmlAsync(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()), Times.Once);
            Mock.Get(_contractRepository).Verify(r => r.UpdateContractAsync(It.IsAny<DataModels.Contract>()), Times.Once);
            _mockLogger.Verify();
        }

        [TestMethod]
        public void ConfirmApprovalAsync_Test_ResultContractStatusExceptionExpected()
        {
            // Arrange
            var dummyData = GetDummyDataModelsContract_Datas();
            SetMockRepo_GetContractWithContractDataAsync_mockDataModel(dummyData);
            SetMockRepo_UpdateContractAsync();
            SetUpMockUriService("action");
            SetupLogger_LogInformationMethod();
            SetupAuditService_TrySendAuditAsyncMethod(new ActionType[] { ActionType.ContractConfirmApproval });
            SetMockContractDocumentService();
            SetMockContractValidator_Validate();
            SetMockContractValidator_Validate_ContractStatusException();
            var contractService = GetContractService();
            var request = new UpdateConfirmApprovalRequest() { Id = 7, ContractNumber = "Main-0002", ContractVersion = 2, FileName = "Main-0002_v2_637503726040684393.xml" };

            // Act
            Func<Task> result = async () => await contractService.ConfirmApprovalAsync(request);

            // Assert
            result.Should().Throw<ContractStatusException>();
            Mock.Get(_contractRepository)
                .Verify(r => r.GetContractWithDatasAsync(It.IsAny<int>()), Times.Once);
            Mock.Get(_mockContractValidator)
                .Verify(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>(), c => c.ContractData != null), Times.Never);
            Mock.Get(_mockContractValidator)
                .Verify(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), true), Times.Never);
            Mock.Get(_contractDocumentService)
                .Verify(d => d.UpsertOriginalContractXmlAsync(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()), Times.Never);
            Mock.Get(_contractRepository).Verify(r => r.UpdateContractAsync(It.IsAny<DataModels.Contract>()), Times.Never);
            _mockAuditService.Verify(e => e.TrySendAuditAsync(It.IsAny<AuditModels.Audit>()), Times.Never);
            _mockLogger.Verify();
        }

        [TestMethod]
        public void ConfirmApprovalAsync_ContractNullTest_ResultContractNotFoundExceptionExpected()
        {
            // Arrange
            SetMockRepo_GetContractWithContractDataAsync_mockDataModel(null);
            SetMockRepo_UpdateContractAsync();
            SetUpMockUriService("action");
            SetupLogger_LogInformationMethod();
            SetupAuditService_TrySendAuditAsyncMethod(new ActionType[] { ActionType.ContractConfirmApproval });
            SetMockContractDocumentService();
            SetMockContractValidator_Validate_ContractNotFoundException();
            var contractService = GetContractService();
            var request = new UpdateConfirmApprovalRequest() { Id = 7, ContractNumber = "Main-0002", ContractVersion = 2, FileName = "Main-0002_v2_637503726040684393.xml" };

            // Act
            Func<Task> result = async () => await contractService.ConfirmApprovalAsync(request);

            // Assert
            result.Should().Throw<ContractNotFoundException>();
            Mock.Get(_contractRepository)
                .Verify(r => r.GetContractWithDatasAsync(It.IsAny<int>()), Times.Once);
            Mock.Get(_mockContractValidator)
                .Verify(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>(), c => c.ContractData != null), Times.Never);
            Mock.Get(_mockContractValidator)
                .Verify(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), true), Times.Never);
            Mock.Get(_contractDocumentService)
                .Verify(d => d.UpsertOriginalContractXmlAsync(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()), Times.Never);
            Mock.Get(_contractRepository).Verify(r => r.UpdateContractAsync(It.IsAny<DataModels.Contract>()), Times.Never);
            _mockAuditService.Verify(e => e.TrySendAuditAsync(It.IsAny<AuditModels.Audit>()), Times.Never);
            _mockLogger.Verify();
        }

        [TestMethod]
        public void ConfirmApprovalAsync_ContractNullTest_ResultBlobNoContentExceptionExpected()
        {
            // Arrange
            var dummyData = GetDummyDataModelsContract_Datas();
            dummyData.ContractData.OriginalContractXml = null;
            SetMockRepo_GetContractWithContractDataAsync_mockDataModel(dummyData);
            SetMockRepo_UpdateContractAsync();
            SetUpMockUriService("action");
            SetupLogger_LogInformationMethod();
            SetupAuditService_TrySendAuditAsyncMethod(new ActionType[] { ActionType.ContractConfirmApproval });
            SetMockContractDocumentService();
            SetMockContractValidator_Validate();
            SetMockContractValidator_Validate_BlobNoContentException();
            var contractService = GetContractService();
            var request = new UpdateConfirmApprovalRequest() { Id = 7, ContractNumber = "Main-0002", ContractVersion = 2, FileName = "Main-0002_v2_637503726040684393.xml" };

            // Act
            Func<Task> result = async () => await contractService.ConfirmApprovalAsync(request);

            // Assert
            result.Should().Throw<BlobNoContentException>();
            Mock.Get(_contractRepository)
                .Verify(r => r.GetContractWithDatasAsync(It.IsAny<int>()), Times.Once);
            Mock.Get(_mockContractValidator)
                .Verify(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>(), c => c.ContractData != null), Times.Never);
            Mock.Get(_mockContractValidator)
                .Verify(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), true), Times.Never);
            Mock.Get(_contractDocumentService)
                .Verify(d => d.UpsertOriginalContractXmlAsync(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()), Times.Never);
            Mock.Get(_contractRepository).Verify(r => r.UpdateContractAsync(It.IsAny<DataModels.Contract>()), Times.Never);
            _mockAuditService.Verify(e => e.TrySendAuditAsync(It.IsAny<AuditModels.Audit>()), Times.Never);
            _mockLogger.Verify();
        }

        [TestMethod]
        public void ConfirmApprovalAsync_ContractNullTest_ResultBlobExceptionExpected()
        {
            // Arrange
            var dummyData = GetDummyDataModelsContract_Datas();
            SetMockRepo_GetContractWithContractDataAsync_mockDataModel(dummyData);
            SetMockRepo_UpdateContractAsync();
            SetUpMockUriService("action");
            SetupLogger_LogInformationMethod();
            SetupAuditService_TrySendAuditAsyncMethod(new ActionType[] { ActionType.ContractConfirmApproval });
            SetMockContractDocumentService();
            SetMockContractValidator_Validate();
            SetMockContractValidator_Validate_BlobException();
            var contractService = GetContractService();
            var request = new UpdateConfirmApprovalRequest() { Id = 7, ContractNumber = "Main-0002", ContractVersion = 2, FileName = null };

            // Act
            Func<Task> result = async () => await contractService.ConfirmApprovalAsync(request);

            // Assert
            result.Should().Throw<BlobException>();
            Mock.Get(_contractRepository)
                .Verify(r => r.GetContractWithDatasAsync(It.IsAny<int>()), Times.Once);
            Mock.Get(_mockContractValidator)
                .Verify(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>(), c => c.ContractData != null), Times.Never);
            Mock.Get(_mockContractValidator)
                .Verify(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), true), Times.Never);
            Mock.Get(_contractDocumentService)
                .Verify(d => d.UpsertOriginalContractXmlAsync(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()), Times.Never);
            Mock.Get(_contractRepository).Verify(r => r.UpdateContractAsync(It.IsAny<DataModels.Contract>()), Times.Never);
            _mockAuditService.Verify(e => e.TrySendAuditAsync(It.IsAny<AuditModels.Audit>()), Times.Never);
            _mockLogger.Verify();
        }

        #endregion UpdateContractConfirmApproval Tests


        #region ApproveManuallyAsync tests

        [TestMethod]
        public async Task ApproveManuallyAsync_TestAsync_SuccessResultExpected()
        {
            // Arrange
            var dummyData = GetDummyDataModelsContract();
            SetMockRepo_ApproveManuallyAsync_mockDataModel(dummyData);
            SetUpMockUriService("action");
            SetupLogger_LogInformationMethod();
            SetMockDocumentService();
            SetMockContractValidator();
            SetMockContractDocumentService();
            SetupMediator_Publish();
            var contractService = GetContractService();
            var request = GetContractRequest();

            // Act
            var result = await contractService.ApproveManuallyAsync(request);

            // Assert
            result.Should().NotBeNull();
            Mock.Get(_contractRepository)
               .Verify(r => r.GetContractWithContentAndDatasAsync(It.IsAny<int>()), Times.Once);
            Mock.Get(_mockContractValidator)
               .Verify(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>(), c => c.ContractContent != null), Times.Once);
            Mock.Get(_mockContractValidator)
              .Verify(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), true), Times.Once);
            Mock.Get(_mockDocumentService)
                .Verify(d => d.AddSignedDocumentPage(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), true, ContractFundingType.CityDeals, null), Times.Once);
            Mock.Get(_contractDocumentService)
                .Verify(d => d.UpsertOriginalContractXmlAsync(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()), Times.Once);
            Mock.Get(_contractRepository).Verify(r => r.UpdateContractAsync(It.IsAny<DataModels.Contract>()), Times.Once);
            Mock.Get(_mockMediator).Verify(x => x.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()), Times.Once());

            _mockLogger.Verify();
        }

        [TestMethod]
        public void ApproveManuallyAsync_Test_ResultContractStatusExceptionExpected()
        {
            // Arrange
            var dummyData = GetDummyDataModelsContract();
            SetMockRepo_ApproveManuallyAsync_mockDataModel(dummyData);
            SetUpMockUriService("action");
            SetupLogger_LogInformationMethod();
            SetMockDocumentService();
            SetMockContractValidator_ContractStatusException();
            SetMockContractDocumentService();
            SetupMediator_Publish();
            var contractService = GetContractService();
            var request = GetContractRequest();

            // Act
            Func<Task> act = async () => await contractService.ApproveManuallyAsync(request);

            // Assert
            act.Should().Throw<ContractStatusException>();
            Mock.Get(_contractRepository)
                .Verify(r => r.GetContractWithContentAndDatasAsync(It.IsAny<int>()), Times.Once);
            Mock.Get(_mockContractValidator)
                .Verify(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>(), c => c.ContractContent != null), Times.Once);
            Mock.Get(_mockContractValidator)
                .Verify(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), true), Times.Once);
            Mock.Get(_mockDocumentService)
                .Verify(d => d.AddSignedDocumentPage(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), true, ContractFundingType.CityDeals, null), Times.Never);
            Mock.Get(_contractDocumentService)
               .Verify(d => d.UpsertOriginalContractXmlAsync(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()), Times.Never);
            Mock.Get(_contractRepository).Verify(r => r.UpdateContractAsync(It.IsAny<DataModels.Contract>()), Times.Never);
            Mock.Get(_mockMediator).Verify(x => x.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()), Times.Never());
            _mockLogger.Verify();
        }

        [TestMethod]
        public void ApproveManuallyAsync_ContractNullTest_ResultContractNotFoundExceptionExpected()
        {
            // Arrange
            SetMockRepo_ApproveManuallyAsync_mockDataModel(null);
            SetUpMockUriService("action");
            SetupLogger_LogInformationMethod();
            SetMockDocumentService();
            SetMockContractValidator_ContractNotFoundException();
            SetMockContractDocumentService();
            SetupMediator_Publish();
            var contractService = GetContractService();
            var request = GetContractRequest();

            // Act
            Func<Task> act = async () => await contractService.ApproveManuallyAsync(request);

            // Assert
            act.Should().Throw<ContractNotFoundException>();
            Mock.Get(_contractRepository)
                .Verify(r => r.GetContractWithContentAndDatasAsync(It.IsAny<int>()), Times.Once);
            Mock.Get(_mockContractValidator)
                .Verify(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>(), c => c.ContractContent != null), Times.Once);
            Mock.Get(_mockContractValidator)
                .Verify(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), true), Times.Never);
            Mock.Get(_mockDocumentService)
                .Verify(d => d.AddSignedDocumentPage(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), true, ContractFundingType.CityDeals, null), Times.Never);
            Mock.Get(_contractDocumentService)
               .Verify(d => d.UpsertOriginalContractXmlAsync(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()), Times.Never);
            Mock.Get(_contractRepository).Verify(r => r.UpdateContractAsync(It.IsAny<DataModels.Contract>()), Times.Never);
            Mock.Get(_mockMediator).Verify(x => x.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()), Times.Never());
            _mockLogger.Verify();
        }

        [TestMethod]
        public void ApproveManuallyAsync_ContractContentNullTest_ResultContractExpectationFailedExceptionExpected()
        {
            // Arrange
            var dummyData = GetDummyDataModelsContract();
            dummyData.ContractContent = null;
            SetMockRepo_ApproveManuallyAsync_mockDataModel(dummyData);
            SetUpMockUriService("action");
            SetupLogger_LogInformationMethod();
            SetMockDocumentService();
            SetMockContractValidator_ContractExpectationFailedException();
            SetMockContractDocumentService();
            SetupMediator_Publish();
            var contractService = GetContractService();
            var request = GetContractRequest();

            // Act
            Func<Task> act = async () => await contractService.ApproveManuallyAsync(request);

            // Assert
            act.Should().Throw<ContractExpectationFailedException>();
            Mock.Get(_contractRepository)
                .Verify(r => r.GetContractWithContentAndDatasAsync(It.IsAny<int>()), Times.Once);
            Mock.Get(_mockContractValidator)
                .Verify(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>(), c => c.ContractContent != null), Times.Once);
            Mock.Get(_mockContractValidator)
                .Verify(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), true), Times.Never);
            Mock.Get(_mockDocumentService)
                .Verify(d => d.AddSignedDocumentPage(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), true, ContractFundingType.CityDeals, null), Times.Never);
            Mock.Get(_contractDocumentService)
               .Verify(d => d.UpsertOriginalContractXmlAsync(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()), Times.Never);
            Mock.Get(_contractRepository).Verify(r => r.UpdateContractAsync(It.IsAny<DataModels.Contract>()), Times.Never);
            Mock.Get(_mockMediator).Verify(x => x.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()), Times.Never());
            _mockLogger.Verify();
        }

        [TestMethod]
        public void ApproveManuallyAsync_ContractNumberTest_ResultInvalidContractRequestExceptionExpected()
        {
            // Arrange
            var dummyData = GetDummyDataModelsContract();
            dummyData.ContractNumber = "xyz";
            SetMockRepo_ApproveManuallyAsync_mockDataModel(dummyData);
            SetUpMockUriService("action");
            SetupLogger_LogInformationMethod();
            SetMockDocumentService();
            SetMockContractValidator_InvalidContractRequestException();
            SetMockContractDocumentService();
            SetupMediator_Publish();
            var contractService = GetContractService();
            var request = GetContractRequest();

            // Act
            Func<Task> act = async () => await contractService.ApproveManuallyAsync(request);

            // Assert
            act.Should().Throw<InvalidContractRequestException>();
            Mock.Get(_contractRepository)
                .Verify(r => r.GetContractWithContentAndDatasAsync(It.IsAny<int>()), Times.Once);
            Mock.Get(_mockContractValidator)
               .Verify(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>(), c => c.ContractContent != null), Times.Once);
            Mock.Get(_mockContractValidator)
                .Verify(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), true), Times.Never);
            Mock.Get(_mockDocumentService)
                .Verify(d => d.AddSignedDocumentPage(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), true, ContractFundingType.CityDeals, null), Times.Never);
            Mock.Get(_contractDocumentService)
               .Verify(d => d.UpsertOriginalContractXmlAsync(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()), Times.Never);
            Mock.Get(_contractRepository)
                .Verify(r => r.UpdateContractAsync(It.IsAny<DataModels.Contract>()), Times.Never);
            Mock.Get(_mockMediator).Verify(x => x.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()), Times.Never());
            _mockLogger.Verify();
        }

        [TestMethod]
        public void ApproveManuallyAsync_TestAsync_ResultContractUpdateConcurrencyExceptionExpected()
        {
            // Arrange
            var dummyData = GetDummyDataModelsContract();
            SetMockRepo_ApproveManuallyAsync_ContractUpdateConcurrencyException(dummyData);
            SetUpMockUriService("action");
            SetupLogger_LogInformationMethod();
            SetMockDocumentService();
            SetMockContractValidator();
            SetMockContractDocumentService();
            SetupMediator_Publish();
            var contractService = GetContractService();
            var request = GetContractRequest();

            // Act
            Func<Task> act = async () => await contractService.ApproveManuallyAsync(request);

            // Assert
            act.Should().Throw<ContractUpdateConcurrencyException>();
            Mock.Get(_contractRepository)
               .Verify(r => r.GetContractWithContentAndDatasAsync(It.IsAny<int>()), Times.Once);
            Mock.Get(_mockContractValidator)
               .Verify(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>(), c => c.ContractContent != null), Times.Once);
            Mock.Get(_mockContractValidator)
              .Verify(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), true), Times.Once);
            Mock.Get(_mockDocumentService)
                .Verify(d => d.AddSignedDocumentPage(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), true, ContractFundingType.CityDeals, null), Times.Once);
            Mock.Get(_contractDocumentService)
              .Verify(d => d.UpsertOriginalContractXmlAsync(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()), Times.Once);
            Mock.Get(_contractRepository).Verify(r => r.UpdateContractAsync(It.IsAny<DataModels.Contract>()), Times.Once);
            Mock.Get(_mockMediator).Verify(x => x.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockLogger.Verify();
        }

        #endregion UpdateContractManualApprove tests


        #region UpdateContractWithdrawalAsync

        [TestMethod]
        public async Task UpdateContractWithdrawalAsync_TestAsync_SuccessResultExpected()
        {
            // Arrange
            var mockDataModel = GetDataModelsContract();
            SetMockRepo_GetContractWithContractDataAsync_mockDataModel(mockDataModel);
            SetMockRepo_GetAsyncUpdateContractAsync(mockDataModel);
            SetUpMockUriService("action");
            SetupLogger_LogInformationMethod();
            SetMockkContractValidator_WithdrawalRequest();
            SetupAuditService_TrySendAuditAsyncMethod(new ActionType[] { ActionType.ContractConfirmApproval });
            SetupMediator_Publish();
            SetMockContractDocumentService();
            var contractService = GetContractService();
            var request = GetASingleUpdateContractWithdrawalRequest();

            // Act
            var result = await contractService.WithdrawalAsync(request);

            // Assert
            result.Should().NotBeNull();
            Mock.Get(_contractRepository)
              .Verify(r => r.GetAsync(It.IsAny<int>()), Times.Never);
            Mock.Get(_mockContractValidator)
            .Verify(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<UpdateContractWithdrawalRequest>()), Times.Once);
            Mock.Get(_mockContractValidator)
              .Verify(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), false), Times.Once);
            Mock.Get(_contractDocumentService)
                .Verify(d => d.UpsertOriginalContractXmlAsync(It.IsAny<DataModels.Contract>(), It.IsAny<UpdateContractWithdrawalRequest>()), Times.Once);
            Mock.Get(_contractRepository).Verify(r => r.UpdateContractAsync(It.IsAny<DataModels.Contract>()), Times.Once);
            Mock.Get(_mockMediator).Verify(x => x.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockLogger.Verify();
        }

        [TestMethod]
        public void UpdateContractWithdrawalAsync_TestAsync_ResultContractStatusExceptionExpected()
        {
            // Arrange
            var mockDataModel = GetDataModelsContract();
            SetMockRepo_GetContractWithContractDataAsync_mockDataModel(mockDataModel);
            SetMockRepo_GetAsyncUpdateContractAsync(mockDataModel);
            SetUpMockUriService("action");
            SetupLogger_LogInformationMethod();
            SetMockkContractValidator_WithdrawalRequest_ContractStatusException();
            SetupMediator_Publish();
            var contractService = GetContractService();
            var request = GetASingleUpdateContractWithdrawalRequest();

            // Act
            Func<Task> act = async () => await contractService.WithdrawalAsync(request);

            // Assert
            act.Should().Throw<ContractStatusException>();
            Mock.Get(_contractRepository)
                .Verify(r => r.GetContractWithDatasAsync(It.IsAny<int>()), Times.Once);
            Mock.Get(_contractRepository)
                .Verify(r => r.GetAsync(It.IsAny<int>()), Times.Never);
            Mock.Get(_mockContractValidator)
                .Verify(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<UpdateContractWithdrawalRequest>()), Times.Once);
            Mock.Get(_mockContractValidator)
                .Verify(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), false), Times.Once);
            Mock.Get(_contractRepository).Verify(r => r.UpdateContractAsync(It.IsAny<DataModels.Contract>()), Times.Never);
            Mock.Get(_mockMediator).Verify(x => x.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockLogger.Verify();
        }

        [TestMethod]
        public void UpdateContractWithdrawalAsync_TestAsync_ResultContractNotFoundExceptionExpected()
        {
            // Arrange
            var mockDataModel = GetDataModelsContract();
            SetMockRepo_GetContractWithContractDataAsync_mockDataModel(mockDataModel);
            SetMockRepo_GetAsyncUpdateContractAsync(mockDataModel);
            SetUpMockUriService("action");
            SetupLogger_LogInformationMethod();
            SetMockkContractValidator_WithdrawalRequest_ContractNotFoundException();
            SetupMediator_Publish();
            SetMockContractDocumentService();

            //SetMockContractValidator_Validate_ContractNotFoundException();
            var contractService = GetContractService();
            var request = GetASingleUpdateContractWithdrawalRequest();

            // Act
            Func<Task> act = async () => await contractService.WithdrawalAsync(request);

            // Assert
            act.Should().Throw<ContractNotFoundException>();

            Mock.Get(_contractRepository)
                .Verify(r => r.GetContractWithDatasAsync(It.IsAny<int>()), Times.Once);
            Mock.Get(_contractRepository)
                .Verify(r => r.GetAsync(It.IsAny<int>()), Times.Never);
            Mock.Get(_mockContractValidator)
                .Verify(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<UpdateContractWithdrawalRequest>()), Times.Once);
            Mock.Get(_mockContractValidator)
                .Verify(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), false), Times.Never);
            Mock.Get(_contractRepository).Verify(r => r.UpdateContractAsync(It.IsAny<DataModels.Contract>()), Times.Never);
            Mock.Get(_mockMediator).Verify(x => x.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockLogger.Verify();
        }

        [TestMethod]
        public void UpdateContractWithdrawalAsync_ContractNumberTest_ResultInvalidContractRequestExceptionExpected()
        {
            // Arrange
            var mockDataModel = GetDataModelsContract();
            mockDataModel.ContractNumber = "xyz";
            SetMockRepo_GetContractWithContractDataAsync_mockDataModel(mockDataModel);
            SetMockRepo_GetAsyncUpdateContractAsync(mockDataModel);
            SetUpMockUriService("action");
            SetupLogger_LogInformationMethod();
            SetMockContractDocumentService();
            SetMockkContractValidator_WithdrawalRequest_InvalidContractRequestException();
            SetupMediator_Publish();
            var contractService = GetContractService();
            var request = GetASingleUpdateContractWithdrawalRequest();

            // Act
            Func<Task> act = async () => await contractService.WithdrawalAsync(request);

            // Assert
            act.Should().Throw<InvalidContractRequestException>();
            Mock.Get(_contractRepository)
                .Verify(r => r.GetContractWithDatasAsync(It.IsAny<int>()), Times.Once);
            Mock.Get(_contractRepository)
                .Verify(r => r.GetAsync(It.IsAny<int>()), Times.Never);
            Mock.Get(_mockContractValidator)
                .Verify(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<UpdateContractWithdrawalRequest>()), Times.Once);
            Mock.Get(_mockContractValidator)
                .Verify(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), false), Times.Never);
            Mock.Get(_contractRepository).Verify(r => r.UpdateContractAsync(It.IsAny<DataModels.Contract>()), Times.Never);
            Mock.Get(_mockMediator).Verify(x => x.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockLogger.Verify();
        }

        [TestMethod]
        public void UpdateContractWithdrawalAsync_InvalidWithdrawalTypeTest_ResultInvalidContractRequestExceptionExpected()
        {
            // Arrange
            var mockDataModel = GetDataModelsContract();
            SetMockRepo_GetContractWithContractDataAsync_mockDataModel(mockDataModel);
            SetMockRepo_GetAsyncUpdateContractAsync(mockDataModel);
            SetUpMockUriService("action");
            SetupLogger_LogInformationMethod();
            SetMockContractDocumentService();
            SetMockkContractValidator_WithdrawalRequest_InvalidContractRequestException();
            SetupMediator_Publish();
            var contractService = GetContractService();
            var request = GetASingleUpdateContractWithdrawalRequest();
            request.WithdrawalType = ContractStatus.Approved;

            // Act
            Func<Task> act = async () => await contractService.WithdrawalAsync(request);

            // Assert
            act.Should().Throw<InvalidContractRequestException>();
            Mock.Get(_contractRepository)
                .Verify(r => r.GetContractWithDatasAsync(It.IsAny<int>()), Times.Once);
            Mock.Get(_contractRepository)
                .Verify(r => r.GetAsync(It.IsAny<int>()), Times.Never);
            Mock.Get(_mockContractValidator)
                .Verify(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<UpdateContractWithdrawalRequest>()), Times.Once);
            Mock.Get(_mockContractValidator)
                .Verify(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), false), Times.Never);
            Mock.Get(_contractRepository).Verify(r => r.UpdateContractAsync(It.IsAny<DataModels.Contract>()), Times.Never);
            Mock.Get(_mockMediator).Verify(x => x.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockLogger.Verify();
        }

        [TestMethod]
        public void UpdateContractWithdrawalAsync_TestAsync_ExceptionResultExpected()
        {
            // Arrange
            var mockDataModel = GetDataModelsContract();
            SetMockRepo_GetAsyncUpdateContractAsync(mockDataModel);
            SetUpMockUriService("action");
            SetupLogger_LogInformationMethod();
            SetMockkContractValidator_WithdrawalRequest();
            SetupMediator_Publish_ThrowsException();
            var contractService = GetContractService();
            var request = GetASingleUpdateContractWithdrawalRequest();

            // Act
            Func<Task> act = async () => await contractService.WithdrawalAsync(request);

            // Assert
            act.Should().Throw<Exception>();

            //Mock.Get(_contractRepository)
            //  .Verify(r => r.GetAsync(It.IsAny<int>()), Times.Once);
            //Mock.Get(_mockContractValidator)
            //.Verify(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<UpdateContractWithdrawalRequest>()), Times.Once);
            //Mock.Get(_mockContractValidator)
            //  .Verify(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), false), Times.Once);
            //Mock.Get(_contractRepository).Verify(r => r.UpdateContractAsync(It.IsAny<DataModels.Contract>()), Times.Once);
            //Mock.Get(_mockMediator).Verify(x => x.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockLogger.Verify();
        }

        [TestMethod]
        public void UpdateContractWithdrawalAsync_TestAsync_ResultContractUpdateConcurrencyExceptionExpected()
        {
            // Arrange
            var mockDataModel = GetDataModelsContract();
            SetMockRepo_GetContractWithContractDataAsync_mockDataModel(mockDataModel);
            SetMockRepo_GetAsyncUpdateContractAsync_ContractUpdateConcurrencyException(mockDataModel);
            SetUpMockUriService("action");
            SetupLogger_LogInformationMethod();
            SetMockkContractValidator_WithdrawalRequest();
            SetupMediator_Publish();
            SetMockContractDocumentService();
            var contractService = GetContractService();
            var request = GetASingleUpdateContractWithdrawalRequest();

            // Act
            Func<Task> act = async () => await contractService.WithdrawalAsync(request);

            // Assert
            act.Should().Throw<ContractUpdateConcurrencyException>();
            Mock.Get(_contractRepository)
                .Verify(r => r.GetContractWithDatasAsync(It.IsAny<int>()), Times.Once);
            Mock.Get(_contractRepository)
              .Verify(r => r.GetAsync(It.IsAny<int>()), Times.Never);
            Mock.Get(_mockContractValidator)
            .Verify(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<UpdateContractWithdrawalRequest>()), Times.Once);
            Mock.Get(_mockContractValidator)
              .Verify(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), false), Times.Once);
            Mock.Get(_contractDocumentService)
                .Verify(d => d.UpsertOriginalContractXmlAsync(It.IsAny<DataModels.Contract>(), It.IsAny<UpdateContractWithdrawalRequest>()), Times.Once);
            Mock.Get(_contractRepository).Verify(r => r.UpdateContractAsync(It.IsAny<DataModels.Contract>()), Times.Once);
            Mock.Get(_mockMediator).Verify(x => x.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockLogger.Verify();
        }

        #endregion UpdateContractWithdrawalAsync


        #region Arrange Helpers

        private ContractRequest GetASingleUpdateLastEmailReminderSentRequest()
        {
            return new ContractRequest() { Id = 1, ContractNumber = "abc", ContractVersion = 1 };
        }

        private UpdateConfirmApprovalRequest GetASingleUpdateConfirmApprovalRequest()
        {
            return new UpdateConfirmApprovalRequest() { Id = 1, ContractNumber = "Main-0002", ContractVersion = 1, FileName = "Main-0002_v2_637503726040684393.xml" };
        }

        private UpdateContractWithdrawalRequest GetASingleUpdateContractWithdrawalRequest()
        {
            return new UpdateContractWithdrawalRequest() { Id = 1, ContractNumber = "abc", ContractVersion = 1, WithdrawalType = ContractStatus.WithdrawnByAgency, FileName = "sample-blob-file.xml" };
        }

        private ContractRequest GetContractRequest()
        {
            return new ContractRequest() { Id = 1, ContractNumber = "abc", ContractVersion = 1 };
        }

        private void SetUpMockUriService(string actionUrl)
        {
            _mockUriService = Mock.Of<IUriService>(MockBehavior.Strict);
            Mock.Get(_mockUriService)
                .Setup(m => m.GetUri(actionUrl))
                .Returns(new Uri("https://localhost:5001"))
                .Verifiable();
        }

        private void SetupLogger_LogInformationMethod()
        {
            _mockLogger
                .Setup(logger => logger.LogInformation(It.IsAny<string>()))
                .Verifiable();
        }

        private void SetupLogger_LogErrorMethod<TException>()
            where TException : Exception
        {
            _mockLogger
                .Setup(logger => logger.LogError(It.IsAny<TException>(), It.IsAny<string>()))
                .Verifiable();
        }

        private void SetupAuditService_TrySendAuditAsyncMethod(ActionType[] expectedActionTypes)
        {
            _mockAuditService
                .Setup(e => e.TrySendAuditAsync(It.IsAny<AuditModels.Audit>()))
                .Callback((AuditModels.Audit audit) =>
                {
                    expectedActionTypes.Should().Contain(audit.Action);
                })
                .Returns(Task.CompletedTask)
                .Verifiable();
        }

        private DataModels.Contract GetDummyDataModelsContract()
        {
            return new DataModels.Contract()
            {
                Id = 1,
                ContractNumber = "abc",
                ContractVersion = 1,
                FundingType = (int)ContractFundingType.CityDeals,
                ContractContent = new DataModels.ContractContent()
                {
                    Id = 1,
                    FileName = "abc_v1.pdf",
                    Content = BitConverter.GetBytes(20210225),
                    Size = BitConverter.GetBytes(20210225).Length
                }
            };
        }

        private DataModels.Contract GetDummyDataModelsContract_Datas()
        {
            return new DataModels.Contract()
            {
                Id = 7,
                ContractNumber = "Main-0002",
                ContractVersion = 2,
                ContractData = new DataModels.ContractData()
                {
                    Id = 7,
                    OriginalContractXml = "sampl-xml-content"
                }
            };
        }

        private void SetMockRepo_GetContractWithContractContentAsync(DataModels.Contract contract)
        {
            Mock.Get(_contractRepository)
                .Setup(r => r.GetContractWithContentAndDatasAsync(It.IsAny<int>()))
                .ReturnsAsync(contract)
                .Verifiable();
        }

        private void SetMockRepo_GetContractWithContractDataAsync_mockDataModel(DataModels.Contract contract)
        {
            Mock.Get(_contractRepository)
              .Setup(r => r.GetAsync(It.IsAny<int>()))
              .ReturnsAsync(It.IsAny<DataModels.Contract>())
              .Verifiable();

            Mock.Get(_contractRepository)
                .Setup(r => r.GetContractWithDatasAsync(It.IsAny<int>()))
                .ReturnsAsync(contract)
                .Verifiable();
        }

        private void SetMockRepo_UpdateContractAsync()
        {
            Mock.Get(_contractRepository)
                .Setup(r => r.UpdateContractAsync(It.IsAny<DataModels.Contract>()))
                .Returns(Task.CompletedTask)
                .Verifiable();
        }

        private void SetMockRepo_UpdateContractAsync_ContractUpdateConcurrencyException()
        {
            Mock.Get(_contractRepository)
              .Setup(r => r.UpdateContractAsync(It.IsAny<DataModels.Contract>()))
              .Throws(new ContractUpdateConcurrencyException(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ContractStatus>()))
              .Verifiable();
        }

        private void SetMockRepo_ApproveManuallyAsync_mockDataModel(DataModels.Contract contract)
        {
            SetMockRepo_GetContractWithContractContentAsync(contract);
            SetMockRepo_UpdateContractAsync();
        }

        private void SetMockRepo_ApproveManuallyAsync_ContractUpdateConcurrencyException(DataModels.Contract contract)
        {
            SetMockRepo_GetContractWithContractContentAsync(contract);
            SetMockRepo_UpdateContractAsync_ContractUpdateConcurrencyException();
        }

        private void SetMockDocumentService()
        {
            var dummyContent = BitConverter.GetBytes(20210225);
            Mock.Get(_mockDocumentService)
                .Setup(d => d.AddSignedDocumentPage(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), true, ContractFundingType.CityDeals, null))
                .Returns(dummyContent)
                .Verifiable();
        }

        private void SetMockContractDocumentService()
        {
            Mock.Get(_contractDocumentService)
                .Setup(d => d.UpsertOriginalContractXmlAsync(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()))
                .Returns(Task.CompletedTask)
                .Verifiable();
        }

        private void SetMockContractValidator()
        {
            Mock.Get(_mockContractValidator)
                .Setup(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>(), c => c.ContractContent != null))
                .Verifiable();
            Mock.Get(_mockContractValidator)
                .Setup(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), true))
                .Verifiable();
        }

        private void SetMockContractValidator_Validate()
        {
            Mock.Get(_mockContractValidator)
                .Setup(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()))
                .Verifiable();
            Mock.Get(_mockContractValidator)
                .Setup(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), false))
                .Verifiable();
        }

        private void SetMockContractValidator_ContractStatusException()
        {
            Mock.Get(_mockContractValidator)
                .Setup(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>(), c => c.ContractContent != null))
                .Verifiable();
            Mock.Get(_mockContractValidator)
                .Setup(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), true))
                .Throws(new ContractStatusException("Contract status is not in a valid status."))
                .Verifiable();
        }

        private void SetMockContractValidator_Validate_ContractStatusException()
        {
            Mock.Get(_mockContractValidator)
                .Setup(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()))
                .Verifiable();
            Mock.Get(_mockContractValidator)
                .Setup(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), false))
                .Throws(new ContractStatusException("Contract status is not in a valid status."))
                .Verifiable();
        }

        private void SetMockContractValidator_Validate_BlobNoContentException()
        {
            Mock.Get(_mockContractValidator)
                .Setup(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()))
                .Verifiable();
            Mock.Get(_mockContractValidator)
                .Setup(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), false))
                .Throws(new BlobNoContentException(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .Verifiable();
        }

        private void SetMockContractValidator_Validate_BlobException()
        {
            Mock.Get(_mockContractValidator)
                .Setup(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()))
                .Verifiable();
            Mock.Get(_mockContractValidator)
                .Setup(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), false))
                .Throws(new BlobException("Exception occurred retrieving blob document."))
                .Verifiable();
        }

        private void SetMockContractValidator_Validate_ContractNotFoundException()
        {
            Mock.Get(_mockContractValidator)
                .Setup(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()))
                .Throws(new ContractNotFoundException("Contract was not found."))
                .Verifiable();
            Mock.Get(_mockContractValidator)
                .Setup(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), true))
                .Verifiable();
        }

        private void SetMockContractValidator_ContractNotFoundException()
        {
            Mock.Get(_mockContractValidator)
                .Setup(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>(), c => c.ContractContent != null))
                .Throws(new ContractNotFoundException("Contract status is not in a valid status."))
                .Verifiable();
            Mock.Get(_mockContractValidator)
                .Setup(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), true))
                .Verifiable();
        }

        private void SetMockContractValidator_ContractExpectationFailedException()
        {
            Mock.Get(_mockContractValidator)
                .Setup(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>(), c => c.ContractContent != null))
                .Throws(new ContractExpectationFailedException("acb", 1, 1, "null"))
                .Verifiable();
            Mock.Get(_mockContractValidator)
                .Setup(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), true))
                .Verifiable();
        }

        private void SetMockContractValidator_InvalidContractRequestException()
        {
            Mock.Get(_mockContractValidator)
                .Setup(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>(), c => c.ContractContent != null))
                .Throws(new InvalidContractRequestException("abc", 1, 1))
                .Verifiable();
            Mock.Get(_mockContractValidator)
                .Setup(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), true))
                .Verifiable();
        }

        private void SetMockContractValidator_NoContent()
        {
            Mock.Get(_mockContractValidator)
                .Setup(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()))
                .Verifiable();
        }

        private void SetMockContractValidator_NoContent_ContractStatusException()
        {
            Mock.Get(_mockContractValidator)
                .Setup(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()))
                .Verifiable();
        }

        private void SetMockContractValidator_NoContent_ContractNotFoundException()
        {
            Mock.Get(_mockContractValidator)
                .Setup(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()))
                .Throws(new ContractNotFoundException("Contract status is not in a valid status."))
                .Verifiable();
        }

        private void SetMockContractValidator_NoContent_ContractExpectationFailedException()
        {
            Mock.Get(_mockContractValidator)
                .Setup(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()))
                .Throws(new ContractExpectationFailedException("acb", 1, 1, "null"))
                .Verifiable();
        }

        private void SetMockContractValidator_NoContent_InvalidContractRequestException()
        {
            Mock.Get(_mockContractValidator)
                .Setup(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<ContractRequest>()))
                .Throws(new InvalidContractRequestException("abc", 1, 1))
                .Verifiable();
        }

        private void SetupAuditService_AuditAsyncMethod()
        {
            _mockAuditService
                .Setup(e => e.AuditAsync(It.IsAny<AuditModels.Audit>()))
                .Returns(Task.CompletedTask)
                .Verifiable();
        }

        private void SetupMediator_Publish()
        {
            Mock.Get(_mockMediator)
                .Setup(e => e.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();
        }

        private void SetupMediator_Publish_ThrowsException()
        {
            Mock.Get(_mockMediator)
                .Setup(e => e.Publish(It.IsAny<UpdatedContractStatusResponse>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception())
                .Verifiable();
        }

        private ContractService GetContractService()
        {
            var contractService = new ContractService(
                _contractRepository,
                _mapper,
                _mockUriService,
                _mockLogger.Object,
                _mockAuditService.Object,
                _semaphoreOnEntity,
                _mockDocumentService,
                _mockContractValidator,
                _mockMediator,
                _contractDocumentService);
            return contractService;
        }

        private void SetupRepository_GetByContractNumberAndVersionAsyncMethod(CreateContractRequest createRequest, DataModels.Contract result)
        {
            Mock.Get(_contractRepository)
                .Setup(p => p.GetByContractNumberAndVersionAsync(createRequest.ContractNumber, createRequest.ContractVersion))
                .Returns(Task.FromResult(result))
                .Verifiable();
        }

        private void SetupRepository_GetByContractNumberAsyncMethod(CreateContractRequest createRequest, IEnumerable<DataModels.Contract> matchedRecords)
        {
            Mock.Get(_contractRepository)
                .Setup(p => p.GetByContractNumberAsync(createRequest.ContractNumber))
                .Returns(Task.FromResult(matchedRecords))
                .Verifiable();
        }

        private void SetupRepository_CreateAsyncMethod(DataModels.Contract contractRecord, Task result)
        {
            Mock.Get(_contractRepository)
                .Setup(p => p.CreateAsync(contractRecord))
                .Returns(result)
                .Verifiable();
        }

        private CreateContractRequest Generate_CreateContractRequest()
        {
            string contractNumber = "TestContractNumber";
            int contractVersion = 1;

            var createRequest = new CreateContractRequest()
            {
                ContractNumber = contractNumber,
                ContractVersion = contractVersion
            };
            return createRequest;
        }

        private DataModels.Contract GetDataModelsContract(ContractStatus status = ContractStatus.PublishedToProvider)
        {
            return new DataModels.Contract()
            {
                Id = 1,
                ContractNumber = "abc",
                ContractVersion = 1,
                FundingType = (int)ContractFundingType.CityDeals,
                Status = (int)status
            };
        }

        private void SetMockRepo_GetAsync(DataModels.Contract contract)
        {
            Mock.Get(_contractRepository)
                .Setup(r => r.GetAsync(It.IsAny<int>()))
                .ReturnsAsync(contract)
                .Verifiable();
        }

        private void SetMockRepo_GetAsyncUpdateContractAsync(DataModels.Contract contract)
        {
            SetMockRepo_GetAsync(contract);
            SetMockRepo_UpdateContractAsync();
        }

        private void SetMockRepo_GetAsyncUpdateContractAsync_ContractUpdateConcurrencyException(DataModels.Contract contract)
        {
            SetMockRepo_GetAsync(contract);
            SetMockRepo_UpdateContractAsync_ContractUpdateConcurrencyException();
        }

        private void SetMockkContractValidator_ValidateStatusChange(bool isManualApproval = false)
        {
            Mock.Get(_mockContractValidator)
                .Setup(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), isManualApproval))
                .Verifiable();
        }

        private void SetMockkContractValidator_Validate_WithdrawalRequest()
        {
            Mock.Get(_mockContractValidator)
                .Setup(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<UpdateContractWithdrawalRequest>()))
                .Verifiable();
        }

        private void SetMockkContractValidator_Validate_WithdrawalRequest_ContractNotFoundException()
        {
            Mock.Get(_mockContractValidator)
                .Setup(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<UpdateContractWithdrawalRequest>()))
                .Throws(new ContractNotFoundException("Contract status is not in a valid status."))
                .Verifiable();
        }

        private void SetMockkContractValidator_Validate_WithdrawalRequest_InvalidContractRequestException()
        {
            Mock.Get(_mockContractValidator)
                .Setup(v => v.Validate(It.IsAny<DataModels.Contract>(), It.IsAny<UpdateContractWithdrawalRequest>()))
                .Throws(new InvalidContractRequestException("abc", 1, 1))
                .Verifiable();
        }

        private void SetMockkContractValidator_ValidateStatusChange_ContractStatusException(bool isManualApproval = false)
        {
            Mock.Get(_mockContractValidator)
                .Setup(v => v.ValidateStatusChange(It.IsAny<DataModels.Contract>(), It.IsAny<ContractStatus>(), isManualApproval))
                .Throws(new ContractStatusException("Contract status is not in a valid status."))
                .Verifiable();
        }

        private void SetMockkContractValidator_WithdrawalRequest()
        {
            SetMockkContractValidator_Validate_WithdrawalRequest();
            SetMockkContractValidator_ValidateStatusChange();
        }

        private void SetMockkContractValidator_WithdrawalRequest_ContractStatusException()
        {
            SetMockkContractValidator_Validate_WithdrawalRequest();
            SetMockkContractValidator_ValidateStatusChange_ContractStatusException();
        }

        private void SetMockkContractValidator_WithdrawalRequest_ContractNotFoundException()
        {
            SetMockkContractValidator_Validate_WithdrawalRequest_ContractNotFoundException();
            SetMockkContractValidator_ValidateStatusChange();
        }

        private void SetMockkContractValidator_WithdrawalRequest_InvalidContractRequestException()
        {
            SetMockkContractValidator_Validate_WithdrawalRequest_InvalidContractRequestException();
            SetMockkContractValidator_ValidateStatusChange();
        }

        private void SetupSemaphoreOnEntity()
        {
            Mock.Get(_semaphoreOnEntity)
                .Setup(p => p.WaitAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            Mock.Get(_semaphoreOnEntity)
                .Setup(p => p.Release(It.IsAny<string>()))
                .Verifiable();
        }
        #endregion


        #region Verify

        private void VerifyAll()
        {
            Mock.Get(_contractRepository).Verify();
            Mock.Get(_mapper).Verify();
            Mock.Get(_mockUriService).Verify();
            _mockLogger.Verify();
            _mockAuditService.Verify();
            Mock.Get(_mockContractValidator).Verify();
            Mock.Get(_semaphoreOnEntity).Verify();
        }

        #endregion
    }
}