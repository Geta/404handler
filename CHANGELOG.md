# Changelog

All notable changes to this project will be documented in this file.

## [11.1.12], [10.3.11]

### Changed
- [#106](https://github.com/Geta/404handler/issues/106) Fixed validation error appearance on the search field when submitting new redirect.

## [11.1.8], [10.3.7]

### Added
- Norwegian translation for widget
- Uploaded file type checks

### Changed
- [#89](https://github.com/Geta/404handler/issues/89) Fixed thread abort exception in log
- [#84](https://github.com/Geta/404handler/issues/84) Fixed exception when referrer parameter is null
- [#92](https://github.com/Geta/404handler/issues/92) Fixed suggestion list fail when no suggestions
- [#93](https://github.com/Geta/404handler/issues/93) Fixed redirect loops

## [11.1.5], [10.3.4]

### Changed
- Fixed null reference exception when HttpContext is null.

## [11.1.4], [10.3.3]

### Changed
- Fixed thread abort exception when 404 handled.

## [11.1.3], [10.3.2]

### Changed
- Fixed redirects not working in Azure
- Fixed NotFoundPageUtil.GetUrlNotFound throwing index out of bounds exception

## [11.1.0], [10.3.0]

### Added
- Added interface for configuration - IConfiguration
- Added interface for redirect handler - IRedirectHandler
- Added interface for request logger - IRequestLogger
- Added dependency injection initialization
- Added tests for request handling, error handling, redirect lookup

### Changed
- Refactored Custom404Handler by extracting into RequestHandler and ErrorHandler
- [#46](https://github.com/Geta/404handler/issues/46) Fixed redirect loop
- [#38](https://github.com/Geta/404handler/issues/38) Fixed culture specific URLs to fail when has similar English URL version

### Removed
- Custom404Handler
- [#66](https://github.com/Geta/404handler/issues/66) Removed Microsoft.CodeDom.Providers.DotNetCompilerPlatform package dependency
