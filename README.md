# CloudClinic

CloudClinic is a robust medical management platform built with an **ASP.NET Core 8.0** backend and an **Angular** frontend. This system is designed to streamline healthcare workflows including user authentication, doctor-patient management, and clinical diagnostics.


## 🚀 Key Features

### Role-Based Modules
The system is architected into specialized modules to manage clinical and administrative workflows:
* **DrAccountant**: Manages financial transactions, including payment viewing and confirmation.
* **DrAssistant**: Handles appointment scheduling, searches, rejection/un-rejection, and practice time/holiday configuration.
* **DrCheckPat**: The clinical core, managing patient reports, prescriptions, and referrals for labs/X-rays.
* **DrLabStaff**: Manages lab test referrals and result submission with file attachments.
* **DrPharmacyStaff**: Manages drug inventory and prescription tracking.
* **DrFeedback/Details/Progress**: Tools for communication, profile management, and clinical performance analytics.

## 🛠 Technical Details
* **Framework**: ASP.NET Core 8.0, Entity Framework Core, SQL Server.
* **API Documentation**: Swagger/OpenAPI 3.0.1 integrated.
* **Database Strategy**: Circular deletion dependencies resolved using `DeleteBehavior.Restrict`.
* **Security**: JWT-based authentication and role-based access control.

## 📋 Setup & Maintenance

### Initialization
1. Ensure .NET 8.0 SDK and SQL Server are installed.
2. Configure `appsettings.json` with your connection string.
3. Apply database schema: `dotnet ef database update`.

### Maintenance Protocols
* **Clean Build**: `dotnet clean` and remove `bin`/`obj` folders.
* **Cache Refresh**: `dotnet nuget locals all --clear`.
* **Migration Reset**: 
    1. Delete `Migrations/` folder.
    2. `dotnet ef migrations add InitialFullMigration`.
    3. `dotnet ef database update`.

### 🖥 Angular Client App
The frontend is a modular Single Page Application (SPA) utilizing:
* **Service-Oriented Design**: Dedicated services for each clinical domain (Lab, Pharmacy, Assistant).
* **Type Safety**: TypeScript models synchronized with C# DTOs.
* **Authentication Interceptor**: Automated JWT injection for secure API communication.
* **Feature-Based Routing**: Clean separation of clinical dashboards and management consoles.

## Overview

CloudClinic ClientApp is the Angular frontend for the CloudClinic healthcare management system. This repository contains the legacy Angular 9 client application designed to work with a .NET Core backend api.

> Note: This is a legacy project with specific environment requirements. It is optimized for Windows 10/11 and Node.js 14.21.3 to avoid dependency and build compatibility issues.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Setup](#setup)
- [Legacy Build & Troubleshooting](#legacy-build--troubleshooting)
- [Backend Configuration](#backend-configuration)
- [Common Commands](#common-commands)
- [Configuration](#configuration)
- [License](#license)

## Features

- Angular 9 single-page application
- Role-based UI for patients, doctors, lab staff, pharmacy, and administrators
- Angular Material design system
- Responsive layout via `@angular/flex-layout`
- Offline-capable setup with Angular service worker
- Local data persistence with `localforage` and `ngforage`
- Real-time updates using `@aspnet/signalr`
- Dashboard visualizations via `@swimlane/ngx-charts`

## Technology Stack

- Angular 9
- Angular CLI 9
- TypeScript
- RxJS
- Angular Material
- @aspnet/signalr
- @swimlane/ngx-charts
- localforage / ngforage

## Project Structure

- `src/app/` — application modules, components, and services
- `src/app/auth/` — authentication flows, registration, login, email verification, and route guards
- `src/app/dr/` — doctor and staff views, reports, clinical services, and shared workflows
- `src/app/home/` — landing pages, home dashboard, search/explorer views, and access control
- `src/app/shared/` — shared components, services, dialogs, models, and constants
- `src/assets/` — static assets, images, icons, and themes
- `src/environments/` — development and production environment settings

## Prerequisites

- Windows 10 or Windows 11
- Node.js `14.21.3` (recommended via `nvm-windows`)
- npm 6.x or later
- Angular CLI 9.x

## Setup

From the `ClientApp` directory, install dependencies:

```bash
npm install --legacy-peer-deps
```

Start the development server:

```bash
npm start
```

Open the app in your browser:

```text
http://localhost:4200/
```

## Legacy Build & Troubleshooting

This legacy Angular 9 project may require manual fixes for modern systems.

### 1. Clear the Environment

Remove existing dependencies and clear the npm cache:

```powershell
rd /s /q node_modules
del package-lock.json
npm cache clean --force
```

### 2. Install Dependencies

Use the legacy peer dependency flag to prevent package conflicts:

```bash
npm install --legacy-peer-deps
```

### 3. Enable Legacy OpenSSL

If you see `ERR_OSSL_EVP_UNSUPPORTED`, update the `scripts` section of `package.json`:

```json
"scripts": {
  "start": "set NODE_OPTIONS=--openssl-legacy-provider && ng serve",
  "build": "set NODE_OPTIONS=--openssl-legacy-provider && ng build"
}
```

### 4. Fix `querystring` Module Errors

If you encounter `Module not found: Can't resolve 'querystring'`, add this alias to `package.json`:

```json
"browser": {
  "querystring": "querystring-es3"
}
```

### 5. Avoid Breaking Compatibility

- Do not run `npm update` in this project root.
- Use `npx ng serve` when the globally installed Angular CLI version does not match the local project version.

## Backend Configuration

If you encounter `System.ArgumentOutOfRangeException` during login, verify the backend settings:

- **JWT Secret**: Ensure `JWT_Secret` in `appsettings.json` is at least 32 characters long.
- **Client URL**: Ensure `Client_URL` in `ApplicationSettings` matches the Angular app URL, typically `http://localhost:4200`.

## Common Commands

- Run via local CLI: `npx ng serve`
- Build production bundle: `npm run build -- --prod`
- Run unit tests: `npm test`
- Run e2e tests: `npm run e2e`

## Configuration

- `src/environments/environment.ts` — development environment settings
- `src/environments/environment.prod.ts` — production environment settings
- `ngsw-config.json` — service worker configuration

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## 🤝 Contributing
Contributions are welcome. Please submit a pull request with a clear description of your changes.# CloudClinic