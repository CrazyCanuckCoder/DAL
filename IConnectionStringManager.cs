namespace DAL
{
    /// <summary>
    /// The interface to the connection string manager class.
    /// </summary>
    /// 
    public interface IConnectionStringManager
    {
        DataProvider ProviderType       { get; }
        string       DataSource         { get; set; }
        string       UserID             { get; set; }
        string       Password           { get; set; }
        bool         IntegratedSecurity { get; set; }
        string       InitialCatalog     { get; set; }
        string       Service            { get; set; }

        string ConstructConnectionString();
    }
}