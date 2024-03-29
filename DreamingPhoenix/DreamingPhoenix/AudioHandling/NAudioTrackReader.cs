﻿using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace WizHat.DreamingPhoenix.AudioHandling
{
    [DebuggerDisplay("Current State: {State.ToString(),np} - File: {FileName}")]
    public class NAudioTrackReader : WaveStream, ISampleProvider
    {
        private WaveStream readerStream; // the waveStream which we will use for all positioning
        private readonly SampleChannel sampleChannel; // sample provider that gives us most stuff we need
        private readonly int destBytesPerSample;
        private readonly int sourceBytesPerSample;
        private readonly long length;
        private readonly object lockObject;

        public NAudioState State = NAudioState.None;
        private bool isPausing = false;
        private bool isStopping = false;
        private bool isStarting = true;
        public event EventHandler AudioStopped;
        public event EventHandler AudioPaused;
        public event EventHandler AudioStarted;

        private int fadeSamplePosition;
        private int fadeSampleCount;
        private FadeState fadeState;
        public event EventHandler FadedOut;

        enum FadeState
        {
            Silence,
            FadingIn,
            FullVolume,
            FadingOut,
        }

        /// <summary>
        /// Gets or Sets the Volume of this NAudioTrackReader. 1.0f is full volume
        /// </summary>
        public float Volume
        {
            get { return sampleChannel.Volume; }
            set { sampleChannel.Volume = value; }
        }

        /// <summary>
        /// File Name
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// WaveFormat of this stream
        /// </summary>
        public override WaveFormat WaveFormat => sampleChannel.WaveFormat;

        /// <summary>
        /// Length of this stream (in bytes)
        /// </summary>
        public override long Length => length;

        /// <summary>
        /// Position of this stream (in bytes)
        /// </summary>
        public override long Position
        {
            get { return SourceToDest(readerStream.Position); }
            set { lock (lockObject) { readerStream.Position = DestToSource(value); } }
        }
        private long oldPosition;

        /// <summary>
        /// Initializes a new instance of NAudioTrackReader
        /// </summary>
        /// <param name="fileName">The file to open</param>
        /// <param name="initiallySilent">If true, we start faded out</param>
        public NAudioTrackReader(string fileName)
        {
            lockObject = new object();
            FileName = fileName;
            CreateReaderStream(fileName);
            sourceBytesPerSample = readerStream.WaveFormat.BitsPerSample / 8 * readerStream.WaveFormat.Channels;
            sampleChannel = new SampleChannel(readerStream, true);
            destBytesPerSample = 4 * sampleChannel.WaveFormat.Channels;
            length = SourceToDest(readerStream.Length);

            State = NAudioState.Playing;
            BeginFadeIn(0);
        }

        /// <summary>
        /// Initializes a new instance of NAudioTrackReader
        /// </summary>
        /// <param name="fileName">The file to open</param>
        /// <param name="initiallySilent">If true, we start faded out</param>
        public NAudioTrackReader(string fileName, double fadeInSpeed)
        {
            lockObject = new object();
            FileName = fileName;
            CreateReaderStream(fileName);
            sourceBytesPerSample = readerStream.WaveFormat.BitsPerSample / 8 * readerStream.WaveFormat.Channels;
            sampleChannel = new SampleChannel(readerStream, true);
            destBytesPerSample = 4 * sampleChannel.WaveFormat.Channels;
            length = SourceToDest(readerStream.Length);

            State = NAudioState.Playing;
            BeginFadeIn(fadeInSpeed);
        }

        /// <summary>
        /// Creates the reader stream, supporting all filetypes in the core NAudio library,
        /// and ensuring we are in PCM format
        /// </summary>
        /// <param name="fileName">File Name</param>
        private void CreateReaderStream(string fileName)
        {
            if (fileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            {
                readerStream = new WaveFileReader(fileName);
                if (readerStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm && readerStream.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
                {
                    readerStream = WaveFormatConversionStream.CreatePcmStream(readerStream);
                    readerStream = new BlockAlignReductionStream(readerStream);
                }
            }
            else if (fileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                //string outFile = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + ".wav");
                //int outRate = 44100;
                //var reader = new Mp3FileReader(fileName);
                //if (reader.WaveFormat.SampleRate == outRate)
                //{
                //    readerStream = reader;
                //    return;
                //}

                //var outFormat = new WaveFormat(outRate, reader.WaveFormat.Channels);
                //var resampler = new MediaFoundationResampler(reader, outFormat);
                //WaveFileWriter.CreateWaveFile(outFile, resampler);

                //FileName = outFile;
                //CreateReaderStream(outFile);
                readerStream = new MediaFoundationReader(fileName); ;
            }
            else if (fileName.EndsWith(".aiff", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".aif", StringComparison.OrdinalIgnoreCase))
            {
                readerStream = new AiffFileReader(fileName);
            }
            else
            {
                // fall back to media foundation reader, see if that can play it
                readerStream = new MediaFoundationReader(fileName);
            }
        }

        /// <summary>
        /// Reads from this wave stream
        /// </summary>
        /// <param name="buffer">Audio buffer</param>
        /// <param name="offset">Offset into buffer</param>
        /// <param name="count">Number of bytes required</param>
        /// <returns>Number of bytes read</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            var waveBuffer = new WaveBuffer(buffer);
            int samplesRequired = count / 4;
            int samplesRead = Read(waveBuffer.FloatBuffer, offset / 4, samplesRequired);
            return samplesRead * 4;
        }

        /// <summary>
        /// Reads audio from this sample provider
        /// </summary>
        /// <param name="buffer">Sample buffer</param>
        /// <param name="offset">Offset into sample buffer</param>
        /// <param name="count">Number of samples required</param>
        /// <returns>Number of samples read</returns>
        public int Read(float[] buffer, int offset, int count)
        {
            lock (lockObject)
            {
                if (State == NAudioState.Paused)
                    Position = oldPosition;

                int sampleRead = sampleChannel.Read(buffer, offset, count);

                if (fadeState == FadeState.FadingIn)
                {
                    FadeIn(buffer, offset, sampleRead);
                }
                else if (fadeState == FadeState.FadingOut)
                {
                    FadeOut(buffer, offset, sampleRead);
                }
                else if (fadeState == FadeState.Silence)
                {
                    ClearBuffer(buffer, offset, count);
                }

                if (sampleRead >= count)
                    isStarting = false;
                if (sampleRead < count && !isStarting && State != NAudioState.Paused && State != NAudioState.Stopped)
                {
                    State = NAudioState.Stopped;
                    AudioStopped?.Invoke(this, EventArgs.Empty);
                    return 0;
                }

                if (State == NAudioState.Stopped)
                {
                    AudioStopped?.Invoke(this, EventArgs.Empty);
                    return 0;
                }

                return sampleRead;
            }
        }

        #region Fading
        /// <summary>
        /// Requests that a fade-in begins (will start on the next call to Read)
        /// </summary>
        /// <param name="fadeDurationInMilliseconds">Duration of fade in milliseconds</param>
        private void BeginFadeIn(double fadeDurationInMilliseconds)
        {
            lock (lockObject)
            {
                fadeSamplePosition = 0;
                fadeSampleCount = (int)(fadeDurationInMilliseconds * readerStream.WaveFormat.SampleRate / 1000);
                fadeState = FadeState.FadingIn;
            }
        }

        /// <summary>
        /// Requests that a fade-out begins (will start on the next call to Read)
        /// </summary>
        /// <param name="fadeDurationInMilliseconds">Duration of fade in milliseconds</param>
        private void BeginFadeOut(double fadeDurationInMilliseconds)
        {
            lock (lockObject)
            {
                fadeSamplePosition = 0;
                fadeSampleCount = (int)(fadeDurationInMilliseconds * readerStream.WaveFormat.SampleRate / 1000);
                fadeState = FadeState.FadingOut;
            }
        }

        private static void ClearBuffer(float[] buffer, int offset, int count)
        {
            for (int n = 0; n < count; n++)
            {
                buffer[n + offset] = 0;
            }
        }

        private void FadeOut(float[] buffer, int offset, int sourceSamplesRead)
        {
            int sample = 0;
            while (sample < sourceSamplesRead)
            {
                float multiplier = 1.0f - fadeSamplePosition / (float)fadeSampleCount;
                for (int ch = 0; ch < readerStream.WaveFormat.Channels; ch++)
                {
                    buffer[offset + sample++] *= multiplier;
                }
                fadeSamplePosition++;
                if (fadeSamplePosition > fadeSampleCount)
                {
                    FadedOut?.Invoke(this, EventArgs.Empty);
                    fadeState = FadeState.Silence;
                    // clear out the end
                    ClearBuffer(buffer, sample + offset, sourceSamplesRead - sample);

                    if (isPausing)
                    {
                        oldPosition = Position;
                        State = NAudioState.Paused;
                        AudioPaused?.Invoke(this, EventArgs.Empty);
                        isPausing = false;
                    }
                    if (isStopping)
                    {
                        State = NAudioState.Stopped;
                        //AudioStopped?.Invoke(this, EventArgs.Empty);
                        Debug.WriteLine("Hör uf Willi! 2");
                        isStopping = false;
                    }
                    break;
                }
            }
        }

        private void FadeIn(float[] buffer, int offset, int sourceSamplesRead)
        {
            int sample = 0;
            while (sample < sourceSamplesRead)
            {
                float multiplier = fadeSamplePosition / (float)fadeSampleCount;
                for (int ch = 0; ch < readerStream.WaveFormat.Channels; ch++)
                {
                    buffer[offset + sample++] *= multiplier;
                }
                fadeSamplePosition++;
                if (fadeSamplePosition > fadeSampleCount)
                {
                    fadeState = FadeState.FullVolume;
                    // no need to multiply any more
                    break;
                }
            }
        }
        #endregion

        #region Converting
        /// <summary>
        /// Helper to convert source to dest bytes
        /// </summary>
        private long SourceToDest(long sourceBytes)
        {
            return destBytesPerSample * (sourceBytes / sourceBytesPerSample);
        }

        /// <summary>
        /// Helper to convert dest to source bytes
        /// </summary>
        private long DestToSource(long destBytes)
        {
            return sourceBytesPerSample * (destBytes / destBytesPerSample);
        }
        #endregion

        #region Audio Controls
        public void Play(double fadeInSpeed = 0)
        {
            State = NAudioState.Playing;
            if (Position != 0)
                Position = oldPosition;
            BeginFadeIn(fadeInSpeed);
            AudioStarted?.Invoke(this, EventArgs.Empty);
        }

        public void Pause(double fadeOutSpeed = 0)
        {
            isPausing = true;
            BeginFadeOut(fadeOutSpeed);
            oldPosition = Position;
        }

        public void Stop(double fadeOutSpeed = 0)
        {
            isStopping = true;
            BeginFadeOut(fadeOutSpeed);
        }
        #endregion

        public void ClearFadedOutEvent()
        {
            FadedOut = null;
        }

        public void ClearAudioStoppedEvent()
        {
            AudioStopped = null;
        }

        public string Test()
        {
            return AudioStopped.Target.ToString();
        }

        /// <summary>
        /// Disposes this NAudioTrackReader
        /// </summary>
        /// <param name="disposing">True if called from Dispose</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (readerStream != null)
                {
                    readerStream.Dispose();
                    readerStream = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
