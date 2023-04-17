from ffmpeg_utils import get_wav, split_region
from speech_recognizer import get_speech_regions
import speech_recognition as sr
import json

def speech_to_text(input_file: str, temp_dir: str):
    wav_file = get_wav(input_file, temp_dir)
    regions = get_speech_regions(wav_file)
    r = sr.Recognizer()
    transcript = []
    for region in regions:
        output_file = split_region(wav_file, region, temp_dir)
        wav_audio = sr.AudioFile(output_file)
        audio = None
        with wav_audio as source:
            audio = r.record(source)
            result = r.recognize_google(audio, show_all=True, language='en-US', pfilter=0)
            if result == []:
                continue
            if 'alternative' in result:
                transcript.append({
                    'start': region[0] / 1000.0,
                    'end': region[1]/ 1000.0,
                    'text': result['alternative'][0]['transcript']
                })
            else:
                transcript.append({
                    'start': region[0] / 1000.0,
                    'end': region[1] / 1000.0,
                    'text': result[0]['transcript']
                })
    if len(transcript) > 0:
        return json.dumps(transcript)
    else:
        return None

