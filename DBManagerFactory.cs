using System;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.OracleClient;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// Provides methods to create different components needed to connect and
    /// access databases.
    /// </summary>
    /// 
    internal static class DBManagerFactory
    {

        /// <summary>
        /// Gets a connection object for the specified data provider.
        /// </summary>
        /// 
        /// <param name="ProviderType">
        /// The data provider indicator.
        /// </param>
        /// 
        /// <returns>
        /// A connection object.
        /// </returns>
        /// 
        internal static IDbConnection GetConnection(DataProvider ProviderType)
        {
            IDbConnection iDbConnection = null;

            switch (ProviderType)
            {
                case DataProvider.SqlServer:
                    iDbConnection = new SqlConnection();
                    break;

                case DataProvider.OleDb:
                    iDbConnection = new OleDbConnection();
                    break;

                case DataProvider.Odbc:
                    iDbConnection = new OdbcConnection();
                    break;

                case DataProvider.Oracle:
                    iDbConnection = new OracleConnection();
                    break;
            }

            return iDbConnection;
        }

        /// <summary>
        /// Gets a command object for the specified data provider.
        /// </summary>
        /// 
        /// <param name="ProviderType">
        /// The data provider indicator.
        /// </param>
        /// 
        /// <returns>
        /// A command object.
        /// </returns>
        /// 
        internal static IDbCommand GetCommand(DataProvider ProviderType)
        {
            IDbCommand idbCommand = null;

            switch (ProviderType)
            {
                case DataProvider.SqlServer:
                    idbCommand = new SqlCommand();
                    break;

                case DataProvider.OleDb:
                    idbCommand = new OleDbCommand();
                    break;

                case DataProvider.Odbc:
                    idbCommand = new OdbcCommand();
                    break;

                case DataProvider.Oracle:
                    idbCommand = new OracleCommand();
                    break;
            }

            idbCommand.CommandTimeout = (int) TimeSpan.FromMinutes(10).TotalSeconds;

            return idbCommand;
        }

        /// <summary>
        /// Returns a data adapter object for the specified data provider.
        /// </summary>
        /// 
        /// <param name="ProviderType">
        /// The data provider indicator.
        /// </param>
        /// 
        /// <returns>
        /// A data adapter object.
        /// </returns>
        /// 
        internal static IDbDataAdapter GetDataAdapter(DataProvider ProviderType)
        {
            IDbDataAdapter idbDataAdapter = null;

            switch (ProviderType)
            {
                case DataProvider.SqlServer:
                    idbDataAdapter = new SqlDataAdapter();
                    break;

                case DataProvider.OleDb:
                    idbDataAdapter = new OleDbDataAdapter();
                    break;

                case DataProvider.Odbc:
                    idbDataAdapter = new OdbcDataAdapter();
                    break;

                case DataProvider.Oracle:
                    idbDataAdapter = new OracleDataAdapter();
                    break;
            }

            return idbDataAdapter;
        }

        /// <summary>
        /// Returns a transaction object for the specified data provider.
        /// </summary>
        /// 
        /// <param name="ProviderType">
        /// The data provider indicator.
        /// </param>
        /// 
        /// <returns>
        /// A transaction object.
        /// </returns>
        /// 
        internal static IDbTransaction GetTransaction(DataProvider ProviderType)
        {
            IDbConnection  iDbConnection  = GetConnection(ProviderType);
            IDbTransaction iDbTransaction = iDbConnection.BeginTransaction();

            return iDbTransaction;
        }

        /// <summary>
        /// Returns a parameter object for the specified data provider.
        /// </summary>
        /// 
        /// <param name="ProviderType">
        /// The data provider indicator.
        /// </param>
        /// 
        /// <returns>
        /// A parameter object.
        /// </returns>
        /// 
        internal static IDataParameter GetParameter(DataProvider ProviderType)
        {
            IDataParameter iDataParameter = null;

            switch (ProviderType)
            {
                case DataProvider.SqlServer:
                    iDataParameter = new SqlParameter();
                    break;

                case DataProvider.OleDb:
                    iDataParameter = new OleDbParameter();
                    break;

                case DataProvider.Odbc:
                    iDataParameter = new OdbcParameter();
                    break;

                case DataProvider.Oracle:
                    iDataParameter = new OracleParameter();
                    break;
            }

            return iDataParameter;
        }
    }
}