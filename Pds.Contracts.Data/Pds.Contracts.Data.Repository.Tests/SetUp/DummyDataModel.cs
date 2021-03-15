namespace Pds.Contracts.Data.Repository.Tests.SetUp
{
    /// <summary>
    /// Dummy data model used to unit test <see cref="Pds.Contracts.Data.Repository.Interfaces.IRepository{T}"/>.
    /// </summary>
    public class DummyDataModel
    {
        public int Id { get; set; }

        public string ATableColumn { get; set; }
    }
}