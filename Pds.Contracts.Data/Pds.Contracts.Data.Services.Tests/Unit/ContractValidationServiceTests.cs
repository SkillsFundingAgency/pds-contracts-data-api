using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.Contracts.Data.Common.CustomExceptionHandlers;
using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Services.Implementations;
using Pds.Contracts.Data.Services.Models;
using System;
using System.Collections.Generic;
using DataModels = Pds.Contracts.Data.Repository.DataModels;

namespace Pds.Contracts.Data.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class ContractValidationServiceTests
    {
        private ContractValidationService contractValidationService = new ContractValidationService();

        #region Validate tests

        [TestMethod]
        public void Validate_NoException()
        {
            // Arrange
            var contract = GetContract();
            var contractRequest = GetContractRequest();

            // Act
            Action act = () => contractValidationService.Validate(contract, contractRequest);

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void Validate_ContractNotFoundException()
        {
            // Arrange
            DataModels.Contract contract = null;
            var contractRequest = GetContractRequest();

            // Act
            Action act = () => contractValidationService.Validate(contract, contractRequest);

            // Assert
            act.Should().ThrowExactly<ContractNotFoundException>();
        }

        [TestMethod]
        public void Validate_InputExpression_NoException()
        {
            // Arrange
            var contract = GetContract();
            contract.ContractContent = GetContractContent();
            var contractRequest = GetContractRequest();

            // Act
            Action act = () => contractValidationService.Validate(contract, contractRequest, c => c.ContractContent != null);

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void Validate_InputExpression_ContractExpectationFailedException()
        {
            // Arrange
            var contract = GetContract();
            var contractRequest = GetContractRequest();

            // Act
            Action act = () => contractValidationService.Validate(contract, contractRequest, c => c.ContractContent != null);

            // Assert
            act.Should().ThrowExactly<ContractExpectationFailedException>();
        }

        #endregion Validate tests


        #region ValidateStatusChange tests

        [TestMethod]
        public void ValidateStatusChange_ManualApproval_NoException()
        {
            // Arrange
            ContractStatus newStatus = ContractStatus.Approved;
            bool isManualApproval = true;
            var contract = GetContract();

            // Act
            Action act = () => contractValidationService.ValidateStatusChange(contract, newStatus, isManualApproval);

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void ValidateStatusChange_ManualApproval_ContractStatusException()
        {
            // Arrange
            ContractStatus newStatus = ContractStatus.Approved;
            bool isManualApproval = true;
            var contract = GetContract(ContractStatus.Approved);

            // Act
            Action act = () => contractValidationService.ValidateStatusChange(contract, newStatus, isManualApproval);

            // Assert
            act.Should().ThrowExactly<ContractStatusException>();
        }

        [TestMethod]
        public void ValidateStatusChange_NotManualApproval_NoException()
        {
            // Arrange
            ContractStatus newStatus = ContractStatus.Approved;
            bool isManualApproval = false;
            var contract = GetContract(ContractStatus.ApprovedWaitingConfirmation);

            // Act
            Action act = () => contractValidationService.ValidateStatusChange(contract, newStatus, isManualApproval);

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void ValidateStatusChange_NotManualApproval_ContractStatusException()
        {
            // Arrange
            ContractStatus newStatus = ContractStatus.Approved;
            bool isManualApproval = false;
            var contract = GetContract();

            // Act
            Action act = () => contractValidationService.ValidateStatusChange(contract, newStatus, isManualApproval);

            // Assert
            act.Should().ThrowExactly<ContractStatusException>();
        }

        #endregion ValidateStatusChange tests


        #region ValidateForNewContract

        [TestMethod]
        public void Validate_ValidateForNewContract_WhenNoMatchesInExpected_RaisesNoExceptions()
        {
            // Arrange
            var contractNumber = "Test123";
            var contractVersion = 123;

            var request = GetNewContractRequest(contractNumber, contractVersion);
            var existingContracts = GetExistingContracts(contractNumber, contractVersion - 5, 3);

            // Act
            Action act = () => contractValidationService.ValidateForNewContract(request, existingContracts);

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void Validate_ValidateForNewContract_WhenDifferntContractWithSameVersion_RaisesNoExceptions()
        {
            // Arrange
            var contractNumber = "Test123";
            var contractVersion = 123;

            var request = GetNewContractRequest(contractNumber, contractVersion);
            var existingContracts = GetExistingContracts(contractNumber, contractVersion - 5, 3);
            existingContracts.Add(new DataModels.Contract() { ContractNumber = "SomeOther", ContractVersion = contractVersion });

            // Act
            Action act = () => contractValidationService.ValidateForNewContract(request, existingContracts);

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void Validate_ValidateForNewContract_WhenCurrentContractVersionExists_RaisesDuplicateContractException()
        {
            // Arrange
            var contractNumber = "Test123";
            var contractVersion = 123;

            var request = GetNewContractRequest(contractNumber, contractVersion);
            var existingContracts = GetExistingContracts(contractNumber, contractVersion - 2, 3);

            // Act
            Action act = () => contractValidationService.ValidateForNewContract(request, existingContracts);

            // Assert
            act.Should().Throw<DuplicateContractException>();
        }

        [TestMethod]
        public void Validate_ValidateForNewContract_WhenHigherContractVersionExists_RaisesContractWithHigherVersionAlreadyExistsException()
        {
            // Arrange
            var contractNumber = "Test123";
            var contractVersion = 123;

            var request = GetNewContractRequest(contractNumber, contractVersion);
            var existingContracts = GetExistingContracts(contractNumber, contractVersion + 1, 3);

            // Act
            Action act = () => contractValidationService.ValidateForNewContract(request, existingContracts);

            // Assert
            act.Should().Throw<ContractWithHigherVersionAlreadyExistsException>();
        }

        #endregion


        #region Arrange Helpers

        private DataModels.Contract GetContract(ContractStatus newStatus = ContractStatus.PublishedToProvider)
        {
            return new DataModels.Contract() { Id = 1, ContractNumber = "abc", ContractVersion = 1, Status = (int)newStatus };
        }

        private DataModels.ContractContent GetContractContent()
        {
            return new DataModels.ContractContent() { Id = 1, FileName = "abc" };
        }

        private ContractRequest GetContractRequest()
        {
            return new ContractRequest() { ContractNumber = "abc", ContractVersion = 1 };
        }

        private CreateContractRequest GetNewContractRequest(string contractNumber, int contractVersion)
            => new CreateContractRequest()
            {
                ContractNumber = contractNumber,
                ContractVersion = contractVersion
            };

        private IList<DataModels.Contract> GetExistingContracts(string contractNumber, int startingContractVersion, int count)
        {
            var contracts = new List<DataModels.Contract>();
            for (int i = 0; i < count; i++)
            {
                contracts.Add(new DataModels.Contract()
                {
                    ContractNumber = contractNumber,
                    ContractVersion = startingContractVersion + i
                });
            }

            return contracts;
        }

        #endregion
    }
}
