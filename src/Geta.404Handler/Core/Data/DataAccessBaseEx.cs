// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

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
        public DataAccessBaseEx(IDatabaseExecutor handler)
            : base(handler)
        {
            Executor = handler;
        }

        public static DataAccessBaseEx GetWorker()
        {
            return EPiServer.ServiceLocation.ServiceLocator.Current.GetInstance<DataAccessBaseEx>();
        }

        private const string Redirectstable = "[dbo].[BVN.NotFoundRequests]";

        private static readonly ILogger Logger = LogManager.GetLogger();

        public DataSet ExecuteSql(string sqlCommand, params IDbDataParameter[] parameters)
        {
            return Executor.Execute(delegate
            {
                var ds = new DataSet();
                try
                {
                    using (var command = CreateCommand(sqlCommand, parameters))
                    {
                        using (var da = CreateDataAdapter(command))
                        {
                            da.Fill(ds);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(
                        $"An error occurred in the ExecuteSQL method with the following sql: {sqlCommand}", ex);
                }

                return ds;
            });

        }

        private DbCommand CreateCommand(string sqlCommand, params IDbDataParameter[] parameters)
        {
            var command = base.CreateCommand(sqlCommand);

            if (parameters != null)
            {
                foreach (var dbDataParameter in parameters)
                {
                    var parameter = (SqlParameter)dbDataParameter;
                    command.Parameters.Add(parameter);
                }
            }

            command.CommandType = CommandType.Text;

            return command;
        }

        public bool ExecuteNonQuery(string sqlCommand, params IDbDataParameter[] parameters)
        {
            return Executor.Execute(delegate
            {
                var success = true;

                try
                {
                    using (var command = CreateCommand(sqlCommand, parameters))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    success = false;
                    Logger.Error(
                        $"An error occurred in the ExecuteSQL method with the following sql: {sqlCommand}", ex);
                }
                return success;
            });
        }

        public int ExecuteScalar(string sqlCommand)
        {
            return Executor.Execute(delegate
            {
                int result;
                try
                {
                    using (var command = CreateCommand(sqlCommand))
                    {
                        result = (int)command.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    result = 0;
                    Logger.Error(
                        $"An error occurred in the ExecuteScalar method with the following sql: {sqlCommand}", ex);
                }
                return result;
            });
        }

        public DataSet GetAllClientRequestCount()
        {
            var sqlCommand =
                $"SELECT [OldUrl], COUNT(*) as Requests FROM {Redirectstable} GROUP BY [OldUrl] order by Requests desc";
            return ExecuteSql(sqlCommand);
        }

        public void DeleteRowsForRequest(string oldUrl)
        {
            var sqlCommand = $"DELETE FROM {Redirectstable} WHERE [OldUrl] = @oldurl";
            var oldUrlParam = CreateParameter("oldurl", DbType.String, 4000);
            oldUrlParam.Value = oldUrl;

            ExecuteNonQuery(sqlCommand, oldUrlParam);
        }

        public void DeleteSuggestions(int maxErrors, int minimumDaysOld)
        {
            var sqlCommand = $@"delete from {Redirectstable}
                                                where [OldUrl] in (
                                                select [OldUrl]
                                                  from (
                                                      select [OldUrl]
                                                      from {Redirectstable}
                                                      Where DATEDIFF(day, [Requested], getdate()) >= {minimumDaysOld}
                                                      group by [OldUrl]
                                                      having count(*) <= {maxErrors}
                                                      ) t
                                                )";
            ExecuteNonQuery(sqlCommand);
        }
        public void DeleteAllSuggestions()
        {
            var sqlCommand = $@"delete from {Redirectstable}";
            ExecuteNonQuery(sqlCommand);
        }

        public DataSet GetRequestReferers(string url)
        {
            var sqlCommand =
                $"SELECT [Referer], COUNT(*) as Requests FROM {Redirectstable} where [OldUrl] = @oldurl  GROUP BY [Referer] order by Requests desc";
            var oldUrlParam = CreateParameter("oldurl", DbType.String, 4000);
            oldUrlParam.Value = url;

            return ExecuteSql(sqlCommand, oldUrlParam);
        }

        public int GetTotalNumberOfSuggestions()
        {
            var sqlCommand = $"SELECT COUNT(DISTINCT [OldUrl]) FROM {Redirectstable}";
            return ExecuteScalar(sqlCommand);
        }

        public int Check404Version()
        {
            return Executor.Execute(() =>
            {
                var sqlCommand = "dbo.bvn_notfoundversion";
                var version = -1;
                try
                {
                    using (var command = CreateCommand())
                    {
                        command.Parameters.Add(CreateReturnParameter());
                        command.CommandText = sqlCommand;
                        command.CommandType = CommandType.StoredProcedure;
                        command.ExecuteNonQuery();
                        version = Convert.ToInt32(GetReturnValue(command).ToString());
                    }
                }
                catch (SqlException)
                {
                    Logger.Information("Stored procedure not found. Creating it.");
                    return version;
                }
                catch (Exception ex)
                {
                    Logger.Error("Error during NotFoundHandler version check", ex);
                }
                return version;
            });
        }

        public void LogRequestToDb(string oldUrl, string referer, DateTime now)
        {
            Executor.Execute(() =>
               {
                   var sqlCommand = @"INSERT INTO [dbo].[BVN.NotFoundRequests]
                                    (Requested, OldUrl, Referer)
                                    VALUES
                                    (@requested, @oldurl, @referer)";
                   try
                   {
                       var requstedParam = CreateParameter("requested", DbType.DateTime, 0);
                       requstedParam.Value = now;

                       var refererParam = CreateParameter("referer", DbType.String, 4000);
                       refererParam.Value = referer ?? string.Empty;

                       var oldUrlParam = CreateParameter("oldurl", DbType.String, 4000);
                       oldUrlParam.Value = oldUrl;

                       using (var command = CreateCommand(sqlCommand, requstedParam, refererParam, oldUrlParam))
                       {
                           command.ExecuteNonQuery();
                       }
                   }
                   catch (Exception ex)
                   {

                       Logger.Error("An error occurred while logging a 404 handler error.", ex);
                   }
                   return true;
               });
        }
    }
}