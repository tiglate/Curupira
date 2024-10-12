# 🌳 Curupira - Your Friendly Automation Companion for Windows Servers! 🦥

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=tiglate_Curupira&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=tiglate_Curupira)
&nbsp;
[![CI Build](https://github.com/tiglate/Curupira/actions/workflows/curupira-console.yml/badge.svg)](https://github.com/tiglate/Curupira/actions/workflows/curupira-console.yml)
&nbsp;
[![CI Build](https://github.com/tiglate/Curupira/actions/workflows/curupira-service.yml/badge.svg)](https://github.com/tiglate/Curupira/actions/workflows/curupira-service.yml)
&nbsp;
[![CodeQL Scan](https://github.com/tiglate/Curupira/actions/workflows/codeql.yml/badge.svg)](https://github.com/tiglate/Curupira/actions/workflows/codeql.yml)

👋 Hey there, fellow IT wizards! ✨

Tired of manually connecting to your Windows servers via RDP to perform routine tasks like software installations, updates, and backups? 😫  Wish there was a simpler, more secure way to automate these repetitive chores? 🤔

Fear not, for **Curupira** is here to the rescue! 🦸

## 🤔 What is Curupira?

Curupira is a lightweight, open-source "Command and Control Server" designed to streamline your Windows server management tasks. Think of it as your trusty sidekick, always ready to lend a helping hand (or paw! 🐾) with those tedious server chores. 😉

## ✨ Why Curupira?

In small IT departments or organizations without dedicated automation tools, managing Windows servers often involves:

* **Manual RDP connections:**  Someone from infrastructure, DevOps, IT Ops, or even the development team has to  remotely connect to the server and perform tasks manually. 🤯
* **Tribal knowledge:**  The steps required for non-trivial procedures (like application installations) often reside solely in the minds of a few individuals (and rarely in proper documentation). 🤫
* **Error-prone processes:** Manual processes are more susceptible to human error. 😓
* **Lack of accountability:** Manual processes lack logs or audit trails, making it difficult to track changes and troubleshoot issues. 🕵️‍♂️
* **Security risks:** Unmonitored manual processes can leave your servers vulnerable to malicious activities. 🚨

While professional automation tools exist, they often come with hefty price tags and steep learning curves, making them impractical for smaller teams. 😵‍💫  Curupira offers a **free**, **easy-to-use** alternative that simplifies server automation without breaking the bank or requiring extensive training. 💰

## 🛡️ Secure by Design

Curupira prioritizes security by limiting actions to those pre-defined in a configuration file (currently supports XML format). This "whitelist" approach minimizes the attack surface and ensures that only authorized actions can be executed. 💪

## 🚀 Two Ways to Run Curupira

1. **Command-line Interface (CLI):** Ideal for integrating Curupira with scripts (*.bat, PowerShell, Python, etc.) and testing configuration files. ⌨️
2. **REST API:** Curupira can be installed as a Windows NT Service, exposing a REST API for remote execution. Securely access and manage your servers from any application or script with an API key. 🌐

## 🛠️ Built with Developers in Mind

* **C# and .NET Framework 4.8:** Ensures broad compatibility with both new and older Windows servers and workstations. 💻
* **Plugin-based architecture:**  Easily extend Curupira's functionality with custom plugins while maintaining code organization and reusability. 🧩
* **Comprehensive testing:**  With a focus on server stability, Curupira boasts high test coverage (at least 80%). 🧪
* **SonarCloud and CodeQL integration:**  Continuous code quality and security analysis with SonarCloud and CodeQL ensure a robust and secure codebase. 📈

## 📖 How to Get Started

Curupira is distributed under the GPL v3 license. Feel free to clone the repository or download a pre-compiled package. It's 100% free! 😊

* **Clone the repository:** `git clone https://github.com/tiglate/Curupira.git`

**Contributions, bug reports, and feature requests are always welcome!** 🙌

Let Curupira be your go-to solution for simple, secure, and efficient Windows server automation. Happy automating! 😄

## 🧰 Building Curupira From Source

Feeling adventurous? Want to tinker with Curupira's inner workings or contribute to its development?  Awesome! 🎉 Here's how to compile Curupira from source:

### 🛠️ Requirements

* **Visual Studio 2022 or later:**  You'll need a recent version of Visual Studio to open the solution file and build the project. 
* **.NET Framework 4.8:** Make sure you have the .NET Framework 4.8 installed on your development machine.
* **Windows 10 or 11:**  Curupira is designed for Windows environments, so a Windows 10 or 11 workstation is recommended for development.

### 🐛  Building in Debug Mode (Easy Peasy!)

1. Open the `Curupira.sln` solution file in Visual Studio.
2. Go to the "Build" menu and click on "Build Solution".

That's it! You can now run Curupira directly from Visual Studio or even execute the unit tests to ensure everything is working as expected.  ✅

### 🚀 Building in Release Mode (with a sprinkle of PowerShell magic ✨)

Building in release mode often involves a few extra steps. To simplify this process and keep your output organized, we've created some handy PowerShell scripts:

####  `build-console.ps1`

This script builds the console version of Curupira and neatly organizes the output files in a `dist` folder at the root of the solution.

**Here's how it works:**

* **No arguments:** Compiles the console application and generates the `dist` folder with the following subfolders:
    * `\bin`: Contains the executable files (`.exe`).
    * `\lib`:  Stores the required libraries (`.dll`).
    * `\conf`:  Holds configuration files (`.conf`, `.xml`, etc.).
    * `\logs`:  Keeps log files generated by the application.
* **`clean` argument:**  Deletes the `dist` folder and cleans the solution by removing the `bin/Debug`, `bin/Release`, and `obj/` folders in each project.
    ```powershell
    .\build-console.ps1 clean
    ```
* **`test` argument:**  Runs all unit tests. Make sure to compile in release mode first by running the script without any arguments.
    ```powershell
    .\build-console.ps1 test
    ```

#### `build-service.ps1`

This script functions similarly to `build-console.ps1` but builds a Windows NT Service version of Curupira, ready for installation and deployment.

**Example:**

To build the service and create a clean output directory:

```powershell
.\build-service.ps1
```
