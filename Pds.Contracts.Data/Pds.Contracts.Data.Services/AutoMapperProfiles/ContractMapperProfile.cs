using AutoMapper;
using DataModel = Pds.Contracts.Data.Repository.DataModels;
using ServiceModel = Pds.Contracts.Data.Services.Models;

namespace Pds.Contracts.Data.Services.AutoMapperProfiles
{
    /// <summary>
    /// Automapper profile for data models and service models.
    /// </summary>
    /// <seealso cref="AutoMapper.Profile" />
    public class ContractMapperProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContractMapperProfile"/> class.
        /// </summary>
        public ContractMapperProfile()
        {
            CreateMap<DataModel.Contract, ServiceModel.Contract>();
        }
    }
}