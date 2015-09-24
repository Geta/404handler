using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Web;
using System.Data;
using log4net;
using System.Data.SqlClient;

namespace BVNetwork.Bvn.FileNotFound.Data
{
    public class DataAccessBaseEx : EPiServer.DataAccess.DataAccessBase
    {
        public DataAccessBaseEx(EPiServer.Data.IDatabaseHandler handler)
            : base(handler)
        {
            this.Database = handler;
        }

        public static DataAccessBaseEx GetWorker()
        {
            return EPiServer.ServiceLocation.ServiceLocator.Current.GetInstance<DataAccessBaseEx>();
        }
        private const string REDIRECTSTABLE = "[dbo].[BVN.NotFoundRequests]";

        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public DataSet ExecuteSQL(string sqlCommand, List<IDbDataParameter> parameters)
        {


            return base.Database.Execute<DataSet>(delegate
            {
                DataSet ds = new DataSet();
                try
                {
                    DbCommand command = this.CreateCommand(sqlCommand);
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
                    _log.Error(string.Format("An error occureding in the ExecuteSQL method with the following sql{0}. Exception:{1}", sqlCommand, ex));
                }

                return ds;
            });

        }

        public bool ExecuteNonQuery(string sqlCommand)
        {
            return base.Database.Execute<bool>(delegate
            {
                bool success = true;

                try
                {
                    IDbCommand command = this.CreateCommand(sqlCommand);
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    success = false;
                    _log.Error(string.Format("An error occureding in the ExecuteSQL method with the following sql{0}. Exception:{1}", sqlCommand, ex));

                }
                return success;

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

            return Database.Execute<int>(() =>
    {

        string sqlCommand = "dbo.bvn_notfoundversion";
        int version = -1;
        try
        {

            //  base.Database.Connection.Open();
            DbCommand command = this.CreateCommand();

            command.Parameters.Add(this.CreateReturnParameter());
            command.CommandText = sqlCommand;
            command.CommandType = CommandType.StoredProcedure;
            //  command.Connection = base.Database.Connection;
            command.ExecuteNonQuery();
            version = Convert.ToInt32(this.GetReturnValue(command).ToString());
        }
        catch (SqlException)
        {
            _log.Info("Stored procedure not found. Creating it.");
            return version;
        }
        catch (Exception ex)
        {
            _log.Error(string.Format("Error during NotFoundHandler version check:{0}", ex));
        }
        finally
        {
            // base.Database.Connection.Close();
        }
        return version;
    });

        }


        public void LogRequestToDb(string oldUrl, string referer, DateTime now)
        {
            Database.Execute<bool>(() =>
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
                       //   base.Database.Connection.Open();
                       // this.OpenConnection();
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
                       command.Connection = base.Database.Connection;
                       command.ExecuteNonQuery();
                   }
                   catch (Exception ex)
                   {

                       _log.Error("An error occured while logging a 404 handler error. Ex:" + ex);
                   }
                   finally
                   {
                       //    base.Database.Connection.Close();
                   }
                   return true;
               });
        }




    }
}