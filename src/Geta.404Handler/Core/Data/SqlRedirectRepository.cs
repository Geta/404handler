// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using BVNetwork.NotFound.Core.CustomRedirects;
using EPiServer.Data;
using EPiServer.Logging;

namespace BVNetwork.NotFound.Core.Data
{
    public class SqlRedirectRepository : IRepository<CustomRedirect>, IRedirectLoader
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly IDatabaseExecutor _executor;

        private const string RedirectsTable = "[dbo].[404Handler.Redirects]";

        private const string AllFields = "Id, OldUrl, NewUrl, State, WildCardSkipAppend";

        public SqlRedirectRepository(IDatabaseExecutor executor)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
        }

        public void Save(CustomRedirect entity)
        {
            if (entity.Id == null)
            {
                Create(entity);
                return;
            }
            Update(entity);
        }

        private void Create(CustomRedirect entity)
        {
            var sqlCommand = $@"INSERT INTO {RedirectsTable}
                                    (Id, OldUrl, NewUrl, State, WildCardSkipAppend)
                                    VALUES
                                    (@id, @oldurl, @newurl, @state, @wildcardskipappend)";

            ExecuteNonQuery(() =>
                CreateCommand(
                    sqlCommand,
                    CreateGuidParameter("id", Guid.NewGuid()),
                    CreateStringParameter("oldurl", entity.OldUrl),
                    CreateStringParameter("newurl", entity.NewUrl),
                    CreateIntParameter("state", entity.State),
                    CreateBoolParameter("wildcardskipappend", entity.WildCardSkipAppend)),
                    "An error occurred while creating a redirect.");
        }

        private void Update(CustomRedirect entity)
        {
            var sqlCommand = $@"UPDATE {RedirectsTable}
                                    SET OldUrl = @oldurl
                                        ,NewUrl = @newurl
                                        ,State = @state
                                        ,WildCardSkipAppend = @wildcardskipappend
                                    WHERE Id = @id";

            ExecuteNonQuery(() =>
                CreateCommand(
                    sqlCommand,
                    CreateGuidParameter("id", entity.Id.ExternalId),
                    CreateStringParameter("oldurl", entity.OldUrl),
                    CreateStringParameter("newurl", entity.NewUrl),
                    CreateIntParameter("state", entity.State),
                    CreateBoolParameter("wildcardskipappend", entity.WildCardSkipAppend)),
                    "An error occurred while updating a redirect.");
        }

        public void Delete(CustomRedirect entity)
        {
            var sqlCommand = $@"DELETE FROM {RedirectsTable}
                                    WHERE Id = @id";

            ExecuteNonQuery(() =>
                CreateCommand(
                    sqlCommand,
                    CreateGuidParameter("id", entity.Id.ExternalId)),
                    "An error occurred while deleting a redirect.");
        }

        public CustomRedirect GetByOldUrl(string oldUrl)
        {
            var sqlCommand = $@"SELECT {AllFields} FROM {RedirectsTable}
                                    WHERE OldUrl = @oldurl";

            return ExecuteQuery(() =>
                CreateCommand(
                    sqlCommand,
                    CreateStringParameter("oldurl", oldUrl)))
                .FirstOrDefault();
        }

        public IEnumerable<CustomRedirect> GetAll()
        {
            var sqlCommand = $@"SELECT {AllFields} FROM {RedirectsTable}";

            return ExecuteQuery(() => CreateCommand(sqlCommand));
        }

        public IEnumerable<CustomRedirect> GetByState(RedirectState state)
        {
            var sqlCommand = $@"SELECT {AllFields} FROM {RedirectsTable}
                                    WHERE State = @state";

            return ExecuteQuery(() =>
                CreateCommand(
                    sqlCommand,
                    CreateIntParameter("state", (int)state)));
        }

        public IEnumerable<CustomRedirect> Find(string searchText)
        {
            var sqlCommand = $@"SELECT {AllFields} FROM {RedirectsTable}
                                    WHERE OldUrl like '%' + @searchText + '%'
                                    OR NewUrl like '%' + @searchText + '%'";

            return ExecuteQuery(() =>
                CreateCommand(
                    sqlCommand,
                    CreateStringParameter("searchText", searchText)));
        }

        private static CustomRedirect ToCustomRedirect(DataRow x)
        {
            return new CustomRedirect(
                x.Field<string>("OldUrl"),
                x.Field<string>("NewUrl"),
                x.Field<bool>("WildCardSkipAppend"))
            {
                Id = Identity.NewIdentity(x.Field<Guid>("Id")),
                State = x.Field<int>("State")
            };
        }

        private DbParameter CreateGuidParameter(string name, Guid value)
        {
            return _executor.CreateParameter(name, DbType.Guid, ParameterDirection.Input, value);
        }

        private DbParameter CreateStringParameter(string name, string value)
        {
            var param = _executor.CreateParameter(name, DbType.String, ParameterDirection.Input, value);
            param.Size = 2000;
            return param;
        }

        private DbParameter CreateIntParameter(string name, int value)
        {
            return _executor.CreateParameter(name, DbType.Int32, ParameterDirection.Input, value);
        }

        private DbParameter CreateBoolParameter(string name, bool value)
        {
            return _executor.CreateParameter(name, DbType.Boolean, ParameterDirection.Input, value);
        }

        private DbCommand CreateCommand(string sqlCommand, params DbParameter[] parameters)
        {
            var command = _executor.CreateCommand();

            foreach (var parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }
            command.CommandText = sqlCommand;
            command.CommandType = CommandType.Text;
            return command;
        }

        private void ExecuteNonQuery(Func<DbCommand> createCommand, string errorMessage)
        {
            _executor.Execute(() =>
            {
                try
                {
                    createCommand().ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Logger.Error(errorMessage, ex);
                    throw;
                }
            });
        }

        private IEnumerable<CustomRedirect> ExecuteQuery(Func<DbCommand> createCommand)
        {
            return _executor.Execute(() =>
            {
                try
                {
                    return ExecuteEnumerableQuery(createCommand());
                }
                catch (Exception ex)
                {
                    Logger.Error("An error occurred while retrieving redirects.", ex);
                    throw;
                }
            });
        }

        private IEnumerable<CustomRedirect> ExecuteEnumerableQuery(DbCommand command)
        {
            var table = ExecuteDataTableQuery(command);

            return table
                .AsEnumerable()
                .Select(ToCustomRedirect);
        }

        private DataTable ExecuteDataTableQuery(DbCommand command)
        {
            var adapter = _executor.DbFactory.CreateDataAdapter();
            if (adapter == null) throw new Exception("Unable to create DbDataAdapter");

            adapter.SelectCommand = command;
            var ds = new DataSet();
            adapter.Fill(ds);
            return ds.Tables[0];
        }
    }
}