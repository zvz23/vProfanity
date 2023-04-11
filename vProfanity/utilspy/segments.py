def clean_segments(segments):
    """
    Cleans a list of segments by merging or removing any overlapping or nested segments.

    Args:
        segments (list): A list of tuples representing segments, where each tuple contains the start and end time in seconds.

    Returns:
        list: A new list of segments with any overlapping or nested segments merged or removed.
    """
    # Sort the segments by start time
    segments = sorted(segments)

    # Initialize the cleaned segments list with the first segment
    cleaned_segments = [segments[0]]

    # Loop through the remaining segments
    for segment in segments[1:]:
        # Get the end time of the previous segment
        prev_end = cleaned_segments[-1][1]

        # If the current segment is completely inside the previous segment, skip it
        if prev_end >= segment[1]:
            continue

        # If the current segment starts before or at the end of the previous segment, merge them
        if prev_end >= segment[0]:
            cleaned_segments[-1] = (cleaned_segments[-1][0], segment[1])
        # Otherwise, add the current segment to the cleaned segments list
        else:
            cleaned_segments.append(segment)

    return cleaned_segments

segments = [(10, 14), (8, 10)]
print(clean_segments(segments))