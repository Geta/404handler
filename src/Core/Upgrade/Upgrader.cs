using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BVNetwork.NotFound.Core.Configuration;
using BVNetwork.NotFound.Core.CustomRedirects;
using BVNetwork.NotFound.Core.Data;
using log4net;

namespace BVNetwork.Bvn.FileNotFound.Upgrade
{
    public static class Upgrader
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

            _log.Info("Create 404 handler redirects table START");
            string createTableScript = @"CREATE TABLE [dbo].[BVN.NotFoundRequests](
	                                    [ID] [int] IDENTITY(1,1) NOT NULL,
	                                    [OldUrl] [nvarchar](2000) NOT NULL,
	                                    [Requested] [datetime] NULL,
	                                    [Referer] [nvarchar](2000) NULL
                                        ) ON [PRIMARY]";
            create = dba.ExecuteNonQuery(createTableScript);

            _log.Info("Create 404 handler redirects table END");


            if (create)
            {
                _log.Info("Create 404 handler version SP START");
                string versionSP = @"CREATE PROCEDURE [dbo].[bvn_notfoundversion] AS RETURN " + Configuration.CURRENT_VERSION;

                if (!dba.ExecuteNonQuery(versionSP))
                {
                    create = false;
                    _log.Error("An error occured during the creation of the 404 handler version stored procedure. Canceling.");

                    _log.Info("Create 404 handler version SP END");
                }
            }
            Valid = create;

            // copy dds items, if there are any.
            try
            {
                // the old redirect class is obsolete, and should only be used for this upgrade
                var oldCustomrRedirectStore = DataStoreFactory.GetStore(typeof(BVNetwork.FileNotFound.CustomRedirects.CustomRedirect));
                var oldCustomRedirects = oldCustomrRedirectStore.Items<BVNetwork.FileNotFound.CustomRedirects.CustomRedirect>().ToList();

                if (oldCustomRedirects.Count > 0)
                {
                    var newCustomrRedirectStore = DataStoreFactory.GetStore(typeof(CustomRedirect));
                    DataStoreHandler dsHandler = new DataStoreHandler();
                    foreach (var oldCustomRedirect in oldCustomRedirects)
                    {
                        var newRedirect = new CustomRedirect(oldCustomRedirect.OldUrl, oldCustomRedirect.NewUrl, oldCustomRedirect.WildCardSkipAppend);
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
            string versionSP = @"ALTER PROCEDURE [dbo].[bvn_notfoundversion] AS RETURN " + Configuration.CURRENT_VERSION;
            var dba = DataAccessBaseEx.GetWorker();
            Valid = dba.ExecuteNonQuery(versionSP);
            // TODO: Alter table if necessary
        }



    }
}