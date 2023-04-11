using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using vProfanity.Services;
using Python.Runtime;

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
            string pythonDll = @"C:\Program Files\Python311\python311.dll";
            Runtime.PythonDLL = pythonDll;
            Environment.SetEnvironmentVariable("PYTHONPATH", AppConstants.PYTHON_UTILS_FOLDER);
            PythonEngine.Initialize();
            var m_threadState = PythonEngine.BeginAllowThreads();

            Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
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

            AppDBContext.Initialize_Database();

            Application.Run(new Main());

          
        }
    }
}
