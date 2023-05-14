import os

FFMPEG_PATH = os.path.join(os.getcwd(), 'ffmpeg', 'bin')
FFMPEG_EXE = os.path.join(FFMPEG_PATH, 'ffmpeg.exe')

FFMPEG_GET_WAV_CMD: str = FFMPEG_EXE + ' -hide_banner -y -i "[input_file]" -vn -ac 1 -ar 48000 "[output_file]"'
FFMPEG_SPLIT_WAV_CMD: str = FFMPEG_EXE + ' -y -ss [start] -i "[input_file]" -t [end] -vn -ac 2 -ar 16000 "[output_file]"'
FFMPEG_EXPORT_IMAGE_BY_SECOND_CMD = FFMPEG_EXE + ' -ss [start] -to [end] -i [input_file] -vf fps=1/1 [output_path]\[prefix]_%03d.jpg'

DEFAULT_ENERGY_THRESHOLD = 50
DEFAULT_MAX_REGION_SIZE = 10.0
DEFAULT_MIN_REGION_SIZE = 0.5
MIN_REGION_SIZE_LIMIT = 0.3
MAX_REGION_SIZE_LIMIT = 60.0
DEFAULT_CONTINUOUS_SILENCE = 0.2