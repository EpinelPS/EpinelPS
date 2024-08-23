using Avalonia.Controls;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
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

        if (OperatingSystem.IsWindows() && !new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
        {
            TabPc.Content = new TextBlock()
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Text = "Administrator privileges are required to change servers."
            };
        }

        LblStatus.Text = "Status: " + ServerSwitcher.CheckIntegrity();
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
            ShowWarningMsg("Launcher path is invalid. Make sure that the game executable exists in the launcher folder", "Error");
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

        MainUI.IsVisible = false;
        LoadingUI.IsVisible = true;

        try
        {
            ServerSwitcher.SaveCfg(CmbServerSelection.SelectedIndex == 0, txtGamePath.Text, txtLauncherPath.Text, TxtIpAddress.Text);
        }
        catch (Exception ex)
        {
            ShowWarningMsg("Failed to save configuration: " + ex.ToString(), "Error");
        }

        LoadingUI.IsVisible = false;
        MainUI.IsVisible = true;
    }

    private void CmbServerSelection_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (CmbServerSelection != null)
            TxtIpAddress.IsEnabled = CmbServerSelection.SelectedIndex == 1;
    }

    public static void ShowWarningMsg(string text, string title)
    {
        ContentDialog dlg = new ContentDialog() { Title = title, Content = text, PrimaryButtonText = "OK" };
        dlg.ShowAsync();
    }

    private async void BtnSelectGamePath_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        if (topLevel != null)
        {
            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select game path (with game executable)",
                AllowMultiple = false
            });

            if (files.Count >= 1)
            {
                txtGamePath.Text = files[0].TryGetLocalPath();

                // validate if the folder has game exe
                if (!string.IsNullOrEmpty(txtGamePath.Text))
                {
                    if (!File.Exists(Path.Combine(txtGamePath.Text, "nikke.exe")))
                    {
                        ShowWarningMsg("Game path is invalid. Make sure that nikke.exe exists in the launcher folder", "Error");
                        return;
                    }
                }
            }
        }
    }
    private async void BtnSelectLauncherPath_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        if (topLevel != null)
        {
            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select launcher path",
                AllowMultiple = false
            });

            if (files.Count >= 1)
            {
                txtLauncherPath.Text = files[0].TryGetLocalPath();

                // validate if the folder has game exe
                if (!string.IsNullOrEmpty(txtLauncherPath.Text))
                {
                    if (!File.Exists(Path.Combine(txtLauncherPath.Text, "nikke_launcher.exe")))
                    {
                        ShowWarningMsg("Launcher path is invalid. Make sure that the game executable exists in the launcher folder", "Error");
                        return;
                    }
                }
            }
        }
    }

}
