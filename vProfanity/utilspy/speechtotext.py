import ffmpeg_utils
import speech_recognizer as sr
import speech_recognition
from pprint import pprint
import json
import requests
import os


class GoogleSpeechV2:  # pylint: disable=too-few-public-methods
    """
    Class for performing speech-to-text using Google Speech V2 API for an input FLAC file.
    """
    def __init__(self,
                 api_url,
                 headers,
                 min_confidence=0.0,
                 retries=3,
                 is_keep=False,
                 is_full_result=False):
        # pylint: disable=too-many-arguments
        self.min_confidence = min_confidence
        self.retries = retries
        self.api_url = api_url
        self.is_keep = is_keep
        self.headers = headers
        self.is_full_result = is_full_result

    def __call__(self, filename):
        try:  # pylint: disable=too-many-nested-blocks
            audio_file = open(filename, mode='rb')
            audio_data = audio_file.read()
            audio_file.close()
            if not self.is_keep:
                os.remove(filename)
            for _ in range(self.retries):
                try:
                    result = requests.post(self.api_url, data=audio_data, headers=self.headers)
                except requests.exceptions.ConnectionError:
                    continue

                # receive several results delimited by LF
                result_list = result.content.decode('utf-8').split("\n")
                # get the one with valid content
                for line in result_list:
                    try:
                        line_dict = json.loads(line)
                        transcript = get_google_speech_v2_transcript(
                            self.min_confidence,
                            line_dict)
                        if transcript:
                            # make sure it is the valid transcript
                            if not self.is_full_result:
                                return transcript
                            return line_dict

                    except (ValueError, IndexError):
                        # no result
                        continue

                # Every line of the result can't be loaded to json
                return None

        except KeyboardInterrupt:
            return None

        return None

def get_google_speech_v2_transcript(
        min_confidence,
        result_dict):
    """
    Function for getting transcript from Google Speech-to-Text V2 json format string result.
    """
    if 'result' in result_dict and result_dict['result'] \
            and 'alternative' in result_dict['result'][0] \
            and result_dict['result'][0]['alternative'] \
            and 'transcript' in result_dict['result'][0]['alternative'][0]:
        text = result_dict['result'][0]['alternative'][0]['transcript']

        if 'confidence' in result_dict['result'][0]['alternative'][0]:
            confidence = \
                float(result_dict['result'][0]['alternative'][0]['confidence'])
            if confidence > min_confidence:
                result = text[:1].upper() + text[1:]
                result = result.replace('’', '\'')
                return result
            return None

        # can't find confidence in json
        # means it's 100% confident
        result = text[:1].upper() + text[1:]
        result = result.replace('’', '\'')
        return result

    return None

# Generate transcript from wav file
def generate_transcript(audio_file):
    wav_file = ffmpeg_utils.get_wav(audio_file)
    regions = sr.get_speech_regions(wav_file)
    transcript = []
    min_confidence = 0.0
    headers = {"Content-Type": "audio/x-flac; rate=44100"}
    url = "http://www.google.com/speech-api/v2/recognize?client=chromium&lang=en-US&key=AIzaSyBOti4mM-6x9WDnZIjIeyEU21OpBXqWBgw"
    
    r = speech_recognition.Recognizer()
    for region in regions:
        output_file = ffmpeg_utils.split_region(wav_file, region)
        print(output_file)
        # wav_audio = speech_recognition.AudioFile(output_file)
        rr = GoogleSpeechV2(url, headers)
        result = rr(output_file)
        pprint(result)
            

if __name__ == '__main__':
    generate_transcript('shortvideo.mp4') 