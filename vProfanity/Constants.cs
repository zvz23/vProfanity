using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vProfanity
{
    public static class AppConstants
    {
        public static readonly string TEMP_FOLDER_NAME = "WordSearcher";
        public static readonly string APP_DATA_FOLDER_NAME = "WordSearcher";
        public static readonly string ABS_TEMP_FOLDER = Path.Combine(Path.GetTempPath(), TEMP_FOLDER_NAME);
        public static readonly string ABS_APP_DATA_FOLDER = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), APP_DATA_FOLDER_NAME);
        public static readonly string AUTOSUB_FOLDER = Path.Combine(ABS_APP_DATA_FOLDER, "autosub");
        public static readonly string FFMPEG_FOLDER = Path.Combine(ABS_APP_DATA_FOLDER, "ffmpeg");
        public static readonly string FFMPEG_EXE = Path.Combine(FFMPEG_FOLDER, "bin");

    }

    public static class DbConstants
    {
        public static readonly string DB_NAME = "AppDB.db";
        public static readonly string ABS_DB_PATH = Path.Combine(AppConstants.ABS_APP_DATA_FOLDER, DB_NAME);
        public const string TRANSCRIPT_HISTORY_TABLE_QUERY =
            @"
            CREATE TABLE IF NOT EXISTS TranscriptHistory (
	            TranscriptHistoryId	INTEGER NOT NULL,
	            videohash	TEXT,
	            transcript	TEXT,
	            PRIMARY KEY(TranscriptHistoryId AUTOINCREMENT)
            )
            ";

        public const string PROFANE_WORDS_TABLE_QUERY =
            @"
            CREATE TABLE IF NOT EXISTS ProfaneWords (
	            ProfaneWordId	INTEGER NOT NULL,
	            Word	TEXT UNIQUE,
	            PRIMARY KEY(ProfaneWordId AUTOINCREMENT)
            )
            ";
    }
}
