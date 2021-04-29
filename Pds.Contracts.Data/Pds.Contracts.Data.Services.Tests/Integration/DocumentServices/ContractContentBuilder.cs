using Pds.Contracts.Data.Repository.DataModels;
using System;
using System.IO;

namespace Pds.Contracts.Data.Services.Tests.Integration.DocumentServices
{
    public class ContractContentBuilder
    {
        private static readonly byte[] DefaultDocumentBytes = GetDocumentBytes("12345678_CityDeals-0002_v1.pdf");

        protected ContractContentBuilder()
        {
        }

        private static ContractContent Default => new ContractContent() { Content = DefaultDocumentBytes, FileName = "12345678_CityDeals-0002_v1.pdf" };

        public static byte[] GetDocumentBytes(string documentName)
        {
            var projectFolder = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            return File.ReadAllBytes(Path.Combine(projectFolder + "\\Documents", documentName));
        }
    }
}
