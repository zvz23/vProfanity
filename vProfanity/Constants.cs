using System;
using System.IO;

namespace vProfanity
{
    public static class AppConstants
    {
        public static readonly string TEMP_FOLDER_NAME = "vProfanity";
        public static readonly string APP_DATA_FOLDER_NAME = "vProfanity";
        public static readonly string ABS_TEMP_FOLDER = Path.Combine(Path.GetTempPath(), TEMP_FOLDER_NAME);
        public static readonly string ABS_APP_DATA_FOLDER = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), APP_DATA_FOLDER_NAME);
        public static readonly string FFMPEG_FOLDER = Path.Combine(Environment.CurrentDirectory, "ffmpeg");
        public static readonly string FFMPEG_EXE = Path.Combine(FFMPEG_FOLDER, "bin");
        public static readonly string PYTHON_UTILS_FOLDER = Path.Combine(Environment.CurrentDirectory, "utilspy");
        public static readonly string CONFIG_PATH = Path.Combine(ABS_APP_DATA_FOLDER, "config.json");
        public static readonly string CENSORED_VIDEO_OUTPUT_FOLER = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "vProfanity");



    }

    public static class DbConstants
    {
        public static readonly string DB_NAME = "vProfanity.db";
        public static readonly string ABS_DB_PATH = Path.Combine(AppConstants.ABS_APP_DATA_FOLDER, DB_NAME);
        public const string FILE_HISTORY_TABLE_QUERY =
            @"
            CREATE TABLE IF NOT EXISTS filehistory(
    ID                        INTEGER PRIMARY KEY AUTOINCREMENT
                                      NOT NULL,
    FILE_HASH                 TEXT    NOT NULL
                                      UNIQUE,
    KEYFRAMES TEXT,
    TRANSCRIPT  TEXT,
);

            ";

        public const string PROFANE_WORDS_TABLE_QUERY =
            @"
           CREATE TABLE IF NOT EXISTS profanewords (
    ID           INTEGER PRIMARY KEY AUTOINCREMENT,
    PROFANE_WORD TEXT    NOT NULL
                         UNIQUE
);

            ";
    }
}
