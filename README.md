# vProfanity

<p align="center">
  <img src="images/logo.png" width="400">
</p>

###### Image Source: https://tinyurl.com/ycy9ufmv

[![License](https://img.shields.io/github/license/zvz23/vProfanity.svg?style=flat-square)](https://github.com/zvz23/vProfanity/blob/master/LICENSE)

vProfanity is a software solution that automates the detection and censorship of profanity words and sexual material in video content.

vProfanity uses machine learning models, specifically speech recognition and image classification.

**Download vProfanity [here](https://www.mediafire.com/file/apaf59qveg4yxcf/vProfanitySetup.msi/file). Please read the [installation](#installation) first after installing vProfanity on your local device.**

**vProfanity has a second repository intended for open source contributors. Click [here](https://github.com/zvz23/vProfanityOpenSource)**

## Why vProfanity was Built

In modern teaching, teachers frequently use digital multimedia files as teaching aids during classes. Digital video is one of the most frequently used teaching aids among other multimedia files because students can interact more through watching videos. Teachers access and download these videos from different video sharing platforms, which allow such content as long as it is educational, documentary, scientific, or creative, and not unreasonable. However, teachers cannot provide these videos to students because they are underage and not appropriate for them to watch. In order to avoid these content, teachers manually review and remove profane words and sexual material present in the videos, which is labor-intensive, time-consuming, tedious, and not very efficient. Automating the process would lessen the tedious task of removing profanity words and sexual material in every video.

## What Exactly is vProfanity

vProfanity was developed by Ziegfred V. Zorrilla as the main programmer of the research team, followed by the junior programmer and the team leader of the research team, John Keneth P. Paluca. It was also assisted by Kent Usman L. Pacudan, the negotiator of the research team, and lastly, Jhenelmar M. Baje, the resource manager of the research team. The vision is to help teachers review and remove inappropriate content in digital videos, specifically profane words and sexual material.

vProfanity uses two machine learning models, namely speech recognition and image classification. The speech recognition generates text from the audio detached from the source video. Each word from the generated text is checked for profane words, and if any are found, the corresponding segment containing the profane word is censored by muting it. On the other hand, the image classification model classifies each image/frame from the source video as either sexy, nude, or safe. If the image is classified as sexy or nude, it is censored by applying a black screen filter.

vProfanity was tested on 50 sample videos, which yielded a result of **83.49%** for *accuracy*, **79.51%** for *precision*, **56%** for *recall*, and **61.79%** for *f-score*.

## Overview of the Conceptual Framework

<p align="center">
  <img src="images/Conceptual Framework.png" width="1000">
</p>

## Overview of the System Architectural Design

<p align="center">
  <img src="images/System Architectural Design.png" width="1000">
</p>

## Installation

Download vProfanity [here](https://www.mediafire.com/file/apaf59qveg4yxcf/vProfanitySetup.msi/file) and install it on any computer device that has the Windows operating system installed, starting from Windows 10 up to Windows 11.

Make sure that after installing, you always run/open vProfanity as an administrator to allow administrative privileges.

## Report An Issue

If you're facing problems while using vProfanity, please report the issue immediately to the [second repository](https://github.com/zvz23/vProfanityOpenSource).

## License

vProfanity is licensed under the MIT License. See the [LICENSE](LICENSE) for more details.

## Contribute

### How to Contribute

vProfanity is an archived public repository in order to preserve its code base. To facilitate contributions, the research team create a second repository of vProfanity for open source contributers/developers. Click [here](https://github.com/zvz23/vProfanityOpenSource).

### Contributors

<a href="https://github.com/zvz23/vProfanity/graphs/contributors">
  <img src="https://contributors-img.web.app/image?repo=zvz23/vProfanity" />
</a>
