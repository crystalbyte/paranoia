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

namespace Crystalbyte.Paranoia.Messaging {
    public sealed class SmtpConnection : IDisposable {

        #region Private Fields

        private readonly TcpClient _tcpClient = new TcpClient();
        private StreamReader _reader;
        private SslStream _secureStream;
        private StreamWriter _writer;

        #endregion

        #region Construction

        public SmtpConnection() {
            Security = SecurityPolicy.Implicit;
            Certificates = new X509Certificate2Collection();
            Capabilities = new HashSet<string>();
        }

        #endregion

        #region Public Events

        public event EventHandler<EncryptionProtocolNegotiatedEventArgs> EncryptionProtocolNegotiated;

        public void OnEncryptionProtocolNegotiated(EncryptionProtocolNegotiatedEventArgs e) {
            var handler = EncryptionProtocolNegotiated;
            if (handler != null)
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

        #endregion

        #region Public Properties

        public HashSet<string> Capabilities { get; internal set; }
        public X509Certificate2Collection Certificates { get; private set; }
        public SecurityPolicy Security { get; set; }

        #endregion

        public bool IsConnected {
            get { return _tcpClient.Connected; }
        }

        public bool IsSecure {
            get { return _secureStream != null && _secureStream.IsEncrypted; }
        }

        public bool IsSigned {
            get { return _secureStream != null && _secureStream.IsSigned; }
        }

        public async Task<SmtpAuthenticator> ConnectAsync(string host, short port) {
            _tcpClient.Connect(host, port);

            var stream = _tcpClient.GetStream();
            _reader = new StreamReader(stream, Encoding.UTF8, false);
            _writer = new StreamWriter(stream) { AutoFlush = true };

            // Use implicit encryption (SSL).
            if (Security == SecurityPolicy.Implicit) {
                await NegotiateEncryptionProtocolsAsync(host);
                Capabilities = await RequestCapabilitiesAsync();
                return new SmtpAuthenticator(this);
            }

            // Use explicit encryption (TLS).
            Capabilities = await RequestCapabilitiesAsync();
            if (Security == SecurityPolicy.Explicit) {
                throw new NotImplementedException();
                if (Capabilities.Contains(ImapCommands.StartTls)) {
                    var response = await IssueTlsCommandAsync();
                    //if (response.IsOk) {
                    //    await NegotiateEncryptionProtocolsAsync(host);
                    //    // It is suggested to update server capabilities after the initial tls negotiation
                    //    // since some servers may send different capabilities to authenticated clients.
                    //    Capabilities = await RequestCapabilitiesAsync();
                    //    return new SmtpAuthenticator();
                    //}
                }
            }

            // Fail if server supports no encryption.
            throw new ImapException("Unenycrypted connections are not supported by this agent.");
        }

        private async Task<object> IssueTlsCommandAsync() {
            throw new NotImplementedException();
        }

        internal async Task<SmtpResponseLine> ReadAsync() {
            var line = await _reader.ReadLineAsync();
            Debug.WriteLine(line);
            return new SmtpResponseLine(line);
        }

        internal async Task WriteAsync(string command) {
            Debug.WriteLine(command);
            await _writer.WriteLineAsync(command);
        }

        private async Task<HashSet<string>> RequestCapabilitiesAsync() {
            while (true) {
                var line = await ReadAsync();
                if (line.IsTerminated) {
                    break;
                }
            }

            await WriteAsync(string.Format("{0} {1}", SmtpCommands.Ehlo, Environment.MachineName));
            var response = await ReadAsync();
            if (response.IsError) {
                // Fallback to HELO command, EHLO probably not supported.
                await WriteAsync(string.Format("{0} {1}", SmtpCommands.Helo, Environment.MachineName));
            }

            return await ReadCapabilitiesAsync();
        }

        private async Task<HashSet<string>> ReadCapabilitiesAsync() {
            var set = new HashSet<string>();
            while (true) {
                var line = await ReadAsync();
                if (line.IsOk) {
                    set.Add(line.Content);
                }
                if (line.IsTerminated) {
                    break;
                }
            }
            return set;
        }

        private async Task NegotiateEncryptionProtocolsAsync(string host) {
            var stream = _tcpClient.GetStream();
            _secureStream = new SslStream(stream, false, OnRemoteCertificateValidationCallback);
            await _secureStream.AuthenticateAsClientAsync(host, Certificates, SslProtocols.Ssl3 | SslProtocols.Tls, true);

            _reader = new StreamReader(_secureStream, Encoding.UTF8, false);
            _writer = new StreamWriter(_secureStream) { AutoFlush = true };

            OnEncryptionProtocolNegotiated(_secureStream.SslProtocol, _secureStream.CipherStrength);
        }

        private void OnEncryptionProtocolNegotiated(SslProtocols protocol, int strength) {

        }

        private bool OnRemoteCertificateValidationCallback(object sender, X509Certificate cert, X509Chain chain,
                                                           SslPolicyErrors error) {
            return error == SslPolicyErrors.None || OnRemoteCertificateValidationFailed(cert, chain, error);
        }

        public void Dispose() {
            throw new NotImplementedException();
        }
    }
}
