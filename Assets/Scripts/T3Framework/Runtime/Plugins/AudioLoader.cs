using System;
using NAudio.Wave;
using NLayer;
using NVorbis;
using UnityEngine;

namespace T3Framework.Runtime.Plugins
{
	public static class AudioLoader
	{
		private class AudioDataHost
		{
			private readonly float[] audioData;
			private int position = 0;
			private readonly int channels;

			public AudioDataHost(float[] audioData, int channels)
			{
				this.audioData = audioData;
				this.channels = channels;
			}

			public void PCMReaderCallback(float[] buffer)
			{
				if (position <= audioData.Length)
				{
					if (position + buffer.Length <= audioData.Length)
					{
						Array.Copy(audioData, position, buffer, 0, buffer.Length);
					}
					else
					{
						Array.Copy(audioData, position, buffer, 0, audioData.Length - position);
					}
				}

				position += buffer.Length;
			}

			public void PCMSetPositionCallback(int newPosition)
			{
				position = newPosition * channels;
			}
		}

		// This is used to load audio files that supported by NLayer like mp3
		public static AudioClip LoadMp3AudioFile(string path)
		{
			float[] audioData;
			int channels;
			int sampleRate;
			try
			{
				using MpegFile file = new MpegFile(path);
				//Note: to be simple, we do not load large file with samples count larger than int limit
				//This will be enough for most file, especially the sound effects in the skin folder
				if (file.Length > 0x7FFFFFFFL * sizeof(float))
				{
					return null;
				}

				float[] data = new float[file.Length / sizeof(float)];
				file.ReadSamples(data, 0, (int)(file.Length / sizeof(float)));
				channels = file.Channels;
				sampleRate = file.SampleRate;
				audioData = data;
			}
			catch
			{
				return null;
			}

			AudioDataHost dataHost = new AudioDataHost(audioData, channels);
			AudioClip clip = AudioClip.Create(path, audioData.Length / channels, channels, sampleRate, true,
				dataHost.PCMReaderCallback, dataHost.PCMSetPositionCallback);
			return clip;
		}

		// This is used to load audio files that supported by NAudio like wav/mp3(on windows)
		public static AudioClip LoadWavOrMp3AudioFile(string path)
		{
			float[] audioData;
			int channels;
			int sampleRate;
			try
			{
				using AudioFileReader reader = new AudioFileReader(path);
				//Note: to be simple, we do not load large file with samples count larger than int limit
				//This will be enough for most file, especially the sound effects in the skin folder
				if (reader.Length > 0x7FFFFFFFL * sizeof(float))
				{
					return null;
				}

				float[] data = new float[reader.Length / sizeof(float)];
				reader.Read(data, 0, (int)(reader.Length / sizeof(float)));
				channels = reader.WaveFormat.Channels;
				sampleRate = reader.WaveFormat.SampleRate;
				audioData = data;
			}
			catch
			{
				return null;
			}

			AudioDataHost dataHost = new AudioDataHost(audioData, channels);
			AudioClip clip = AudioClip.Create(path, audioData.Length / channels, channels, sampleRate, true,
				dataHost.PCMReaderCallback, dataHost.PCMSetPositionCallback);
			return clip;
		}

		// This is used to load audio files that supported by NVorbis like ogg
		public static AudioClip LoadOggAudioFile(string path)
		{
			float[] audioData;
			int channels;
			int sampleRate;
			try
			{
				using VorbisReader reader = new VorbisReader(path);
				//Note: Same here
				if (reader.TotalSamples * reader.Channels > 0x7FFFFFFFL)
				{
					return null;
				}

				float[] data = new float[reader.TotalSamples * reader.Channels];
				reader.ReadSamples(data, 0, (int)(reader.TotalSamples * reader.Channels));
				channels = reader.Channels;
				sampleRate = reader.SampleRate;
				audioData = data;
			}
			catch
			{
				return null;
			}

			AudioDataHost dataHost = new AudioDataHost(audioData, channels);
			AudioClip clip = AudioClip.Create(path, audioData.Length / channels, channels, sampleRate, true,
				dataHost.PCMReaderCallback, dataHost.PCMSetPositionCallback);
			return clip;
		}

		// This try all decoders
		public static AudioClip LoadAudioFile(string path)
		{
			var clip = LoadOggAudioFile(path);
			if (clip != null)
			{
				return clip;
			}

			clip = LoadWavOrMp3AudioFile(path);
			if (clip != null)
			{
				return clip;
			}

			clip = LoadMp3AudioFile(path);
			if (clip != null)
			{
				return clip;
			}

			return clip;
		}
	}
}