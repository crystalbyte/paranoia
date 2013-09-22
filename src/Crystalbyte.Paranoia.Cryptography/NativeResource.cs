using System;

namespace Crystalbyte.Paranoia.Cryptography {
    public abstract class NativeResource : IDisposable {
        private bool _disposed;

        ~NativeResource() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing) {
            if (_disposed) {
                return;
            }

            if (disposing) {
                DisposeManaged();
            }

            DisposeNative();
            _disposed = true;
        }

        protected virtual void DisposeManaged() {
            // Override
        }
        protected virtual void DisposeNative() {
            // Override
        }
    }
}
