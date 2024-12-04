using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace FlexIO.Browser;

public class MainForm : Form
{
    private WebView2 webView;
    private TextBox addressBar;
    private Button goButton;
    private Button backButton;
    private Button forwardButton;
    private Button refreshButton;

    public MainForm()
    {
        InitializeComponents();
    }

    private async void InitializeComponents()
    {
        this.Size = new System.Drawing.Size(1024, 768);
        this.Text = "FlexIO Browser";

        // Create controls
        addressBar = new TextBox();
        goButton = new Button();
        backButton = new Button();
        forwardButton = new Button();
        refreshButton = new Button();
        webView = new WebView2();

        // Configure navigation panel
        var navPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40
        };

        // Setup buttons
        backButton.Text = "←";
        backButton.Width = 30;
        backButton.Click += (s, e) => { if (webView.CanGoBack) webView.GoBack(); };

        forwardButton.Text = "→";
        forwardButton.Width = 30;
        forwardButton.Click += (s, e) => { if (webView.CanGoForward) webView.GoForward(); };

        refreshButton.Text = "⟳";
        refreshButton.Width = 30;
        refreshButton.Click += (s, e) => webView.Reload();

        goButton.Text = "Go";
        goButton.Width = 50;
        goButton.Click += (s, e) => Navigate();

        // Setup address bar
        addressBar.Width = 500;
        addressBar.KeyPress += (s, e) => 
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                Navigate();
                e.Handled = true;
            }
        };

        // Layout controls
        navPanel.Controls.AddRange(new Control[] 
        {
            backButton,
            forwardButton,
            refreshButton,
            addressBar,
            goButton
        });

        // Position controls
        backButton.Left = 5;
        forwardButton.Left = backButton.Right + 5;
        refreshButton.Left = forwardButton.Right + 5;
        addressBar.Left = refreshButton.Right + 10;
        goButton.Left = addressBar.Right + 5;

        // Center controls vertically
        foreach (Control control in navPanel.Controls)
        {
            control.Top = (navPanel.Height - control.Height) / 2;
        }

        // Setup WebView2
        webView.Dock = DockStyle.Fill;

        // Add controls to form
        Controls.Add(navPanel);
        Controls.Add(webView);

        try
        {
            await webView.EnsureCoreWebView2Async();
            webView.Source = new Uri("https://www.google.com");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error initializing WebView2: " + ex.Message);
        }
    }

    private void Navigate()
    {
        if (!string.IsNullOrEmpty(addressBar.Text))
        {
            string url = addressBar.Text;
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = "https://" + url;
            }
            try
            {
                webView.Source = new Uri(url);
            }
            catch (UriFormatException)
            {
                MessageBox.Show("Invalid URL format");
            }
        }
    }
}