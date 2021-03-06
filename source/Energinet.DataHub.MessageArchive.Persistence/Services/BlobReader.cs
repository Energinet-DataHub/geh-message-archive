// Copyright 2020 Energinet DataHub A/S
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Energinet.DataHub.MessageArchive.Processing.Models;
using Energinet.DataHub.MessageArchive.Processing.Services;
using Energinet.DataHub.MessageArchive.Utilities;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MessageArchive.Persistence.Services
{
    public class BlobReader : IBlobReader
    {
        private readonly ILogger<BlobReader> _logger;
        private readonly string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
        private readonly BlobContainerClient _blobContainerClient;

        public BlobReader(
            string connectionString,
            string containerName,
            ILogger<BlobReader> logger)
        {
            _logger = logger;
            _blobContainerClient = new BlobContainerClient(connectionString, containerName);
        }

        public async Task<List<BlobItemData>> GetBlobsReadyForProcessingAsync()
        {
            var blobPagesToProcess = _blobContainerClient
                .GetBlobsAsync(BlobTraits.All)
                .AsPages(default, 500);

            var tasks = new List<Task<BlobItemData>>();

            await foreach (Azure.Page<BlobItem> blobPage in blobPagesToProcess)
            {
                foreach (var blobItem in blobPage.Values)
                {
                    var blobDataTask = DownloadBlobDataAsync(blobItem);
                    tasks.Add(blobDataTask);
                }

                if (tasks.Count > 0 || string.IsNullOrEmpty(blobPage.ContinuationToken))
                {
                    break;
                }
            }

            _logger.LogInformation("Starts downloading log content for {TaskCount} tasks", tasks.Count);

            await Task.WhenAll(tasks).ConfigureAwait(false);

            _logger.LogInformation("Downloading done for all {TaskCount} tasks", tasks.Count);

            var downloadedBlobData = tasks.Select(t => t.Result);

            return downloadedBlobData.ToList();
        }

        private async Task<BlobItemData> DownloadBlobDataAsync(BlobItem blobItemToDownload)
        {
            Guard.ThrowIfNull(blobItemToDownload, nameof(blobItemToDownload));

            var metaData = blobItemToDownload.Metadata ?? new Dictionary<string, string>();
            var indexTags = blobItemToDownload.Tags ?? new Dictionary<string, string>();
            var properties = blobItemToDownload.Properties;
            var name = blobItemToDownload.Name;

            var blobClient = _blobContainerClient.GetBlobClient(blobItemToDownload.Name);
            var response = await blobClient.DownloadAsync().ConfigureAwait(false);
            using var streamReader = new StreamReader(response.Value.Content, Encoding.UTF8);
            var downloadedContent = await streamReader.ReadToEndAsync().ConfigureAwait(false);

            var cleanContent = CleanStringForUtf8Preamble(downloadedContent);
            var createdOnUtc = properties.CreatedOn.GetValueOrDefault().ToUniversalTime();
            return new BlobItemData(name, metaData, indexTags, cleanContent, createdOnUtc, blobClient.Uri);
        }

        private string CleanStringForUtf8Preamble(string content)
        {
            if (content.StartsWith(_byteOrderMarkUtf8, StringComparison.Ordinal))
            {
                content = content.Remove(0, _byteOrderMarkUtf8.Length);
            }

            if (content.EndsWith(_byteOrderMarkUtf8, StringComparison.Ordinal))
            {
                content = content.Remove(content.Length - _byteOrderMarkUtf8.Length, _byteOrderMarkUtf8.Length);
            }

            return content;
        }
    }
}
