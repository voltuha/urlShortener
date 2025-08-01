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
	1. Simple random generator with db lookup to guarantee unique values. 
	2. Base62 random generator without db lookup (theoretically has lower probability of collisions).
	Implementation can be selected in appsettings.{env}.json -> ShortCodeGeneration.Strategy (Database or Random).
- Basic unit tests for UrlService

## Requires
- Dotnet 9

## Test deployment
App is deployed and ready-to-test at https://urlshortener-production-d8e5.up.railway.app/swagger/index.html
