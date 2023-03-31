import cv2

def is_image_similar(first_image: str, second_image: str):

    # Load the two images
    img1 = cv2.imread(first_image)
    img2 = cv2.imread(second_image)

    # Resize the images to the same size
    img1 = cv2.resize(img1, (300, 300))
    img2 = cv2.resize(img2, (300, 300))

    # Calculate the MSE between the two images
    mse = ((img1 - img2) ** 2).mean()

    # Compare the MSE to a threshold to determine similarity
    threshold = 50 # Adjust this value as needed
    if mse < threshold:
        return True
    else:
        return False
