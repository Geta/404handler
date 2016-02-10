Important information about the 404 Handler
===========================================
If you are upgrading this package from a version previous to 4.1.1, make sure you inspect your configuration.

Instead of specifying the fileNotFoundPage attribute on the bvn404handler, use the standard httpErrors section instead.

See https://github.com/BVNetwork/404handler#configuration for more information.

If you are using MVC, make sure your controller action is decorated correctly in order to return the correct HTTP status code. 

See https://github.com/BVNetwork/404handler#custom-404-page for more information.