using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using EPiServer.Data;
using EPiServer.Logging;

namespace BVNetwork.NotFound.Core.Data
{
    public class DataAccessBaseEx  : EPiServer.DataAccess.DataAccessBase
    {
#if CMS10
        public DataAccessBaseEx(EPiServer.Data.IDatabaseExecutor handler)
#else
        private IDatabaseHandler Executor
        {
            get { return Database; }
            set { Database = value; }
        }
        public DataAccessBaseEx(EPiServer.Data.IDatabaseHandler handler)
#endif
            : base(handler)
        {
            Executor = handler;
        }

        public static DataAccessBaseEx GetWorker()
        {
            return EPiServer.ServiceLocation.ServiceLocator.Current.GetInstance<DataAccessBaseEx>();
        }
        private const string REDIRECTSTABLE = "[dbo].[BVN.NotFoundRequests]";

        private static readonly ILogger Logger = LogManager.GetLogger();

        public DataSet ExecuteSQL(string sqlCommand, List<IDbDataParameter> parameters)
        {


            return Executor.Execute<DataSet>(delegate
            {
                using (DataSet ds = new DataSet())
                {
                    try
                    {
                        DbCommand command = CreateCommand(sqlCommand);
                        if (parameters != null)
                        {
                            foreach (SqlParameter parameter in parameters)
                            {
                                command.Parameters.Add(parameter);
                            }
                        }
                        command.CommandType = CommandType.Text;
                        base.CreateDataAdapter(command).Fill(ds);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(string.Format("An error occureding in the ExecuteSQL method with the following sql{0}. Exception:{1}", sqlCommand, ex));
                    }

                    return ds;
                }
            });

        }

        public bool ExecuteNonQuery(string sqlCommand)
        {
            return Executor.Execute<bool>(delegate
            {
                bool success = true;

                try
                {
                    IDbCommand command = CreateCommand(sqlCommand);
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    success = false;
                    Logger.Error(string.Format("An error occureding in the ExecuteSQL method with the following sql{0}. Exception:{1}", sqlCommand, ex));

                }
                return success;

            });
        }

        public int ExecuteScalar(string sqlCommand)
        {
            return Executor.Execute<int>(delegate
            {
                int result;
                try
                {
                    IDbCommand dbCommand = this.CreateCommand(sqlCommand);
                    dbCommand.CommandType = CommandType.Text;
                    result = (int)dbCommand.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    result = 0;
                    Logger.Error(
                        string.Format(
                            "An error occureding in the ExecuteScalar method with the following sql{0}. Exception:{1}",
                            sqlCommand,
                            ex));

                }
                return result;
            });
        }

        public DataSet GetAllClientRequestCount()
        {
            string sqlCommand = string.Format("SELECT [OldUrl], COUNT(*) as Requests FROM {0} GROUP BY [OldUrl] order by Requests desc", REDIRECTSTABLE);
            return ExecuteSQL(sqlCommand, null);
        }

        public void DeleteRowsForRequest(string oldUrl)
        {
            string sqlCommand = string.Format("DELETE FROM {0} WHERE [OldUrl] = @oldurl", REDIRECTSTABLE);
            var oldUrlParam = this.CreateParameter("oldurl", DbType.String, 4000);
            oldUrlParam.Value = oldUrl;
            var parameters = new List<IDbDataParameter>();
            parameters.Add(oldUrlParam);
            ExecuteSQL(sqlCommand, parameters);
        }

        public void DeleteSuggestions(int maxErrors, int minimumDaysOld)
        {
            string sqlCommand = string.Format(@"delete from {0}
                                                where [OldUrl] in (
                                                select [OldUrl]
                                                  from (
                                                      select [OldUrl]
                                                      from {0}
                                                      Where DATEDIFF(day, [Requested], getdate()) >= {1}
                                                      group by [OldUrl]
                                                      having count(*) <= {2}
                                                      ) t
                                                )",REDIRECTSTABLE, minimumDaysOld, maxErrors);
            ExecuteSQL(sqlCommand, null);
        }
        public void DeleteAllSuggestions()
        {
            string sqlCommand = string.Format(@"delete from {0}", REDIRECTSTABLE);
            ExecuteSQL(sqlCommand, null);
        }

        public DataSet GetRequestReferers(string url)
        {
            string sqlCommand = string.Format("SELECT [Referer], COUNT(*) as Requests FROM {0} where [OldUrl] = @oldurl  GROUP BY [Referer] order by Requests desc", REDIRECTSTABLE);
            var oldUrlParam = this.CreateParameter("oldurl", DbType.String, 4000);
            oldUrlParam.Value = url;

            var parameters = new List<IDbDataParameter>();
            parameters.Add(oldUrlParam);
            return ExecuteSQL(sqlCommand, parameters);

        }

        public DataSet GetTotalNumberOfSuggestions()
        {

            string sqlCommand = string.Format("SELECT COUNT(DISTINCT [OldUrl]) FROM {0}", REDIRECTSTABLE);
            return ExecuteSQL(sqlCommand, null);
        }

        public int Check404Version()
        {

            return Executor.Execute<int>(() =>
            {

                string sqlCommand = "dbo.bvn_notfoundversion";
                int version = -1;
                try
                {
                    DbCommand command = this.CreateCommand();

                    command.Parameters.Add(this.CreateReturnParameter());
                    command.CommandText = sqlCommand;
                    command.CommandType = CommandType.StoredProcedure;
                    command.ExecuteNonQuery();
                    version = Convert.ToInt32(this.GetReturnValue(command).ToString());
                }
                catch (SqlException)
                {
                    Logger.Information("Stored procedure not found. Creating it.");
                    return version;
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format("Error during NotFoundHandler version check:{0}", ex));
                }
                return version;
            });

        }

        public void LogRequestToDb(string oldUrl, string referer, DateTime now)
        {
            Executor.Execute<bool>(() =>
               {
                   string sqlCommand = "INSERT INTO [dbo].[BVN.NotFoundRequests] (" +
                                       "Requested, OldUrl, " +
                                       "Referer" +
                                       ") VALUES (" +
                                       "@requested, @oldurl, " +
                                       "@referer" +
                                       ")";
                   try
                   {
                       IDbCommand command = this.CreateCommand();

                       var requstedParam = this.CreateParameter("requested", DbType.DateTime, 0);
                       requstedParam.Value = now;
                       var refererParam = this.CreateParameter("referer", DbType.String, 4000);
                       refererParam.Value = referer;
                       var oldUrlParam = this.CreateParameter("oldurl", DbType.String, 4000);
                       oldUrlParam.Value = oldUrl;
                       command.Parameters.Add(requstedParam);
                       command.Parameters.Add(refererParam);
                       command.Parameters.Add(oldUrlParam);
                       command.CommandText = sqlCommand;
                       command.CommandType = CommandType.Text;
                       command.ExecuteNonQuery();
                   }
                   catch (Exception ex)
                   {

                       Logger.Error("An error occured while logging a 404 handler error. Ex:" + ex);
                   }
                   return true;
               });
        }
    }
}