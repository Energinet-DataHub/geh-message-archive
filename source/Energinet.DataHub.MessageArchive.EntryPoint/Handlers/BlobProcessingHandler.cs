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
using System.Threading.Tasks;
using Energinet.DataHub.MessageArchive.EntryPoint.BlobServices;
using Energinet.DataHub.MessageArchive.EntryPoint.LogParsers;
using Energinet.DataHub.MessageArchive.EntryPoint.Models;
using Energinet.DataHub.MessageArchive.EntryPoint.Repository;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MessageArchive.EntryPoint.Handlers
{
    public class BlobProcessingHandler : IBlobProcessingHandler
    {
        private readonly IBlobReader _blobReader;
        private readonly IBlobArchive _blobArchive;
        private readonly IStorageWriter<CosmosRequestResponseLog> _storageWriter;
        private readonly ILogger<BlobProcessingHandler> _logger;

        public BlobProcessingHandler(
            IBlobReader blobReader,
            IBlobArchive blobArchive,
            IStorageWriter<CosmosRequestResponseLog> storageWriter,
            ILogger<BlobProcessingHandler> logger)
        {
            _blobReader = blobReader;
            _blobArchive = blobArchive;
            _storageWriter = storageWriter;
            _logger = logger;
        }

        public async Task HandleAsync()
        {
            var blobDataToProcess = await _blobReader.GetBlobsReadyForProcessingAsync().ConfigureAwait(false);

            foreach (var blobItemData in blobDataToProcess)
            {
                var contentType = blobItemData.MetaData.TryGetValue("contenttype", out var contentTypeValue) ? contentTypeValue : string.Empty;
                var httpStatusCode = blobItemData.MetaData.TryGetValue("statuscode", out var statusCodeValue) ? statusCodeValue : string.Empty;

                var parser = ParserFinder.FindParser(contentType, httpStatusCode, blobItemData.Content);

                try
                {
                    await ParseAndSaveAsync(parser, blobItemData).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error in processing item: {name}", blobItemData.Name);
                    await ParseAndSaveAsync(new LogParserBlobProperties(), blobItemData).ConfigureAwait(false);
                }
            }
        }

        private async Task ParseAndSaveAsync(ILogParser parser, BlobItemData blobItemData)
        {
            var parsedModel = parser.Parse(blobItemData);
            var cosmosModel = Mappers.CosmosRequestResponseLogMapper.ToCosmosRequestResponseLog(parsedModel);

            var archiveUri = await _blobArchive.MoveToArchiveAsync(blobItemData).ConfigureAwait(false);
            cosmosModel.BlobContentUri = archiveUri.AbsoluteUri;
            await _storageWriter.WriteAsync(cosmosModel).ConfigureAwait(false);
        }
    }
}