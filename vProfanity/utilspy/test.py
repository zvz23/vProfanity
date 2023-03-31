from speech_recognizer import get_speech_regions
from ffmpeg_utils import split_region, get_wav, split_regions
import speech_recognition as sr
import os
import json
import mimetypes
import tempfile
from pprint import pprint

print(mimetypes.guess_type('shortvideo.mp4'))

def speech_to_text(input_file: str, output_path: str):
    wav_file = get_wav(input_file)
    regions = get_speech_regions(wav_file)
    r = sr.Recognizer()
    transcript = []
    for region in regions:
        output_file = split_region(wav_file, region)
        print(output_file)
        wav_audio = sr.AudioFile(output_file)
        audio = None
        with wav_audio as source:
            audio = r.record(source)
            result = r.recognize_google(audio, show_all=True)
            if result == []:
                continue
            if 'alternative' in result:
                transcript.append({
                    'start': region[0],
                    'end': region[1],
                    'text': result['alternative'][0]['transcript']
                })
            else:
                transcript.append({
                    'start': region[0],
                    'end': region[1],
                    'text': result[0]['transcript']
                })
    transcript_path = f'{tempfile.mktemp()}.json'
    with open(transcript_path, 'w') as f:
        json.dump)

def main():
    wav_file = get_wav('shortvideo.mp4')
    r = sr.Recognizer()
    transcript = []
    for region in regions:
        output_file = split_region(wav_file, region)
        print(output_file)
        wav_audio = sr.AudioFile(output_file)
        audio = None
        text = ''
        with wav_audio as source:
            audio = r.record(source)
            result = r.recognize_google(audio, show_all=True)
            if result == []:
                continue
            if 'alternative' in result:
                transcript.append({
                    'start': region[0],
                    'end': region[1],
                    'text': result['alternative'][0]['transcript']
                })
            else:
                transcript.append({
                    'start': region[0],
                    'end': region[1],
                    'text': result[0]['transcript']
                })

    with open('transcript.json', 'w') as f:
        json.dump(transcript, f)



