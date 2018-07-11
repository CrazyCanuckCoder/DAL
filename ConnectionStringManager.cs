using System;
using System.Configuration;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Web;
using System.Web.Configuration;
using System.Windows.Forms;

namespace DAL
{
    /// <summary>
    /// A class for constructing database type independent connection strings.
    /// </summary>
    /// 
    public class ConnectionStringManager : IConnectionStringManager
    {
        /// <summary>
        /// Sets the database type and loads the settings from the application's
        /// configuration file.
        /// </summary>
        /// 
        /// <param name="CurrentProvider">
        /// The type of database.
        /// </param>
        /// 
        public ConnectionStringManager(DataProvider CurrentProvider)
        {
            this.ProviderType = CurrentProvider;
            this.LoadFromConfig();
        }

        /// <summary>
        /// Sets the database type, loads the settings from the application's
        /// configuration file and sets the web context property to indicate
        /// that the calling application is web based.
        /// </summary>
        /// 
        /// <param name="CurrentProvider">
        /// The type of database.
        /// </param>
        /// 
        /// <param name="CurrentContext">
        /// The HttpContext of the web based application.
        /// </param>
        /// 
        public ConnectionStringManager(DataProvider CurrentProvider, HttpContext CurrentContext)
        {
            this.ProviderType = CurrentProvider;

            if (CurrentContext != null)
            {
                this._webContext = CurrentContext;
                this._isWebApp   = true;
            }

            this.LoadFromConfig();
        }




        /// <summary>
        /// The HttpContext used to access the web based configuration file.
        /// </summary>
        /// 
        private readonly HttpContext _webContext = null;

        /// <summary>
        /// Set to true if the calling application is web based.
        /// </summary>
        /// 
        private readonly bool _isWebApp = false;

        /// <summary>
        /// Indicates which configuration group to use, if any.
        /// </summary>
        /// 
        private string _configGroupName = "";




        /// <summary>
        /// Gets the type of database.
        /// </summary>
        /// 
        public DataProvider ProviderType { get; } = DataProvider.None;

        /// <summary>
        /// Gets/sets the data source.
        /// </summary>
        /// 
        public string DataSource { get; set; } = "";

        /// <summary>
        /// Gets/sets the user name to access the database.
        /// </summary>
        /// 
        public string UserID { get; set; } = "";

        /// <summary>
        /// Gets/sets the password to access the database.
        /// </summary>
        /// 
        public string Password { get; set; } = "";

        /// <summary>
        /// Gets/sets the indicator specifying integrated security will be 
        /// used to access the database.
        /// </summary>
        /// 
        public bool IntegratedSecurity { get; set; } = false;

        /// <summary>
        /// Gets/sets the database to access for some server based data providers.
        /// </summary>
        /// 
        public string InitialCatalog { get; set; } = "";

        /// <summary>
        /// For ODBC sources, this represents the Driver value.  For OLEDB data
        /// sources, this represents the Provider value.
        /// </summary>
        /// 
        public string Service { get; set; } = "";




        /// <summary>
        /// Checks the AppSettings section of the current configuration file for
        /// any of the settings of this class.
        /// </summary>
        /// 
        private void LoadFromConfig()
        {
            AppSettingsSection appSettings = this.GetAppSettings();

            foreach (string keyName in appSettings.Settings.AllKeys)
            {
                if (this._configGroupName == "")
                {
                    this.DetermineConfigFromSettings(appSettings.Settings, keyName, keyName);
                }
                else
                {
                    if (keyName.Contains(this._configGroupName + "."))
                    {
                        this.DetermineConfigFromSettings(appSettings.Settings, keyName,
                                                    keyName.TrimStart((this._configGroupName + ".").ToCharArray()));
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves a value for a specified property from a configuration collection.
        /// </summary>
        /// 
        /// <param name="SettingsCollection">
        /// The configuration collection containing the settings values.
        /// </param>
        /// 
        /// <param name="FullKeyName">
        /// The key name in the collection.
        /// </param>
        /// 
        /// <param name="KeyName">
        /// The key name associated with a property in this class.
        /// </param>
        /// 
        private void DetermineConfigFromSettings(KeyValueConfigurationCollection SettingsCollection, string FullKeyName,
                                                 string KeyName)
        {
            string keyValue = SettingsCollection[FullKeyName].Value;

            switch (KeyName.ToUpper())
            {
                case "DATASOURCE":
                    this.DataSource = keyValue;
                    break;

                case "USERID":
                    this.UserID = keyValue;
                    break;

                case "PASSWORD":
                    this.Password = keyValue;
                    break;

                case "INTEGRATEDSECURITY":
                    this.IntegratedSecurity = Convert.ToBoolean(keyValue);
                    break;

                case "INITIALCATALOG":
                    this.InitialCatalog = keyValue;
                    break;

                case "SERVICE":
                    this.Service = keyValue;
                    break;
            }
        }

        /// <summary>
        /// Retrieves the AppSettings section from the current configuration
        /// file.  Unprotects the section if necessary.
        /// </summary>
        /// 
        /// <returns>
        /// The AppSettingsSection object found from the configuration file.
        /// </returns>
        /// 
        private AppSettingsSection GetAppSettings()
        {
            Configuration config = null;

            if (this._isWebApp)
            {
                config = WebConfigurationManager.OpenWebConfiguration(this._webContext.Request.ApplicationPath);
            }
            else
            {
                config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
            }

            AppSettingsSection section = config.AppSettings;

            if (!section.SectionInformation.IsProtected)
            {
                section.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                section.SectionInformation.ForceSave = true;
                config.Save(ConfigurationSaveMode.Full);
            }
            section.SectionInformation.UnprotectSection();

            return section;
        }

        /// <summary>
        /// Creates the connection string for the specified provider.
        /// </summary>
        /// 
        /// <param name="Provider">
        /// A string containing the provider text.
        /// </param>
        /// 
        /// <returns>
        /// The constructed connection string.
        /// </returns>
        /// 
        private string ConstructOleDBString(string Provider)
        {
            var stringBuilder = new OleDbConnectionStringBuilder
                {
                    Provider = Provider,
                    DataSource = this.DataSource
                };

            if (!string.IsNullOrEmpty(this.UserID))
            {
                stringBuilder["User ID"] = this.UserID;
                if (!string.IsNullOrEmpty(this.Password))
                {
                    stringBuilder["Password"] = this.Password;
                }
            }

            return stringBuilder.ConnectionString;
        }

        /// <summary>
        /// Creates the connection string for an ODBC source.
        /// </summary>
        /// 
        /// <returns>
        /// The constructed connection string.
        /// </returns>
        /// 
        private string ConstructODBCString()
        {
            var stringBuilder = new OdbcConnectionStringBuilder
            {
                Driver = this.Service
            };

            stringBuilder["Dbq"] = this.DataSource;

            if (!string.IsNullOrEmpty(this.UserID))
            {
                stringBuilder["uid"] = this.UserID;
                if (!string.IsNullOrEmpty(this.Password))
                {
                    stringBuilder["pwd"] = this.Password;
                }
            }

            return stringBuilder.ConnectionString;
        }

        /// <summary>
        /// Creates the connection string for a SQL server.
        /// </summary>
        /// 
        /// <returns>
        /// The constructed connection string.
        /// </returns>
        /// 
        private string ConstructSQLString()
        {
            var stringBuilder = new SqlConnectionStringBuilder
                {
                    DataSource         = this.DataSource,
                    InitialCatalog     = this.InitialCatalog,
                    IntegratedSecurity = this.IntegratedSecurity
                };

            if (!this.IntegratedSecurity)
            {
                stringBuilder.UserID   = this.UserID;
                stringBuilder.Password = this.Password;
            }

            return stringBuilder.ConnectionString;
        }

        /// <summary>
        /// Creates the connection string for an Oracle server.
        /// </summary>
        /// 
        /// <returns>
        /// The constructed connection string.
        /// </returns>
        /// 
        private string ConstructOracleString()
        {
            var stringBuilder = new OracleConnectionStringBuilder
                {
                    DataSource         = this.DataSource,
                    IntegratedSecurity = this.IntegratedSecurity
                };

            if (!IntegratedSecurity)
            {
                stringBuilder.UserID   = this.UserID;
                stringBuilder.Password = this.Password;
            }

            return stringBuilder.ConnectionString;
        }




        /// <summary>
        /// Creates the connection string for the current data provider.
        /// </summary>
        /// 
        /// <remarks>
        /// This method assumes that all of the relevant properties for the 
        /// current data provider have been set.
        /// </remarks>
        /// 
        /// <returns>
        /// A string containing the constructed connection string.
        /// </returns>
        /// 
        public string ConstructConnectionString()
        {
            string connectionString = "";

            switch (ProviderType)
            {
                case DataProvider.Access:
                    connectionString = this.ConstructOleDBString("Microsoft.Jet.OLEDB.4.0");
                    break;

                case DataProvider.OleDb:
                    connectionString = this.ConstructOleDBString(this.Service);
                    break;

                case DataProvider.Odbc:
                    connectionString = this.ConstructODBCString();
                    break;

                case DataProvider.Oracle:
                    connectionString = this.ConstructOracleString();
                    break;

                case DataProvider.SqlServer:
                    connectionString = this.ConstructSQLString();
                    break;
            }

            return connectionString;
        }

        /// <summary>
        /// Creates the connection string for the current data provider 
        /// and based on the specified group name.
        /// </summary>
        /// 
        /// <remarks>
        /// This method will load the relevant properties from the current
        /// configuration file based on the specified group name.
        /// </remarks>
        /// 
        /// <param name="SectionGroupName">
        /// The name of the group containing the connection string properties.
        /// </param>
        /// 
        /// <returns>
        /// A string containing the constructed connection string.
        /// </returns>
        /// 
        public string ConstructConnectionString(string SectionGroupName)
        {
            this._configGroupName = SectionGroupName;

            this.LoadFromConfig();

            return this.ConstructConnectionString();
        }
    }
}