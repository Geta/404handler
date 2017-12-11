using System;
using System.Linq;
using BVNetwork.NotFound.Core.CustomRedirects;
using BVNetwork.NotFound.Core.Data;

using EPiServer.Logging;

namespace BVNetwork.NotFound.Core.Upgrade
{
    public static class Upgrader
    {
        private static readonly ILogger _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
        /// Create redirects table and SP for version number
        /// </summary>
        private static void Create()
        {
            bool create = true;

            var dba = DataAccessBaseEx.GetWorker();

            _log.Information("Create 404 handler redirects table START");
            string createTableScript = @"CREATE TABLE [dbo].[BVN.NotFoundRequests](
	                                    [ID] [int] IDENTITY(1,1) NOT NULL,
	                                    [OldUrl] [nvarchar](2000) NOT NULL,
	                                    [Requested] [datetime] NULL,
	                                    [Referer] [nvarchar](2000) NULL
                                        ) ON [PRIMARY]";
            create = dba.ExecuteNonQuery(createTableScript);

            _log.Information("Create 404 handler redirects table END");


            if (create)
            {
                _log.Information("Create 404 handler version SP START");
                string versionSP = @"CREATE PROCEDURE [dbo].[bvn_notfoundversion] AS RETURN " + Configuration.Configuration.CURRENT_VERSION;

                if (!dba.ExecuteNonQuery(versionSP))
                {
                    create = false;
                    _log.Error("An error occured during the creation of the 404 handler version stored procedure. Canceling.");
                }

                _log.Information("Create 404 handler version SP END");
            }

            if (create)
            {
                _log.Information("Create Clustered index START");
                string clusteredIndex =
                    "CREATE CLUSTERED INDEX NotFoundRequests_ID ON [dbo].[BVN.NotFoundRequests] (ID)";

                if (!dba.ExecuteNonQuery(clusteredIndex))
                {
                    create = false;
                    _log.Error("An error occurred during the creation of the 404 handler redirects clustered index. Canceling.");
                }

                _log.Information("Create Clustered index END");
            }

            Valid = create;

            // copy dds items, if there are any.
            try
            {
                // the old redirect class is obsolete, and should only be used for this upgrade
#pragma warning disable CS0618
                var oldCustomrRedirectStore = DataStoreFactory.GetStore(typeof(FileNotFound.CustomRedirects.CustomRedirect));
                var oldCustomRedirects = oldCustomrRedirectStore.Items<FileNotFound.CustomRedirects.CustomRedirect>().ToList();
#pragma warning restore CS0618
                if (oldCustomRedirects.Count > 0)
                {
                    var newCustomrRedirectStore = DataStoreFactory.GetStore(typeof(CustomRedirect));
                    DataStoreHandler dsHandler = new DataStoreHandler();
                    foreach (var oldCustomRedirect in oldCustomRedirects)
                    {
                        var newRedirect = new CustomRedirect(oldCustomRedirect.OldUrl, oldCustomRedirect.NewUrl, oldCustomRedirect.WildCardSkipAppend, false, false);
                        dsHandler.SaveCustomRedirect(newRedirect);
                    }
                    // oldCustomrRedirectStore.DeleteAll();
                }
            }
            catch (Exception ex)
            {
                _log.Error("Error during DDS upgrade: " + ex);
            }

        }

        private static void Upgrade()
        {
            var dba = DataAccessBaseEx.GetWorker();

            string indexCheck =
                "SELECT COUNT(*) FROM sys.indexes WHERE name='NotFoundRequests_ID' AND object_id = OBJECT_ID('[dbo].[BVN.NotFoundRequests]')";

            int num = dba.ExecuteScalar(indexCheck);
            if (num == 0)
            {
                if (!dba.ExecuteNonQuery("CREATE CLUSTERED INDEX NotFoundRequests_ID ON [dbo].[BVN.NotFoundRequests] (ID)"))
                {
                    Valid = false;
                    _log.Error("An error occurred during the creation of the 404 handler redirects clustered index. Canceling.");
                }
                _log.Information("Create Clustered index END");
            }
            if (Valid)
            {
                string versionSP = @"ALTER PROCEDURE [dbo].[bvn_notfoundversion] AS RETURN " + Configuration.Configuration.CURRENT_VERSION;
                Valid = dba.ExecuteNonQuery(versionSP);
                // TODO: Alter table if necessary
            }
        }
    }
}