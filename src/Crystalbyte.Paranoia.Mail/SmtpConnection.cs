#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia.Mail
// 
// Crystalbyte.Paranoia.Mail is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License.
// 
// Foobar is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Using Directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Mail.Properties;

#endregion

namespace Crystalbyte.Paranoia.Mail {
    public sealed class SmtpConnection : IDisposable {
        #region Private Fields

        private readonly TcpClient _tcpClient = new TcpClient();
        private StreamReader _reader;
        private SslStream _secureStream;
        private StreamWriter _writer;

        #endregion

        #region Construction

        public SmtpConnection() {
            Security = SecurityProtocol.Implicit;
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
                return !e.IsCanceled;
            }
            return false;
        }

        #endregion

        #region Public Properties

        public HashSet<string> Capabilities { get; internal set; }
        public X509Certificate2Collection Certificates { get; private set; }
        public SecurityProtocol Security { get; set; }

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
            _writer = new StreamWriter(stream) {AutoFlush = true};

            // Use implicit encryption (SSL).
            if (Security == SecurityProtocol.Implicit) {
                await NegotiateEncryptionProtocolsAsync(host);
                await RequestCapabilitiesAsync();
                return new SmtpAuthenticator(this);
            }

            // Use explicit encryption (TLS).
            await RequestCapabilitiesAsync();
            if (Security == SecurityProtocol.Explicit) {
                if (Capabilities.Contains(SmtpCommands.StartTls)) {
                    await WriteAsync(SmtpCommands.StartTls);
                    var response = await ReadAsync();
                    //server returns "2.0.0 SMTP server ready" by success
                    if (response.IsOk | response.IsServiceReady) {
                        await NegotiateEncryptionProtocolsAsync(host);
                        await RequestCapabilitiesAsync();
                        return new SmtpAuthenticator(this);
                    }
                    throw new SmtpException(response.Content);
                }
            }

            // Fail if server supports no encryption.
            throw new SmtpException(Resources.NoEncryptionNotSupported);
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

        internal async Task<List<SmtpResponseLine>> ReadToEndAsync() {
            var lines = new List<SmtpResponseLine>();
            while (true) {
                var line = await ReadAsync();
                lines.Add(line);
                if (line.IsTerminated) {
                    break;
                }
            }

            return lines;
        }

        private async Task RequestCapabilitiesAsync() {
            //TODO why read to end ??? timeout caused by it
            //await ReadToEndAsync();
            await WriteAsync(string.Format("{0} {1}", SmtpCommands.Ehlo, Environment.MachineName));
            var response = await ReadAsync();
            if (response.IsError) {
                // Fallback to HELO command, EHLO probably not supported.
                await WriteAsync(string.Format("{0} {1}", SmtpCommands.Helo, Environment.MachineName));
            }

            Capabilities.Clear();
            Capabilities.AddRange(await ReadCapabilitiesAsync());
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
            await
                _secureStream.AuthenticateAsClientAsync(host, Certificates, SslProtocols.Ssl3 | SslProtocols.Tls, true);

            _reader = new StreamReader(_secureStream, Encoding.UTF8, false);
            _writer = new StreamWriter(_secureStream) {AutoFlush = true};

            OnEncryptionProtocolNegotiated(_secureStream.SslProtocol, _secureStream.CipherStrength);
        }

        private void OnEncryptionProtocolNegotiated(SslProtocols protocol, int strength) {
            var handler = EncryptionProtocolNegotiated;
            if (handler == null)
                return;

            var e = new EncryptionProtocolNegotiatedEventArgs(protocol, strength);
            handler(this, e);
        }

        private bool OnRemoteCertificateValidationCallback(object sender, X509Certificate cert, X509Chain chain,
            SslPolicyErrors error) {
            return error == SslPolicyErrors.None
                   || (ServicePointManager.ServerCertificateValidationCallback != null
                       && ServicePointManager.ServerCertificateValidationCallback(sender, cert, chain, error))
                   || OnRemoteCertificateValidationFailed(cert, chain, error);
        }

        public void Dispose() {
            _tcpClient.Close();
        }
    }
}