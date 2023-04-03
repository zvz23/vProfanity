import cv2
import os

# Load the video file
video_file = r'D:\Ziegfred\Videos\shortvideo.mp4'
cap = cv2.VideoCapture(video_file)

# Define parameters for keyframe extraction
thresh = 0.01  # threshold for frame similarity
keyframes = []  # list to store keyframes

# Initialize variables for similarity comparison
fps = int(cap.get(cv2.CAP_PROP_FPS))
frame_count = 0
while True:
    ret, frame = cap.read()

    if not ret:
        break

    frame_count += 1
    milliseconds = cap.get(cv2.CAP_PROP_POS_MSEC)
    file_name = os.path.join('images', f'{str(int(milliseconds))}.jpg')
    if frame_count % int(fps) == 0:
        cv2.imwrite(file_name, frame)