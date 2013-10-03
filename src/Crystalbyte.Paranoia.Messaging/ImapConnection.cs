#region Microsoft Public License (Ms-PL)

// // Microsoft Public License (Ms-PL)
// // 
// // This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.
// // 
// // 1. Definitions
// // 
// // The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
// // 
// // A "contribution" is the original software, or any additions or changes to the software.
// // 
// // A "contributor" is any person that distributes its contribution under this license.
// // 
// // "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// // 
// // 2. Grant of Rights
// // 
// // (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// // 
// // (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// // 
// // 3. Conditions and Limitations
// // 
// // (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// // 
// // (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
// // 
// // (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
// // 
// // (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
// // 
// // (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.

#endregion

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
    public sealed class ImapConnection : IDisposable {
        private readonly TcpClient _tcpClient = new TcpClient();
        private StreamReader _reader;
        private SslStream _secureStream;
        private StreamWriter _writer;

        public ImapConnection() {
            Security = SecurityPolicies.Explicit;
            Certificates = new X509Certificate2Collection();
        }

        public X509Certificate2Collection Certificates { get; private set; }
        public SecurityPolicies Security { get; set; }

        public bool IsConnected {
            get { return _tcpClient.Connected; }
        }

        public bool IsSecure {
            get { return _secureStream != null && _secureStream.IsEncrypted; }
        }

        public bool IsSigned {
            get { return _secureStream != null && _secureStream.IsSigned; }
        }

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

        internal async Task<ResponseLine> ReadAsync() {
            var line = await _reader.ReadLineAsync();
            Debug.WriteLine(line);
            return new ResponseLine(line);
        }

        public async Task<ImapAuthenticator> ConnectAsync(string host, int port) {
            _tcpClient.Connect(host, port);

            var stream = _tcpClient.GetStream();
            _reader = new StreamReader(stream, Encoding.UTF8, false);
            _writer = new StreamWriter(stream) { AutoFlush = true };

            HashSet<string> capabilities;

            // Use implicit encryption (SSL).
            if (Security == SecurityPolicies.Implicit) {
                await NegotiateEncryptionProtocolsAsync(host);
                capabilities = await RequestCapabilitiesAsync();
                return new ImapAuthenticator(capabilities, this);
            }

            // Use explicit encryption (TLS).
            capabilities = await RequestCapabilitiesAsync();
            if (Security == SecurityPolicies.Explicit) {
                if (capabilities.Contains(Commands.StartTls)) {
                    var response = await IssueTlsCommandAsync();
                    if (response.IsOk) {
                        await NegotiateEncryptionProtocolsAsync(host);
                        // It is suggested to update server capabilities after the initial tls negotiation
                        // since some servers may send different capabilities to authenticated clients.
                        capabilities = await RequestCapabilitiesAsync();
                        return new ImapAuthenticator(capabilities, this);
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

        private async Task<HashSet<string>> RequestCapabilitiesAsync() {
            await ReadAsync();
            var id = await WriteCommandAsync(Commands.Capability);
            return await ReadCapabilitiesAsync(id);
        }

        internal async Task<HashSet<string>> ReadCapabilitiesAsync(string commandId) {
            var set = new HashSet<string>();
            while (true) {
                var line = await ReadAsync();
                if (line.TerminatesCommand(commandId)) {
                    break;
                }

                foreach (var value in line.Text.Split(' ').Where(x => x != "*" && x != Commands.Capability)) {
                    set.Add(value);
                }
            }
            return set;
        }

        internal async Task WriteAsync(string command) {
            await _writer.WriteLineAsync(command);
        }

        private async Task<ResponseLine> IssueTlsCommandAsync() {
            await WriteAsync(Commands.StartTls);
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
            await _secureStream.AuthenticateAsClientAsync(host, Certificates, SslProtocols.Ssl3 | SslProtocols.Tls, true);

            _reader = new StreamReader(_secureStream, Encoding.UTF8, false);
            _writer = new StreamWriter(_secureStream) { AutoFlush = true };

            OnEncryptionProtocolNegotiated(_secureStream.SslProtocol, _secureStream.CipherStrength);
        }

        private bool OnRemoteCertificateValidationCallback(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error) {
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