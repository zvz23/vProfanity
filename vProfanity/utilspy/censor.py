from tracemalloc import start
from moviepy.editor import *
from uuid import uuid4
import os

def censor(video_file: str, segments: dict, output_path: str):
    output_file = os.path.join(output_path, f'{str(uuid4())}.mp4')
    source_video = VideoFileClip(video_file)
    video_segments = segments['video']
    audio_segments = segments['audio']

    edited_video_clips = []
    last_video_end_time = 0

    ended = False

    for segment in video_segments:
        start_time, end_time = segment
        edited_video_clips.append(source_video.subclip(last_video_end_time, start_time))
        if end_time < source_video.duration:
            black_clip = ColorClip(size=source_video.size, color=(0, 0, 0), duration=end_time - start_time)
            edited_video_clips.append(black_clip)
            last_video_end_time = end_time
        else:
            black_clip = ColorClip(size=source_video.size, color=(0, 0, 0), duration=source_video.duration - start_time)
            edited_video_clips.append(black_clip)
            ended = True
            break

    if not ended:
        edited_video_clips.append(source_video.subclip(last_video_end_time))

    censored_sexual_video = concatenate_videoclips(edited_video_clips)

    edited_video_clips = []
    last_video_end_time = 0

    for segment in audio_segments:
        start_time, end_time = segment
        edited_video_clips.append(censored_sexual_video.subclip(last_video_end_time, start_time))
        no_audio_clip = censored_sexual_video.subclip(start_time, end_time).without_audio()
        edited_video_clips.append(no_audio_clip)
        last_video_end_time = end_time

    edited_video_clips.append(censored_sexual_video.subclip(last_video_end_time))

    censored_profanity_video = concatenate_videoclips(edited_video_clips)

    
    censored_profanity_video.write_videofile(output_file, logger=None, threads=8)

    source_video.close()

    return output_file

