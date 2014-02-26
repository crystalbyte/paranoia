#region Using directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

#endregion

namespace Crystalbyte.Paranoia.Messaging {
    public sealed class ImapConnection : IDisposable {
        private readonly TcpClient _tcpClient;
        private StreamReader _reader;
        private SslStream _secureStream;
        private StreamWriter _writer;

        public ImapConnection() {
            Security = SecurityPolicy.Explicit;
            Capabilities = new HashSet<string>();
            Certificates = new X509Certificate2Collection();

            _tcpClient = new TcpClient { ReceiveTimeout = 5000, SendTimeout = 2000 };
        }

        public X509Certificate2Collection Certificates { get; private set; }
        public SecurityPolicy Security { get; set; }

        public bool IsConnected {
            get { return _tcpClient.Connected; }
        }

        public bool IsSecure {
            get { return _secureStream != null && _secureStream.IsEncrypted; }
        }

        public bool IsSigned {
            get { return _secureStream != null && _secureStream.IsSigned; }
        }

        public bool CanMove {
            get { return Capabilities.Contains("MOVE"); }
        }

        public HashSet<string> Capabilities { get; internal set; }

        #region Implementation of IDisposable

        public void Dispose() {
            if (IsConnected) {
                Disconnect();
            }
        }

        #endregion

        public event EventHandler<EncryptionProtocolNegotiatedEventArgs> EncryptionProtocolNegotiated;

        private void OnEncryptionProtocolNegotiated(SslProtocols protocol, int strength) {
            var handler = EncryptionProtocolNegotiated;
            if (handler == null)
                return;

            var e = new EncryptionProtocolNegotiatedEventArgs(protocol, strength);
            handler(this, e);
        }

        public event EventHandler<RemoteCertificateValidationFailedEventArgs> RemoteCertificateValidationFailed;

        private bool OnRemoteCertificateValidationFailed(X509Certificate cert, X509Chain chain, SslPolicyErrors error) {
            var handler = RemoteCertificateValidationFailed;
            if (handler != null) {
                var e = new RemoteCertificateValidationFailedEventArgs(cert, chain, error);
                handler(this, e);
                return !e.IsCancelled;
            }
            return false;
        }

        internal async Task<ImapResponseLine> ReadAsync() {
            var line = await _reader.ReadLineAsync();
            Debug.WriteLine(line);
            return new ImapResponseLine(line);
        }

        public async Task<ImapAuthenticator> ConnectAsync(string host, int port) {
            _tcpClient.Connect(host, port);

            var stream = _tcpClient.GetStream();
            _reader = new StreamReader(stream, Encoding.UTF8, false);
            _writer = new StreamWriter(stream) { AutoFlush = true };

            // Use implicit encryption (SSL).
            if (Security == SecurityPolicy.Implicit) {
                await NegotiateEncryptionProtocolsAsync(host);
                await RequestCapabilitiesAsync();
                return new ImapAuthenticator(this);
            }

            // Use explicit encryption (TLS).
            await RequestCapabilitiesAsync();
            if (Security == SecurityPolicy.Explicit) {
                if (Capabilities.Contains(ImapCommands.StartTls)) {
                    var response = await IssueTlsCommandAsync();
                    if (response.IsOk) {
                        await NegotiateEncryptionProtocolsAsync(host);
                        // It is suggested to update server capabilities after the initial tls negotiation
                        // since some servers may send different capabilities to authenticated clients.
                        await RequestCapabilitiesAsync();
                        return new ImapAuthenticator(this);
                    }
                }
            }

            // Fail if server supports no encryption.
            throw new ImapException("Unenycrypted connections are not supported by this agent.");
        }

        internal async Task<string> WriteCommandAsync(string command) {
            var id = SequenceIdentifier.CreateNext();
            await WriteAsync(string.Format("{0} {1}", id, command));
            Debug.WriteLine("{0} {1}", id, command);
            return id;
        }

        private async Task RequestCapabilitiesAsync() {
            await ReadAsync();
            var id = await WriteCommandAsync(ImapCommands.Capability);
            Capabilities.Clear();
            Capabilities.AddRange(await ReadCapabilitiesAsync(id));
        }

        internal async Task<HashSet<string>> ReadCapabilitiesAsync(string commandId) {
            var set = new HashSet<string>();
            while (true) {
                var line = await ReadAsync();
                if (line.TerminatesCommand(commandId)) {
                    break;
                }

                foreach (var value in line.Text.Split(' ').Where(x => x != "*" && x != ImapCommands.Capability)) {
                    set.Add(value.ToUpper());
                }
            }
            return set;
        }

        internal async Task WriteAsync(string command) {
            await _writer.WriteLineAsync(command);
        }

        private async Task<ImapResponseLine> IssueTlsCommandAsync() {
            await WriteAsync(ImapCommands.StartTls);
            return await ReadAsync();
        }

        public void Disconnect() {
            _writer.Dispose();
            _reader.Dispose();
            _tcpClient.Close();
        }

        private async Task NegotiateEncryptionProtocolsAsync(string host) {
            var stream = _tcpClient.GetStream();
            _secureStream = new SslStream(stream, false, OnRemoteCertificateValidationCallback);
            await
                _secureStream.AuthenticateAsClientAsync(host, Certificates, SslProtocols.Ssl3 | SslProtocols.Tls, true);

            _reader = new StreamReader(_secureStream, Encoding.UTF8, false);
            _writer = new StreamWriter(_secureStream) { AutoFlush = true };

            OnEncryptionProtocolNegotiated(_secureStream.SslProtocol, _secureStream.CipherStrength);
        }

        private bool OnRemoteCertificateValidationCallback(object sender, X509Certificate cert, X509Chain chain,
                                                           SslPolicyErrors error) {
            return error == SslPolicyErrors.None || OnRemoteCertificateValidationFailed(cert, chain, error);
        }

        public async Task TerminateCommandAsync(string commandId) {
            // TODO: Need watch (timeout) object. If server goes down and doesn't send termination symbol this may run for quite some time.
            // TODO: Check if socket throws timeout exception at some point, since read operates on the socket stream.
            while (true) {
                var line = await ReadAsync();
                if (line.TerminatesCommand(commandId)) {
                    break;
                }
            }
        }
    }
}