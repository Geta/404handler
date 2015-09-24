Introduction
------------
This library contains a custom 404 handler for your EPiServer project. 
It will replace the default 404 handler with one that you can change, 
in order for it to have a more consistent look.

There is also a more advanced url redirect feature than the built-in 
shortcut url in EPiServer. It handles extensions and querystring 
parameters too. If you have a lot of 404 errors in your logs, you can
use this to redirect the user to the correct page. Especially useful if 
you move templates or pages around, or have just installed EPiServer 
and have a lot of urls that is no longer available.

Installation
------------
1. Open EPiServer Deployment Center
2. Select the version you want to install the module
3. Select Install ZIP module
4. Select the downloaded .epimodule file
5. Select the site you want to install the module to
6. Complete the wizard
7. Test it by opening 
   * http://localhost/login which redirects to /util/login.aspx
   * http://localhost/urldoesnotexist redirects to /404notfound.aspx

Changing the notfound page
--------------------------
You can - and should - change the "/404notfound.aspx" file to look
somewhat like the rest of your site. The .aspx file is NOT an EPiServer 
template put of the box. Open the 404notfound.aspx and read the comments
there for more information on how to change the look of your 404 page.
S
Custom Redirects
----------------
Open the /CustomRedirects.config file in an Xml editor, and change it 
according to your needs. The shipped xml file has example redirects that
you can look at to get an understanding of how the redirects works. 

There is also a plug-in in Admin mode to show the contents of this file.

Configuration
-------------
See the online documentation:
https://www.coderesort.com/p/epicode/wiki/404Handler
for more information about available configuration.

Support
-------
This code is unsupported, use at your own risk. Use the EPiCode ticket
system to ask for peer support.

Report a bug:
https://www.coderesort.com/p/epicode/newticket?component=404Handler&type=defect
Add a feature request:
https://www.coderesort.com/p/epicode/newticket?component=404Handler&type=enhancement



