Important information about the 404 Handler
===========================================
If you are upgrading this package from a version previous to 4.1.1 (or 3.4.0 for CMS 8), make sure you inspect your configuration.

Instead of specifying the fileNotFoundPage attribute on the bvn404handler, use the standard httpErrors section instead.

See https://github.com/BVNetwork/404handler#configuration for more information.

If you are using MVC, make sure your controller action is decorated correctly in order to return the correct HTTP status code. 

See https://github.com/BVNetwork/404handler#custom-404-page for more information.

If you are upgrading this package from a version previous to 11.2.0 (or 10.4.0 for CMS 10), you should run a migration of data. Open administration view on the gadget and under "Migrate redirects from DDS to SQL." click "Migrate." It will take some time depending on how many redirects you have. After the migration, the message will be displayed with information about how many redirects were moved to the SQL store. The message looks like - "Migrated 1000 redirects from DDS to SQL".