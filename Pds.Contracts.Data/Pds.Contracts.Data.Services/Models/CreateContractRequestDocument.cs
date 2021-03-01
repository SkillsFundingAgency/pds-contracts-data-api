using System.ComponentModel.DataAnnotations;

namespace Pds.Contracts.Data.Services.Models
{
    /// <summary>
    /// Representing a contract document.
    /// </summary>
    public class CreateContractRequestDocument
    {
        /// <summary>
        /// Gets or sets the binbary contents of the document.
        /// </summary>
        [Required]
        public byte[] Content { get; set; }

        /// <summary>
        /// Gets or sets the file size in bytes of the PDF.
        /// </summary>
        [Required]
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the name of the document.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string FileName { get; set; }
    }
}