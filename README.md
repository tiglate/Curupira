# Curupira - Ampliar Project

<div align="center">
   [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=tiglate_Curupira&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=tiglate_Curupira)
   &nbsp;
   [![CI Build](https://github.com/tiglate/Curupira/actions/workflows/curupira-console.yml/badge.svg)](https://github.com/tiglate/Curupira/actions/workflows/curupira-console.yml)
   &nbsp;
   [![CodeQL Scan](https://github.com/tiglate/Curupira/actions/workflows/codeql.yml/badge.svg)](https://github.com/tiglate/Curupira/actions/workflows/codeql.yml)
</div>

Curupira is part of the **Ampliar Project**, an initiative that aims to offer high-quality, free, and open-source software. Curupira provides an extensible platform with powerful automation capabilities through plugins, making it suitable for a variety of use cases like file manipulation, software deployment, and system management.

## Available Plugins

The Curupira platform currently supports the following plugins:

### 1. **Installer Plugin**
   - **Handles**: ZIP files, MSI installers, Windows Executables, and BAT scripts.
   - **Features**:
     - Extract files from ZIP archives.
     - Install or uninstall MSI packages.
     - Execute EXE or BAT scripts.
   - **Usage**:
     - Command-line options can control source files, target directories, and more.

### 2. **Folders Creator Plugin**
   - **Handles**: Creation of directory structures.
   - **Features**:
     - Create multiple directories as part of the automation process.
     - Validate existing directory paths and permissions.

### 3. **Service Manager Plugin**
   - **Handles**: Managing Windows Services.
   - **Features**:
     - Start or stop Windows services defined in configuration.
     - Manage service bundles for batch operations.

## Command Line Usage

Curupira is built as a console application that can be extended with different plugins. The primary command-line interface supports various operations using the `-p` (plugin) and other options.

### Basic Command:
```
Curupira.Console.exe --plugin <plugin-name> [options]
```

### Example Commands:
1. **To execute the Installer Plugin**:
    ```
    Curupira.Console.exe --plugin Installer --params SourceFile=C:\example.zip TargetDir=C:\output
    ```
2. **To list available plugins**:
    ```
    Curupira.Console.exe --list-plugins
    ```

### Available Command-Line Options:
- `-p, --plugin`: The name of the plugin to execute.
- `-l, --level`: Set the log level (optional). Default is "Info".
- `-n, --no-logo`: Hides the application logo.
- `-b, --no-progressbar`: Disables the progress bar.
- `-a, --list-plugins`: Lists all available plugins.
- `--params`: Additional parameters specific to the plugin.

### Example Configuration:
The configuration for each plugin can be specified using XML or JSON, depending on the plugin’s needs. Each plugin may have its own specific parameters.

## REST API Usage

Curupira can also be deployed as a REST API for remote execution of plugins. The API exposes several endpoints for managing automation tasks.

### Example Endpoints:
1. **Execute a Plugin**:
   - `POST /api/plugins/execute`
   - **Request Body**:
     ```json
     {
       "plugin": "Installer",
       "params": {
         "SourceFile": "C:\\example.zip",
         "TargetDir": "C:\\output"
       }
     }
     ```
   - **Response**:
     - Status 200: Success
     - Status 400: Invalid parameters

2. **List Plugins**:
   - `GET /api/plugins`
   - **Response**:
     ```json
     {
       "plugins": [
         "Installer",
         "FoldersCreator",
         "ServiceManager"
       ]
     }
     ```

## Getting Started

1. **Clone the Repository**:
    ```bash
    git clone https://github.com/your-repo/curupira.git
    cd curupira
    ```

2. **Build the Solution**:
    - Open the solution in Visual Studio or use the command line:
    ```bash
    msbuild Curupira.sln /p:Configuration=Release
    ```

3. **Run the Application**:
    ```bash
    Curupira.Console.exe --plugin Installer --params SourceFile=C:\example.zip TargetDir=C:\output
    ```

4. **Deploy the REST API**:
    - Follow standard deployment practices for .NET applications to expose the API in your environment.

## Contributing

We welcome contributions to Curupira and the broader **Ampliar Project**. Please refer to the [contributing guide](CONTRIBUTING.md) for details on how to get started.
