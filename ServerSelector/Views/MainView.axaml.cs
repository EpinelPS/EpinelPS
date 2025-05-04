using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using System;
using System.IO;
using System.Net;
using System.Security.Principal;

namespace ServerSelector.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        CmbServerSelection.SelectedIndex = ServerSwitcher.IsUsingOfficalServer() ? 0 : 1;

        TxtIpAddress.IsEnabled = !ServerSwitcher.IsUsingOfficalServer();
        ChkOffline.IsChecked = ServerSwitcher.IsOffline();

        if (OperatingSystem.IsWindows() && !new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
        {
            TabPc.Content = new TextBlock()
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Text = "Administrator privileges are required to change servers."
            };
        }
        if (OperatingSystem.IsLinux() && Environment.UserName != "root")
        {
            TabPc.Content = new TextBlock()
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Text = "Root is required to change servers in order to modify /etc/hosts."
            };
        }

        UpdateIntegrityLabel();

        txtGamePath.Text = GameSettings.Settings.GameRoot;
        TxtIpAddress.Text = GameSettings.Settings.LastIp;
    }

    private void SetGamePathValid(bool isValid)
    {
        if (isValid)
        {
            txtGamePath.BorderBrush = null;
        }
        else
        {
            txtGamePath.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        }
    }

    private string? GamePath
    {
        get
        {
            if (txtGamePath.Text == null) return null;
            return Path.Combine(txtGamePath.Text, "NIKKE", "game");
        }
    }
    private string? LauncherPath
    {
        get
        {
            if (txtGamePath.Text == null) return null;
            return Path.Combine(txtGamePath.Text, "Launcher");
        }
    }
    private bool ValidatePaths(bool showMessage)
    {
        if (string.IsNullOrEmpty(txtGamePath.Text) || LauncherPath == null)
        {
            SetGamePathValid(false);
            if (showMessage)
                ShowWarningMsg("Game path is blank", "Error");
            return false;
        }

        if (!Directory.Exists(GamePath))
        {
            SetGamePathValid(false);
            if (showMessage)
                ShowWarningMsg("game folder does not exist in the game root folder", "Error");
            return false;
        }

        if (!File.Exists(Path.Combine(LauncherPath, "nikke_launcher.exe")))
        {
            SetGamePathValid(false);
            if (showMessage)
                ShowWarningMsg("Game path is invalid. Make sure that nikke_launcher.exe exists in the <Game Path>/launcher folder", "Error");

            return false;
        }

        SetGamePathValid(true);

        return true;
    }

    private async void UpdateIntegrityLabel()
    {
        if (!ValidatePaths(false) || txtGamePath.Text == null || GamePath == null || LauncherPath == null)
            return;

        SetLoadingScreenVisible(true);
        LblStatus.Text = "Status: " + await ServerSwitcher.CheckIntegrity(GamePath, LauncherPath);
        SetLoadingScreenVisible(false);
    }

    private void SetLoadingScreenVisible(bool visible)
    {
        if (visible)
        {
            MainUI.IsVisible = false;
            LoadingUI.IsVisible = true;
        }
        else
        {
            LoadingUI.IsVisible = false;
            MainUI.IsVisible = true;
        }
    }

    private async void Save_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!ValidatePaths(true) || txtGamePath.Text == null || GamePath == null || LauncherPath == null)
            return;

        if (CmbServerSelection.SelectedIndex == 1 && !IPAddress.TryParse(TxtIpAddress.Text, out _))
        {
            ShowWarningMsg("Invalid IP address. The entered IP address should be IPv4 or IPv6.", "Error");
            return;
        }

        GameSettings.Settings.GameRoot = txtGamePath.Text;
        GameSettings.Settings.LastIp = TxtIpAddress.Text ?? "127.0.0.1";

        TxtIpAddress.Text ??= "";

        GameSettings.Save();

        SetLoadingScreenVisible(true);
        try
        {
            var res = await ServerSwitcher.SaveCfg(CmbServerSelection.SelectedIndex == 0, GamePath, LauncherPath, TxtIpAddress.Text, ChkOffline.IsChecked ?? false);

            if (!res.IsSupported)
            {
                ShowWarningMsg("Game version might not be supported", "Warning");
            }
        }
        catch (Exception ex)
        {
            ShowWarningMsg("Failed to save configuration: " + ex.ToString(), "Error");
        }
        UpdateIntegrityLabel();
        SetLoadingScreenVisible(false);
    }

    private void CmbServerSelection_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (CmbServerSelection != null)
        {
            TxtIpAddress.IsEnabled = CmbServerSelection.SelectedIndex == 1;
            ChkOffline.IsEnabled = CmbServerSelection.SelectedIndex == 1;
            LblIp.IsEnabled = CmbServerSelection.SelectedIndex == 1;
        }
    }

    public static void ShowWarningMsg(string text, string title)
    {
        ContentDialog dlg = new() { Title = title, Content = text, PrimaryButtonText = "OK" };
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
                    if (!File.Exists(Path.Combine(txtGamePath.Text, "NIKKE", "game", "nikke.exe")))
                    {
                        ShowWarningMsg("Game path is invalid. Make sure that nikke.exe exists in the game folder", "Error");
                        return;
                    }
                    if (!File.Exists(Path.Combine(txtGamePath.Text, "Launcher", "nikke_launcher.exe")))
                    {
                        ShowWarningMsg("Game path is invalid. Make sure that nikke_launcher.exe exists in the Launcher folder", "Error");
                        return;
                    }
                }
                UpdateIntegrityLabel();
            }
        }
    }

    private void BtnSelectApkPath_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // To be implemented
    }

    private void GamePath_TextChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateIntegrityLabel();
    }
    private void LauncherPath_TextChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateIntegrityLabel();
    }
}
