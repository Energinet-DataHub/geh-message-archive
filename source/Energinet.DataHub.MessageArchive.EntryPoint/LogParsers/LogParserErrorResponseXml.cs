﻿// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Xml.Linq;
using Energinet.DataHub.MessageArchive.EntryPoint.LogParsers.ErrorParsers;
using Energinet.DataHub.MessageArchive.EntryPoint.Models;
using Energinet.DataHub.MessageArchive.Utilities;

namespace Energinet.DataHub.MessageArchive.EntryPoint.LogParsers
{
    public class LogParserErrorResponseXml : LogParserBlobProperties
    {
        public override BaseParsedModel Parse(BlobItemData blobItemData)
        {
            Guard.ThrowIfNull(blobItemData, nameof(blobItemData));

            var nocontentParse = base.Parse(blobItemData);

            try
            {
                var xmlDocument = XElement.Parse(blobItemData.Content);
                XNamespace ns = xmlDocument.Name.Namespace;
                nocontentParse.Errors = XmlErrorParser.ParseErrors(xmlDocument);
            }
            catch
            {
                // ignored
            }

            return nocontentParse;
        }
    }
}