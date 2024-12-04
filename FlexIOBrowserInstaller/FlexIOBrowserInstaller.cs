using System.Diagnostics;
using System.Net.Http;
using System.Security.Principal;
using System.Reflection;

namespace FlexIO.Browser.Installer;

public class Installer : Form
{
    private ProgressBar progressBar;
    private Label statusLabel;
    private Button installButton;
    private Button cancelButton;
    private string installPath;
    private bool isInstalling = false;

    public Installer()
    {
        InitializeComponents();
        CheckAdminRights();
        installPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "FlexIO Browser"
        );
    }

    private void InitializeComponents()
    {
        this.Size = new Size(500, 300);
        this.Text = "FlexIO Browser Installer";
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;

        // Logo or banner could be embedded as a resource
        var welcomeLabel = new Label
        {
            Text = "Welcome to FlexIO Browser Installation",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 50
        };

        statusLabel = new Label
        {
            Text = "Ready to install",
            Dock = DockStyle.Top,
            Height = 30,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 10, 0)
        };

        progressBar = new ProgressBar
        {
            Style = ProgressBarStyle.Continuous,
            Height = 20,
            Margin = new Padding(10),
            Minimum = 0,
            Maximum = 100
        };

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 50,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(10)
        };

        installButton = new Button
        {
            Text = "Install",
            Width = 100,
            Height = 30
        };
        installButton.Click += InstallButton_Click;

        cancelButton = new Button
        {
            Text = "Cancel",
            Width = 100,
            Height = 30
        };
        cancelButton.Click += (s, e) => Application.Exit();

        buttonPanel.Controls.AddRange(new Control[] { cancelButton, installButton });

        var container = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            Padding = new Padding(20)
        };

        container.Controls.Add(progressBar);
        container.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        this.Controls.AddRange(new Control[] 
        { 
            welcomeLabel,
            statusLabel,
            container,
            buttonPanel
        });
    }

    private void CheckAdminRights()
    {
        bool isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent())
            .IsInRole(WindowsBuiltInRole.Administrator);
        
        if (!isAdmin)
        {
            MessageBox.Show(
                "This installer requires administrative privileges.\nPlease run as administrator.",
                "Admin Rights Required",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            Application.Exit();
        }
    }

    private async void InstallButton_Click(object sender, EventArgs e)
    {
        if (isInstalling) return;
        isInstalling = true;
        installButton.Enabled = false;
        cancelButton.Enabled = false;

        try
        {
            await InstallAsync();
            
            MessageBox.Show(
                "Installation completed successfully!",
                "Installation Complete",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
            
            if (MessageBox.Show(
                "Would you like to launch FlexIO Browser now?",
                "Launch Application",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Process.Start(Path.Combine(installPath, "FlexIO-Browser.exe"));
            }
            
            Application.Exit();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Installation failed: {ex.Message}",
                "Installation Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            installButton.Enabled = true;
            cancelButton.Enabled = true;
            isInstalling = false;
        }
    }

    private async Task InstallAsync()
    {
        UpdateStatus("Preparing installation...");
        UpdateProgress(0);

        // Create installation directory
        UpdateStatus("Creating installation directory...");
        Directory.CreateDirectory(installPath);
        UpdateProgress(10);

        // Extract embedded files
        UpdateStatus("Extracting application files...");
        ExtractEmbeddedFiles();
        UpdateProgress(30);

        // Download WebView2 Runtime if needed
        UpdateStatus("Checking WebView2 Runtime...");
        await EnsureWebView2RuntimeAsync();
        UpdateProgress(60);

        // Create shortcuts
        UpdateStatus("Creating shortcuts...");
        CreateShortcuts();
        UpdateProgress(80);

        // Register application
        UpdateStatus("Registering application...");
        RegisterApplication();
        UpdateProgress(100);

        UpdateStatus("Installation completed!");
    }

    private void ExtractEmbeddedFiles()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceNames = assembly.GetManifestResourceNames();

        foreach (string resourceName in resourceNames)
        {
            if (!resourceName.StartsWith("FlexIOBrowserInstaller.files."))
                continue;

            string fileName = resourceName.Replace("FlexIOBrowserInstaller.files.", "");
            string targetPath = Path.Combine(installPath, fileName);

            // Создаем директории, если они не существуют
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);

            using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName)!)
            using (FileStream fileStream = new FileStream(targetPath, FileMode.Create))
            {
                resourceStream.CopyTo(fileStream);
            }

            // Если это exe файл, устанавливаем права на выполнение
            if (Path.GetExtension(targetPath).Equals(".exe", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    File.SetAttributes(targetPath, File.GetAttributes(targetPath) | FileAttributes.Normal);
                }
                catch { /* Ignore permission setting errors */ }
            }
        }
    }

    private async Task EnsureWebView2RuntimeAsync()
    {
        if (CheckWebView2Installed()) return;

        UpdateStatus("Downloading WebView2 Runtime...");
        using var client = new HttpClient();
        byte[] installer = await client.GetByteArrayAsync(
            "https://go.microsoft.com/fwlink/p/?LinkId=2124703"
        );
        
        string installerPath = Path.Combine(
            Path.GetTempPath(),
            "MicrosoftEdgeWebview2Setup.exe"
        );
        
        await File.WriteAllBytesAsync(installerPath, installer);

        UpdateStatus("Installing WebView2 Runtime...");
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = installerPath,
            Arguments = "/silent /install",
            UseShellExecute = true
        });
        await process!.WaitForExitAsync();
    }

    private bool CheckWebView2Installed()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}"
            );
            return key != null;
        }
        catch
        {
            return false;
        }
    }

    private void CreateShortcuts()
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        string startMenuPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
            "Programs",
            "FlexIO Browser"
        );

        Directory.CreateDirectory(startMenuPath);

        // Создаем ярлыки через командную строку
        string targetPath = Path.Combine(installPath, "FlexIO-Browser.exe");
        
        // Ярлык на рабочем столе
        CreateShortcutUsingCmd(targetPath, Path.Combine(desktopPath, "FlexIO Browser.lnk"));
        
        // Ярлык в меню Пуск
        CreateShortcutUsingCmd(targetPath, Path.Combine(startMenuPath, "FlexIO Browser.lnk"));
    }

    private void CreateShortcutUsingCmd(string targetPath, string shortcutPath)
    {
        string script = $@"
        Set oWS = WScript.CreateObject(""WScript.Shell"")
        sLinkFile = ""{shortcutPath}""
        Set oLink = oWS.CreateShortcut(sLinkFile)
        oLink.TargetPath = ""{targetPath}""
        oLink.WorkingDirectory = ""{Path.GetDirectoryName(targetPath)}""
        oLink.Description = ""FlexIO Browser""
        oLink.IconLocation = ""{targetPath},0""
        oLink.Save
    ";

        string vbsPath = Path.Combine(Path.GetTempPath(), "CreateShortcut.vbs");
        File.WriteAllText(vbsPath, script);

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cscript.exe",
                Arguments = $"//NoLogo \"{vbsPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        process.Start();
        process.WaitForExit();

        try
        {
            File.Delete(vbsPath);
        }
        catch { }
    }

    private void RegisterApplication()
    {
        // Add to Programs and Features
        using var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\FlexIO Browser"
        );
        
        key.SetValue("DisplayName", "FlexIO Browser");
        key.SetValue("UninstallString", $"\"{Path.Combine(installPath, "uninstall.exe")}\"");
        key.SetValue("DisplayIcon", $"{Path.Combine(installPath, "FlexIO-Browser.exe")},0");
        key.SetValue("Publisher", "FlexIO");
        key.SetValue("DisplayVersion", "1.0.0");
        key.SetValue("InstallLocation", installPath);
    }

    private void UpdateStatus(string status)
    {
        if (statusLabel.InvokeRequired)
        {
            statusLabel.Invoke(() => statusLabel.Text = status);
        }
        else
        {
            statusLabel.Text = status;
        }
    }

    private void UpdateProgress(int value)
    {
        if (progressBar.InvokeRequired)
        {
            progressBar.Invoke(() => progressBar.Value = value);
        }
        else
        {
            progressBar.Value = value;
        }
    }
}

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new Installer());
    }
}