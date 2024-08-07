using System;
using System.ComponentModel.DataAnnotations;

namespace Pds.Contracts.Data.Repository.DataModels
{
    /// <summary>
    /// FullSubcontractorDeclaration POCO.
    /// </summary>
    public class FullSubcontractorDeclaration
    {
        #region Enums

        /// <summary>
        /// Represents the states a full subcontractor declaration can be in.
        /// </summary>
        public enum SubcontractorDeclarationStatus
        {
            /// <summary>
            /// The Subcontractor Declarations is in draft mode.
            /// </summary>
            Draft = 0,

            /// <summary>
            /// The Subcontractor Declarations has been approved by provider.
            /// </summary>
            Approved = 1,

            /// <summary>
            /// The Subcontractor Declarations has been closed by SFS service.
            /// </summary>
            Closed = 2
        }

        /// <summary>
        /// Represents the type of declaration that the provider is submitting.
        /// </summary>
        public enum SubcontractorDeclarationSubmissionType
        {
            /// <summary>
            /// The Subcontractor Declaration is a nil declaration.
            /// </summary>
            Nil = 0,

            /// <summary>
            /// The Subcontractor Declaration is a full declaration.
            /// </summary>
            Full = 1
        }

        #endregion

        /// <summary>
        /// Gets or sets the data store id of this instance.
        /// </summary>
        public int Id { get; protected set; }

        /// <summary>
        /// Gets or sets who the subcontractor declaration is for.
        /// </summary>
        [Required]
        public int Ukprn { get; protected set; }

        /// <summary>
        /// Gets or sets the version number of the Subcontractor declaration.
        /// </summary>
        [Required]
        public int Version { get; protected set; }

        /// <summary>
        /// Gets or sets the charity registration number of provider.
        /// </summary>
        public virtual string CharityRegistrationNumber { get; protected set; }

        /// <summary>
        /// Gets or sets the organisation name of ukprn.
        /// </summary>
        public string OrganisationName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether does provider subcontract any of their funded provision.
        /// </summary>
        public virtual bool SubContractFundedProvision { get; protected set; }

        /// <summary>
        /// Gets or sets when this instance was created.
        /// </summary>
        [Required]
        public virtual DateTime CreatedAt { get; protected set; }

        /// <summary>
        /// Gets or sets when this instance was last updated.
        /// </summary>
        [Required]
        public virtual DateTime LastUpdatedAt { get; protected set; }

        /// <summary>
        /// Gets or sets the web link.
        /// </summary>
        public virtual string WebLink { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether [second level sub contract funded provision].
        /// </summary>
        /// <value>
        /// <c>true</c> if [second level sub contract funded provision]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool SecondLevelSubContractFundedProvision { get; protected set; }

        /// <summary>
        /// Gets or sets status of subcontractor declaration.
        /// </summary>
        public virtual SubcontractorDeclarationStatus Status { get; protected set; }

        /// <summary>
        /// Gets or sets the period of the subcontractor declaration.
        /// </summary>
        public string Period { get; protected set; }

        /// <summary>
        /// Gets or sets the datetime that the subcontractor declaration has been submitted by.
        /// </summary>
        public DateTime? SubmittedAt { get; protected set; }

        /// <summary>
        /// Gets or sets the user who submitted the subcontractor declaration.
        /// </summary>
        public string SubmittedBy { get; protected set; }

        /// <summary>
        /// Gets or sets the type of subcontractor declaration.
        /// </summary>
        public virtual SubcontractorDeclarationSubmissionType SubcontractorDeclarationType { get; protected set; }

        /// <summary>
        /// Gets or sets if a subcontractor declaration has been submitted, then this will be populated by the display name of the provider agent who approved it.
        /// </summary>
        public string SubmittedByDisplayName { get; protected set; }
    }
}
