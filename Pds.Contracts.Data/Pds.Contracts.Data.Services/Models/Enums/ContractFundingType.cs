using System.ComponentModel.DataAnnotations;

namespace Pds.Contracts.Data.Services.Models.Enums
{
    /// <summary>
    /// The types of funding that the system supports.
    /// </summary>
    public enum ContractFundingType
    {
        /// <summary>
        /// Unknown funding type.
        /// </summary>
        [Display(Name = "", Description = "Unknown")]
        Unknown = 0,

        /// <summary>
        /// Mainstream funding type.
        /// </summary>
        [Display(Name = "Mainstream", Description = "Mainstream")]
        Mainstream = 1,

        /// <summary>
        /// Esf funding type
        /// </summary>
        [Display(Name = "ESF", Description = "European social fund (ESF)")]
        Esf = 2,

        /// <summary>
        /// TwentyFourPlusLoan funding type
        /// </summary>
        [Display(Name = "24+LOANS", Description = "24+LOANS")]
        TwentyFourPlusLoan = 3,

        /// <summary>
        /// Age funding type
        /// </summary>
        [Display(Name = "AGE", Description = "AGE Grant")]
        Age = 4,

        /// <summary>
        /// Eop funding type
        /// </summary>
        [Display(Name = "EOP", Description = "EOP")]
        Eop = 5,

        /// <summary>
        /// Eof funding type
        /// </summary>
        [Display(Name = "EOF", Description = "EOF")]
        Eof = 6,

        /// <summary>
        /// CityDeals funding type.
        /// </summary>
        [Display(Name = "City Deal", Description = "City deal")]
        CityDeals = 7,

        /// <summary>
        /// LocalGrowth funding type.
        /// </summary>
        [Display(Name = "Local Growth", Description = "Local growth")]
        LocalGrowth = 8,

        /// <summary>
        /// Levy funding type.
        /// </summary>
        [Display(Name = "apprenticeship agreement", Description = "Apprenticeship agreement")]
        Levy = 9,

        /// <summary>
        /// NCS funding type.
        /// </summary>
        [Display(Name = "National Careers Service (NCS)", Description = "National Careers Service (NCS)")]
        Ncs = 10,

        /// <summary>
        /// Non Levy funding type.
        /// </summary>
        [Display(Name = "Apprenticeship contract", Description = "Apprenticeship contract")]
        NonLevy = 11,

        /// <summary>
        /// 1619Fund funding type.
        /// </summary>
        [Display(Name = "16 to 19 funding", Description = "16-19 funding")]
        SixteenNineteenFunding = 12,

        /// <summary>
        /// AEBP funding type.
        /// </summary>
        [Display(Name = "Procured adult education", Description = "Procured adult education)")]
        Aebp = 13,

        /// <summary>
        /// NLA funding type.
        /// </summary>
        [Display(Name = "Procured non levy apprenticeship contract", Description = "Procured non levy apprenticeship contract")]
        Nla = 14,

        /// <summary>
        /// Advanced Leaner Loans funding type
        /// </summary>
        [Display(Name = "Advanced Leaner Loans", Description = "Advanced Leaner Loans")]
        AdvancedLearnerLoans = 15,

        /// <summary>
        /// Education and skills funding contract.
        /// </summary>
        [Display(Name = "Education and skills funding contract", Description = "Education and skills funding contract")]
        EducationAndSkillsFunding = 16,

        /// <summary>
        /// Non-learning grant funding type.
        /// </summary>
        [Display(Name = "Non-learning grant funding agreement", Description = "Non-learning grant funding agreement")]
        NonLearningGrant = 17,

        /// <summary>
        /// 16-18 Forensic Unit.
        /// </summary>
        [Display(Name = "16 to 18 Forensic Unit", Description = "16-18 Forensic Unit")]
        SixteenEighteenForensicUnit = 18,

        /// <summary>
        /// Dance and Drama Awards.
        /// </summary>
        [Display(Name = "Dance and Drama Awards", Description = "Dance and Drama Awards")]
        DanceAndDramaAwards = 19,

        /// <summary>
        /// College Collaboration Fund Agreements.
        /// </summary>
        [Display(Name = "College Collaboration Fund", Description = "College Collaboration Fund")]
        CollegeCollaborationFund = 20,

        /// <summary>
        /// Further Education Condition Allocation agreements.
        /// </summary>
        [Display(Name = "Further Education Condition Allocation", Description = "Further Education Condition Allocation")]
        FurtherEducationConditionAllocation = 21,

        /// <summary>
        /// The procured nineteen to twenty four traineeship
        /// </summary>
        [Display(Name = "Procured 19 to 24 traineeship", Description = "Procured 19 to 24 traineeship")]
        ProcuredNineteenToTwentyFourTraineeship = 22
    }
}