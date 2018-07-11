using System;
using System.Collections.Generic;
using System.Data;

namespace DAL
{
    /// <summary>
    /// The class that accesses the data provider.
    /// </summary>
    /// 
    public class DBManager : IDBManager
    {
        /// <summary>
        /// Standard constructor.
        /// </summary>
        /// 
        public DBManager()
        {
        }

        /// <summary>
        /// Sets the data provider type.
        /// </summary>
        /// 
        /// <param name="ProviderType"> 
        /// The data provider type.
        /// </param>
        /// 
        public DBManager(DataProvider ProviderType)
        {
            this.ProviderType = ProviderType;
        }

        /// <summary>
        /// Sets the provider type and the connection string.
        /// </summary>
        /// 
        /// <param name="ProviderType">
        /// The data provider type.
        /// </param>
        /// 
        /// <param name="ConnectionString">
        /// The connection string to use for this object.
        /// </param>
        /// 
        public DBManager(DataProvider ProviderType, string ConnectionString) : this(ProviderType)
        {
            this.ConnectionString = ConnectionString ?? throw new ArgumentNullException(nameof(ConnectionString));
        }




        /// <summary>
        /// Gets the connection object.
        /// </summary>
        /// 
        public IDbConnection Connection { get; private set; }

        /// <summary>
        /// Gets/sets the database type.
        /// </summary>
        /// 
        public DataProvider ProviderType { get; set; }

        /// <summary>
        /// Gets/sets the connection string.
        /// </summary>
        /// 
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets the command object.
        /// </summary>
        /// 
        public IDbCommand Command { get; private set; }

        /// <summary>
        /// Gets the transaction object.
        /// </summary>
        /// 
        public IDbTransaction Transaction { get; private set; }

        /// <summary>
        /// Gets the array of parameter objects.
        /// </summary>
        /// 
        public List<IDataParameter> Parameters { get; private set; }




        /// <summary>
        /// Attaches the specified parameters to a command object.
        /// </summary>
        /// 
        /// <param name="NewCommand">
        /// The command object.
        /// </param>
        /// 
        /// <param name="CommandParameters">
        /// An array of parameters to attach to the command object.
        /// </param>
        /// 
        private void AttachParameters(IDbCommand NewCommand, IEnumerable<IDataParameter> CommandParameters)
        {
            if (NewCommand == null)
            {
                throw new ArgumentNullException(nameof(NewCommand));
            }
            else if (CommandParameters == null)
            {
                throw new ArgumentNullException(nameof(CommandParameters));
            }

            foreach (IDataParameter idbParameter in CommandParameters)
            {
                if (idbParameter.Value == null)
                {
                    idbParameter.Value = DBNull.Value;
                }
                NewCommand.Parameters.Add(idbParameter);
            }
        }

        /// <summary>
        /// Sets up the command object with the specified parameters.
        /// </summary>
        /// 
        /// <param name="NewCommand">
        /// The command object to setup.
        /// </param>
        /// 
        /// <param name="NewConnection">
        /// The connection object to use with the command object.
        /// </param>
        /// 
        /// <param name="NewTransaction">
        /// The transaction object to use with the command object.
        /// </param>
        /// 
        /// <param name="CommandType">
        /// The command type of the command object.
        /// </param>
        /// 
        /// <param name="CommandText">
        /// The command text to use with the command object.
        /// </param>
        /// 
        /// <param name="CommandParameters">
        /// An array of parameters to use with the command object.
        /// </param>
        /// 
        /// <exception cref="ArgumentNullException" />
        /// 
        private void PrepareCommand(IDbCommand NewCommand, IDbConnection NewConnection, IDbTransaction NewTransaction,
                                    CommandType CommandType, string CommandText,
                                    IEnumerable<IDataParameter> CommandParameters)
        {
            if (NewCommand == null)
            {
                throw new ArgumentNullException(nameof(NewCommand));
            }

            NewCommand.Connection     = NewConnection ?? throw new ArgumentNullException(nameof(NewConnection));
            NewCommand.CommandText    = CommandText   ?? throw new ArgumentNullException(nameof(CommandText));
            NewCommand.CommandType    = CommandType;
            NewCommand.CommandTimeout = (int) TimeSpan.FromMinutes(15).TotalSeconds;

            if (NewTransaction != null)
            {
                NewCommand.Transaction = NewTransaction;
            }

            if (CommandParameters != null)
            {
                AttachParameters(NewCommand, CommandParameters);
            }
        }

        /// <summary>
        /// Opens the connection to the data provider.
        /// </summary>
        /// 
        /// <remarks>
        /// Assumes the connection has already been created.
        /// </remarks>
        /// 
        private void Open()
        {
            if (this.Connection == null)
            {
                throw new Exception("Connection has not been setup yet.");
            }

            this.Connection.ConnectionString = ConnectionString;
            if (this.Connection.State != ConnectionState.Open)
            {
                this.Connection.Open();
            }
        }

        /// <summary>
        /// Closes the connection the data provider.
        /// </summary>
        /// 
        private void Close()
        {
            if (this.Connection?.State != ConnectionState.Closed)
            {
                this.Connection.Close();
                this.Connection.Dispose();
            }
        }




        /// <summary>
        /// Disposes this object.
        /// </summary>
        /// 
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Close();
            this.Command     = null;
            this.Transaction = null;
            this.Connection  = null;
        }

        /// <summary>
        /// Adds a new parameter to the list of parameters.
        /// </summary>
        /// 
        /// <param name="ParamName">
        /// The name of the parameter.
        /// </param>
        /// 
        /// <param name="ObjValue">
        /// The value to assign to the parameter.
        /// </param>
        /// 
        public void AddParameter(string ParamName, object ObjValue)
        {
            if (this.Parameters == null)
            {
                this.Parameters = new List<IDataParameter>();
            }

            IDataParameter newParameter = DBManagerFactory.GetParameter(this.ProviderType);
            newParameter.ParameterName = ParamName ?? throw new ArgumentNullException(nameof(ParamName));
            newParameter.Value = ObjValue ?? DBNull.Value;

            this.Parameters.Add(newParameter);
        }

        /// <summary>
        /// Starts the transaction.  A transaction is created if one was not already.
        /// </summary>
        /// 
        public void BeginTransaction()
        {
            if (this.Transaction == null)
            {
                this.Transaction = DBManagerFactory.GetTransaction(ProviderType);
            }
            this.Command.Transaction = this.Transaction;
        }

        /// <summary>
        /// Commits changes to the current transaction.
        /// </summary>
        /// 
        public void CommitTransaction()
        {
            this.Transaction?.Commit();
            this.Transaction = null;
        }

        /// <summary>
        /// Creates and then returns a data reader object.
        /// </summary>
        /// 
        /// <remarks>
        /// It is up to the calling procedure to close the reader and dispose
        /// of the connection object.
        /// </remarks>
        /// 
        /// <param name="CommandType">
        /// The command type to use in creating the data reader.
        /// </param>
        /// 
        /// <param name="CommandText">
        /// The command text to use in creating the data reader.
        /// </param>
        /// 
        /// <returns>
        /// The created data reader.
        /// </returns>
        /// 
        public IDataReader ExecuteReader(CommandType CommandType, string CommandText)
        {
            this.Connection = DBManagerFactory.GetConnection(this.ProviderType);
            this.Open();

            this.Command = DBManagerFactory.GetCommand(this.ProviderType);
            this.PrepareCommand(this.Command, this.Connection, this.Transaction, CommandType, CommandText, this.Parameters);
            IDataReader dataReader = this.Command.ExecuteReader(CommandBehavior.CloseConnection);
            this.Command.Parameters.Clear();

            return dataReader;
        }

        /// <summary>
        /// Executes a query that does not intend to return a query.
        /// </summary>
        /// 
        /// <param name="CommandType">
        /// The type of command to execute.
        /// </param>
        /// 
        /// <param name="CommandText">
        /// The command text for the command.
        /// </param>
        /// 
        /// <returns>
        /// An integer value indicating success or failure of the command.
        /// </returns>
        /// 
        public int ExecuteNonQuery(CommandType CommandType, string CommandText)
        {
            int returnValue = 0;

            using (this.Connection = DBManagerFactory.GetConnection(this.ProviderType))
            {
                this.Open();

                this.Command = DBManagerFactory.GetCommand(this.ProviderType);
                this.PrepareCommand(this.Command, this.Connection, this.Transaction, CommandType, CommandText, this.Parameters);
                returnValue = this.Command.ExecuteNonQuery();
                this.Command.Parameters.Clear();
            }

            return returnValue;
        }

        /// <summary>
        /// Executes a command intended to return a single value.
        /// </summary>
        /// 
        /// <param name="CommandType">
        /// The type of command to execute.
        /// </param>
        /// 
        /// <param name="CommandText">
        /// The command text for the command.
        /// </param>
        /// 
        /// <returns>
        /// The query's returned value.
        /// </returns>
        /// 
        public T ExecuteScalar<T>(CommandType CommandType, string CommandText)
        {
            T returnValue = default(T);

            using (this.Connection = DBManagerFactory.GetConnection(this.ProviderType))
            {
                this.Open();

                this.Command = DBManagerFactory.GetCommand(this.ProviderType);
                this.PrepareCommand(this.Command, this.Connection, this.Transaction, CommandType, CommandText, this.Parameters);

                try
                { 
                    returnValue = (T) Convert.ChangeType(this.Command.ExecuteScalar(), typeof(T));
                }
                catch (InvalidCastException)
                {
                    returnValue = default(T);
                }
                catch (FormatException)
                {
                    returnValue = default(T);
                }
                catch (OverflowException)
                {
                    returnValue = default(T);
                }

                this.Command.Parameters.Clear();
            }

            return returnValue;
        }

        /// <summary>
        /// Executes a command and returns the result in a dataset.
        /// </summary>
        /// 
        /// <param name="CommandType">
        /// The type of command to execute.
        /// </param>
        /// 
        /// <param name="CommandText">
        /// The command text for the command.
        /// </param>
        /// 
        /// <returns>
        /// A DataSet containing the results of the command.
        /// </returns>
        /// 
        public DataSet ExecuteDataSet(CommandType CommandType, string CommandText)
        {
            var dataSet = new DataSet();

            using (this.Connection = DBManagerFactory.GetConnection(this.ProviderType))
            {
                this.Open();

                this.Command = DBManagerFactory.GetCommand(this.ProviderType);
                this.PrepareCommand(this.Command, this.Connection, this.Transaction, CommandType, CommandText, this.Parameters);
                IDbDataAdapter dataAdapter = DBManagerFactory.GetDataAdapter(this.ProviderType);
                dataAdapter.SelectCommand = this.Command;
                dataAdapter.Fill(dataSet);
                this.Command.Parameters.Clear();
            }

            return dataSet;
        }

        /// <summary>
        /// Executes a command and returns the result in a data table.
        /// </summary>
        /// 
        /// <param name="CommandType">
        /// The type of command to execute.
        /// </param>
        /// 
        /// <param name="CommandText">
        /// The command text for the command.
        /// </param>
        /// 
        /// <returns>
        /// A DataTable containing the results of the command.
        /// </returns>
        /// 
        public DataTable ExecuteDataTable(CommandType CommandType, string CommandText)
        {
            DataTable returnTable = null;

            DataSet queryDS = this.ExecuteDataSet(CommandType, CommandText);
            if (queryDS?.Tables.Count > 0)
            {
                returnTable = queryDS.Tables[0].Copy();
            }

            return returnTable;
        }
    }
}