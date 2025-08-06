# PyExec - Python Script Manager

A comprehensive WPF application for managing and executing Python scripts with advanced virtual environment support and dual-panel organization.

![PyExec Screenshot](https://via.placeholder.com/800x500?text=PyExec+Screenshot)

## 🚀 Features

### Core Functionality
- **Dual-Panel Interface** - Organize scripts in two separate, customizable panels
- **Virtual Environment Integration** - Seamless support for Python virtual environments
- **Multiple Execution Methods** - Support for `python.exe`, `pythonw.exe`, and `uv run`
- **Template System** - Save and load script collections as reusable templates
- **Script Output Logging** - Real-time monitoring of script execution with detailed logs

### User Experience
- **Drag & Drop Support** - Easy script addition through file explorer
- **Keyboard Shortcuts** - Quick access to common operations (F5 to run, Ctrl+S to save templates)
- **Resizable Layout** - Customizable panel sizes with persistent window state
- **Recent Templates** - Quick access to recently used template configurations
- **Context Menus** - Right-click operations for enhanced productivity

### File Format Support
- **Python Scripts** - `.py` and `.pyw` files
- **Jupyter Notebooks** - `.ipynb` files (display and execution)
- **Executable Files** - `.exe` files with proper working directory handling
- **Template Files** - `.tpl` files for saving script collections

## 📋 Requirements

- **Operating System**: Windows 7 or later
- **.NET Runtime**: .NET 8.0 or later
- **Python**: Python 3.7+ (optional, for script execution)
- **Virtual Environments**: Standard Python venv or compatible environments

## 🔧 Installation

### Option 1: Download Release
1. Go to the [Releases](../../releases) page
2. Download the latest `PyExec.exe` or installer package
3. Run the executable - no installation required for portable version

### Option 2: Build from Source
```bash
# Clone the repository
git clone https://github.com/yourusername/PyExec.git
cd PyExec

# Build the project
dotnet build --configuration Release

# Run the application
dotnet run --project PyExec/PyExec.csproj
```

## 🎯 Quick Start

### First Launch
1. **Set Default Virtual Environment Root** (optional)
   - Go to `Tools` → `Set Default VirtualEnv Folder`
   - Select your virtual environments parent directory (e.g., `C:\Venvs`)

2. **Add Python Scripts**
   - Click `File` → `Add Program` or use the add button
   - Select one or more Python files from your file system
   - Scripts will appear in the active panel with auto-detected names

3. **Run Scripts**
   - Double-click on any script entry or press `F5`
   - View execution output in the integrated log viewer

### Virtual Environment Setup
```bash
# Example: Create a virtual environment in your default root
cd C:\Venvs
python -m venv myproject_env

# PyExec will automatically detect and use this environment
# when you set C:\Venvs as your default root
```

## 📖 Usage Guide

### Panel Management
- **Switch Active Panel** - Click on either panel to make it active
- **Move Scripts** - Use ▲/▼ buttons to reorder scripts within panels
- **Resize Panels** - Drag the splitter bars to adjust panel sizes

### Virtual Environment Configuration
- **Global Default** - Set a root folder where all virtual environments are stored
- **Per-Script Override** - Assign specific virtual environments to individual scripts
- **Automatic Detection** - PyExec searches for `venv`, `.venv`, `env` subfolders

### Template System
- **Save Templates** - `File` → `Save Template` to create reusable script collections
- **Load Templates** - `File` → `Load Template` to restore saved configurations
- **Recent Templates** - Quick access panel shows recently used templates
- **Overwrite Templates** - `Ctrl+S` to update existing templates

### Execution Methods
- **Python Mode** - Uses `python.exe` from the configured virtual environment
- **UV Run Mode** - Uses `uv run` for modern Python project management
- **Auto-Detection** - `.exe` files run directly, `.pyw` files run windowless

## ⚙️ Configuration

### Settings Files
PyExec stores configuration in JSON files alongside the executable:

```
PyExec/
├── window_settings.json      # UI layout and window state
├── recent_templates.json     # Recently used templates
├── default_venv_root.json   # Default virtual environment path
├── programs1.json           # Panel 1 script collection
└── programs2.json           # Panel 2 script collection
```

### Keyboard Shortcuts
| Shortcut | Action |
|----------|--------|
| `F5` | Run selected script |
| `Ctrl+S` | Overwrite current template |
| `Ctrl+Up` | Move script up |
| `Ctrl+Down` | Move script down |

## 🛠️ Advanced Features

### Process Diagnostics
- **Start Info Display** - View detailed process startup information
- **Environment Variables** - See exactly what environment is passed to scripts
- **Working Directory** - Verify script execution context
- **Path Resolution** - Debug virtual environment path detection

### Script Management Tools
- **Convert Extensions** - Toggle between `.py` and `.pyw` files
- **Auto-Description** - Apply filename or folder name as script description
- **Bulk Operations** - Select multiple scripts for batch virtual environment assignment

### Command Line Integration
- **Open CMD** - Launch command prompt with virtual environment activated
- **Working Directory** - CMD opens in the script's directory
- **Environment Setup** - Automatic activation of the correct virtual environment

## 🔍 Troubleshooting

### Common Issues

**Scripts won't run**
- Verify Python is installed and accessible
- Check virtual environment path configuration
- Ensure script files exist and are readable

**Virtual environment not detected**
- Confirm the environment contains a `Scripts` folder
- Check that `python.exe` exists in `Scripts` folder
- Verify default root path is correctly set

**Templates not loading**
- Check file permissions on template files
- Ensure JSON format is valid
- Verify file paths in templates still exist

### Debug Information
Use `Tools` → `Process Start Information` to view:
- Resolved virtual environment paths
- Environment variables being set
- Command line arguments
- Working directory configuration

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

### Development Setup
```bash
# Clone and setup development environment
git clone https://github.com/yourusername/PyExec.git
cd PyExec

# Restore NuGet packages
dotnet restore

# Build in debug mode
dotnet build

# Run with debugging
dotnet run --project PyExec/PyExec.csproj
```

### Code Structure
```
PyExec/
├── Models/              # Business logic and data models
├── ViewModels/          # MVVM pattern implementation
├── Helpers/             # Utility classes and services
├── Converters/          # XAML data binding converters
├── Views/               # XAML user interface files
└── Resources/           # Images, icons, and assets
```

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with [.NET 8.0](https://dotnet.microsoft.com/) and WPF
- JSON serialization powered by `System.Text.Json`
- Virtual environment detection inspired by Python community best practices

## 📞 Support

- **Issues**: [GitHub Issues](../../issues)
- **Discussions**: [GitHub Discussions](../../discussions)
- **Wiki**: [Project Wiki](../../wiki)

---

**Made with ❤️ for Python developers who love organized workflows**