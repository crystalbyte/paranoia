#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia
// 
// Crystalbyte.Paranoia is free software: you can redistribute it and/or modify
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
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using Crystalbyte.Paranoia.Mail;
using NLog;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class EmlSchemeHandler : ISchemeHandler {
        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Implementation of ISchemeHandler

        public bool ProcessRequestAsync(IRequest request, ISchemeHandlerResponse response,
            OnRequestCompletedHandler requestCompletedCallback) {
            try {
                Task.Run(() => {
                             ComposeFileCompositionResponse(request, response);
                             requestCompletedCallback();
                         });
            }
            catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }

            return false;
        }

        #endregion

        #region Methods

        private static byte[] GetFileBytes(FileSystemInfo file) {
            return File.ReadAllBytes(file.FullName);
        }

        private static void ComposeFileCompositionResponse(IRequest request, ISchemeHandlerResponse response) {
            const string key = "path";
            var uri = new Uri(request.Url);
            var arguments = uri.PathAndQuery.ToPageArguments();
            if (!arguments.ContainsKey(key)) {
                throw new KeyNotFoundException(key);
            }

            var path = arguments[key];
            var filename = Uri.UnescapeDataString(path);

            var bytes = GetFileBytes(new FileInfo(filename));
            var reader = new MailMessageReader(bytes);

            var text = HtmlSupport.FindBestSupportedBody(reader);
            text = HtmlSupport.PrepareHtmlForInspection(text);
            text = HtmlSupport.ModifyEmbeddedParts(text, new FileInfo(path));

            var b = Encoding.UTF8.GetBytes(text);
            response.MimeType = "text/html";
            response.ContentLength = bytes.Length;
            response.ResponseStream = new MemoryStream(b);
        }

        #endregion
    }
}