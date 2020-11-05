// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using BVNetwork.NotFound.Core.Data;

using EPiServer.Logging;

namespace BVNetwork.NotFound.Core.Upgrade
{
    public static class Upgrader
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        public static bool Valid { get; set; }

        public static void Start(int version)
        {
            if (version == -1)
            {
                Create();
            }
            else
            {
                Upgrade();
            }
        }
        /// <summary>
        /// Create redirects and suggestions tables and SP for version number
        /// </summary>
        private static void Create()
        {
            var dba = DataAccessBaseEx.GetWorker();

            var created = CreateRedirectsTable(dba);

            if (created)
            {
                created = CreateSuggestionsTable(dba);
            }

            if (created)
            {
                created = CreateVersionNumberSp(dba);
            }

            Valid = created;
        }

        private static bool CreateRedirectsTable(DataAccessBaseEx dba)
        {
            Logger.Information("Create 404 handler redirects table START");
            var createTableScript = @"CREATE TABLE [dbo].[404Handler.Redirects](
                                        [Id] [uniqueidentifier] NOT NULL,
                                        [OldUrl] [nvarchar](2000) NOT NULL,
                                        [NewUrl] [nvarchar](2000) NOT NULL,
                                        [State] [int] NOT NULL,
                                        [WildCardSkipAppend] [bit] NOT NULL,
                                        [RedirectType] [int] NOT NULL,
                                        CONSTRAINT [PK_404HandlerRedirects] PRIMARY KEY CLUSTERED ([Id] ASC) ON [PRIMARY]
                                        ) ON [PRIMARY]";
            var created = dba.ExecuteNonQuery(createTableScript);
            Logger.Information("Create 404 handler redirects table END");

            return created;
        }

        private static bool AddRedirectTypeColumnToRedirectsTable(DataAccessBaseEx dba)
        {
            Logger.Information("Alter 404 handler redirects table to add RedirectType column START");
            var alterTableScript = @"ALTER TABLE [dbo].[404Handler.Redirects]
                                            ADD [RedirectType] int NOT NULL
                                        DEFAULT (301)
                                    WITH VALUES";
            var created = dba.ExecuteNonQuery(alterTableScript);
            Logger.Information("Alter 404 handler redirects table to add RedirectType column END");

            return created;
        }

        private static bool CreateSuggestionsTable(DataAccessBaseEx dba)
        {
            Logger.Information("Create 404 handler suggestions table START");
            var createTableScript = @"CREATE TABLE [dbo].[BVN.NotFoundRequests](
                                        [ID] [int] IDENTITY(1,1) NOT NULL,
                                        [OldUrl] [nvarchar](2000) NOT NULL,
                                        [Requested] [datetime] NULL,
                                        [Referer] [nvarchar](2000) NULL
                                        ) ON [PRIMARY]";
            var created = dba.ExecuteNonQuery(createTableScript);
            Logger.Information("Create 404 handler suggestions table END");

            if (created)
            {
                created = CreateSuggestionsTableIndex(dba);
            }

            return created;
        }

        private static bool CreateSuggestionsTableIndex(DataAccessBaseEx dba)
        {
            Logger.Information("Create suggestions table clustered index START");
            var clusteredIndex =
                "CREATE CLUSTERED INDEX NotFoundRequests_ID ON [dbo].[BVN.NotFoundRequests] (ID)";

            var created = dba.ExecuteNonQuery(clusteredIndex);
            if (!created)
            {
                Logger.Error("An error occurred during the creation of the 404 handler redirects clustered index. Canceling.");
            }

            Logger.Information("Create suggestions table clustered index END");
            return created;
        }

        private static bool CreateVersionNumberSp(DataAccessBaseEx dba)
        {
            Logger.Information("Create 404 handler version SP START");
            var versionSp =
                $@"CREATE PROCEDURE [dbo].[bvn_notfoundversion] AS RETURN {Configuration.Configuration.CurrentVersion}";

            var created = dba.ExecuteNonQuery(versionSp);

            if (!created)
            {
                Logger.Error("An error occurred during the creation of the 404 handler version stored procedure. Canceling.");
            }

            Logger.Information("Create 404 handler version SP END");
            return created;
        }

        private static void Upgrade()
        {
            var dba = DataAccessBaseEx.GetWorker();

            if (!TableExists("404Handler.Redirects", dba))
            {
                Valid = CreateRedirectsTable(dba);
            }

            if (!ColumnExists("404Handler.Redirects", "RedirectType", dba))
            {
                Valid = AddRedirectTypeColumnToRedirectsTable(dba);
            }

            if (!SuggestionsTableIndexExists(dba))
            {
                Valid = CreateSuggestionsTableIndex(dba);
            }

            if (Valid)
            {
                UpdateVersionNumber(dba);
            }
        }

        private static bool TableExists(string tableName, DataAccessBaseEx dba)
        {
            var cmd = $@"SELECT *
                 FROM INFORMATION_SCHEMA.TABLES
                 WHERE TABLE_SCHEMA = 'dbo'
                 AND  TABLE_NAME = '{tableName}'";
            var num = dba.ExecuteScalar(cmd);
            return num != 0;
        }

        private static bool ColumnExists(string tableName, string columnName, DataAccessBaseEx dba)
        {
            var cmd = $@"SELECT 1
                        FROM sys.columns
                        WHERE Name = '{columnName}'
                        AND  Object_ID = Object_ID(N'dbo.[{tableName}]')";
            var num = dba.ExecuteScalar(cmd);
            return num != 0;
        }

        private static bool SuggestionsTableIndexExists(DataAccessBaseEx dba)
        {
            var indexCheck =
                $@"SELECT COUNT(*)
                 FROM sys.indexes
                 WHERE name='NotFoundRequests_ID' AND object_id = OBJECT_ID('[dbo].[BVN.NotFoundRequests]')";

            var num = dba.ExecuteScalar(indexCheck);
            return num != 0;
        }

        private static void UpdateVersionNumber(DataAccessBaseEx dba)
        {
            var versionSp =
                $@"ALTER PROCEDURE [dbo].[bvn_notfoundversion] AS RETURN {Configuration.Configuration.CurrentVersion}";
            Valid = dba.ExecuteNonQuery(versionSp);
        }
    }
}
