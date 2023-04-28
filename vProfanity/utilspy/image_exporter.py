import cv2
import os
import tempfile
import shutil
import json

def next_second(millisecond):
    whole_second = int(millisecond // 1000) + 1
    millisecond_of_second = whole_second * 1000
    return whole_second, millisecond_of_second


def export_video_images_by_keyframes(video_file: str, output_dir: str):

    # # Create the output directory if it doesn't already exist
    if os.path.exists(output_dir):
        shutil.rmtree(output_dir)

    os.mkdir(output_dir)

    if not os.path.exists(output_dir):
        os.makedirs(output_dir)

    keyframes = []

    cap = cv2.VideoCapture(video_file)
    
    fps = int(cap.get(cv2.CAP_PROP_FPS))
    frame_count = 0
    last_milliseconds = 0
    while True:
        ret, frame = cap.read()
        if not ret:
            break
        
        frame_count += 1

        if frame_count % int(fps) == 0:
            milliseconds = cap.get(cv2.CAP_PROP_POS_MSEC)
            with tempfile.NamedTemporaryFile(delete=False, suffix='.jpg', dir=output_dir) as f:
                cv2.imwrite(f.name, frame)
                seconds = milliseconds / 1000.0
                temp_next_seconds = (milliseconds + 1000.0) / 1000.0
                keyframes.append({
                    'FilePath': f.name,
                    'Milliseconds': milliseconds,
                    'Seconds': seconds,
                    'NextSeconds': temp_next_seconds
                })
        last_milliseconds = cap.get(cv2.CAP_PROP_POS_MSEC)
    keyframes[-1]['NextSeconds'] = last_milliseconds / 1000
    cap.release()
    return json.dumps(keyframes)



