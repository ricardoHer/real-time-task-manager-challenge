# Real-Time Task Manager

A modern, clean task management application built with .NET 8 API and React frontend, featuring real-time updates via SignalR and AI-powered task summaries.

## Features

- ✅ **Real-time Updates** - Tasks appear instantly across all connected clients
- 🤖 **AI Task Summaries** - Generate intelligent summaries using GitHub Models
- 🎨 **Clean Design** - Minimalist, dark-themed UI focused on productivity
- 🔄 **Live Sync** - SignalR enables real-time communication
- 📱 **Responsive** - Works seamlessly on desktop and mobile

## Tech Stack

### Backend (.NET 8 API)
- ASP.NET Core 8.0
- SignalR for real-time communication
- Entity Framework Core (In-Memory Database)
- GitHub Models integration for AI summaries

### Frontend (React + Vite)
- React 19
- Vite for fast development
- Axios for HTTP requests
- SignalR client for real-time updates

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) 
- [Git](https://git-scm.com/)
- GitHub Personal Access Token (for AI summaries) - *Optional*

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/ricardoHer/real-time-task-manager-challenge.git
cd real-time-task-manager-challenge
```

### 2. Backend Setup (.NET API)

#### Navigate to API directory
```bash
cd task-manager-api
```

#### Configure AI Integration (Optional)

Create a `localsettings.json` file in the API directory:

```json
{
  "GitHubModelsKey": "YOUR_GITHUB_PAT_HERE"
}
```

**To get a GitHub Personal Access Token:**
1. Go to [GitHub Settings > Personal Access Tokens](https://github.com/settings/tokens)
2. Generate new token (classic)
3. Select `public_repo` scope
4. Copy the token to `localsettings.json`

> **Note:** AI summaries will fall back to local summaries if no API keys are provided.

#### Build and run the API
```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the API
dotnet run
```

The API will start at `http://localhost:5261`

### 3. Frontend Setup (React)

Open a new terminal and navigate to the frontend directory:

```bash
cd task-manager-ui/task-manager-ui
```

#### Install dependencies
```bash
npm install
```

#### Start the development server
```bash
npm run dev
```

The frontend will start at `http://localhost:5173`

## Usage

1. **Open your browser** and navigate to `http://localhost:5173`
2. **Add tasks** using the clean form interface
3. **See real-time updates** - tasks appear instantly across all browser windows
4. **Generate AI summaries** - Click "Generate Summary" to get intelligent task insights

## API Endpoints

### Tasks
- `GET /api/tasks` - Retrieve all tasks
- `POST /api/tasks` - Create a new task
- `POST /api/tasks/summary` - Generate AI-powered task summary

### Development
- `GET /` - Health check
- `GET /weatherforecast` - Sample endpoint

### SignalR Hub
- `/taskhub` - Real-time communication hub

## Project Structure

```
real-time-task-manager-challenge/
├── task-manager-api/           # .NET 8 API
│   ├── Data/                   # Entity Framework models
│   ├── DTO/                    # Data Transfer Objects
│   ├── Services/               # Business logic & AI services
│   ├── Hubs/                   # SignalR hubs
│   ├── Properties/             # Launch settings
│   └── Program.cs              # API configuration
├── task-manager-ui/
│   └── task-manager-ui/        # React frontend
│       ├── src/
│       │   ├── components/     # React components
│       │   ├── App.jsx         # Main application
│       │   └── main.jsx        # Entry point
│       └── package.json
└── README.md
```

## Development Features

### Debugging
- VS Code debugging configured for both frontend and backend
- Launch configurations available in `.vscode/launch.json`
- Press `F5` in VS Code to start debugging the API

### Hot Reload
- Frontend: Automatic reload on file changes
- Backend: Use `dotnet watch run` for hot reload

## Environment Configuration

### API Configuration
The API uses the following configuration hierarchy:
1. `appsettings.json` (base settings)
2. `appsettings.Development.json` (development overrides)  
3. `localsettings.json` (local/sensitive settings - not committed)

### CORS
The API is configured to accept requests from:
- `http://localhost:3000` (React default)
- `http://localhost:5173` (Vite dev server)
- `http://localhost:5261` (API self-requests)

## Troubleshooting

### Common Issues

**API won't start:**
- Ensure .NET 8 SDK is installed: `dotnet --version`
- Check port 5261 isn't in use
- Verify `task-manager-api.csproj` exists

**Frontend won't start:**
- Ensure Node.js 18+ is installed: `node --version`
- Run `npm install` to install dependencies
- Check port 5173 isn't in use

**Real-time updates not working:**
- Verify both API and frontend are running
- Check browser console for SignalR connection errors
- Ensure CORS is properly configured

**AI summaries not working:**
- Verify GitHub PAT is correct in `localsettings.json`
- Check API logs for authentication errors
- Ensure you haven't exceeded GitHub Models quota
- Confirm your GitHub token has the required permissions

### Development Tips

1. **Use two terminals** - one for API, one for frontend
2. **Check browser devtools** for frontend errors
3. **Monitor API logs** in the terminal for backend issues
4. **Use the debugger** - F5 in VS Code for API debugging

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

If you encounter any issues:
1. Check the troubleshooting section above
2. Review the browser console and API logs
3. Ensure all prerequisites are properly installed
4. Create an issue in the GitHub repository

---

**Happy Task Managing! 🚀**