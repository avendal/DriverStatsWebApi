# DriverStats-web-api

This project is an ASP.NET Core Web API that provides a /login endpoint for user authentication. The endpoint accepts a username and password, validates them, and returns the result as JSON.

## How to Run

1. Make sure you have the .NET SDK installed (version 9.0 or later).
2. Open a terminal in the project directory.
3. Run the API with:
   ```powershell
   dotnet run
   ```
4. The API will be available at `http://localhost:5000` (or the port shown in the console).

## Endpoints

- `POST /login` - Accepts JSON with `username` and `password` fields. Returns authentication result as JSON.

## Next Steps
- Implement the /login endpoint in a dedicated controller.
- Add user validation logic.
