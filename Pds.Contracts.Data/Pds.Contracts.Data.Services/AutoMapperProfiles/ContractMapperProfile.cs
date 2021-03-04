using AutoMapper;
using Pds.Contracts.Data.Common.Responses;
using Pds.Contracts.Data.Services.Models;
using DataModel = Pds.Contracts.Data.Repository.DataModels;
using ServiceModel = Pds.Contracts.Data.Services.Models;

namespace Pds.Contracts.Data.Services.AutoMapperProfiles
{
    /// <summary>
    /// Automapper profile for data models and service models.
    /// </summary>
    /// <seealso cref="AutoMapper.Profile"/>
    public class ContractMapperProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContractMapperProfile"/> class.
        /// </summary>
        public ContractMapperProfile()
        {
            CreateMap<DataModel.Contract, ServiceModel.Contract>();
            CreateMap<DataModel.Contract, ServiceModel.ContractReminderItem>();

            CreateMap<ServiceModel.CreateContractRequestDocument, DataModel.ContractContent>()
                .ForMember(p => p.Id, opt => opt.Ignore())
                .ForMember(p => p.IdNavigation, opt => opt.Ignore());

            CreateMap<ServiceModel.CreateContractCode, DataModel.ContractFundingStreamPeriodCode>()
                .ForMember(p => p.Id, opt => opt.Ignore())
                .ForMember(p => p.ContractId, opt => opt.Ignore())
                .ForMember(p => p.Contract, opt => opt.Ignore());

            CreateMap<ServiceModel.CreateContractRequest, DataModel.Contract>()
                .ForMember(p => p.ContractData, opt => opt.Ignore())
                .ForMember(p => p.Id, opt => opt.Ignore())
                .ForMember(p => p.SignedBy, opt => opt.Ignore())
                .ForMember(p => p.SignedOn, opt => opt.Ignore())
                .ForMember(p => p.SignedByDisplayName, opt => opt.Ignore())
                .ForMember(p => p.CreatedAt, opt => opt.Ignore())
                .ForMember(p => p.LastUpdatedAt, opt => opt.Ignore())
                .ForMember(p => p.EmailReminder, opt => opt.Ignore())
                .ForMember(p => p.WasManuallyApproved, opt => opt.Ignore())
                .ForMember(p => p.LastEmailReminderSent, opt => opt.Ignore())
                .ForMember(p => p.HasNotificationBeenRead, opt => opt.Ignore())
                .ForMember(p => p.NotificationReadBy, opt => opt.Ignore())
                .ForMember(p => p.NotificationReadAt, opt => opt.Ignore());

            CreateMap<UpdatedContractStatusResponse, ContractNotification>()
                .ForMember(dest => dest.Status, u => u.MapFrom(src => src.NewStatus));
        }
    }
}