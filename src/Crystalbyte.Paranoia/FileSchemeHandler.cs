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
using System.IO;
using System.Net;
using System.Threading.Tasks;
using CefSharp;
using NLog;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class FileSchemeHandler : ISchemeHandler {
        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Implementation of ISchemeHandler

        public bool ProcessRequestAsync(IRequest request, ISchemeHandlerResponse response,
            OnRequestCompletedHandler requestCompletedCallback) {
            Task.Run(() => {
                         try {
                             ComposeByteResponse(request, response);
                             requestCompletedCallback();
                         }
                         catch (Exception ex) {
                             Logger.Error(ex);
                         }
                     });

            return false;
        }

        private static void ComposeByteResponse(IRequest request, ISchemeHandlerResponse response) {
            var path = request.Url.Replace("file:///", string.Empty);

            var info = new FileInfo(path);
            if (!info.Exists) {
                response.StatusCode = 404;
                return;
            }

            if (info.Extension.EqualsIgnoreCase(".png")) {
                response.MimeType = "image/png";
                goto next;
            }

            if (info.Extension.EqualsIgnoreCase(".jpg")) {
                response.MimeType = "image/jpg";
                goto next;
            }

            if (info.Extension.EqualsIgnoreCase(".jpeg")) {
                response.MimeType = "image/jpg";
                goto next;
            }

            if (info.Extension.EqualsIgnoreCase(".html")) {
                response.MimeType = "text/html";
                goto next;
            }

            if (info.Extension.EqualsIgnoreCase(".txt")) {
                response.MimeType = "text/plain";
                goto next;
            }

            response.MimeType = "application/octet-stream";

            next:

            var bytes = File.ReadAllBytes(WebUtility.UrlDecode(path));

            response.ContentLength = bytes.Length;
            response.ResponseStream = new MemoryStream(bytes);
            response.StatusCode = 200;
        }

        #endregion
    }
}