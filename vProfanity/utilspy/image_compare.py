import cv2


def is_frame_similar(frame1, frame2):

    # Resize the frames to the same size
    frame1 = cv2.resize(frame1, (300, 300))
    frame2 = cv2.resize(frame2, (300, 300))

    # Calculate the MSE between the two frames
    mse = ((frame1 - frame2) ** 2).mean()

    # Compare the MSE to a threshold to determine similarity
    threshold = 10 # Adjust this value as needed
    if mse < threshold:
        return True
    else:
        return False