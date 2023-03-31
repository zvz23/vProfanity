import subprocess
import os
import tempfile
import constants
from uuid import uuid4

# Time conversion: seconds to milliseconds
def seconds_to_time_ms(s):
    # Split the floating-point value into its integer and fractional parts
    minutes, seconds = divmod(s, 60)
    hours, minutes = divmod(minutes, 60)
    seconds, milliseconds = divmod(seconds * 1000, 1000)
    # Return the time format as a string
    return f"{int(hours):02d}:{int(minutes):02d}:{int(seconds):02d}.{int(milliseconds):03d}"


# Converts the file into wav
def get_wav(video_file: str):
    if not os.path.exists(video_file):
        raise FileNotFoundError
    output_file = f'{tempfile.mktemp()}.wav'
    cmd = constants.FFMPEG_GET_WAV_CMD.replace('[input_file]', video_file).replace('[output_file]', output_file)
    p = subprocess.Popen(cmd, stdin=subprocess.PIPE, stderr=subprocess.PIPE)
    p.communicate()
    return output_file

# Extracts the specified region
def split_region(wav_file: str, region: tuple):
    start_ms, end_ms = region
    start = float(start_ms) / 1000.0
    end = float(end_ms) / 1000.0
    duration = end - start
    output_file = f'{tempfile.mktemp()}.wav'
    cmd = constants.FFMPEG_SPLIT_WAV_CMD.replace('[input_file]', wav_file).replace('[start]', str(start)).replace('[end]', str(duration)).replace('[output_file]', output_file)
    p = subprocess.Popen(cmd, stdin=subprocess.PIPE, stderr=subprocess.PIPE)
    p.wait()
    return output_file

# Extracts the specified regions
def split_regions(wav_file: str, regions: list):
    wav_files = []
    for region in regions:
        start_ms, end_ms = region
        start = float(start_ms)/ 1000.0
        end = float(end_ms)/ 1000.0
        duration = end - start
        output_file = f'{tempfile.mktemp()}.wav'
        cmd = constants.FFMPEG_SPLIT_WAV_CMD.replace('[input_file]', wav_file).replace('[start]', str(start)).replace('[end]', str(duration)).replace('[output_file]', output_file)
        p = subprocess.Popen(cmd, stdin=subprocess.PIPE, stderr=subprocess.PIPE)
        p.wait()
        wav_files.append((region, output_file))
    return wav_files






