using Newtonsoft.Json;
using Python.Runtime;
using System;
using System.IO;
using System.Windows.Forms;
using vProfanity.Services;

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
             AppDBContext.Initialize_Database();
             Runtime.PythonDLL = AppConstants.PYTHON_DLL_PATH;
            PythonEngine.PythonPath = AppConstants.PYTHON_PATH + ";" + AppConstants.PYTHON_UTILS_FOLDER + ";" + AppConstants.PYTHON_THIRD_PARTY_LIBRARIES_PATH;
            PythonEngine.Initialize();
             var m_threadState = PythonEngine.BeginAllowThreads();
             Application.EnableVisualStyles();
             Application.SetCompatibleTextRenderingDefault(false);
             Application.Run(new Main());
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
