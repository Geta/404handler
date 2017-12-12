using BVNetwork.NotFound.Core.Data;

using EPiServer.Logging;

namespace BVNetwork.NotFound.Core.Upgrade
{
    public static class Upgrader
    {
        private static readonly ILogger Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
            var dba = DataAccessBaseEx.GetWorker();

            Log.Information("Create 404 handler redirects table START");
            var createTableScript = @"CREATE TABLE [dbo].[BVN.NotFoundRequests](
	                                    [ID] [int] IDENTITY(1,1) NOT NULL,
	                                    [OldUrl] [nvarchar](2000) NOT NULL,
	                                    [Requested] [datetime] NULL,
	                                    [Referer] [nvarchar](2000) NULL
                                        ) ON [PRIMARY]";
            var create = dba.ExecuteNonQuery(createTableScript);

            Log.Information("Create 404 handler redirects table END");


            if (create)
            {
                Log.Information("Create 404 handler version SP START");
                var versionSp = @"CREATE PROCEDURE [dbo].[bvn_notfoundversion] AS RETURN " + Configuration.Configuration.CurrentVersion;

                if (!dba.ExecuteNonQuery(versionSp))
                {
                    create = false;
                    Log.Error("An error occured during the creation of the 404 handler version stored procedure. Canceling.");
                }

                Log.Information("Create 404 handler version SP END");
            }

            if (create)
            {
                Log.Information("Create Clustered index START");
                var clusteredIndex =
                    "CREATE CLUSTERED INDEX NotFoundRequests_ID ON [dbo].[BVN.NotFoundRequests] (ID)";

                if (!dba.ExecuteNonQuery(clusteredIndex))
                {
                    create = false;
                    Log.Error("An error occurred during the creation of the 404 handler redirects clustered index. Canceling.");
                }

                Log.Information("Create Clustered index END");
            }

            Valid = create;
        }

        private static void Upgrade()
        {
            var dba = DataAccessBaseEx.GetWorker();

            var indexCheck =
                "SELECT COUNT(*) FROM sys.indexes WHERE name='NotFoundRequests_ID' AND object_id = OBJECT_ID('[dbo].[BVN.NotFoundRequests]')";

            var num = dba.ExecuteScalar(indexCheck);
            if (num == 0)
            {
                if (!dba.ExecuteNonQuery("CREATE CLUSTERED INDEX NotFoundRequests_ID ON [dbo].[BVN.NotFoundRequests] (ID)"))
                {
                    Valid = false;
                    Log.Error("An error occurred during the creation of the 404 handler redirects clustered index. Canceling.");
                }
                Log.Information("Create Clustered index END");
            }
            if (Valid)
            {
                var versionSp = @"ALTER PROCEDURE [dbo].[bvn_notfoundversion] AS RETURN " + Configuration.Configuration.CurrentVersion;
                Valid = dba.ExecuteNonQuery(versionSp);
            }
        }
    }
}