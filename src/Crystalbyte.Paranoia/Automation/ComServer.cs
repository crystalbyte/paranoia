using System;
using System.Runtime.InteropServices;
using System.Threading;
using NLog;

namespace Crystalbyte.Paranoia.Automation {
    internal sealed class ComServer : IComServer {

        #region Private Fields

        private int _objects;
        private int _serverLocks;
        private uint _comRegistryToken;
        private readonly ApplicationClassFactory _factory;
        private App _app;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public ComServer(App app) {
            _app = app;
            _factory = new ApplicationClassFactory(this);
        }

        #endregion

        #region Methods

        public void Start() {
            try {
                var classId = new Guid(Application.ClassId);
                const uint flags = RegclsMultipleUse | RegclsSuspended;

                var hResult = NativeMethods.CoRegisterClassObject(classId, _factory, ClsctxLocalServer, flags, out _comRegistryToken);
                if (hResult != 0) {
                    throw new COMException("CoRegisterClassObject failed.", hResult);
                }

                hResult = NativeMethods.CoResumeClassObjects();
                if (hResult == 0)
                    return;

                // Revoke the registration on failure.
                if (_comRegistryToken != 0) {
                    NativeMethods.CoRevokeClassObject(_comRegistryToken);
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        public void Stop() {
            try {
                NativeMethods.CoRevokeClassObject(_comRegistryToken);
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        #endregion

        #region Implementation of IComServer

        public void IncrementObjectCount() {
            Interlocked.Increment(ref _objects);
        }

        public void DecrementObjectCount() {
            Interlocked.Decrement(ref _objects);
        }

        public void IncrementServerLock() {
            Interlocked.Increment(ref _serverLocks);
        }

        public void DecrementServerLock() {
            Interlocked.Decrement(ref _serverLocks);
        }

        #endregion

        #region Native COM Support

        private const uint ClsctxInProcServer = 0x1;
        private const uint ClsctxLocalServer = 0x4;
        private const uint RegclsSingleUse = 0x0;
        private const uint RegclsMultipleUse = 0x1;
        private const uint RegclsMultiSeparate = 0x2;
        private const uint RegclsSuspended = 0x4;

        private static class NativeMethods {

            [DllImport("ole32.dll")]
            public static extern uint CoRevokeClassObject(uint dwRegister);

            [DllImport("ole32.dll")]
            public static extern int CoResumeClassObjects();


            [DllImport("ole32.dll")]
            public static extern int CoRegisterClassObject(
                [MarshalAs(UnmanagedType.LPStruct)] Guid rclsid,
                [MarshalAs(UnmanagedType.Interface)] IClassFactory pUnk, 
                uint dwClsContext, uint flags, out uint lpdwRegister);
        }

        #endregion
    }
}
