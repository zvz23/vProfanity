import cv2
import os
import tempfile
import shutil
from image_compare import is_frame_similar
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
    while True:
        ret, frame = cap.read()
        if not ret:
            break

        frame_count += 1

        if frame_count % int(fps) == 0:
            milliseconds = cap.get(cv2.CAP_PROP_POS_MSEC)
            with tempfile.NamedTemporaryFile(delete=False, suffix='.jpg', dir=output_dir) as f:
                cv2.imwrite(f.name, frame)
                keyframes.append({
                    'FilePath': f.name,
                    'Milliseconds': milliseconds,
                    'Seconds': milliseconds / 1000.0,
                    'NextSeconds': (milliseconds + 1000) / 1000
                })
    return json.dumps(keyframes)

# def export_video_images_by_keyframes(video_file: str, folder_name: str):
#     # Set the path to the input video file
#     # Set the path to the output directory where the extracted frames will be saved
#     temp_dir = tempfile.gettempdir()
#     output_dir = os.path.join(temp_dir, folder_name)

#     # # Create the output directory if it doesn't already exist
#     if os.path.exists(output_dir):
#         shutil.rmtree(output_dir)

#     os.mkdir(output_dir)

#     if not os.path.exists(output_dir):
#         os.makedirs(output_dir)

#     # Create a VideoCapture object to read the input video file
#     cap = cv2.VideoCapture(video_file)
#     keyframes = []
#     # Get the frame rate of the video
#     prev_frame = None
#     while True:
#         # Read the current frame
#         ret, frame = cap.read()

#         # Check if the frame was read successfully
#         if not ret:
#             break
#         current_milli = cap.get(cv2.CAP_PROP_POS_MSEC)
#         current_milli_str = str(int(current_milli))
#         file_path = os.path.join(output_dir, f'{str(current_milli_str)}.jpg')
#         if prev_frame is None:
#             cv2.imwrite(file_path, frame)
#             keyframes.append({
#                 'FilePath': file_path,
#                 'Millisecond': current_milli
#             })
#         else:
#             is_similar = is_frame_similar(frame, prev_frame['frame'])
#             if is_similar:
#                 if current_milli <= prev_frame['next_second_milli']:
#                     continue
#                 cv2.imwrite(os.path.join(output_dir, f'{str(current_milli_str)}.jpg'), frame)
#                 keyframes.append({
#                 'FilePath': file_path,
#                 'Millisecond': current_milli
#                 })
#             else:
#                 cv2.imwrite(os.path.join(output_dir, f'{str(current_milli_str)}.jpg'), frame)
#                 keyframes.append({
#                 'FilePath': file_path,
#                 'Millisecond': current_milli
#             })


#         prev_frame = {
#             'frame': frame,
#             'milli': current_milli,
#             'next_second_milli': next_second(current_milli)[1]
#         }


#     cap.release()
#     cv2.destroyAllWindows()
#     return output_dir


