﻿#region Copyright Notice & Copying Permission

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
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Resources;
using CefSharp;
using Crystalbyte.Paranoia.UI;
using dotless.Core;
using NLog;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class ResourceSchemeHandler : ISchemeHandler {
        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly object Mutex = new object();

        #endregion

        #region Implementation of ISchemeHandler

        public bool ProcessRequestAsync(IRequest request, ISchemeHandlerResponse response,
            OnRequestCompletedHandler requestCompletedCallback) {
            Task.Run(() => {
                try {
                    ComposeResourceResponse(request, response);
                    requestCompletedCallback();
                } catch (Exception ex) {
                    Logger.ErrorException(ex.Message, ex);
                }
            });

            return true;
        }

        private static void ComposeResourceResponse(IRequest request, ISchemeHandlerResponse response) {
            var uri = new Uri(request.Url);
            var resource = new Uri(uri.LocalPath, UriKind.RelativeOrAbsolute);

            // BUG: Occasionally throws ExecutionEngineException if not locked, so sad ... :(
            StreamResourceInfo info;
            lock (Mutex) {
                info = Application.GetResourceStream(resource);
                if (info == null) {
                    throw new ResourceNotFoundException(resource.AbsoluteUri);
                }
            }

            response.CloseStream = true;
            if (uri.LocalPath.EndsWith("less")) {
                using (var reader = new StreamReader(info.Stream)) {
                    var less = reader.ReadToEnd();
                    var css = Less.Parse(less);
                    var bytes = Encoding.UTF8.GetBytes(css);
                    response.ResponseStream = new MemoryStream(bytes);
                    response.MimeType = "text/css";
                }
                return;
            }

            response.ResponseStream = info.Stream;
        }

        #endregion
    }
}