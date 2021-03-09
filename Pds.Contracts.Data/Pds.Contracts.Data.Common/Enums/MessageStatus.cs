namespace Pds.Contracts.Data.Common.Enums
{
    /// <summary>
    /// Represents the states a message can be in notification service bus.
    /// </summary>
    public enum MessageStatus
    {
        /// <summary>
        /// The contract is now approved.
        /// </summary>
        Approved,

        /// <summary>
        /// The contract is now Withdrawn By Agency or Provider.
        /// </summary>
        Withdrawn,
    }
}
