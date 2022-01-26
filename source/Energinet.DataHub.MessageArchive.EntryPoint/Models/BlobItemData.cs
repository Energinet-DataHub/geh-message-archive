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
using System.Collections.Generic;
using Azure.Storage.Blobs.Models;

namespace Energinet.DataHub.MessageArchive.EntryPoint.Models
{
#pragma warning disable SA1313
    public record BlobItemData(string Name, IDictionary<string, string> MetaData, IDictionary<string, string> IndexTags,
        string Content, BlobItemProperties Properties, Uri Uri)
    {
    }
#pragma warning restore SA1313
}