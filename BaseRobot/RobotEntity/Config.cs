using ControlzEx.Theming;
using Newtonsoft.Json;
using MahApps.Metro.Controls;
using System.IO;
using System.Windows;

namespace BaseRobot.RobotEntity
{
    public class Config
    {
        public Config() { }

        #region =========================== Properties =========================================

        public List<ConfigRobot> ConfigRobots { get; set; } = new List<ConfigRobot>();

        public double Left { get; set; } = 50;
        public double Top { get; set; } = 50;
        public double Width { get; set; } = 800;
        public double Height { get; set; } = 600;
        public string? Theme { get; set; }

        #endregion

        #region =========================== Methods =========================================

        public void SaveConfig (MetroWindow window, Theme? theme)
        {
            Left = window.Left; 
            Top = window.Top;
            Width = window.Width; 
            Height = window.Height;

            Theme = theme?.Name ?? "";

            SaveConfig();
        }
        private void SaveConfig()
        {
            if (!Directory.Exists(@"Paraments"))
            {
                Directory.CreateDirectory(@"Paraments");
            }

            try
            {
                using (StreamWriter writer = new StreamWriter(@"Paraments\paraments.config", false))
                {
                    string json = JsonConvert.SerializeObject(this);

                    writer.WriteLine(json);

                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static Config LoadConfig()
        {
            if (!Directory.Exists(@"Paraments"))
            {
                return new Config();
            }

            try
            {
                using (StreamReader reader = new StreamReader(@"Paraments\paraments.config"))
                {
                    string? str = reader.ReadLine();

                    if (str != null)
                    {
                        Config? config = JsonConvert.DeserializeObject<Config>(str);

                        if (config != null)
                        {
                            return config;
                        }
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }

            return new Config();
        }

        #endregion


    }
}
