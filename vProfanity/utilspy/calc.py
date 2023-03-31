def seconds_to_time(s):
    # Split the floating-point value into its integer and fractional parts
    minutes, seconds = divmod(s, 60)
    hours, minutes = divmod(minutes, 60)
    # Return the time format as a string
    return f"{int(hours):02d}:{int(minutes):02d}:{int(seconds):02d}"

def seconds_to_time_ms(s):
    # Split the floating-point value into its integer and fractional parts
    minutes, seconds = divmod(s, 60)
    hours, minutes = divmod(minutes, 60)
    seconds, milliseconds = divmod(seconds * 1000, 1000)
    # Return the time format as a string
    return f"{int(hours):02d}:{int(minutes):02d}:{int(seconds):02d}:{int(milliseconds):03d}"

