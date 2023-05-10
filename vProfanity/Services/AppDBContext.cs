using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace vProfanity.Services
{
    public class AppDBContext
    {

        public AppDBContext()
        {

        }

        public bool HasRecord(string videoHash)
        {
            using (var connection = new SQLiteConnection($"Data Source={DbConstants.ABS_DB_PATH}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"SELECT ID FROM filehistory WHERE FILE_HASH=$videohash";
                command.Parameters.AddWithValue("$videohash", videoHash);
                object result = command.ExecuteScalar();
                if (result != null)
                {
                    return true;
                }
                return false;
            }
        }

        public void SaveTranscript(string videoHash, string transcript)
        {

            using (var connection = new SQLiteConnection($"Data Source={DbConstants.ABS_DB_PATH}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                if (!HasRecord(videoHash))
                {
                    command.CommandText =
                    @"
                        INSERT INTO filehistory(FILE_HASH, TRANSCRIPT)
                        VALUES ($videohash, $transcript)
                    ";
                    command.Parameters.AddWithValue("$videohash", videoHash);
                    command.Parameters.AddWithValue("$transcript", transcript);
                    command.ExecuteNonQuery();
                }
                else
                {
                    command.CommandText =
                    @"
                        UPDATE filehistory SET TRANSCRIPT=$transcript WHERE FILE_HASH=$videohash
                    ";
                    command.Parameters.AddWithValue("$videohash", videoHash);
                    command.Parameters.AddWithValue("$transcript", transcript);
                    command.ExecuteNonQuery();
                }

            }
        }



        public void SaveFramesInfos(string videoHash, string framesInfosJson)
        {
            using (var connection = new SQLiteConnection($"Data Source={DbConstants.ABS_DB_PATH}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                if (!HasRecord(videoHash))
                {
                    command.CommandText =
                    @"
                        INSERT INTO filehistory(FILE_HASH, KEYFRAMES)
                        VALUES ($videohash, $detected)
                    ";
                    command.Parameters.AddWithValue("$videohash", videoHash);
                    command.Parameters.AddWithValue("$detected", framesInfosJson);
                    command.ExecuteNonQuery();
                }
                else
                {
                    command.CommandText =
                    @"
                        UPDATE filehistory SET KEYFRAMES=$detected WHERE FILE_HASH=$videohash
                    ";

                    command.Parameters.AddWithValue("$videohash", videoHash);
                    command.Parameters.AddWithValue("$detected", framesInfosJson);
                    command.ExecuteNonQuery();
                }

            }

            using (var connection = new SQLiteConnection($"Data Source={DbConstants.ABS_DB_PATH}"))
            {
                connection.Open();
                var command = connection.CreateCommand();

            }
        }

        public string GetTranscript(string videoHash)
        {
            string transcript = null;
            using (var connection = new SQLiteConnection($"Data Source={DbConstants.ABS_DB_PATH}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"SELECT TRANSCRIPT FROM filehistory WHERE FILE_HASH=$videohash";
                command.Parameters.AddWithValue("$videohash", videoHash);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        transcript = reader.GetString(0);
                    }

                }
            }
            return transcript;
        }

        public string GetKeyFrames(string videoHash)
        {
            using (var connection = new SQLiteConnection($"Data Source={DbConstants.ABS_DB_PATH}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"SELECT KEYFRAMES FROM filehistory WHERE FILE_HASH=$videohash";
                command.Parameters.AddWithValue("$videohash", videoHash);
                object result = command.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToString(result);

                }
                return null;
            }
        }



        public bool IsProfane(string word)
        {
            using (var connection = new SQLiteConnection($"Data Source={DbConstants.ABS_DB_PATH}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"SELECT * FROM profanewords WHERE PROFANE_WORD=$word";
                command.Parameters.AddWithValue("$word", word);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        public void AddProfane(string word)
        {
            using (var connection = new SQLiteConnection($"Data Source={DbConstants.ABS_DB_PATH}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                    @"
                        INSERT INTO profanewords(PROFANE_WORD)
                        VALUES ($word)
                    ";
                command.Parameters.AddWithValue("$word", word);
                command.ExecuteNonQuery();

            }
        }

        //Continue
        public void AddProfanes(List<string> words)
        {
            using (var connection = new SQLiteConnection($"Data Source={DbConstants.ABS_DB_PATH}"))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                        @"
                        INSERT OR IGNORE INTO profanewords(PROFANE_WORD)
                        VALUES ($word)
                    ";
                    command.Parameters.AddWithValue("$word", words);
                    command.ExecuteNonQuery();
                }


            }
        }

        public static void Initialize_Database()
        {
            string defaultDatabasePath = Path.Combine(Environment.CurrentDirectory, "defaultvProfanity.db");

            if (!File.Exists(DbConstants.ABS_DB_PATH))
            {
                File.Copy(defaultDatabasePath, DbConstants.ABS_DB_PATH);
            }

        }


    }


}
