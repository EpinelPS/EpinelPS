using Avalonia.Controls;
using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace ServerSelector.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        CmbServerSelection.SelectedIndex = ServerSwitcher.IsUsingOfficalServer() ? 0 : 1;

        TxtIpAddress.IsEnabled = !ServerSwitcher.IsUsingOfficalServer();
    }

    private void Save_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(txtGamePath.Text) || string.IsNullOrEmpty(txtLauncherPath.Text))
        {
            ShowWarningMsg("Game path / launcher path is empty", "Error");
            return;
        }

        if (!Directory.Exists(txtGamePath.Text))
        {
            ShowWarningMsg("Game path folder does not exist", "Error");
            return;
        }

        if (!Directory.Exists(txtLauncherPath.Text))
        {
            ShowWarningMsg("Launcher folder does not exist", "Error");
            return;
        }

        if (!File.Exists(Path.Combine(txtLauncherPath.Text, "nikke_launcher.exe")))
        {
            ShowWarningMsg("Launcher path is invalid. Make sure that nikke_launcher.exe exists in the launcher folder", "Error");
            return;
        }

        if (!File.Exists(Path.Combine(txtGamePath.Text, "nikke.exe")))
        {
            ShowWarningMsg("Game path is invalid. Make sure that nikke.exe exists in the launcher folder", "Error");
            return;
        }

        if (CmbServerSelection.SelectedIndex == 1)
        {
            if (!IPAddress.TryParse(TxtIpAddress.Text, out _))
            {
                ShowWarningMsg("Invalid IP address. The entered IP address should be IPv4 or IPv6.", "Error");
                return;
            }
        }
        if (TxtIpAddress.Text == null) TxtIpAddress.Text = "";


        try
        {
            ServerSwitcher.SaveCfg(CmbServerSelection.SelectedIndex == 0, txtGamePath.Text, txtLauncherPath.Text, TxtIpAddress.Text);
        }
        catch (Exception ex)
        {
            ShowWarningMsg("Failed to save configuration: " + ex.ToString(), "Error");
        }
    }

    private void CmbServerSelection_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (CmbServerSelection != null)
            TxtIpAddress.IsEnabled = CmbServerSelection.SelectedIndex == 1;
    }

    // for some stupid reason avalonia does not support message boxes.

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    public static void ShowWarningMsg(string text, string title)
    {
        MessageBox(IntPtr.Zero, text, title, 0x00000030);
    }
}
