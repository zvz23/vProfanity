import cv2
import tempfile
import uuid
import os
from image_exporter import next_second
import json
video_file = r'D:\Ziegfred\Videos\shortvideo.mp4'

def censor_video(video_file: str, censor_seconds: list, output_path: str):
# Load video
    cap = cv2.VideoCapture(video_file)
    file_name = os.path.join(output_path, f'{str(uuid.uuid4())}.mp4')
    # Define the codec and create a VideoWriter object
    fourcc = cv2.VideoWriter_fourcc(*'mp4v')
    out = cv2.VideoWriter(file_name, fourcc, cap.get(cv2.CAP_PROP_FPS), (int(cap.get(cv2.CAP_PROP_FRAME_WIDTH)), int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))))


    # Loop through all frames in the input video
    while True:
        # Read the current frame
        ret, frame = cap.read()

        # Check if we have reached the end of the video
        if not ret:
            break

        # Check if the current frame is in any of the censoring times
        milliseconds = cap.get(cv2.CAP_PROP_POS_MSEC)
        for censor_time in censor_seconds:
            if milliseconds >= censor_time['Milliseconds'] and milliseconds <= censor_time['NextSeconds']:
                x, y, w, h = 0, 0, int(cap.get(cv2.CAP_PROP_FRAME_WIDTH)), int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
                cv2.rectangle(frame, (x, y), (x + w, y + h), (0, 0, 0), -1)
                break

        # Write the current frame to the output video file
        out.write(frame)

    # Release video capture and writer objects
    cap.release()
    out.release()

    return file_name

