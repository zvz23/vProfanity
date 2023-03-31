import cv2

# Load video
cap = cv2.VideoCapture('shortvideo.mp4')

# Define the codec and create a VideoWriter object
fourcc = cv2.VideoWriter_fourcc(*'mp4v')
out = cv2.VideoWriter('output.mp4', fourcc, cap.get(cv2.CAP_PROP_FPS), (int(cap.get(cv2.CAP_PROP_FRAME_WIDTH)), int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))))

# Loop through all frames in the input video
for i in range(int(cap.get(cv2.CAP_PROP_FRAME_COUNT))):
    # Read the current frame
    ret, frame = cap.read()

    # Check if we have reached the end of the video
    if not ret:
        break

    # Check if the current frame is in the 10th second
    milliseconds = cap.get(cv2.CAP_PROP_POS_MSEC)
    if milliseconds >= 15000 and milliseconds < 16000:

        x, y, w, h = 0, 0, int(cap.get(cv2.CAP_PROP_FRAME_WIDTH)), int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
        cv2.rectangle(frame, (x, y), (x + w, y + h), (0, 0, 0), -1)

    # Write the current frame to the output video file
    out.write(frame)

# Release video capture and writer objects
cap.release()
out.release()
