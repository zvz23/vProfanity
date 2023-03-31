using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
namespace vProfanity.Services
{
    public class AppDBContext
    {

        public AppDBContext() 
        {
        
        }

        public void SaveTranscript(string videoHash, string transcript)
        {
            
            using (var connection = new SQLiteConnection($"Data Source={DbConstants.ABS_DB_PATH}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                    @"
                        INSERT INTO TranscriptHistory(videohash, transcript)
                        VALUES ($videohash, $transcript)
                    ";
                command.Parameters.AddWithValue("$videohash", videoHash);
                command.Parameters.AddWithValue("$transcript", transcript);
                command.ExecuteNonQuery();
            }
        }

        public string GetTranscript(string videoHash)
        {
            string transcript = null;
            using (var connection = new SQLiteConnection($"Data Source={DbConstants.ABS_DB_PATH}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"SELECT transcript FROM TranscriptHistory WHERE videohash=$videohash";
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

        public void UpdateTranscript(string videoHash, string newTranscript)
        {
            using (var connection = new SQLiteConnection($"Data Source={DbConstants.ABS_DB_PATH}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                    @"
                        UPDATE TranscriptHistory SET transcript=$transcript WHERE videohash=$videohash
                    ";
                command.Parameters.AddWithValue("$videohash", videoHash);
                command.Parameters.AddWithValue("$transcript", newTranscript);
                command.ExecuteNonQuery();
            }
        }

        public bool IsProfane(string word)
        {
            using (var connection = new SQLiteConnection($"Data Source={DbConstants.ABS_DB_PATH}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"SELECT * FROM ProfaneWords WHERE Word=$word";
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
                        INSERT INTO ProfaneWords(Word)
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
                        INSERT OR IGNORE INTO ProfaneWords(Word)
                        VALUES ($word)
                    ";
                    command.Parameters.AddWithValue("$word", words);
                    command.ExecuteNonQuery();
                }
                

            }
        }

        public static void Initialize_Database()
        {
            using (var connection = new SQLiteConnection($"Data Source={DbConstants.ABS_DB_PATH}"))
            {
                connection.Open();
                var transcript_command = connection.CreateCommand();
                transcript_command.CommandText = DbConstants.TRANSCRIPT_HISTORY_TABLE_QUERY;
                transcript_command.ExecuteNonQuery();

                var profane_command = connection.CreateCommand();
                profane_command.CommandText = DbConstants.PROFANE_WORDS_TABLE_QUERY;
                profane_command.ExecuteNonQuery();

            }
        }


    }

    
}
