using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using vProfanity.Services;
using Python.Runtime;
using Newtonsoft.Json;

namespace vProfanity
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {


            createAppDirectories();

            if (!File.Exists(Path.Combine(AppConstants.ABS_APP_DATA_FOLDER, "config.json")))
            {
                File.Copy(Path.Combine(Environment.CurrentDirectory, "defaultConfig.json"), Path.Combine(AppConstants.ABS_APP_DATA_FOLDER, "config.json"));
            }

            AppDBContext.Initialize_Database();

            bool configResult = loadConfig();
            if (!configResult)
            {
                Environment.Exit(1);
            }
            else
            {
                Environment.SetEnvironmentVariable("PYTHONPATH", AppConstants.PYTHON_UTILS_FOLDER);
                Runtime.PythonDLL = Environment.GetEnvironmentVariable("PYTHON_DLL_PATH");
                PythonEngine.Initialize();
                var m_threadState = PythonEngine.BeginAllowThreads();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Main());
            }

        }

        private static bool loadConfig()
        {
            if (!File.Exists(AppConstants.CONFIG_PATH))
            {
                MessageBox.Show($"The program was not able to find the config file.\nPlease create a config file at {Path.GetDirectoryName(AppConstants.CONFIG_PATH)}", "Config File Not Found", MessageBoxButtons.OK);
                return false;
            }
            string rawAppConfig = File.ReadAllText(AppConstants.CONFIG_PATH);
            try
            {
                AppConfig appConfig = JsonConvert.DeserializeObject<AppConfig>(rawAppConfig);
                if ((string.IsNullOrWhiteSpace(appConfig.FFMPEG_EXECUTABLES_PATH) || !Directory.Exists(appConfig.FFMPEG_EXECUTABLES_PATH)))
                {
                    MessageBox.Show($"The FFMPEG_EXECUTABLES_PATH was not set in the config file.\nPlease add the FFMPEG_EXECUTABLES_PATH in the config file at {Path.GetDirectoryName(AppConstants.CONFIG_PATH)}", "MODEL_PATH NOT SET", MessageBoxButtons.OK);
                    return false;
                }
                if ((string.IsNullOrWhiteSpace(appConfig.MODEL_PATH) || !File.Exists(appConfig.MODEL_PATH)))
                {
                    MessageBox.Show($"The MODEL_PATH was not set in the config file.\nPlease add the MODEL_PATH in the config file at {Path.GetDirectoryName(AppConstants.CONFIG_PATH)}", "MODEL_PATH NOT SET", MessageBoxButtons.OK);
                    return false;
                }
                if ((string.IsNullOrWhiteSpace(appConfig.PYTHON_DLL_PATH) || !File.Exists(appConfig.PYTHON_DLL_PATH)))
                {
                    MessageBox.Show($"The PYTHON_DLL_PATH was not set in the config file.\nPlease add the PYTHON_DLL_PATH in the config file at {Path.GetDirectoryName(AppConstants.CONFIG_PATH)}", "PYTHON_DLL_PATH NOT SET", MessageBoxButtons.OK);
                    return false;
                }

                Environment.SetEnvironmentVariable("FFMPEG_EXECUTABLES_PATH", appConfig.FFMPEG_EXECUTABLES_PATH);
                Environment.SetEnvironmentVariable("PYTHON_DLL_PATH", appConfig.PYTHON_DLL_PATH);
                Environment.SetEnvironmentVariable("MODEL_PATH", appConfig.MODEL_PATH);

                return true;
            }
            catch (Exception)
            {
                MessageBox.Show($"The program was not able to open the config file because of bad foramtting.\nPlease fix the config file at {Path.GetDirectoryName(AppConstants.CONFIG_PATH)}", "Config File Invalid Format", MessageBoxButtons.OK);
                return false;
            }
        }

        private static void createAppDirectories()
        {
            if (!Directory.Exists(AppConstants.ABS_TEMP_FOLDER))
            {
                Directory.CreateDirectory(AppConstants.ABS_TEMP_FOLDER);
            }
            if (!Directory.Exists(AppConstants.ABS_APP_DATA_FOLDER))
            {
                Directory.CreateDirectory(AppConstants.ABS_APP_DATA_FOLDER);

            }


            if (!Directory.Exists(AppConstants.CENSORED_VIDEO_OUTPUT_FOLER))
            {
                Directory.CreateDirectory(AppConstants.CENSORED_VIDEO_OUTPUT_FOLER);
            }
        }

    }
}
