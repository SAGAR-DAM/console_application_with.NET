using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DiceDistGUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void RunSimulation(object sender, RoutedEventArgs e)
{
    try
    {
        string terms = termsInput.Text;
        InfoBlock.Text = $"Running simulation with terms={terms}";

        string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;
        string scriptPath = Path.Combine(projectRoot, "PythonBackend", "simulate.py");
        string outputImage = Path.Combine(projectRoot, "PythonBackend", "output.png");

        string command = $"\"{scriptPath}\" {terms} \"{outputImage}\"";
        ResultImage.Source = null;

        Console.WriteLine($"Running command: python {command}");

        var psi = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = command,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = Process.Start(psi);
        if (process == null)
        {
            InfoBlock.Text += "\n❌ Failed to start Python process.";
            return;
        }

        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        Console.WriteLine($"Output: {output}");
        Console.WriteLine($"Error: {error}");

        if (process.ExitCode != 0)
        {
            InfoBlock.Text += $"\n❌ Python script failed with exit code {process.ExitCode}.";
            InfoBlock.Text += $"\nError Output:\n{error}";
            return;
        }

        if (File.Exists(outputImage))
        {
            InfoBlock.Text += "\n✅ Simulation completed successfully.";
            using (FileStream stream = new FileStream(outputImage, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                ResultImage.Source = bitmap;
            }
        }
        else
        {
            InfoBlock.Text += $"\n❌ output.png not found at {outputImage}";
        }
    }
    catch (Exception ex)
    {
        InfoBlock.Text += $"\n❌ Error: {ex.Message}";
    }
}

    }
}
