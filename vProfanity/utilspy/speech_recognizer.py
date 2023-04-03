import auditok
import constants

# Get speech regions from a wav file (Note: wav file must be processed with ffmpeg_utils.get_wav)
def get_speech_regions(
        audio_wav,
        energy_threshold=constants.DEFAULT_ENERGY_THRESHOLD,
        min_region_size=constants.DEFAULT_MIN_REGION_SIZE,
        max_region_size=constants.DEFAULT_MAX_REGION_SIZE,
        max_continuous_silence=constants.DEFAULT_CONTINUOUS_SILENCE,
        mode=auditok.StreamTokenizer.STRICT_MIN_LENGTH):
    
    asource = auditok.ADSFactory.ads(
        filename=audio_wav, record=True)
    validator = auditok.AudioEnergyValidator(
        sample_width=asource.get_sample_width(),
        energy_threshold=energy_threshold)
    asource.open()
    tokenizer = auditok.StreamTokenizer(
        validator=validator,
        min_length=int(min_region_size * 100),
        max_length=int(max_region_size * 100),
        max_continuous_silence=int(max_continuous_silence * 100),
        mode=mode)

    tokens = tokenizer.tokenize(asource)
    regions = []
    for token in tokens:
        regions.append((token[1] * 10, token[2] * 10))
    asource.close()

    return regions


