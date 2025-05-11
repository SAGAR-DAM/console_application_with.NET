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
        // Get user input
        string sample = SampleInput.Text;
        string sumLength = SumLengthInput.Text;

        InfoBlock.Text = $"Running simulation with Sample={sample}, SumLength={sumLength}";

        string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;
        string scriptPath = Path.Combine(projectRoot, "PythonBackend", "simulate.py");
        string outputImage = Path.Combine(projectRoot, "PythonBackend", "output.png");

        // Construct the command to run Python script
        string command = $"\"{scriptPath}\" {sample} {sumLength} \"{outputImage}\"";
        
        // Clear previous image (if any)
        ResultImage.Source = null;

        // Debugging output to console
        Console.WriteLine($"Running command: python {command}");

        var psi = new ProcessStartInfo
        {
            FileName = "python",  // Make sure Python is correctly installed
            Arguments = command,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Start Python process
        var process = Process.Start(psi);
        if (process == null)
        {
            InfoBlock.Text += "\n❌ Failed to start Python process.";
            return;
        }

        // Read output and errors
        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        // Log output and error to the console
        Console.WriteLine($"Output: {output}");
        Console.WriteLine($"Error: {error}");

        // Check if the image has been generated
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
            InfoBlock.Text += "\n❌ output.png not found.";
        }
    }
    catch (Exception ex)
    {
        InfoBlock.Text += $"\n❌ Error: {ex.Message}";
    }
}

    }
}
