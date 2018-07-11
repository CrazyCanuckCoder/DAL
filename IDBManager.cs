using System;
using System.Collections.Generic;
using System.Data;

namespace DAL
{
    /// <summary>
    /// Enumerates the different type of databases.
    /// </summary>
    /// 
    public enum DataProvider
    {
        None,
        Oracle,
        SqlServer,
        OleDb,
        Odbc,
        Access
    }

    /// <summary>
    /// The interface for the database connection manager.
    /// </summary>
    /// 
    public interface IDBManager : IDisposable
    {
        DataProvider ProviderType { get; set; }

        string ConnectionString { get; set; }

        IDbConnection Connection { get; }

        IDbTransaction Transaction { get; }

        IDbCommand Command { get; }

        List<IDataParameter> Parameters { get; }

        void BeginTransaction();
        void CommitTransaction();
        void AddParameter(string ParamName, object ObjValue);
        IDataReader ExecuteReader(CommandType CommandType, string CommandText);
        DataSet ExecuteDataSet(CommandType CommandType, string CommandText);
        DataTable ExecuteDataTable(CommandType CommandType, string CommandText);
        T ExecuteScalar<T>(CommandType CommandType, string CommandText);
        int ExecuteNonQuery(CommandType CommandType, string CommandText);
    }
}