using System.IO;
using NAudio.Wave;
using NVorbis.NAudioSupport;
using System;

namespace Crystalbyte.Paranoia {
    public sealed class AudioPlayer : IDisposable {
        private readonly WaveOut _waveOut;
        private readonly VorbisWaveReader _reader;

        public AudioPlayer(Stream stream) {
            _reader = new VorbisWaveReader(stream);
            _waveOut = new WaveOut();
            _waveOut.Init(_reader);
        }

        public float Volume {
            get { return _waveOut.Volume; } 
            set { _waveOut.Volume = value; }
        }

        public void Play() {
            _waveOut.Play();
        }

        public void Dispose() {
            if (_waveOut != null) {
                _waveOut.Dispose();
            }

            if (_reader != null) {
                _reader.Dispose();
            }
        }
    }
}
