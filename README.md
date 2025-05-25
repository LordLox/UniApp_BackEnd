# UniApp Back-End

This repository contains the back-end API for UniApp, a university application designed to manage users (Admins, Professors, Students), events (Lessons, Conferences, Labs), and attendance tracking via QR codes.

## Features

* **User Management:** CRUD operations for users, role-based access control (Admin, Professor, Student), password management.
* **Event Management:** CRUD operations for events, event creation by professors, event type categorization.
* **Attendance Tracking:** QR code generation for students, QR code validation for event entry, event participation history for both professors and students.
* **Authentication:** Basic Authentication for API endpoints.
* **Security:** Password hashing, AES encryption for QR code data, password strength checking.

## Tech Stack

* .NET 8 (ASP.NET Core Web API)
* Entity Framework Core
* MySQL
* AutoMapper
* QRCoder
* DotNetEnv (for environment variable management)
* CheckPasswordStrength

## Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* MySQL Server (or a Docker container running MySQL)
* A code editor like Visual Studio Code or Visual Studio

## Setup and Installation

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/LordLox/UniApp_BackEnd.git
    cd UniApp_BackEnd
    ```

2.  **Configure Environment Variables:**
    * Create a `.env` file in the root of the `UniApp_BackEnd` project directory (alongside `UniApp_BackEnd.csproj`).
    * Add the following environment variables to your `.env` file, replacing the placeholder values with your actual configuration:

        ```env
        DB_CONNECTION_STRING="server=localhost;port=3306;database=studentapp_db;user=your_db_user;password=your_strong_password"
        SETTINGS_AESKEY="your_very_secret_and_strong_aes_key_32_bytes" # Must be a strong key, e.g., 32 characters for AES-256
        SETTINGS_BCODEELAPSESECONDS_DEV=5000000 # Barcode validity in seconds for development
        SETTINGS_BCODEELAPSESECONDS_PROD=30     # Barcode validity in seconds for production
        ASPNETCORE_ENVIRONMENT="Development" # Or "Production"
        ```
    * **Important:** Ensure your `SETTINGS_AESKEY` is cryptographically strong. You can generate one using a password manager or a random string generator.
    * **Note:** The `DB_CONNECTION_STRING` assumes MySQL is running on `localhost:3306`. Adjust if your setup is different (e.g., using Docker, a different port, or a remote database). Ensure the specified database (`studentapp_db` in the example) exists, or that your DB user has permissions to create it.

3.  **Install Dependencies:**
    The .NET SDK will automatically restore NuGet packages when you build or run the project. If you want to do it manually:
    ```bash
    dotnet restore
    ```

4.  **Database Migrations:**
    The application uses Entity Framework Core migrations to set up the database schema. These migrations will be applied automatically when the application starts for the first time.
    * Ensure your MySQL server is running and accessible with the credentials provided in your `.env` file.
    * The application will attempt to create the database if it doesn't exist and apply any pending migrations.

## Running the Application

1.  **Using the .NET CLI:**
    Navigate to the `UniApp_BackEnd` project directory in your terminal and run:
    ```bash
    dotnet run
    ```
    By default, the application will run on `http://localhost:5000` (or as configured in `Properties/launchSettings.json`). You can access the Swagger UI for API documentation and testing at `http://localhost:5000/swagger`.

2.  **Using Visual Studio or Visual Studio Code:**
    * Open the project/solution in your IDE.
    * Ensure the startup project is set to `UniApp_BackEnd`.
    * Run the application (usually by pressing F5 or a "Start Debugging" button).

## API Endpoints

The application exposes several API endpoints for managing users, events, and barcodes. Refer to the Swagger UI (`/swagger`) once the application is running for a detailed list of endpoints, request/response models, and to try them out.

Key endpoint groups:
* `/users`: For user management (Admin only for most operations).
* `/users/changepass`: For users to change their own password.
* `/users/userinfo`: For authenticated users to get their encrypted info.
* `/events`: For event management (Professors for creation/updates, Admins for full access).
* `/events/personal`: For professors to view their events.
* `/events/entry/{eventId}`: For professors to record student entry to an event.
* `/events/entry/history`: For professors to get event history.
* `/me/event-history`: For students to get their own participation history.
* `/barcode/qr`: For students to get their QR code.
* `/barcode/qr/{userid}`: For professors/admins to get a user's QR code.

## Contributing

Contributions are welcome! If you'd like to contribute, please follow these steps:

1.  **Fork the repository.**
2.  **Create a new branch** for your feature or bug fix:
    ```bash
    git checkout -b feature/your-feature-name
    ```
    or
    ```bash
    git checkout -b bugfix/issue-description
    ```
3.  **Make your changes.** Ensure you adhere to the existing coding style and add tests if applicable.
4.  **Commit your changes:**
    ```bash
    git commit -m "feat: Describe your feature"
    ```
    or
    ```bash
    git commit -m "fix: Describe your bug fix"
    ```
    (Consider using [Conventional Commits](https://www.conventionalcommits.org/) for commit messages.)
5.  **Push to your branch:**
    ```bash
    git push origin feature/your-feature-name
    ```
6.  **Create a Pull Request** against the `main` (or `develop`) branch of the original repository.
7.  Clearly describe your changes in the Pull Request and link to any relevant issues.

Please ensure your code builds and any tests pass before submitting a pull request.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.