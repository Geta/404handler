# Changelog

All notable changes to this project will be documented in this file.

## [1.2.1]
- Special character support for redirect URLs.
- Format exception fix.
- Duplicate redirect fix.

## [1.2.0]

### Added
- Redirect type - Permanent or Temporal

## [1.1.0]

### Added
- Sandbox application (based on AlloyTech)

## [1.0.0]

### Changed
- Package has been renamed to "Geta.404Handler"

# BVN.404Handler Package Changes

## [11.4.0]

### Changed

- Episerver CMS10 support is dropped
- This package is bneing renamed to `Geta.404Handler`. Dependency is set to latest `Geta.404Handler` package version. This will allow to transition easily to new package.


## [11.3.0], [10.5.0]

### Changed

- Fixed issue when least specific URLs were handled before most specific [#158](https://github.com/Geta/404handler/issues/158).
- Performance fix: do not call ToLower on every get, do it once during set [#162](https://github.com/Geta/404handler/issues/162).
- Incorrect language path for NotFound page Title fixed [#161](https://github.com/Geta/404handler/issues/161).
- Refactoring [#164](https://github.com/Geta/404handler/issues/164), [#165](https://github.com/Geta/404handler/issues/165), [#166](https://github.com/Geta/404handler/issues/166).

## [11.2.3], [10.4.3]

### Changed
- Fixed issue unable to ignore suggestions [#142](https://github.com/Geta/404handler/issues/142).
- Fixed issue when Episerver continued to log static resource 404 errors as exceptions.

## [11.2.2], [10.4.2]

### Changed
- Fixed language detection when using NotFoundPageAttribute [#145](https://github.com/Geta/404handler/issues/145).

## [11.2.1], [10.4.1]

### Changed
- Fixed DI for IRequestService.
- Logging headers written exceptions separately and not throwing to fix [#134](https://github.com/Geta/404handler/issues/134).

## [11.2.0], [10.4.0]

### Changed
- Changed DDS store to SQL store for redirects. A data migration is not done automatically. Open administration view on the gadget and under "Migrate redirects from DDS to SQL." click "Migrate." It will take some time depending on how many redirects you have. After the migration, the message will be displayed with information about how many redirects were moved to the SQL store. The message looks like - "Migrated 1000 redirects from DDS to SQL".

## [11.1.13], [10.3.12]

### Changed
- [#118](https://github.com/Geta/404handler/issues/118) Fixed partial match on deleted-urls always gives NullReferenceException.
- [#117](https://github.com/Geta/404handler/issues/117) Fixed System.ArgumentOutOfRangeException when stored URL is same as new URL.
- [#115](https://github.com/Geta/404handler/issues/115) Fixed Off mode.

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
