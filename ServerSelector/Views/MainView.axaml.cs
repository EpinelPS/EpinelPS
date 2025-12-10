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
        CmbServerSelection.SelectedIndex = ServerSwitcher.IsUsingLocalServer() ? 1 : 0;

        TxtIpAddress.IsEnabled = !ServerSwitcher.IsUsingLocalServer();
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

    private string? BasePath
    {
        get
        {
            if (txtGamePath.Text == null) return null;
            return txtGamePath.Text;
        }
    }
    private bool ValidatePaths(bool showMessage)
    {
        if (string.IsNullOrEmpty(BasePath))
        {
            SetGamePathValid(false);
            if (showMessage)
                ShowWarningMsg("Game path is blank", "Error");
            return false;
        }

        if (!Directory.Exists(BasePath))
        {
            SetGamePathValid(false);
            if (showMessage)
                ShowWarningMsg("Game path does not exist", "Error");
            return false;
        }

        var result = ServerSwitcher.SetBasePath(BasePath);

        if (!result.Item1)
        {
            SetGamePathValid(false);
            if (showMessage)
                ShowWarningMsg(result.Item2, "Error");
            return false;
        }

        SetGamePathValid(true);

        return true;
    }

    private async void UpdateIntegrityLabel()
    {
        if (!ValidatePaths(false) || txtGamePath.Text == null || BasePath == null)
            return;

        SetLoadingScreenVisible(true);
        LblStatus.Text = "Status: " + await ServerSwitcher.CheckIntegrity();
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
        if (!ValidatePaths(true) || txtGamePath.Text == null || BasePath == null)
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
            ServerSwitchResult res = await ServerSwitcher.SaveCfg(CmbServerSelection.SelectedIndex == 0, TxtIpAddress.Text, ChkOffline.IsChecked ?? false);

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
        TopLevel? topLevel = TopLevel.GetTopLevel(this);

        if (topLevel != null)
        {
            // Start async operation to open the dialog.
            System.Collections.Generic.IReadOnlyList<IStorageFolder> files = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select game path, with launcher and game folder",
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
