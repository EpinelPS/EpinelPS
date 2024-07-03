using Avalonia.Controls;
using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace ServerSelector.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        CmbServerSelection.SelectedIndex = ServerSwitcher.IsUsingOfficalServer() ? 0 : 1;
    }

    private void Save_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            ServerSwitcher.SaveCfg(CmbServerSelection.SelectedIndex == 0, txtGamePath.Text, txtLauncherPath.Text);
        }
        catch(Exception ex)
        {
            ShowWarningMsg("Failed to save configuration: " + ex.ToString(), "Error");
        }
    }

    // for some stupid reason avalonia does not support message boxes.

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    public static void ShowWarningMsg(string text, string title)
    {
        MessageBox(IntPtr.Zero, text, title, 0x00000030);
    }
}
