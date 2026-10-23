================================================================================
README: Assets/authoring
The sample-video corpus - real recordings, in the containers this program reads
================================================================================

This is REAL VIDEO: thirty-two files derived from two phone recordings. They
exist so that a player, a sample, a demonstration or a performance measurement has
something to open that looks and sounds like the video people actually have - 4K,
portrait and landscape, sound included - rather than four seconds of a test
pattern.

Everything here is Public Domain. See the next section.


THE ORIGINALS, AND THE LICENCE THAT COVERS EVERYTHING HERE
==========================================================
    landscape_video_test_4k.mp4      3840x2160, H.264 + AAC, 30 fps, 4.03 s
    portrait_video_test_4k.mp4       stored 3840x2160 WITH ROTATION METADATA,
                                     displayed 2160x3840, H.264 + AAC, 30 fps,
                                     5.47 s

These two files were created by Jeremy Ellis on his phone, and placed by him in
the PUBLIC DOMAIN on 2026-08-29.

That declaration covers everything derived from them, so every file in MKV/,
WebM/, CodeBrix-Mode1/, CodeBrix-Mode2/ and CodeBrix-Mode3/ is Public Domain as
well. (The alpha matte two of the Mode3 files carry is generated from a formula;
it is not media of anybody's either.) Nothing in this folder is third-party media
and nothing here carries a licence obligation.

The two originals themselves are NOT carried here. Nothing in this family reads an
ISOBMFF file, so an MP4 sitting in the corpus would only ever offer a failure.
They are described because the derived files are what they are because of them.

A NOTE ON THE PORTRAIT FILE, because it is the reason it is here. It is stored
landscape - 3840 wide by 2160 tall - with a -90 degree rotation in its metadata,
which is what every phone does and what a great many players get wrong. Re-encoding
it BAKES the rotation in: the derived files have TRUE portrait pixels, taller than
they are wide, with no rotation left for anything downstream to have to apply. The
generator asserts that, and so do the tests.


MKV/ and WebM/  -  THE OFF-THE-SHELF DERIVATIVES
================================================
    landscape_4k     3840x2160        portrait_4k      2160x3840
    landscape_hd     1920x1080        portrait_hd      1080x1920
    landscape_720p   1280x720         portrait_720p    720x1280

Six files each: AV1 video (libsvtav1, 8-bit 4:2:0) with Opus audio (libopus,
48 kHz stereo), 30 frames per second, the full duration of the source.

These are deliberately ORDINARY. They are muxed the way anything on the internet
is muxed - FFmpeg's plain -f matroska and -f webm, with the muxer left to put the
cues wherever it puts them, which is at the END of the file. That is the point of
them: they are what a reader will actually be handed, and a reader that only
copes with a well-laid-out file is not finished.


CodeBrix-Mode1/  -  CODEBRIX VIDEO MODE1
===============================
    the same six files again, same resolutions, same codecs, extension .cbv

"CodeBrix Video Mode1" is a PROFILE, not a new container: a Mode1 file is a plain
WebM file that has been laid out so it can be opened and scrubbed immediately,
including over a network, and it is given the .cbv extension to say so. Any tool
that reads WebM reads these files unchanged.

What the profile guarantees, and what each guarantee buys: the video is AV1 and
the audio is Opus or Vorbis, so a player needs one royalty-free decoder pair and
no others; the seek index sits BEFORE the first cluster, so a reader that has the
first few kilobytes of the file already has the whole index and the first scrub
costs no second round trip; every element declares a known size, so nothing has to
be scanned to be skipped; the Info element states a duration, so a scrub bar can
be drawn before a single frame is decoded; timestamps ascend within every track,
so nothing has to be re-ordered; and the picture is 8-bit 4:2:0, which every
decoder and every display path handles without a conversion nobody asked for. The
cbvinfo verb of the CodeBrix.VideoPlayback command-line tooling checks all of that
and exits non-zero when a file misses any of it:

    cbvinfo Assets/authoring/CodeBrix-Mode1/landscape_hd.cbv

Run the same command against any file in MKV/ or WebM/ and it fails the "cues sit
before the first cluster" rule and passes the rest. That contrast is the whole
reason both sets are here.

Mode1 is the FIRST of the .cbv profiles. It carries plain audio and video only -
no captions, no chapters - which is what this corpus needs.


CodeBrix-Mode2/  -  CODEBRIX VIDEO MODE2
========================================
    the same six files again, same resolutions, same picture, extension .cbv

"CodeBrix Video Mode2" is the OTHER .cbv flavour, and unlike Mode1 it is not a
profile of somebody else's container: it is the BESPOKE format the
CodeBrix.VideoPlayback family defines, byte for byte, in its own published format
specification. A Mode2 file begins with
the four bytes "CBVF" rather than with EBML's signature, and a reader tells the
two apart by looking at them. No other tool reads these files; that is the trade.

What the format buys, and why it is worth a format of its own: the index is in
front of the media data BY CONSTRUCTION rather than by asking a muxer nicely, so
there is no layout to get wrong; whole caption tracks and the whole chapter table
live in the header region, so every cue and every chapter title is available the
instant the header is read, including right after a seek; chapter titles are
MULTILINGUAL, which a WebM cannot manage; and a track's codec is named by an
identifier in its own header, so a new codec rides in without a format version
bump.

The video is the same AV1 as the other three folders. THE AUDIO IS VORBIS, NOT
OPUS, and that is the point of the folder rather than a detail of it: a Vorbis
track plays with the core playback package alone, while an Opus track needs the
application to reference CodeBrix.Audio.Opus and call its Register(). Mode2 is the
flavour an application ships INSIDE itself, so its convention is the one that
needs nothing extra. The bit rates are the same numbers as the Opus rungs, so the
two ladders stay comparable.

These six files are written by the CodeBrix authoring library rather than by a
re-mux of somebody else's encoder output: two ffmpeg passes - AV1 into an IVF
wrapper and Vorbis into an Ogg stream - and then this repository's own muxer,
which writes the container. Run cbvinfo over one of them and it passes the
profile on its own three layout rules rather than on Matroska's:

    cbvinfo Assets/authoring/CodeBrix-Mode2/landscape_hd.cbv

Like Mode1, this folder carries plain audio and video only - no captions, no
chapters - which is what this corpus needs. The authoring library's own tests
exercise captions and multilingual chapters, on clips generated on the spot.


CodeBrix-Mode3/  -  CODEBRIX VIDEO MODE3, THE MASTER FORM
=========================================================
    the same six files again, same resolutions, same picture, extension .cbvmaster
    plus landscape_hd_alpha and portrait_hd_alpha: the HD picture with an alpha
    channel

"CodeBrix Video Mode3" is the bespoke container's MASTER form: the same CBVF
format as Mode2, at format version 1. A reader tells the two apart by the
version number straight after the "CBVF" magic, not by the extension - the
.cbvmaster extension is a convention and a hint. What version 1 adds: an
optional ALPHA-PLANE track beside the picture - the picture's alpha channel as
a monochrome, full-range AV1 stream of the same size, in lock step with it - and
LOSSLESS FLAC sound, which is the only sound a master carries. It is the format
to keep a finished piece of video in, rather than the one to ship inside an
application.

The six plain files are the same rungs as every other folder, with the alpha
left out: they play exactly like the Mode2 files, with sound that has lost
nothing. The two _alpha files are where the transparency is. Their picture is
the HD rung behind a soft elliptical matte - opaque inside three-quarters of
the ellipse that touches the picture's edges, fading to fully transparent at
that ellipse, and fully transparent in the four corners beyond it - so a player
shows what is behind the video there. This application draws a checkerboard
behind a transparent picture so that the transparency can be seen.

THE AUDIO IS FLAC: 16-bit, 48 kHz stereo, decoded from the recording's AAC
and then kept exactly. FLAC is built into the core playback package's audio
library, so like Mode2 these files need no audio package registered. The alpha
plane decodes through the same AV1 decoder as the picture.

These eight files are written by the CodeBrix authoring library's MASTER
flavour, which encodes IN PROCESS: the source is decoded by FFmpeg, but the AV1
picture and alpha plane are encoded by the library's own AV1 encoder package -
the libaom reference encoder, not libsvtav1 - and the sound by its own FLAC
encoder, and the library's own muxer writes the container. The rate factors and
key-frame interval are the corpus's own, rung for rung (see MANIFEST.txt); the
alpha plane gets a rate factor six points better than its picture's, because an
error at an edge shows more in the alpha than in the colour. The two _alpha
sources are made first with one FFmpeg command that scales the recording to HD
and merges the matte in as its alpha channel, losslessly (FFV1 in Matroska).

    cbvinfo Assets/authoring/CodeBrix-Mode3/landscape_hd_alpha.cbvmaster

passes the profile, with FLAC accepted as a master file's sound and nowhere
else. Like the others, this folder carries plain audio and video only - no
captions, no chapters.

UNLIKE THE OTHER FOUR FOLDERS, Mode3 is not produced by the corpus generator
described below; it was authored separately, through the same authoring
library, from the same two originals. MANIFEST.txt records it in a section of
its own, with every command line, which is the record to reproduce it from.


MANIFEST.txt
============
Written by the generator every time it runs. For each of the twenty-four files it
records the resolution, the frame rate, the duration, the size, the video and
audio codecs, the exact encoder settings used, the result of reading the finished
file back, the full streamable-profile report, and the exact ffmpeg command lines
that produced it - one for the FFmpeg-muxed folders, two for Mode2, plus what the
muxer made of them. It also records what was found in the two originals and the
encoder-settings table with the reasoning behind the numbers.

It always describes the WHOLE corpus, even after a run that rebuilt one folder:
the folders that were not re-encoded are read back and re-verified, their command
lines are re-derived from the plan, and their encode times are carried over from
the previous manifest.

Read MANIFEST.txt before changing anything here.


HOW THE CORPUS WAS PRODUCED
===========================
A generator rebuilds MKV/, WebM/, CodeBrix-Mode1/, CodeBrix-Mode2/ and
MANIFEST.txt from the two original recordings, verifies every output, and exits
non-zero if any check fails. It takes a couple of minutes on a many-core machine.

WHAT IT NEEDS. FFmpeg and FFprobe on the PATH, built with libsvtav1, libopus and
libvorbis - on a Debian-based machine, `sudo apt install ffmpeg` is enough; check
with `ffmpeg -encoders | grep -E 'libsvtav1|libopus|libvorbis'`. NOTHING ELSE:
the profile check runs in-process, and the Mode2 container is written by managed
code. The generator downloads nothing and installs nothing.

NOTHING IN THIS APPLICATION REGENERATES THE CORPUS. These thirty-two files are
shipped as data, copied to the output folder beside each head's executable, and
read from there.

WHAT IS DETERMINISTIC AND WHAT IS NOT. The SHAPE of the corpus is: the same
twenty-four files, the same resolutions, the same settings, the same command lines
every run. The BYTES are not - the Matroska muxer writes a random track UID and a
fresh muxing date into every file, so two runs differ even when the encoder made an
identical bitstream. Those fields are fixed-length, so the file sizes do tend to
come back the same; that is a convenience, not a guarantee. Do not pin these files
with a hash.


WHAT READS THIS FOLDER
======================
The application scans every sub-folder of it and offers every .mkv, .webm, .cbv
and .cbvmaster file it finds, which is all thirty-two. Three of the five folders
are read with a
Matroska reader and CodeBrix-Mode2 with the bespoke one, because a CBVF file is not
Matroska at all. The Mode2 files are the ones that prove a Vorbis soundtrack needs
no Opus decoder registered.
================================================================================
