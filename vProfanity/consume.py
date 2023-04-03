import json
import sqlite3


with open('profane.json', 'r') as json_file:
    json_data = json.load(json_file)
    with sqlite3.connect(r"C:\Users\Ziegfred\AppData\Roaming\vProfanity\vProfanity.db") as conn:
        cursor = conn.cursor()
        for word in json_data:
            cursor.execute("INSERT OR IGNORE INTO profanewords(PROFANE_WORD) VALUES(?)", (word,))
            conn.commit()
