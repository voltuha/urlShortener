# UrlShortener

A minimal, extensible URL shortening service built with .NET, Entity Framework Core, and Clean Architecture principles.

## Features

- Create short URLs with optional custom codes
- Support for TTL (time-to-live) expiration
- Redirect by short code
- Delete existing short URLs
- SQLite database with Entity Framework Core migrations
- Clean Architecture: Domain / Application / Infrastructure / API
- Two implementations of code generation:
	1. With db lookup. 
	2. Base62 random generator (up to 62^L unique values where L = length of code).
	Implementation can be selected in appsettings.{env}.json -> ShortCodeGeneration.Strategy (Database or Random).
- Basic unit tests for UrlService

## Requires
- Dotnet 9
