using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FourierCalculator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Append the clicked button's content to exprInput
        private void OnTokenClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                exprInput.Text += btn.Content.ToString();
                exprInput.CaretIndex = exprInput.Text.Length;
            }
        }

        // Remove the last character
        private void OnBackspaceClick(object sender, RoutedEventArgs e)
        {
            var txt = exprInput.Text;
            if (!string.IsNullOrEmpty(txt))
            {
                exprInput.Text = txt[0..^1];
                exprInput.CaretIndex = exprInput.Text.Length;
            }
        }

        // Clear the entire expression
        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            exprInput.Clear();
        }

        // Launch the Python script and display results
        private async void RunSimulation(object sender, RoutedEventArgs e)
        {
            InfoBlock.Text = "";

            string expr  = exprInput.Text.Trim();
            string lower = lowerLimitInput.Text.Trim();
            string upper = upperLimitInput.Text.Trim();
            string terms = termsInput.Text.Trim();

            if (string.IsNullOrEmpty(expr) ||
                string.IsNullOrEmpty(lower) ||
                string.IsNullOrEmpty(upper) ||
                string.IsNullOrEmpty(terms))
            {
                InfoBlock.Text = "❌ Please fill in expression, limits, and terms.";
                return;
            }

            // Determine project root and script path
            string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!
                                     .Parent!.Parent!.Parent!.FullName;
            string scriptPath  = Path.Combine(projectRoot, "PythonBackend", "simulate.py");
            string outputImage = Path.Combine(projectRoot, "PythonBackend", "output.png");

            // Quote arguments to handle spaces/special chars
            string args = $"\"{expr}\" \"{lower}\" \"{upper}\" {terms}";

            var psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"\"{scriptPath}\" {args}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            InfoBlock.Text += $"▶ python {scriptPath} {args}\n";

            try
            {
                using var proc = Process.Start(psi);
                if (proc == null)
                {
                    InfoBlock.Text += "❌ Failed to start Python process.\n";
                    return;
                }

                // Read stdout and stderr
                string stdout = await proc.StandardOutput.ReadToEndAsync();
                string stderr = await proc.StandardError.ReadToEndAsync();
                await proc.WaitForExitAsync();

                InfoBlock.Text += stdout;
                if (!string.IsNullOrEmpty(stderr))
                    InfoBlock.Text += $"⚠️ Errors:\n{stderr}\n";

                if (proc.ExitCode != 0)
                {
                    InfoBlock.Text += $"❌ Python exited with code {proc.ExitCode}.\n";
                    return;
                }

                // Load and display the output image
                if (File.Exists(outputImage))
                {
                    InfoBlock.Text += "✅ Fourier plot generated.\n";
                    using var fs = new FileStream(outputImage, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = fs;
                    bmp.EndInit();
                    ResultImage.Source = bmp;
                }
                else
                {
                    InfoBlock.Text += $"❌ Could not find output image at {outputImage}\n";
                }
            }
            catch (Exception ex)
            {
                InfoBlock.Text += $"❌ Exception: {ex.Message}\n";
            }
        }
    }
}
