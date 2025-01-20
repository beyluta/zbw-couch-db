# CouchDB Demo

This repository contains a simple .NET console application demonstrating basic CRUD operations against a CouchDB instance.

## Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) (or higher)
- An instance of [CouchDB](https://couchdb.apache.org/) running and accessible
- Valid CouchDB credentials

## Setup

1. **Clone the repository** or download the code.
2. **Open `Program.cs`** and set the `RemoteUrl` to your CouchDB endpoint.
3. **Set the `Credentials`** to your `<username>:<password>` for basic auth.

```csharp
private static string RemoteUrl = "http://localhost:5984/your_database";
private static string Credentials = "username:password";
```
