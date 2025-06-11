using Newtonsoft.Json; // Add Newtonsoft.Json NuGet package for JSON serialization
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Particle_in_Electromagnetic_field.MainWindow;


namespace Particle_in_Electromagnetic_field
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //this.WindowState = WindowState.Maximized;

            CreateConfigFile();
        }


        // Function to create the config.json file
        /*
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                                Make the config.json

        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        */
        private void CreateConfigFile()
        {

            // Define the directory for helpers
            string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");

            // Define the config file path
            string configFilePath = System.IO.Path.Combine(helpersDir, "config.json");

            // Check if the directory exists, if not, create it
            if (!Directory.Exists(helpersDir))
            {
                Directory.CreateDirectory(helpersDir);
            }

            // Check if the config.json file exists, if not, create it with default data
            var configData = new
            {
                initializator = "Initialized configuration JSON file..."
            };

            string json = JsonConvert.SerializeObject(configData, Formatting.Indented);
            File.WriteAllText(configFilePath, json);

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }



        /*
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        
                             Making textboxes on click for cylindrical, box, plate,... etc                                                
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        */
        private Dictionary<string, TextBox> dynamicInputs = new Dictionary<string, TextBox>();
        private Dictionary<string, TextBox> dynamicInputs_Particle = new Dictionary<string, TextBox>();

        private void AddLabeledInput_Electrode(string labelText)
        {
            StackPanel pair = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5, 0, 10, 0)
            };

            TextBlock label = new TextBlock
            {
                Text = labelText + ": ",
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Bold
            };

            TextBox input = new TextBox
            {
                Width = 60, // Wider for better visibility
                Margin = new Thickness(5, 0, 0, 0)
            };

            // Store the TextBox in the dictionary
            dynamicInputs[labelText] = input;

            pair.Children.Add(label);
            pair.Children.Add(input);
            geometry_inputs.Children.Add(pair);
        }



        private void AddLabeledInput_particle(string labelText)
        {
            StackPanel pair = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5, 0, 10, 0)
            };

            TextBlock label = new TextBlock
            {
                Text = labelText + ": ",
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Bold
            };

            TextBox input = new TextBox
            {
                Width = 60, // Wider for better visibility
                Margin = new Thickness(5, 0, 0, 0)
            };

            // Store the TextBox in the dictionary
            dynamicInputs_Particle[labelText] = input;

            pair.Children.Add(label);
            pair.Children.Add(input);
            particle_inputs.Children.Add(pair);
        }

        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                                     BASE ELECTRODE CLASS                                              

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */
        public class ElectrodeBase
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("potential")]
            public double potential { get; set; }
        }


        int GetNextElectrodeIndex(string dir)
        {
            var existingFiles = Directory.GetFiles(dir, "ElectrodeConfig_*.json");
            int maxIndex = existingFiles
                .Select(f => System.IO.Path.GetFileNameWithoutExtension(f).Split('_').Last())
                .Select(i => int.TryParse(i, out int val) ? val : 0)
                .DefaultIfEmpty(0)
                .Max();

            return maxIndex + 1;
        }


        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                     Method to add CYLINDRICAL Electroede                                               

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */
        public class CylindricalElectrodeConfig
        {
            [JsonProperty("electrodes")]
            public List<CylindricalElectrode> Electrodes { get; set; } = new List<CylindricalElectrode>();
        }

        public class CylindricalElectrode : ElectrodeBase
        {
           
            [JsonProperty("cx")]
            public double cx { get; set; }

            [JsonProperty("cy")]
            public double cy { get; set; }

            [JsonProperty("cz")]
            public double cz { get; set; }

            [JsonProperty("radius")]
            public double radius { get; set; }

            [JsonProperty("height")]
            public double height { get; set; }

            [JsonProperty("axis")]
            public string axis { get; set; }
        }

        private void CylindricalElectrodeAdd(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get values from the input fields
                double cxValue = Convert.ToDouble(dynamicInputs["cx"].Text);
                double cyValue = Convert.ToDouble(dynamicInputs["cy"].Text);
                double czValue = Convert.ToDouble(dynamicInputs["cz"].Text);
                double radiusValue = Convert.ToDouble(dynamicInputs["radius"].Text);
                double heightValue = Convert.ToDouble(dynamicInputs["height"].Text);
                string axisValue = dynamicInputs["axis"].Text;
                double potentialValue = Convert.ToDouble(dynamicInputs["potential"].Text);

                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");
                if (!Directory.Exists(helpersDir))
                    Directory.CreateDirectory(helpersDir);

                int fileIndex = GetNextElectrodeIndex(helpersDir);
                string configFilePath = System.IO.Path.Combine(helpersDir, $"ElectrodeConfig_{fileIndex}.json");


                // Read or initialize config
                CylindricalElectrodeConfig config;
                if (File.Exists(configFilePath))
                {
                    string existingJson = File.ReadAllText(configFilePath);
                    config = JsonConvert.DeserializeObject<CylindricalElectrodeConfig>(existingJson) ?? new CylindricalElectrodeConfig();
                }
                else
                {
                    config = new CylindricalElectrodeConfig();
                }

                // Create the new electrode
                CylindricalElectrode newElectrode = new CylindricalElectrode
                {
                    Type = "Cylinder",
                    cx = cxValue,
                    cy = cyValue,
                    cz = czValue,
                    radius = radiusValue,
                    height = heightValue,
                    axis = axisValue,
                    potential = potentialValue
                };

                // Add to config and save
                config.Electrodes.Add(newElectrode);
                string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, updatedJson);

                // Also display in electrode_list as one-line JSON
                int fileIndex_for_UI = GetNextElectrodeIndex(helpersDir);  // Already used to name file            
                string oneLineJson = JsonConvert.SerializeObject(newElectrode, Formatting.None);

                // Now create the labeled TextBlock
                TextBlock jsonText = new TextBlock
                {
                    Text = $"Electrode {fileIndex}: {oneLineJson}",
                    Margin = new Thickness(4),
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    TextWrapping = TextWrapping.Wrap
                };

                electrode_list.Children.Add(jsonText);

                MessageBox.Show("Cylinder electrode added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }




        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                     Method to add PLATE Electroede                                               

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */

        public class PlateElectrodeConfig
        {
            [JsonProperty("electrodes")]
            public List<PlateElectrode> Electrodes { get; set; } = new List<PlateElectrode>();
        }

        public class PlateElectrode : ElectrodeBase
        {
            [JsonProperty("A")]
            public double A { get; set; }

            [JsonProperty("B")]
            public double B { get; set; }

            [JsonProperty("C")]
            public double C { get; set; }

            [JsonProperty("D")]
            public double D { get; set; }

            [JsonProperty("thickness")]
            public double thickness { get; set; }

        }

        private void PlateElectrodeAdd(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get values from the input fields
                double AValue = Convert.ToDouble(dynamicInputs["A"].Text);
                double BValue = Convert.ToDouble(dynamicInputs["B"].Text);
                double CValue = Convert.ToDouble(dynamicInputs["C"].Text);
                double DValue = Convert.ToDouble(dynamicInputs["D"].Text);
                double thicknessValue = Convert.ToDouble(dynamicInputs["thickness"].Text);
                double potentialValue = Convert.ToDouble(dynamicInputs["potential"].Text);

                // Set paths
                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");
                if (!Directory.Exists(helpersDir))
                    Directory.CreateDirectory(helpersDir);

                int fileIndex = GetNextElectrodeIndex(helpersDir);
                string configFilePath = System.IO.Path.Combine(helpersDir, $"ElectrodeConfig_{fileIndex}.json");

                // Read or initialize config
                PlateElectrodeConfig config;
                if (File.Exists(configFilePath))
                {
                    string existingJson = File.ReadAllText(configFilePath);
                    config = JsonConvert.DeserializeObject<PlateElectrodeConfig>(existingJson) ?? new PlateElectrodeConfig();
                }
                else
                {
                    config = new PlateElectrodeConfig();
                }

                // Create the new electrode
                PlateElectrode newElectrode = new PlateElectrode
                {
                    Type = "Plate",
                    A = AValue,
                    B = BValue,
                    C = CValue,
                    D = DValue,
                    thickness = thicknessValue,
                    potential = potentialValue
                };

                // Add to config and save
                config.Electrodes.Add(newElectrode);
                string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, updatedJson);

                // Also display in electrode_list as one-line JSON
                int fileIndex_for_UI = GetNextElectrodeIndex(helpersDir);  // Already used to name file            
                string oneLineJson = JsonConvert.SerializeObject(newElectrode, Formatting.None);

                // Now create the labeled TextBlock
                TextBlock jsonText = new TextBlock
                {
                    Text = $"Electrode {fileIndex}: {oneLineJson}",
                    Margin = new Thickness(4),
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    TextWrapping = TextWrapping.Wrap
                };

                electrode_list.Children.Add(jsonText);

                MessageBox.Show("Plate electrode added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }





        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                     Method to add HOLLOW ROD Electroede                                               

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */
        public class HollowRodElectrodeConfig
        {
            [JsonProperty("electrodes")]
            public List<HollowRodElectrode> Electrodes { get; set; } = new List<HollowRodElectrode>();
        }

        public class HollowRodElectrode : ElectrodeBase
        {

            [JsonProperty("cx")]
            public double Cx { get; set; }

            [JsonProperty("cy")]
            public double Cy { get; set; }

            [JsonProperty("cz")]
            public double Cz { get; set; }

            [JsonProperty("radius")]
            public double radius { get; set; }

            [JsonProperty("thickness")]
            public double thickness { get; set; }

            [JsonProperty("height")]
            public double height { get; set; }

            [JsonProperty("axis")]
            public string axis { get; set; }

        }

        private void HollowRodElectrodeAdd(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get values from the input fields
                double cxValue = Convert.ToDouble(dynamicInputs["cx"].Text);
                double cyValue = Convert.ToDouble(dynamicInputs["cy"].Text);
                double czValue = Convert.ToDouble(dynamicInputs["cz"].Text);
                double radiusValue = Convert.ToDouble(dynamicInputs["radius"].Text);
                double heightValue = Convert.ToDouble(dynamicInputs["height"].Text);
                double thickness = Convert.ToDouble(dynamicInputs["thickness"].Text);
                string axisValue = dynamicInputs["axis"].Text;
                double potentialValue = Convert.ToDouble(dynamicInputs["potential"].Text);

                // Set paths
                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");
                if (!Directory.Exists(helpersDir))
                    Directory.CreateDirectory(helpersDir);

                int fileIndex = GetNextElectrodeIndex(helpersDir);
                string configFilePath = System.IO.Path.Combine(helpersDir, $"ElectrodeConfig_{fileIndex}.json");

                // Read or initialize config
                HollowRodElectrodeConfig config;
                if (File.Exists(configFilePath))
                {
                    string existingJson = File.ReadAllText(configFilePath);
                    config = JsonConvert.DeserializeObject<HollowRodElectrodeConfig>(existingJson) ?? new HollowRodElectrodeConfig();
                }
                else
                {
                    config = new HollowRodElectrodeConfig();
                }

                // Create the new electrode
                HollowRodElectrode newElectrode = new HollowRodElectrode
                {
                    Type = "HollowRod",
                    Cx = cxValue,
                    Cy = cyValue,
                    Cz = czValue,
                    radius = radiusValue,
                    thickness = thickness,
                    height = heightValue,
                    axis = axisValue,
                    potential = potentialValue
                };

                // Add to config and save
                config.Electrodes.Add(newElectrode);
                string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, updatedJson);

                // Also display in electrode_list as one-line JSON
                int fileIndex_for_UI = GetNextElectrodeIndex(helpersDir);  // Already used to name file            
                string oneLineJson = JsonConvert.SerializeObject(newElectrode, Formatting.None);

                // Now create the labeled TextBlock
                TextBlock jsonText = new TextBlock
                {
                    Text = $"Electrode {fileIndex}: {oneLineJson}",
                    Margin = new Thickness(4),
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    TextWrapping = TextWrapping.Wrap
                };

                electrode_list.Children.Add(jsonText);

                MessageBox.Show("Hollow Rod electrode added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }






        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                     Method to add ELLIPSOIDAL Electroede                                               

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */
        public class EllipsoidalElectrodeConfig
        {
            [JsonProperty("electrodes")]
            public List<EllipsoidalElectrode> Electrodes { get; set; } = new List<EllipsoidalElectrode>();
        }

        public class EllipsoidalElectrode : ElectrodeBase
        {
            [JsonProperty("cx")]
            public double Cx { get; set; }

            [JsonProperty("cy")]
            public double Cy { get; set; }

            [JsonProperty("cz")]
            public double Cz { get; set; }

            [JsonProperty("rx")]
            public double rx { get; set; }

            [JsonProperty("ry")]
            public double ry { get; set; }

            [JsonProperty("rz")]
            public double rz { get; set; }

        }

        private void EllipsoidalElectrodeAdd(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get values from the input fields
                double cxValue = Convert.ToDouble(dynamicInputs["cx"].Text);
                double cyValue = Convert.ToDouble(dynamicInputs["cy"].Text);
                double czValue = Convert.ToDouble(dynamicInputs["cz"].Text);
                double rx = Convert.ToDouble(dynamicInputs["rx"].Text);
                double ry = Convert.ToDouble(dynamicInputs["ry"].Text);
                double rz = Convert.ToDouble(dynamicInputs["rz"].Text);
                double potentialValue = Convert.ToDouble(dynamicInputs["potential"].Text);

                // Set paths
                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");
                if (!Directory.Exists(helpersDir))
                    Directory.CreateDirectory(helpersDir);

                int fileIndex = GetNextElectrodeIndex(helpersDir);
                string configFilePath = System.IO.Path.Combine(helpersDir, $"ElectrodeConfig_{fileIndex}.json");

                // Read or initialize config
                EllipsoidalElectrodeConfig config;
                if (File.Exists(configFilePath))
                {
                    string existingJson = File.ReadAllText(configFilePath);
                    config = JsonConvert.DeserializeObject<EllipsoidalElectrodeConfig>(existingJson) ?? new EllipsoidalElectrodeConfig();
                }
                else
                {
                    config = new EllipsoidalElectrodeConfig();
                }

                // Create the new electrode
                EllipsoidalElectrode newElectrode = new EllipsoidalElectrode
                {
                    Type = "Ellipsoidal",
                    Cx = cxValue,
                    Cy = cyValue,
                    Cz = czValue,
                    rx = rx,
                    ry = ry,
                    rz = rz,
                    potential = potentialValue
                };

                // Add to config and save
                config.Electrodes.Add(newElectrode);
                string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, updatedJson);

                // Also display in electrode_list as one-line JSON
                int fileIndex_for_UI = GetNextElectrodeIndex(helpersDir);  // Already used to name file            
                string oneLineJson = JsonConvert.SerializeObject(newElectrode, Formatting.None);

                // Now create the labeled TextBlock
                TextBlock jsonText = new TextBlock
                {
                    Text = $"Electrode {fileIndex}: {oneLineJson}",
                    Margin = new Thickness(4),
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    TextWrapping = TextWrapping.Wrap
                };

                electrode_list.Children.Add(jsonText);

                MessageBox.Show("Ellipsoidal electrode added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }






        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                     Method to add HYPERBOLOIDAL Electroede                                               

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */
        public class HyperboloidalElectrodeConfig
        {
            [JsonProperty("electrodes")]
            public List<HyperboloidalElectrode> Electrodes { get; set; } = new List<HyperboloidalElectrode>();
        }

        public class HyperboloidalElectrode : ElectrodeBase
        {
            [JsonProperty("cx")]
            public double Cx { get; set; }

            [JsonProperty("cy")]
            public double Cy { get; set; }

            [JsonProperty("cz")]
            public double Cz { get; set; }

            [JsonProperty("a")]
            public double a { get; set; }

            [JsonProperty("b")]
            public double b { get; set; }

            [JsonProperty("c")]
            public double c { get; set; }

            [JsonProperty("waist")]
            public double waist { get; set; }

            [JsonProperty("axis")]
            public string axis { get; set; }


        }

        private void HyperboloidalElectrodeAdd(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get values from the input fields
                double cxValue = Convert.ToDouble(dynamicInputs["cx"].Text);
                double cyValue = Convert.ToDouble(dynamicInputs["cy"].Text);
                double czValue = Convert.ToDouble(dynamicInputs["cz"].Text);
                double a = Convert.ToDouble(dynamicInputs["a"].Text);
                double b = Convert.ToDouble(dynamicInputs["b"].Text);
                double c = Convert.ToDouble(dynamicInputs["c"].Text);
                double waist = Convert.ToDouble(dynamicInputs["waist"].Text);
                string axis = dynamicInputs["axis"].Text;
                double potentialValue = Convert.ToDouble(dynamicInputs["potential"].Text);

                // Set paths
                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");
                if (!Directory.Exists(helpersDir))
                    Directory.CreateDirectory(helpersDir);

                int fileIndex = GetNextElectrodeIndex(helpersDir);
                string configFilePath = System.IO.Path.Combine(helpersDir, $"ElectrodeConfig_{fileIndex}.json");

                // Read or initialize config
                HyperboloidalElectrodeConfig config;
                if (File.Exists(configFilePath))
                {
                    string existingJson = File.ReadAllText(configFilePath);
                    config = JsonConvert.DeserializeObject<HyperboloidalElectrodeConfig>(existingJson) ?? new HyperboloidalElectrodeConfig();
                }
                else
                {
                    config = new HyperboloidalElectrodeConfig();
                }

                // Create the new electrode
                HyperboloidalElectrode newElectrode = new HyperboloidalElectrode
                {
                    Type = "Hyperboloidal",
                    Cx = cxValue,
                    Cy = cyValue,
                    Cz = czValue,
                    a = a,
                    b = b,
                    c = c,
                    waist = waist,
                    axis = axis,
                    potential = potentialValue
                };


                // Add to config and save
                config.Electrodes.Add(newElectrode);
                string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, updatedJson);

                // Also display in electrode_list as one-line JSON
                int fileIndex_for_UI = GetNextElectrodeIndex(helpersDir);  // Already used to name file            
                string oneLineJson = JsonConvert.SerializeObject(newElectrode, Formatting.None);

                // Now create the labeled TextBlock
                TextBlock jsonText = new TextBlock
                {
                    Text = $"Electrode {fileIndex}: {oneLineJson}",
                    Margin = new Thickness(4),
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    TextWrapping = TextWrapping.Wrap
                };

                electrode_list.Children.Add(jsonText);

                MessageBox.Show("Hyperboloidal electrode added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }





        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                     Method to add SPHERICAL Electroede                                               

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */
        public class SphericalElectrodeConfig
        {
            [JsonProperty("electrodes")]
            public List<SphericalElectrode> Electrodes { get; set; } = new List<SphericalElectrode>();
        }

        public class SphericalElectrode : ElectrodeBase
        {
            [JsonProperty("cx")]
            public double Cx { get; set; }

            [JsonProperty("cy")]
            public double Cy { get; set; }

            [JsonProperty("cz")]
            public double Cz { get; set; }

            [JsonProperty("radius")]
            public double radius { get; set; }

        }

        private void SphericalElectrodeAdd(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get values from the input fields
                double cxValue = Convert.ToDouble(dynamicInputs["cx"].Text);
                double cyValue = Convert.ToDouble(dynamicInputs["cy"].Text);
                double czValue = Convert.ToDouble(dynamicInputs["cz"].Text);
                double radius = Convert.ToDouble(dynamicInputs["radius"].Text);
                double potentialValue = Convert.ToDouble(dynamicInputs["potential"].Text);

                // Set paths
                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");
                if (!Directory.Exists(helpersDir))
                    Directory.CreateDirectory(helpersDir);

                int fileIndex = GetNextElectrodeIndex(helpersDir);
                string configFilePath = System.IO.Path.Combine(helpersDir, $"ElectrodeConfig_{fileIndex}.json");

                // Read or initialize config
                SphericalElectrodeConfig config;
                if (File.Exists(configFilePath))
                {
                    string existingJson = File.ReadAllText(configFilePath);
                    config = JsonConvert.DeserializeObject<SphericalElectrodeConfig>(existingJson) ?? new SphericalElectrodeConfig();
                }
                else
                {
                    config = new SphericalElectrodeConfig();
                }

                // Create the new electrode
                SphericalElectrode newElectrode = new SphericalElectrode
                {
                    Type = "Spherical",
                    Cx = cxValue,
                    Cy = cyValue,
                    Cz = czValue,
                    radius = radius,
                    potential = potentialValue
                };


                // Add to config and save
                config.Electrodes.Add(newElectrode);
                string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, updatedJson);

                // Also display in electrode_list as one-line JSON
                int fileIndex_for_UI = GetNextElectrodeIndex(helpersDir);  // Already used to name file            
                string oneLineJson = JsonConvert.SerializeObject(newElectrode, Formatting.None);

                // Now create the labeled TextBlock
                TextBlock jsonText = new TextBlock
                {
                    Text = $"Electrode {fileIndex}: {oneLineJson}",
                    Margin = new Thickness(4),
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    TextWrapping = TextWrapping.Wrap
                };

                electrode_list.Children.Add(jsonText);

                MessageBox.Show("Spherical electrode added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }







        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                     Method to add BOX Electroede                                               

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */
        public class BoxElectrodeConfig
        {
            [JsonProperty("electrodes")]
            public List<BoxElectrode> Electrodes { get; set; } = new List<BoxElectrode>();
        }

        public class BoxElectrode : ElectrodeBase
        {
            [JsonProperty("x0")]
            public double x0 { get; set; }

            [JsonProperty("y0")]
            public double y0 { get; set; }

            [JsonProperty("z0")]
            public double z0 { get; set; }

            [JsonProperty("x1")]
            public double x1 { get; set; }

            [JsonProperty("y1")]
            public double y1 { get; set; }

            [JsonProperty("z1")]
            public double z1 { get; set; }

        }

        private void BoxElectrodeAdd(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get values from the input fields
                double x0 = Convert.ToDouble(dynamicInputs["x0"].Text);
                double y0 = Convert.ToDouble(dynamicInputs["y0"].Text);
                double z0 = Convert.ToDouble(dynamicInputs["z0"].Text);
                double x1 = Convert.ToDouble(dynamicInputs["x1"].Text);
                double y1 = Convert.ToDouble(dynamicInputs["y1"].Text);
                double z1 = Convert.ToDouble(dynamicInputs["z1"].Text);
                double potentialValue = Convert.ToDouble(dynamicInputs["potential"].Text);

                // Set paths
                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");
                if (!Directory.Exists(helpersDir))
                    Directory.CreateDirectory(helpersDir);

                int fileIndex = GetNextElectrodeIndex(helpersDir);
                string configFilePath = System.IO.Path.Combine(helpersDir, $"ElectrodeConfig_{fileIndex}.json");

                // Read or initialize config
                BoxElectrodeConfig config;
                if (File.Exists(configFilePath))
                {
                    string existingJson = File.ReadAllText(configFilePath);
                    config = JsonConvert.DeserializeObject<BoxElectrodeConfig>(existingJson) ?? new BoxElectrodeConfig();
                }
                else
                {
                    config = new BoxElectrodeConfig();
                }

                // Create the new electrode
                BoxElectrode newElectrode = new BoxElectrode
                {
                    Type = "Box",
                    x0 = x0,
                    y0 = y0,
                    z0 = z0,
                    x1 = x1,
                    y1 = y1,
                    z1 = z1,
                    potential = potentialValue
                };


                // Add to config and save
                config.Electrodes.Add(newElectrode);
                string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, updatedJson);

                // Also display in electrode_list as one-line JSON
                int fileIndex_for_UI = GetNextElectrodeIndex(helpersDir);  // Already used to name file            
                string oneLineJson = JsonConvert.SerializeObject(newElectrode, Formatting.None);

                // Now create the labeled TextBlock
                TextBlock jsonText = new TextBlock
                {
                    Text = $"Electrode {fileIndex}: {oneLineJson}",
                    Margin = new Thickness(4),
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    TextWrapping = TextWrapping.Wrap
                };

                electrode_list.Children.Add(jsonText);

                MessageBox.Show("Box electrode added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }







        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                    BASE CLASS FOR ADDING PARTICLES IN THE SIMULATION BOX                                              

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */
        public class ParticleBase
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("x")]
            public double x { get; set; }

            [JsonProperty("y")]
            public double y { get; set; }

            [JsonProperty("z")]
            public double z { get; set; }

            [JsonProperty("Energy")]
            public double Energy { get; set; }

            [JsonProperty("x̂")]
            public double vx { get; set; }

            [JsonProperty("ŷ")]
            public double vy { get; set; }

            [JsonProperty("ẑ")]
            public double vz { get; set; }
        }


        int GetNextParticleIndex(string dir)
        {
            var existingFiles = Directory.GetFiles(dir, "ParticleConfig_*.json");
            int maxIndex = existingFiles
                .Select(f => System.IO.Path.GetFileNameWithoutExtension(f).Split('_').Last())
                .Select(i => int.TryParse(i, out int val) ? val : 0)
                .DefaultIfEmpty(0)
                .Max();

            return maxIndex + 1;
        }

        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                                   ADDING Proton PARTICLE                                              

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */


        public class ProtonParticleConfig
        {
            [JsonProperty("particle")]
            public List<ProtonParticle> Particles { get; set; } = new List<ProtonParticle>();
        }

        public class ProtonParticle : ParticleBase
        {

        }

        private void ProtonParticleAdd(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get values from the input fields
                double x = Convert.ToDouble(dynamicInputs_Particle["x"].Text);
                double y = Convert.ToDouble(dynamicInputs_Particle["y"].Text);
                double z = Convert.ToDouble(dynamicInputs_Particle["z"].Text);
                double Energy = Convert.ToDouble(dynamicInputs_Particle["Energy"].Text);
                double vx = Convert.ToDouble(dynamicInputs_Particle["x̂"].Text);
                double vy = Convert.ToDouble(dynamicInputs_Particle["ŷ"].Text);
                double vz = Convert.ToDouble(dynamicInputs_Particle["ẑ"].Text);

                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");
                if (!Directory.Exists(helpersDir))
                    Directory.CreateDirectory(helpersDir);

                int fileIndex = GetNextParticleIndex(helpersDir);
                string configFilePath = System.IO.Path.Combine(helpersDir, $"ParticleConfig_{fileIndex}.json");


                // Read or initialize config
                ProtonParticleConfig config;
                if (File.Exists(configFilePath))
                {
                    string existingJson = File.ReadAllText(configFilePath);
                    config = JsonConvert.DeserializeObject<ProtonParticleConfig>(existingJson) ?? new ProtonParticleConfig();
                }
                else
                {
                    config = new ProtonParticleConfig();
                }

                // Create the new electrode
                ProtonParticle newParticle = new ProtonParticle
                {
                    Type = "Proton",
                    x = x,
                    y = y,
                    z = z,
                    Energy = Energy,
                    vx = vx,
                    vy = vy,   
                    vz = vz
                };

                // Add to config and save
                config.Particles.Add(newParticle);
                string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, updatedJson);

                // Also display in electrode_list as one-line JSON
                int fileIndex_for_UI = GetNextParticleIndex(helpersDir);  // Already used to name file            
                string oneLineJson = JsonConvert.SerializeObject(newParticle, Formatting.None);

                // Now create the labeled TextBlock
                TextBlock jsonText = new TextBlock
                {
                    Text = $"Particle {fileIndex}: {oneLineJson}",
                    Margin = new Thickness(4),
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    TextWrapping = TextWrapping.Wrap
                };

                particle_list.Children.Add(jsonText);

                MessageBox.Show("Proton added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


                /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                                   ADDING Electron PARTICLE                                              

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */


        public class ElectronParticleConfig
        {
            [JsonProperty("particle")]
            public List<ElectronParticle> Particles { get; set; } = new List<ElectronParticle>();
        }

        public class ElectronParticle : ParticleBase
        {

        }

        private void ElectronParticleAdd(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get values from the input fields
                double x = Convert.ToDouble(dynamicInputs_Particle["x"].Text);
                double y = Convert.ToDouble(dynamicInputs_Particle["y"].Text);
                double z = Convert.ToDouble(dynamicInputs_Particle["z"].Text);
                double Energy = Convert.ToDouble(dynamicInputs_Particle["Energy"].Text);
                double vx = Convert.ToDouble(dynamicInputs_Particle["x̂"].Text);
                double vy = Convert.ToDouble(dynamicInputs_Particle["ŷ"].Text);
                double vz = Convert.ToDouble(dynamicInputs_Particle["ẑ"].Text);

                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");
                if (!Directory.Exists(helpersDir))
                    Directory.CreateDirectory(helpersDir);

                int fileIndex = GetNextParticleIndex(helpersDir);
                string configFilePath = System.IO.Path.Combine(helpersDir, $"ParticleConfig_{fileIndex}.json");


                // Read or initialize config
                ElectronParticleConfig config;
                if (File.Exists(configFilePath))
                {
                    string existingJson = File.ReadAllText(configFilePath);
                    config = JsonConvert.DeserializeObject<ElectronParticleConfig>(existingJson) ?? new ElectronParticleConfig();
                }
                else
                {
                    config = new ElectronParticleConfig();
                }

                // Create the new electrode
                ElectronParticle newParticle = new ElectronParticle
                {
                    Type = "Electron",
                    x = x,
                    y = y,
                    z = z,
                    Energy = Energy,
                    vx = vx,
                    vy = vy,   
                    vz = vz
                };

                // Add to config and save
                config.Particles.Add(newParticle);
                string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, updatedJson);

                // Also display in electrode_list as one-line JSON
                int fileIndex_for_UI = GetNextParticleIndex(helpersDir);  // Already used to name file            
                string oneLineJson = JsonConvert.SerializeObject(newParticle, Formatting.None);

                // Now create the labeled TextBlock
                TextBlock jsonText = new TextBlock
                {
                    Text = $"Particle {fileIndex}: {oneLineJson}",
                    Margin = new Thickness(4),
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    TextWrapping = TextWrapping.Wrap
                };

                particle_list.Children.Add(jsonText);

                MessageBox.Show("Electron added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                                   ADDING C1 PARTICLE                                              

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */


        public class C1ParticleConfig
        {
            [JsonProperty("particle")]
            public List<C1Particle> Particles { get; set; } = new List<C1Particle>();
        }

        public class C1Particle : ParticleBase
        {

        }

        private void C1ParticleAdd(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get values from the input fields
                double x = Convert.ToDouble(dynamicInputs_Particle["x"].Text);
                double y = Convert.ToDouble(dynamicInputs_Particle["y"].Text);
                double z = Convert.ToDouble(dynamicInputs_Particle["z"].Text);
                double Energy = Convert.ToDouble(dynamicInputs_Particle["Energy"].Text);
                double vx = Convert.ToDouble(dynamicInputs_Particle["x̂"].Text);
                double vy = Convert.ToDouble(dynamicInputs_Particle["ŷ"].Text);
                double vz = Convert.ToDouble(dynamicInputs_Particle["ẑ"].Text);

                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");
                if (!Directory.Exists(helpersDir))
                    Directory.CreateDirectory(helpersDir);

                int fileIndex = GetNextParticleIndex(helpersDir);
                string configFilePath = System.IO.Path.Combine(helpersDir, $"ParticleConfig_{fileIndex}.json");


                // Read or initialize config
                C1ParticleConfig config;
                if (File.Exists(configFilePath))
                {
                    string existingJson = File.ReadAllText(configFilePath);
                    config = JsonConvert.DeserializeObject<C1ParticleConfig>(existingJson) ?? new C1ParticleConfig();
                }
                else
                {
                    config = new C1ParticleConfig();
                }

                // Create the new electrode
                C1Particle newParticle = new C1Particle
                {
                    Type = "C1",
                    x = x,
                    y = y,
                    z = z,
                    Energy = Energy,
                    vx = vx,
                    vy = vy,
                    vz = vz
                };

                // Add to config and save
                config.Particles.Add(newParticle);
                string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, updatedJson);

                // Also display in electrode_list as one-line JSON
                int fileIndex_for_UI = GetNextParticleIndex(helpersDir);  // Already used to name file            
                string oneLineJson = JsonConvert.SerializeObject(newParticle, Formatting.None);

                // Now create the labeled TextBlock
                TextBlock jsonText = new TextBlock
                {
                    Text = $"Particle {fileIndex}: {oneLineJson}",
                    Margin = new Thickness(4),
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    TextWrapping = TextWrapping.Wrap
                };

                particle_list.Children.Add(jsonText);

                MessageBox.Show("C1 Particle added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                                   ADDING C2 PARTICLE                                              

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */


        public class C2ParticleConfig
        {
            [JsonProperty("particle")]
            public List<C2Particle> Particles { get; set; } = new List<C2Particle>();
        }

        public class C2Particle : ParticleBase
        {

        }

        private void C2ParticleAdd(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get values from the input fields
                double x = Convert.ToDouble(dynamicInputs_Particle["x"].Text);
                double y = Convert.ToDouble(dynamicInputs_Particle["y"].Text);
                double z = Convert.ToDouble(dynamicInputs_Particle["z"].Text);
                double Energy = Convert.ToDouble(dynamicInputs_Particle["Energy"].Text);
                double vx = Convert.ToDouble(dynamicInputs_Particle["x̂"].Text);
                double vy = Convert.ToDouble(dynamicInputs_Particle["ŷ"].Text);
                double vz = Convert.ToDouble(dynamicInputs_Particle["ẑ"].Text);

                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");
                if (!Directory.Exists(helpersDir))
                    Directory.CreateDirectory(helpersDir);

                int fileIndex = GetNextParticleIndex(helpersDir);
                string configFilePath = System.IO.Path.Combine(helpersDir, $"ParticleConfig_{fileIndex}.json");


                // Read or initialize config
                C2ParticleConfig config;
                if (File.Exists(configFilePath))
                {
                    string existingJson = File.ReadAllText(configFilePath);
                    config = JsonConvert.DeserializeObject<C2ParticleConfig>(existingJson) ?? new C2ParticleConfig();
                }
                else
                {
                    config = new C2ParticleConfig();
                }

                // Create the new electrode
                C2Particle newParticle = new C2Particle
                {
                    Type = "C2",
                    x = x,
                    y = y,
                    z = z,
                    Energy = Energy,
                    vx = vx,
                    vy = vy,
                    vz = vz
                };

                // Add to config and save
                config.Particles.Add(newParticle);
                string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, updatedJson);

                // Also display in electrode_list as one-line JSON
                int fileIndex_for_UI = GetNextParticleIndex(helpersDir);  // Already used to name file            
                string oneLineJson = JsonConvert.SerializeObject(newParticle, Formatting.None);

                // Now create the labeled TextBlock
                TextBlock jsonText = new TextBlock
                {
                    Text = $"Particle {fileIndex}: {oneLineJson}",
                    Margin = new Thickness(4),
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    TextWrapping = TextWrapping.Wrap
                };

                particle_list.Children.Add(jsonText);

                MessageBox.Show("C2 Particle added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                                   ADDING C3 PARTICLE                                              

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */


        public class C3ParticleConfig
        {
            [JsonProperty("particle")]
            public List<C3Particle> Particles { get; set; } = new List<C3Particle>();
        }

        public class C3Particle : ParticleBase
        {

        }

        private void C3ParticleAdd(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get values from the input fields
                double x = Convert.ToDouble(dynamicInputs_Particle["x"].Text);
                double y = Convert.ToDouble(dynamicInputs_Particle["y"].Text);
                double z = Convert.ToDouble(dynamicInputs_Particle["z"].Text);
                double Energy = Convert.ToDouble(dynamicInputs_Particle["Energy"].Text);
                double vx = Convert.ToDouble(dynamicInputs_Particle["x̂"].Text);
                double vy = Convert.ToDouble(dynamicInputs_Particle["ŷ"].Text);
                double vz = Convert.ToDouble(dynamicInputs_Particle["ẑ"].Text);

                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");
                if (!Directory.Exists(helpersDir))
                    Directory.CreateDirectory(helpersDir);

                int fileIndex = GetNextParticleIndex(helpersDir);
                string configFilePath = System.IO.Path.Combine(helpersDir, $"ParticleConfig_{fileIndex}.json");


                // Read or initialize config
                C3ParticleConfig config;
                if (File.Exists(configFilePath))
                {
                    string existingJson = File.ReadAllText(configFilePath);
                    config = JsonConvert.DeserializeObject<C3ParticleConfig>(existingJson) ?? new C3ParticleConfig();
                }
                else
                {
                    config = new C3ParticleConfig();
                }

                // Create the new electrode
                C3Particle newParticle = new C3Particle
                {
                    Type = "C3",
                    x = x,
                    y = y,
                    z = z,
                    Energy = Energy,
                    vx = vx,
                    vy = vy,
                    vz = vz
                };

                // Add to config and save
                config.Particles.Add(newParticle);
                string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, updatedJson);

                // Also display in electrode_list as one-line JSON
                int fileIndex_for_UI = GetNextParticleIndex(helpersDir);  // Already used to name file            
                string oneLineJson = JsonConvert.SerializeObject(newParticle, Formatting.None);

                // Now create the labeled TextBlock
                TextBlock jsonText = new TextBlock
                {
                    Text = $"Particle {fileIndex}: {oneLineJson}",
                    Margin = new Thickness(4),
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    TextWrapping = TextWrapping.Wrap
                };

                particle_list.Children.Add(jsonText);

                MessageBox.Show("C3 Particle added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                                   ADDING C4 PARTICLE                                              

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */


        public class C4ParticleConfig
        {
            [JsonProperty("particle")]
            public List<C4Particle> Particles { get; set; } = new List<C4Particle>();
        }

        public class C4Particle : ParticleBase
        {

        }

        private void C4ParticleAdd(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get values from the input fields
                double x = Convert.ToDouble(dynamicInputs_Particle["x"].Text);
                double y = Convert.ToDouble(dynamicInputs_Particle["y"].Text);
                double z = Convert.ToDouble(dynamicInputs_Particle["z"].Text);
                double Energy = Convert.ToDouble(dynamicInputs_Particle["Energy"].Text);
                double vx = Convert.ToDouble(dynamicInputs_Particle["x̂"].Text);
                double vy = Convert.ToDouble(dynamicInputs_Particle["ŷ"].Text);
                double vz = Convert.ToDouble(dynamicInputs_Particle["ẑ"].Text);

                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");
                if (!Directory.Exists(helpersDir))
                    Directory.CreateDirectory(helpersDir);

                int fileIndex = GetNextParticleIndex(helpersDir);
                string configFilePath = System.IO.Path.Combine(helpersDir, $"ParticleConfig_{fileIndex}.json");


                // Read or initialize config
                C4ParticleConfig config;
                if (File.Exists(configFilePath))
                {
                    string existingJson = File.ReadAllText(configFilePath);
                    config = JsonConvert.DeserializeObject<C4ParticleConfig>(existingJson) ?? new C4ParticleConfig();
                }
                else
                {
                    config = new C4ParticleConfig();
                }

                // Create the new electrode
                C4Particle newParticle = new C4Particle
                {
                    Type = "C4",
                    x = x,
                    y = y,
                    z = z,
                    Energy = Energy,
                    vx = vx,
                    vy = vy,
                    vz = vz
                };

                // Add to config and save
                config.Particles.Add(newParticle);
                string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, updatedJson);

                // Also display in electrode_list as one-line JSON
                int fileIndex_for_UI = GetNextParticleIndex(helpersDir);  // Already used to name file            
                string oneLineJson = JsonConvert.SerializeObject(newParticle, Formatting.None);

                // Now create the labeled TextBlock
                TextBlock jsonText = new TextBlock
                {
                    Text = $"Particle {fileIndex}: {oneLineJson}",
                    Margin = new Thickness(4),
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    TextWrapping = TextWrapping.Wrap
                };

                particle_list.Children.Add(jsonText);

                MessageBox.Show("C4 Particle added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                                   ADDING C5 PARTICLE                                              

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */


        public class C5ParticleConfig
        {
            [JsonProperty("particle")]
            public List<C5Particle> Particles { get; set; } = new List<C5Particle>();
        }

        public class C5Particle : ParticleBase
        {

        }

        private void C5ParticleAdd(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get values from the input fields
                double x = Convert.ToDouble(dynamicInputs_Particle["x"].Text);
                double y = Convert.ToDouble(dynamicInputs_Particle["y"].Text);
                double z = Convert.ToDouble(dynamicInputs_Particle["z"].Text);
                double Energy = Convert.ToDouble(dynamicInputs_Particle["Energy"].Text);
                double vx = Convert.ToDouble(dynamicInputs_Particle["x̂"].Text);
                double vy = Convert.ToDouble(dynamicInputs_Particle["ŷ"].Text);
                double vz = Convert.ToDouble(dynamicInputs_Particle["ẑ"].Text);

                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");
                if (!Directory.Exists(helpersDir))
                    Directory.CreateDirectory(helpersDir);

                int fileIndex = GetNextParticleIndex(helpersDir);
                string configFilePath = System.IO.Path.Combine(helpersDir, $"ParticleConfig_{fileIndex}.json");


                // Read or initialize config
                C5ParticleConfig config;
                if (File.Exists(configFilePath))
                {
                    string existingJson = File.ReadAllText(configFilePath);
                    config = JsonConvert.DeserializeObject<C5ParticleConfig>(existingJson) ?? new C5ParticleConfig();
                }
                else
                {
                    config = new C5ParticleConfig();
                }

                // Create the new electrode
                C5Particle newParticle = new C5Particle
                {
                    Type = "C5",
                    x = x,
                    y = y,
                    z = z,
                    Energy = Energy,
                    vx = vx,
                    vy = vy,
                    vz = vz
                };

                // Add to config and save
                config.Particles.Add(newParticle);
                string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, updatedJson);

                // Also display in electrode_list as one-line JSON
                int fileIndex_for_UI = GetNextParticleIndex(helpersDir);  // Already used to name file            
                string oneLineJson = JsonConvert.SerializeObject(newParticle, Formatting.None);

                // Now create the labeled TextBlock
                TextBlock jsonText = new TextBlock
                {
                    Text = $"Particle {fileIndex}: {oneLineJson}",
                    Margin = new Thickness(4),
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    TextWrapping = TextWrapping.Wrap
                };

                particle_list.Children.Add(jsonText);

                MessageBox.Show("C5 Particle added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                                   ADDING C6 PARTICLE                                              

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */


        public class C6ParticleConfig
        {
            [JsonProperty("particle")]
            public List<C6Particle> Particles { get; set; } = new List<C6Particle>();
        }

        public class C6Particle : ParticleBase
        {

        }

        private void C6ParticleAdd(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get values from the input fields
                double x = Convert.ToDouble(dynamicInputs_Particle["x"].Text);
                double y = Convert.ToDouble(dynamicInputs_Particle["y"].Text);
                double z = Convert.ToDouble(dynamicInputs_Particle["z"].Text);
                double Energy = Convert.ToDouble(dynamicInputs_Particle["Energy"].Text);
                double vx = Convert.ToDouble(dynamicInputs_Particle["x̂"].Text);
                double vy = Convert.ToDouble(dynamicInputs_Particle["ŷ"].Text);
                double vz = Convert.ToDouble(dynamicInputs_Particle["ẑ"].Text);

                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");
                if (!Directory.Exists(helpersDir))
                    Directory.CreateDirectory(helpersDir);

                int fileIndex = GetNextParticleIndex(helpersDir);
                string configFilePath = System.IO.Path.Combine(helpersDir, $"ParticleConfig_{fileIndex}.json");


                // Read or initialize config
                C6ParticleConfig config;
                if (File.Exists(configFilePath))
                {
                    string existingJson = File.ReadAllText(configFilePath);
                    config = JsonConvert.DeserializeObject<C6ParticleConfig>(existingJson) ?? new C6ParticleConfig();
                }
                else
                {
                    config = new C6ParticleConfig();
                }

                // Create the new electrode
                C6Particle newParticle = new C6Particle
                {
                    Type = "C6",
                    x = x,
                    y = y,
                    z = z,
                    Energy = Energy,
                    vx = vx,
                    vy = vy,
                    vz = vz
                };

                // Add to config and save
                config.Particles.Add(newParticle);
                string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, updatedJson);

                // Also display in electrode_list as one-line JSON
                int fileIndex_for_UI = GetNextParticleIndex(helpersDir);  // Already used to name file            
                string oneLineJson = JsonConvert.SerializeObject(newParticle, Formatting.None);

                // Now create the labeled TextBlock
                TextBlock jsonText = new TextBlock
                {
                    Text = $"Particle {fileIndex}: {oneLineJson}",
                    Margin = new Thickness(4),
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    TextWrapping = TextWrapping.Wrap
                };

                particle_list.Children.Add(jsonText);

                MessageBox.Show("C6 Particle added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                                   ADDING O1 PARTICLE                                              

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */


        public class O1ParticleConfig
        {
            [JsonProperty("particle")]
            public List<O1Particle> Particles { get; set; } = new List<O1Particle>();
        }

        public class O1Particle : ParticleBase
        {

        }

        private void O1ParticleAdd(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get values from the input fields
                double x = Convert.ToDouble(dynamicInputs_Particle["x"].Text);
                double y = Convert.ToDouble(dynamicInputs_Particle["y"].Text);
                double z = Convert.ToDouble(dynamicInputs_Particle["z"].Text);
                double Energy = Convert.ToDouble(dynamicInputs_Particle["Energy"].Text);
                double vx = Convert.ToDouble(dynamicInputs_Particle["x̂"].Text);
                double vy = Convert.ToDouble(dynamicInputs_Particle["ŷ"].Text);
                double vz = Convert.ToDouble(dynamicInputs_Particle["ẑ"].Text);

                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");
                if (!Directory.Exists(helpersDir))
                    Directory.CreateDirectory(helpersDir);

                int fileIndex = GetNextParticleIndex(helpersDir);
                string configFilePath = System.IO.Path.Combine(helpersDir, $"ParticleConfig_{fileIndex}.json");


                // Read or initialize config
                O1ParticleConfig config;
                if (File.Exists(configFilePath))
                {
                    string existingJson = File.ReadAllText(configFilePath);
                    config = JsonConvert.DeserializeObject<O1ParticleConfig>(existingJson) ?? new O1ParticleConfig();
                }
                else
                {
                    config = new O1ParticleConfig();
                }

                // Create the new electrode
                O1Particle newParticle = new O1Particle
                {
                    Type = "O1",
                    x = x,
                    y = y,
                    z = z,
                    Energy = Energy,
                    vx = vx,
                    vy = vy,
                    vz = vz
                };

                // Add to config and save
                config.Particles.Add(newParticle);
                string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, updatedJson);

                // Also display in electrode_list as one-line JSON
                int fileIndex_for_UI = GetNextParticleIndex(helpersDir);  // Already used to name file            
                string oneLineJson = JsonConvert.SerializeObject(newParticle, Formatting.None);

                // Now create the labeled TextBlock
                TextBlock jsonText = new TextBlock
                {
                    Text = $"Particle {fileIndex}: {oneLineJson}",
                    Margin = new Thickness(4),
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    TextWrapping = TextWrapping.Wrap
                };

                particle_list.Children.Add(jsonText);

                MessageBox.Show("O1 Particle added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                                   ADDING O2 PARTICLE                                              

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */


        public class O2ParticleConfig
        {
            [JsonProperty("particle")]
            public List<O2Particle> Particles { get; set; } = new List<O2Particle>();
        }

        public class O2Particle : ParticleBase
        {

        }

        private void O2ParticleAdd(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get values from the input fields
                double x = Convert.ToDouble(dynamicInputs_Particle["x"].Text);
                double y = Convert.ToDouble(dynamicInputs_Particle["y"].Text);
                double z = Convert.ToDouble(dynamicInputs_Particle["z"].Text);
                double Energy = Convert.ToDouble(dynamicInputs_Particle["Energy"].Text);
                double vx = Convert.ToDouble(dynamicInputs_Particle["x̂"].Text);
                double vy = Convert.ToDouble(dynamicInputs_Particle["ŷ"].Text);
                double vz = Convert.ToDouble(dynamicInputs_Particle["ẑ"].Text);

                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");
                if (!Directory.Exists(helpersDir))
                    Directory.CreateDirectory(helpersDir);

                int fileIndex = GetNextParticleIndex(helpersDir);
                string configFilePath = System.IO.Path.Combine(helpersDir, $"ParticleConfig_{fileIndex}.json");


                // Read or initialize config
                O2ParticleConfig config;
                if (File.Exists(configFilePath))
                {
                    string existingJson = File.ReadAllText(configFilePath);
                    config = JsonConvert.DeserializeObject<O2ParticleConfig>(existingJson) ?? new O2ParticleConfig();
                }
                else
                {
                    config = new O2ParticleConfig();
                }

                // Create the new electrode
                O2Particle newParticle = new O2Particle
                {
                    Type = "O2",
                    x = x,
                    y = y,
                    z = z,
                    Energy = Energy,
                    vx = vx,
                    vy = vy,
                    vz = vz
                };

                // Add to config and save
                config.Particles.Add(newParticle);
                string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, updatedJson);

                // Also display in electrode_list as one-line JSON
                int fileIndex_for_UI = GetNextParticleIndex(helpersDir);  // Already used to name file            
                string oneLineJson = JsonConvert.SerializeObject(newParticle, Formatting.None);

                // Now create the labeled TextBlock
                TextBlock jsonText = new TextBlock
                {
                    Text = $"Particle {fileIndex}: {oneLineJson}",
                    Margin = new Thickness(4),
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    TextWrapping = TextWrapping.Wrap
                };

                particle_list.Children.Add(jsonText);

                MessageBox.Show("O2 Particle added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                                   ADDING O3 PARTICLE                                              

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */


        public class O3ParticleConfig
        {
            [JsonProperty("particle")]
            public List<O3Particle> Particles { get; set; } = new List<O3Particle>();
        }

        public class O3Particle : ParticleBase
        {

        }

        private void O3ParticleAdd(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get values from the input fields
                double x = Convert.ToDouble(dynamicInputs_Particle["x"].Text);
                double y = Convert.ToDouble(dynamicInputs_Particle["y"].Text);
                double z = Convert.ToDouble(dynamicInputs_Particle["z"].Text);
                double Energy = Convert.ToDouble(dynamicInputs_Particle["Energy"].Text);
                double vx = Convert.ToDouble(dynamicInputs_Particle["x̂"].Text);
                double vy = Convert.ToDouble(dynamicInputs_Particle["ŷ"].Text);
                double vz = Convert.ToDouble(dynamicInputs_Particle["ẑ"].Text);

                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");
                if (!Directory.Exists(helpersDir))
                    Directory.CreateDirectory(helpersDir);

                int fileIndex = GetNextParticleIndex(helpersDir);
                string configFilePath = System.IO.Path.Combine(helpersDir, $"ParticleConfig_{fileIndex}.json");


                // Read or initialize config
                O3ParticleConfig config;
                if (File.Exists(configFilePath))
                {
                    string existingJson = File.ReadAllText(configFilePath);
                    config = JsonConvert.DeserializeObject<O3ParticleConfig>(existingJson) ?? new O3ParticleConfig();
                }
                else
                {
                    config = new O3ParticleConfig();
                }

                // Create the new electrode
                O3Particle newParticle = new O3Particle
                {
                    Type = "O3",
                    x = x,
                    y = y,
                    z = z,
                    Energy = Energy,
                    vx = vx,
                    vy = vy,
                    vz = vz
                };

                // Add to config and save
                config.Particles.Add(newParticle);
                string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, updatedJson);

                // Also display in electrode_list as one-line JSON
                int fileIndex_for_UI = GetNextParticleIndex(helpersDir);  // Already used to name file            
                string oneLineJson = JsonConvert.SerializeObject(newParticle, Formatting.None);

                // Now create the labeled TextBlock
                TextBlock jsonText = new TextBlock
                {
                    Text = $"Particle {fileIndex}: {oneLineJson}",
                    Margin = new Thickness(4),
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    TextWrapping = TextWrapping.Wrap
                };

                particle_list.Children.Add(jsonText);

                MessageBox.Show("O3 Particle added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }



        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                                   ADDING O4 PARTICLE                                              

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */


        public class O4ParticleConfig
        {
            [JsonProperty("particle")]
            public List<O4Particle> Particles { get; set; } = new List<O4Particle>();
        }

        public class O4Particle : ParticleBase
        {

        }

        private void O4ParticleAdd(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get values from the input fields
                double x = Convert.ToDouble(dynamicInputs_Particle["x"].Text);
                double y = Convert.ToDouble(dynamicInputs_Particle["y"].Text);
                double z = Convert.ToDouble(dynamicInputs_Particle["z"].Text);
                double Energy = Convert.ToDouble(dynamicInputs_Particle["Energy"].Text);
                double vx = Convert.ToDouble(dynamicInputs_Particle["x̂"].Text);
                double vy = Convert.ToDouble(dynamicInputs_Particle["ŷ"].Text);
                double vz = Convert.ToDouble(dynamicInputs_Particle["ẑ"].Text);

                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");
                if (!Directory.Exists(helpersDir))
                    Directory.CreateDirectory(helpersDir);

                int fileIndex = GetNextParticleIndex(helpersDir);
                string configFilePath = System.IO.Path.Combine(helpersDir, $"ParticleConfig_{fileIndex}.json");


                // Read or initialize config
                O4ParticleConfig config;
                if (File.Exists(configFilePath))
                {
                    string existingJson = File.ReadAllText(configFilePath);
                    config = JsonConvert.DeserializeObject<O4ParticleConfig>(existingJson) ?? new O4ParticleConfig();
                }
                else
                {
                    config = new O4ParticleConfig();
                }

                // Create the new electrode
                O4Particle newParticle = new O4Particle
                {
                    Type = "O4",
                    x = x,
                    y = y,
                    z = z,
                    Energy = Energy,
                    vx = vx,
                    vy = vy,
                    vz = vz
                };

                // Add to config and save
                config.Particles.Add(newParticle);
                string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, updatedJson);

                // Also display in electrode_list as one-line JSON
                int fileIndex_for_UI = GetNextParticleIndex(helpersDir);  // Already used to name file            
                string oneLineJson = JsonConvert.SerializeObject(newParticle, Formatting.None);

                // Now create the labeled TextBlock
                TextBlock jsonText = new TextBlock
                {
                    Text = $"Particle {fileIndex}: {oneLineJson}",
                    Margin = new Thickness(4),
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    TextWrapping = TextWrapping.Wrap
                };

                particle_list.Children.Add(jsonText);

                MessageBox.Show("O4 Particle added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                                   ADDING O5 PARTICLE                                              

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */


        public class O5ParticleConfig
        {
            [JsonProperty("particle")]
            public List<O5Particle> Particles { get; set; } = new List<O5Particle>();
        }

        public class O5Particle : ParticleBase
        {

        }

        private void O5ParticleAdd(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get values from the input fields
                double x = Convert.ToDouble(dynamicInputs_Particle["x"].Text);
                double y = Convert.ToDouble(dynamicInputs_Particle["y"].Text);
                double z = Convert.ToDouble(dynamicInputs_Particle["z"].Text);
                double Energy = Convert.ToDouble(dynamicInputs_Particle["Energy"].Text);
                double vx = Convert.ToDouble(dynamicInputs_Particle["x̂"].Text);
                double vy = Convert.ToDouble(dynamicInputs_Particle["ŷ"].Text);
                double vz = Convert.ToDouble(dynamicInputs_Particle["ẑ"].Text);

                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");
                if (!Directory.Exists(helpersDir))
                    Directory.CreateDirectory(helpersDir);

                int fileIndex = GetNextParticleIndex(helpersDir);
                string configFilePath = System.IO.Path.Combine(helpersDir, $"ParticleConfig_{fileIndex}.json");


                // Read or initialize config
                O5ParticleConfig config;
                if (File.Exists(configFilePath))
                {
                    string existingJson = File.ReadAllText(configFilePath);
                    config = JsonConvert.DeserializeObject<O5ParticleConfig>(existingJson) ?? new O5ParticleConfig();
                }
                else
                {
                    config = new O5ParticleConfig();
                }

                // Create the new electrode
                O5Particle newParticle = new O5Particle
                {
                    Type = "O5",
                    x = x,
                    y = y,
                    z = z,
                    Energy = Energy,
                    vx = vx,
                    vy = vy,
                    vz = vz
                };

                // Add to config and save
                config.Particles.Add(newParticle);
                string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, updatedJson);

                // Also display in electrode_list as one-line JSON
                int fileIndex_for_UI = GetNextParticleIndex(helpersDir);  // Already used to name file            
                string oneLineJson = JsonConvert.SerializeObject(newParticle, Formatting.None);

                // Now create the labeled TextBlock
                TextBlock jsonText = new TextBlock
                {
                    Text = $"Particle {fileIndex}: {oneLineJson}",
                    Margin = new Thickness(4),
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    TextWrapping = TextWrapping.Wrap
                };

                particle_list.Children.Add(jsonText);

                MessageBox.Show("O5 Particle added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                                   ADDING O6 PARTICLE                                              

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */


        public class O6ParticleConfig
        {
            [JsonProperty("particle")]
            public List<O6Particle> Particles { get; set; } = new List<O6Particle>();
        }

        public class O6Particle : ParticleBase
        {

        }

        private void O6ParticleAdd(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get values from the input fields
                double x = Convert.ToDouble(dynamicInputs_Particle["x"].Text);
                double y = Convert.ToDouble(dynamicInputs_Particle["y"].Text);
                double z = Convert.ToDouble(dynamicInputs_Particle["z"].Text);
                double Energy = Convert.ToDouble(dynamicInputs_Particle["Energy"].Text);
                double vx = Convert.ToDouble(dynamicInputs_Particle["x̂"].Text);
                double vy = Convert.ToDouble(dynamicInputs_Particle["ŷ"].Text);
                double vz = Convert.ToDouble(dynamicInputs_Particle["ẑ"].Text);

                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");
                if (!Directory.Exists(helpersDir))
                    Directory.CreateDirectory(helpersDir);

                int fileIndex = GetNextParticleIndex(helpersDir);
                string configFilePath = System.IO.Path.Combine(helpersDir, $"ParticleConfig_{fileIndex}.json");


                // Read or initialize config
                O6ParticleConfig config;
                if (File.Exists(configFilePath))
                {
                    string existingJson = File.ReadAllText(configFilePath);
                    config = JsonConvert.DeserializeObject<O6ParticleConfig>(existingJson) ?? new O6ParticleConfig();
                }
                else
                {
                    config = new O6ParticleConfig();
                }

                // Create the new electrode
                O6Particle newParticle = new O6Particle
                {
                    Type = "O6",
                    x = x,
                    y = y,
                    z = z,
                    Energy = Energy,
                    vx = vx,
                    vy = vy,
                    vz = vz
                };

                // Add to config and save
                config.Particles.Add(newParticle);
                string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, updatedJson);

                // Also display in electrode_list as one-line JSON
                int fileIndex_for_UI = GetNextParticleIndex(helpersDir);  // Already used to name file            
                string oneLineJson = JsonConvert.SerializeObject(newParticle, Formatting.None);

                // Now create the labeled TextBlock
                TextBlock jsonText = new TextBlock
                {
                    Text = $"Particle {fileIndex}: {oneLineJson}",
                    Margin = new Thickness(4),
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    TextWrapping = TextWrapping.Wrap
                };

                particle_list.Children.Add(jsonText);

                MessageBox.Show("O6 Particle added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                                   ADDING O7 PARTICLE                                              

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */


        public class O7ParticleConfig
        {
            [JsonProperty("particle")]
            public List<O7Particle> Particles { get; set; } = new List<O7Particle>();
        }

        public class O7Particle : ParticleBase
        {

        }

        private void O7ParticleAdd(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get values from the input fields
                double x = Convert.ToDouble(dynamicInputs_Particle["x"].Text);
                double y = Convert.ToDouble(dynamicInputs_Particle["y"].Text);
                double z = Convert.ToDouble(dynamicInputs_Particle["z"].Text);
                double Energy = Convert.ToDouble(dynamicInputs_Particle["Energy"].Text);
                double vx = Convert.ToDouble(dynamicInputs_Particle["x̂"].Text);
                double vy = Convert.ToDouble(dynamicInputs_Particle["ŷ"].Text);
                double vz = Convert.ToDouble(dynamicInputs_Particle["ẑ"].Text);

                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");
                if (!Directory.Exists(helpersDir))
                    Directory.CreateDirectory(helpersDir);

                int fileIndex = GetNextParticleIndex(helpersDir);
                string configFilePath = System.IO.Path.Combine(helpersDir, $"ParticleConfig_{fileIndex}.json");


                // Read or initialize config
                O7ParticleConfig config;
                if (File.Exists(configFilePath))
                {
                    string existingJson = File.ReadAllText(configFilePath);
                    config = JsonConvert.DeserializeObject<O7ParticleConfig>(existingJson) ?? new O7ParticleConfig();
                }
                else
                {
                    config = new O7ParticleConfig();
                }

                // Create the new electrode
                O7Particle newParticle = new O7Particle
                {
                    Type = "O7",
                    x = x,
                    y = y,
                    z = z,
                    Energy = Energy,
                    vx = vx,
                    vy = vy,
                    vz = vz
                };

                // Add to config and save
                config.Particles.Add(newParticle);
                string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, updatedJson);

                // Also display in electrode_list as one-line JSON
                int fileIndex_for_UI = GetNextParticleIndex(helpersDir);  // Already used to name file            
                string oneLineJson = JsonConvert.SerializeObject(newParticle, Formatting.None);

                // Now create the labeled TextBlock
                TextBlock jsonText = new TextBlock
                {
                    Text = $"Particle {fileIndex}: {oneLineJson}",
                    Margin = new Thickness(4),
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    TextWrapping = TextWrapping.Wrap
                };

                particle_list.Children.Add(jsonText);

                MessageBox.Show("O7 Particle added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                                   ADDING O8 PARTICLE                                              

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */


        public class O8ParticleConfig
        {
            [JsonProperty("particle")]
            public List<O8Particle> Particles { get; set; } = new List<O8Particle>();
        }

        public class O8Particle : ParticleBase
        {

        }

        private void O8ParticleAdd(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get values from the input fields
                double x = Convert.ToDouble(dynamicInputs_Particle["x"].Text);
                double y = Convert.ToDouble(dynamicInputs_Particle["y"].Text);
                double z = Convert.ToDouble(dynamicInputs_Particle["z"].Text);
                double Energy = Convert.ToDouble(dynamicInputs_Particle["Energy"].Text);
                double vx = Convert.ToDouble(dynamicInputs_Particle["x̂"].Text);
                double vy = Convert.ToDouble(dynamicInputs_Particle["ŷ"].Text);
                double vz = Convert.ToDouble(dynamicInputs_Particle["ẑ"].Text);

                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");
                if (!Directory.Exists(helpersDir))
                    Directory.CreateDirectory(helpersDir);

                int fileIndex = GetNextParticleIndex(helpersDir);
                string configFilePath = System.IO.Path.Combine(helpersDir, $"ParticleConfig_{fileIndex}.json");


                // Read or initialize config
                O8ParticleConfig config;
                if (File.Exists(configFilePath))
                {
                    string existingJson = File.ReadAllText(configFilePath);
                    config = JsonConvert.DeserializeObject<O8ParticleConfig>(existingJson) ?? new O8ParticleConfig();
                }
                else
                {
                    config = new O8ParticleConfig();
                }

                // Create the new electrode
                O8Particle newParticle = new O8Particle
                {
                    Type = "O8",
                    x = x,
                    y = y,
                    z = z,
                    Energy = Energy,
                    vx = vx,
                    vy = vy,
                    vz = vz
                };

                // Add to config and save
                config.Particles.Add(newParticle);
                string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, updatedJson);

                // Also display in electrode_list as one-line JSON
                int fileIndex_for_UI = GetNextParticleIndex(helpersDir);  // Already used to name file            
                string oneLineJson = JsonConvert.SerializeObject(newParticle, Formatting.None);

                // Now create the labeled TextBlock
                TextBlock jsonText = new TextBlock
                {
                    Text = $"Particle {fileIndex}: {oneLineJson}",
                    Margin = new Thickness(4),
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    TextWrapping = TextWrapping.Wrap
                };

                particle_list.Children.Add(jsonText);

                MessageBox.Show("O8 Particle added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                                   ADDING O8 PARTICLE                                              

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */


        public class CustomParticleConfig
        {
            [JsonProperty("particle")]
            public List<CustomParticle> Particles { get; set; } = new List<CustomParticle>();
        }

        public class CustomParticle : ParticleBase
        {
            [JsonProperty("mass(in mH)")]
            public double mass { get; set; }

            [JsonProperty("charge (in qe)")]
            public double charge { get; set; }
        }


        private void CustomParticleAdd(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get values from the input fields
                double x = Convert.ToDouble(dynamicInputs_Particle["x"].Text);
                double y = Convert.ToDouble(dynamicInputs_Particle["y"].Text);
                double z = Convert.ToDouble(dynamicInputs_Particle["z"].Text);
                double Energy = Convert.ToDouble(dynamicInputs_Particle["Energy"].Text);
                double vx = Convert.ToDouble(dynamicInputs_Particle["x̂"].Text);
                double vy = Convert.ToDouble(dynamicInputs_Particle["ŷ"].Text);
                double vz = Convert.ToDouble(dynamicInputs_Particle["ẑ"].Text);
                double mass = Convert.ToDouble(dynamicInputs_Particle["mass(in mH)"].Text);
                double charge = Convert.ToDouble(dynamicInputs_Particle["charge (in qe)"].Text);

                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");
                if (!Directory.Exists(helpersDir))
                    Directory.CreateDirectory(helpersDir);

                int fileIndex = GetNextParticleIndex(helpersDir);
                string configFilePath = System.IO.Path.Combine(helpersDir, $"ParticleConfig_{fileIndex}.json");


                // Read or initialize config
                CustomParticleConfig config;
                if (File.Exists(configFilePath))
                {
                    string existingJson = File.ReadAllText(configFilePath);
                    config = JsonConvert.DeserializeObject<CustomParticleConfig>(existingJson) ?? new CustomParticleConfig();
                }
                else
                {
                    config = new CustomParticleConfig();
                }

                // Create the new electrode
                CustomParticle newParticle = new CustomParticle
                {
                    Type = "Custom",
                    x = x,
                    y = y,
                    z = z,
                    Energy = Energy,
                    vx = vx,
                    vy = vy,
                    vz = vz,
                    mass = mass,
                    charge = charge
                };

                // Add to config and save
                config.Particles.Add(newParticle);
                string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, updatedJson);

                // Also display in electrode_list as one-line JSON
                int fileIndex_for_UI = GetNextParticleIndex(helpersDir);  // Already used to name file            
                string oneLineJson = JsonConvert.SerializeObject(newParticle, Formatting.None);

                // Now create the labeled TextBlock
                TextBlock jsonText = new TextBlock
                {
                    Text = $"Particle {fileIndex}: {oneLineJson}",
                    Margin = new Thickness(4),
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    TextWrapping = TextWrapping.Wrap
                };

                particle_list.Children.Add(jsonText);

                MessageBox.Show("Custom Particle added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                     ADDING MAIN SIMULATION GEOMWETRY Lx, Ly, Lz, nx, ny, nz                                               

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */
        public class SimulationConfig
        {
            public int nx { get; set; }
            public int ny { get; set; }
            public int nz { get; set; }
            public double Lx { get; set; }
            public double Ly { get; set; }
            public double Lz { get; set; }
        }


        private void Make_Simulationbox_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve values from the textboxes
            int nxValue = Convert.ToInt32(nx.Text);  // Convert to int
            int nyValue = Convert.ToInt32(ny.Text);  // Convert to int
            int nzValue = Convert.ToInt32(nz.Text);  // Convert to int
            double LxValue = Convert.ToDouble(Lx.Text);  // Convert to double
            double LyValue = Convert.ToDouble(Ly.Text);  // Convert to double
            double LzValue = Convert.ToDouble(Lz.Text);  // Convert to double
            double BxValue = Convert.ToDouble(Bx.Text);  // Convert to double
            double ByValue = Convert.ToDouble(By.Text);  // Convert to double
            double BzValue = Convert.ToDouble(Bz.Text);  // Convert to double

            // Define the directory and file path
            string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");
            string configFilePath = System.IO.Path.Combine(helpersDir, "config.json");

            // Ensure the helpers directory exists
            if (!Directory.Exists(helpersDir))
            {
                Directory.CreateDirectory(helpersDir);
            }

            // Check if the config file exists
            if (File.Exists(configFilePath))
            {
                // Read the existing JSON file
                string existingJson = File.ReadAllText(configFilePath);

                // Deserialize the existing JSON into a dictionary (preserving the old values)
                var existingConfig = JsonConvert.DeserializeObject<Dictionary<string, object>>(existingJson);

                // Add new values to the dictionary
                existingConfig["nx"] = nxValue;
                existingConfig["ny"] = nyValue;
                existingConfig["nz"] = nzValue;
                existingConfig["Lx"] = LxValue;
                existingConfig["Ly"] = LyValue;
                existingConfig["Lz"] = LzValue;
                existingConfig["Bx"] = BxValue;
                existingConfig["By"] = ByValue;
                existingConfig["Bz"] = BzValue;

                // Serialize the updated dictionary back into JSON
                string updatedJson = JsonConvert.SerializeObject(existingConfig, Formatting.Indented);

                // Write the updated JSON back to the file
                File.WriteAllText(configFilePath, updatedJson);

                // Optionally, notify the user that the config has been updated
                MessageBox.Show("Configuration updated successfully!");
            }
            else
            {
                // If the file doesn't exist, create a new dictionary with default values
                var newConfig = new Dictionary<string, object>
        {
            { "nx", nxValue },
            { "ny", nyValue },
            { "nz", nzValue },
            { "Lx", LxValue },
            { "Ly", LyValue },
            { "Lz", LzValue },
            { "Bx", BxValue },
            { "By", ByValue },
            { "Bz", BzValue }

        };

                // Serialize the new dictionary to JSON
                string newJson = JsonConvert.SerializeObject(newConfig, Formatting.Indented);

                // Create the config file and write the JSON data to it
                File.WriteAllText(configFilePath, newJson);

                // Optionally, notify the user that the config has been created
                MessageBox.Show("Configuration created successfully!");
            }
        }





        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                     OPTIONS TO MAKE DIFFERENT ELECTRODES...                                               

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */
        private void Add_Electrode(object sender, RoutedEventArgs e)
        {
            Button clicked = sender as Button;

            geometry_inputs.Children.Clear();  // Clear old inputs

            List<string> labels = new List<string>();
            string buttonLabel = "";

            // Determine which electrode type was clicked and set up the inputs and button
            if (clicked == cylindrical)
            {
                labels = new() { "cx", "cy", "cz", "axis", "height", "radius", "potential" };
                buttonLabel = "Add Cylinder";
            }
            else if (clicked == plate)
            {
                labels = new() { "A", "B", "C", "D", "thickness", "potential" };
                buttonLabel = "Add Plate";
            }
            else if (clicked == hollow_rod)
            {
                labels = new() { "cx", "cy", "cz", "radius", "thickness", "height", "axis", "potential" };
                buttonLabel = "Add Hollow Rod";
            }
            else if (clicked == ellipsoidal)
            {
                labels = new() { "cx", "cy", "cz", "rx", "ry", "rz", "potential" };
                buttonLabel = "Add Ellipsoid";
            }
            else if (clicked == hyperboloidal)
            {
                labels = new() { "cx", "cy", "cz", "a", "b", "c", "waist", "axis", "potential" };
                buttonLabel = "Add Hyperboloid";
            }
            else if (clicked == spherical)
            {
                labels = new() { "cx", "cy", "cz", "radius", "potential" };
                buttonLabel = "Add Sphere";
            }
            else if (clicked == box)
            {
                labels = new() { "x0", "y0", "z0", "x1", "y1", "z1", "potential" };
                buttonLabel = "Add Box";
            }
            else
            {
                MessageBox.Show("Unknown electrode type.");
                return;
            }

            // Add labeled textboxes for each parameter
            foreach (var label in labels)
            {
                AddLabeledInput_Electrode(label);
            }

            // Add the corresponding "Add" button
            Button addButton = new Button
            {
                Content = buttonLabel,
                Width = Double.NaN,
                Margin = new Thickness(10, 0, 0, 0),
                FontWeight = FontWeights.Bold,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF6F6F"))
            };

            // Assign the corresponding click event handler based on the electrode type
            if (clicked == cylindrical)
                addButton.Click += CylindricalElectrodeAdd;
            else if (clicked == plate)
                addButton.Click += PlateElectrodeAdd;
            else if (clicked == hollow_rod)
                addButton.Click += HollowRodElectrodeAdd;
            else if (clicked == ellipsoidal)
                addButton.Click += EllipsoidalElectrodeAdd;
            else if (clicked == hyperboloidal)
                addButton.Click += HyperboloidalElectrodeAdd;
            else if (clicked == spherical)
                addButton.Click += SphericalElectrodeAdd;
            else if (clicked == box)
                addButton.Click += BoxElectrodeAdd;

            // Add the button to the layout
            geometry_inputs.Children.Add(addButton);
        }



        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                     Methods for adding particles in the SIM box                                                

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */



        private void Particle_Click(object sender, RoutedEventArgs e)
        {
            Button clicked = sender as Button;

            particle_inputs.Children.Clear();  // Clear old inputs

            List<string> labels = new List<string>();
            string buttonLabel = "";

            if (clicked == Proton)
            {
                labels = new() { "x", "y", "z", "Energy", "x̂", "ŷ", "ẑ" };
                buttonLabel = "Add Proton";
            }
            else if (clicked == Electron)
            {
                labels = new() { "x", "y", "z", "Energy", "x̂", "ŷ", "ẑ" };
                buttonLabel = "Add Electron";
            }
            else if (clicked == C1)
            {
                labels = new() { "x", "y", "z", "Energy", "x̂", "ŷ", "ẑ" };
                buttonLabel = "Add C1";
            }
            else if (clicked == C2)
            {
                labels = new() { "x", "y", "z", "Energy", "x̂", "ŷ", "ẑ" };
                buttonLabel = "Add C2";
            }
            else if (clicked == C3)
            {
                labels = new() { "x", "y", "z", "Energy", "x̂", "ŷ", "ẑ" };
                buttonLabel = "Add C3";
            }
            else if (clicked == C4)
            {
                labels = new() { "x", "y", "z", "Energy", "x̂", "ŷ", "ẑ" };
                buttonLabel = "Add C4";
            }
            else if (clicked == C5)
            {
                labels = new() { "x", "y", "z", "Energy", "x̂", "ŷ", "ẑ" };
                buttonLabel = "Add C5";
            }
            else if (clicked == C6)
            {
                labels = new() { "x", "y", "z", "Energy", "x̂", "ŷ", "ẑ" };
                buttonLabel = "Add C6";
            }
            else if (clicked == O1)
            {
                labels = new() { "x", "y", "z", "Energy", "x̂", "ŷ", "ẑ" };
                buttonLabel = "Add O1";
            }
            else if (clicked == O2)
            {
                labels = new() { "x", "y", "z", "Energy", "x̂", "ŷ", "ẑ" };
                buttonLabel = "Add O2";
            }
            else if (clicked == O3)
            {
                labels = new() { "x", "y", "z", "Energy", "x̂", "ŷ", "ẑ" };
                buttonLabel = "Add O3";
            }
            else if (clicked == O4)
            {
                labels = new() { "x", "y", "z", "Energy", "x̂", "ŷ", "ẑ" };
                buttonLabel = "Add O4";
            }
            else if (clicked == O5)
            {
                labels = new() { "x", "y", "z", "Energy", "x̂", "ŷ", "ẑ" };
                buttonLabel = "Add O5";
            }
            else if (clicked == O6)
            {
                labels = new() { "x", "y", "z", "Energy", "x̂", "ŷ", "ẑ" };
                buttonLabel = "Add O6";
            }
            else if (clicked == O7)
            {
                labels = new() { "x", "y", "z", "Energy", "x̂", "ŷ", "ẑ" };
                buttonLabel = "Add O7";
            }
            else if (clicked == O8)
            {
                labels = new() { "x", "y", "z", "Energy", "x̂", "ŷ", "ẑ" };
                buttonLabel = "Add O8";
            }
            else if(clicked == Custom_particle)
            {
                labels = new() { "mass(in mH)", "charge (in qe)", "x", "y", "z", "Energy", "x̂", "ŷ", "ẑ" };
                buttonLabel = "Add Particle";
            }
            else
            {
                MessageBox.Show("Unknown electrode type.");
                return;
            }

            // Add labeled textboxes for each parameter
            foreach (var label in labels)
            {
                AddLabeledInput_particle(label);
            }

            // Add the corresponding "Add" button
            Button addButton = new Button
            {
                Content = buttonLabel,
                Width = Double.NaN,
                Margin = new Thickness(10, 0, 0, 0),
                FontWeight = FontWeights.Bold,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF6F6F"))
            };

            // Assign the corresponding click event handler based on the electrode type
            if (clicked == Proton)
                addButton.Click += ProtonParticleAdd;
            else if (clicked == Electron)
                addButton.Click += ElectronParticleAdd;
            else if (clicked == C1)
                addButton.Click += C1ParticleAdd;
            else if (clicked == C2)
                addButton.Click += C2ParticleAdd;
            else if (clicked == C3)
                addButton.Click += C3ParticleAdd;
            else if (clicked == C4)
                addButton.Click += C4ParticleAdd;
            else if (clicked == C5)
                addButton.Click += C5ParticleAdd;
            else if (clicked == C6)
                addButton.Click += C6ParticleAdd;
            else if (clicked == O1)
                addButton.Click += O1ParticleAdd;
            else if (clicked == O2)
                addButton.Click += O2ParticleAdd;
            else if (clicked == O3)
                addButton.Click += O3ParticleAdd;
            else if (clicked == O4)
                addButton.Click += O4ParticleAdd;
            else if (clicked == O5)
                addButton.Click += O5ParticleAdd;
            else if (clicked == O6)
                addButton.Click += O6ParticleAdd;
            else if (clicked == O7)
                addButton.Click += O7ParticleAdd;
            else if (clicked == O8)
                addButton.Click += O8ParticleAdd;
            else if (clicked == Custom_particle)
                addButton.Click += CustomParticleAdd;

            // Add the button to the layout
            particle_inputs.Children.Add(addButton);

        }



        /*
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                                      Operational Buttons                                              

        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        */
        private void RunPythonScript(string pythonExe, string scriptPath, string workingDir)
        {
            ProcessStartInfo pyStartInfo = new ProcessStartInfo
            {
                FileName = pythonExe,
                Arguments = $"\"{scriptPath}\"",
                WorkingDirectory = workingDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            Process pyProcess = new Process
            {
                StartInfo = pyStartInfo
            };

            pyProcess.OutputDataReceived += (s, ev) =>
            {
                if (ev.Data != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        ConsoleOutputBox.AppendText(ev.Data + Environment.NewLine);
                        ConsoleOutputBox.ScrollToEnd();
                    });
                }
            };

            pyProcess.ErrorDataReceived += (s, ev) =>
            {
                if (ev.Data != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        ConsoleOutputBox.AppendText("PYTHON ERROR: " + ev.Data + Environment.NewLine);
                        ConsoleOutputBox.ScrollToEnd();
                    });
                }
            };

            try
            {
                pyProcess.Start();
                pyProcess.BeginOutputReadLine();
                pyProcess.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error running Python script:\n" + ex.Message);
            }
        }


        private void Show_Geometry_Click(object sender, RoutedEventArgs e)
        {
            // Paths
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string helperPath = System.IO.Path.Combine(basePath, "helpers");
            string exePath = System.IO.Path.Combine(helperPath, "save_geometry.exe");
            string pythonExe = @"C:\Users\mrsag\anaconda3\python.exe";
            string pythonScript = System.IO.Path.Combine(helperPath, "show_geometry.py");

            // Clear any previous output
            ConsoleOutputBox.Clear();

            if (!File.Exists(exePath))
            {
                MessageBox.Show("Executable not found:\n" + exePath);
                return;
            }
            if (!File.Exists(pythonScript))
            {
                MessageBox.Show("Python script not found:\n" + pythonScript);
                return;
            }

            // Step 1: Run the C++ executable
            ProcessStartInfo exeStartInfo = new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = helperPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            Process exeProcess = new Process
            {
                StartInfo = exeStartInfo,
                EnableRaisingEvents = true
            };

            exeProcess.OutputDataReceived += (s, ev) =>
            {
                if (ev.Data != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        ConsoleOutputBox.AppendText(ev.Data + Environment.NewLine);
                        ConsoleOutputBox.ScrollToEnd();
                    });
                }
            };

            exeProcess.ErrorDataReceived += (s, ev) =>
            {
                if (ev.Data != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        ConsoleOutputBox.AppendText("ERROR: " + ev.Data + Environment.NewLine);
                        ConsoleOutputBox.ScrollToEnd();
                    });
                }
            };

            exeProcess.Exited += (s, ev) =>
            {
                // After the .exe is done, run the Python script
                Dispatcher.Invoke(() =>
                {
                    RunPythonScript(pythonExe, pythonScript, helperPath);
                });
            };

            try
            {
                exeProcess.Start();
                exeProcess.BeginOutputReadLine();
                exeProcess.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error running show_geometry.exe:\n" + ex.Message);
            }
        }

        private void Solve_Potential_Click(object sender, RoutedEventArgs e)
        {
            // Get the absolute path to the executable, assuming it's in /helpers relative to the .exe
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string exePath = System.IO.Path.Combine(basePath, "helpers", "solve_simulation.exe");

            // Clear any previous log output
            //ConsoleOutputBox.Clear();

            if (!File.Exists(exePath))
            {
                MessageBox.Show("Executable not found:\n" + exePath);
                return;
            }

            // Set up the process
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = System.IO.Path.Combine(basePath, "helpers"), // 👈 Set this!
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            Process process = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            // Handle standard output
            process.OutputDataReceived += (s, ev) =>
            {
                if (ev.Data != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        ConsoleOutputBox.AppendText(ev.Data + Environment.NewLine);
                        ConsoleOutputBox.ScrollToEnd();
                    });
                }
            };

            // Handle error output
            process.ErrorDataReceived += (s, ev) =>
            {
                if (ev.Data != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        ConsoleOutputBox.AppendText("ERROR: " + ev.Data + Environment.NewLine);
                        ConsoleOutputBox.ScrollToEnd();
                    });
                }
            };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error running the process:\n" + ex.Message);
            }
        }

        private void Show_Potential_Click(object sender, RoutedEventArgs e)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string helperPath = System.IO.Path.Combine(basePath, "helpers");
            string pythonExe = @"C:\Users\mrsag\anaconda3\python.exe";
            string scriptPath = System.IO.Path.Combine(helperPath, "show_potential.py");

           
            if (!File.Exists(scriptPath))
            {
                MessageBox.Show("Python script not found:\n" + scriptPath);
                return;
            }

            RunPythonScript(pythonExe, scriptPath, helperPath); // ✅ reuse your existing method
        }

        private void Show_Electric_Field_Click(object sender, RoutedEventArgs e)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string helperPath = System.IO.Path.Combine(basePath, "helpers");
            string pythonExe = @"C:\Users\mrsag\anaconda3\python.exe";
            string scriptPath = System.IO.Path.Combine(helperPath, "show_electric_field.py");

            // Clear previous output
            //ConsoleOutputBox.Clear();

            if (!File.Exists(scriptPath))
            {
                MessageBox.Show("Python script not found:\n" + scriptPath);
                return;
            }

            RunPythonScript(pythonExe, scriptPath, helperPath);
        }


        private void Show_Trajectory_And_Potential_Click(object sender, RoutedEventArgs e)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string helperPath = System.IO.Path.Combine(basePath, "helpers");
            string pythonExe = @"C:\Users\mrsag\anaconda3\python.exe";
            string scriptPath = System.IO.Path.Combine(helperPath, "show_particle_trajectory_and_potential.py");

            // Clear previous output
            //ConsoleOutputBox.Clear();

            if (!File.Exists(scriptPath))
            {
                MessageBox.Show("Python script not found:\n" + scriptPath);
                return;
            }

            RunPythonScript(pythonExe, scriptPath, helperPath);
        }

        private void Show_Trajectory_And_Geometry_Click(object sender, RoutedEventArgs e)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string helperPath = System.IO.Path.Combine(basePath, "helpers");
            string pythonExe = @"C:\Users\mrsag\anaconda3\python.exe";
            string scriptPath = System.IO.Path.Combine(helperPath, "show_particle_trajectory_and_geometry.py");

            // Clear previous output
            //ConsoleOutputBox.Clear();

            if (!File.Exists(scriptPath))
            {
                MessageBox.Show("Python script not found:\n" + scriptPath);
                return;
            }

            RunPythonScript(pythonExe, scriptPath, helperPath);
        }

        private void Clear_Electrodes_Click(object sender, RoutedEventArgs e)
        {
            
            try
            {
                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");
                                
                // Delete all ElectrodeConfig_*.json files
                var electrodeFiles = Directory.GetFiles(helpersDir, "ElectrodeConfig_*.json");
                foreach (string file in electrodeFiles)
                {
                    File.Delete(file);
                }
                              
                electrode_list.Children.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while cleaning up config files: " + ex.Message);
            }

        }

        private void Clear_Particles_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");

                var particleFiles = Directory.GetFiles(helpersDir, "ParticleConfig_*.json");
                foreach (string file in particleFiles)
                {
                    File.Delete(file);
                }

                particle_list.Children.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while cleaning up config files: " + ex.Message);
            }


        }

        private void Clear_All_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");

                // Delete main config file if it exists
                string configFilePath = System.IO.Path.Combine(helpersDir, "config.json");
                if (File.Exists(configFilePath))
                    File.Delete(configFilePath);

                string PotentialFilePath = System.IO.Path.Combine(helpersDir, "potential.txt");
                if (File.Exists(PotentialFilePath))
                    File.Delete(PotentialFilePath);

                string GeometryFilePath = System.IO.Path.Combine(helpersDir, "geometry.txt");
                if (File.Exists(GeometryFilePath))
                    File.Delete(GeometryFilePath);

                var trajectoryFiles = Directory.GetFiles(helpersDir, "particle_track_*.txt");
                foreach (string file in trajectoryFiles)
                {
                    File.Delete(file);
                }

                // Delete all ElectrodeConfig_*.json files
                var electrodeFiles = Directory.GetFiles(helpersDir, "ElectrodeConfig_*.json");
                foreach (string file in electrodeFiles)
                {
                    File.Delete(file);
                }

                var particleFiles = Directory.GetFiles(helpersDir, "ParticleConfig_*.json");
                foreach (string file in particleFiles)
                {
                    File.Delete(file);
                }

                particle_list.Children.Clear();
                electrode_list.Children.Clear();
                ConsoleOutputBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while cleaning up config files: " + ex.Message);
            }

        }




        /*
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////

                                     DELETING THE .JSON FILES AT TIME OF APP CLOSE                                                

         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         //////////////////////////////////////////////////////////////////////////////////////////////////////////
         */
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            try
            {
                string helpersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "helpers");

                // Delete main config file if it exists
                string configFilePath = System.IO.Path.Combine(helpersDir, "config.json");
                if (File.Exists(configFilePath))
                    File.Delete(configFilePath);

                string PotentialFilePath = System.IO.Path.Combine(helpersDir, "potential.txt");
                if (File.Exists(PotentialFilePath))
                    File.Delete(PotentialFilePath);

                string GeometryFilePath = System.IO.Path.Combine(helpersDir, "geometry.txt");
                if (File.Exists(GeometryFilePath))
                    File.Delete(GeometryFilePath);

                var trajectoryFiles = Directory.GetFiles(helpersDir, "particle_track_*.txt");
                foreach (string file in trajectoryFiles)
                {
                    File.Delete(file);
                }

                // Delete all ElectrodeConfig_*.json files
                var electrodeFiles = Directory.GetFiles(helpersDir, "ElectrodeConfig_*.json");
                foreach (string file in electrodeFiles)
                {
                    File.Delete(file);
                }

                var particleFiles = Directory.GetFiles(helpersDir, "ParticleConfig_*.json");
                foreach (string file in particleFiles)
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while cleaning up config files: " + ex.Message);
            }
        }

    }
}