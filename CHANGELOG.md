# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

## Added
- Added interface for configuration - IConfiguration
- Added interface for redirect handler - IRedirectHandler
- Added interface for request logger - IRequestLogger
- Added dependency injection initialization
- Added tests for request handling, error handling, redirect lookup

### Changed
- Refactored Custom404Handler by extracting into RequestHandler and ErrorHandler
- #46 Fixed redirect loop


### Removed
- Custom404Handler
