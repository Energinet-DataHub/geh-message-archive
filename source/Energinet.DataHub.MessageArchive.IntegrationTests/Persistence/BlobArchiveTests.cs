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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Energinet.DataHub.MessageArchive.Persistence.Services;
using Energinet.DataHub.MessageArchive.Processing.Models;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MessageArchive.IntegrationTests.Persistence
{
    [Collection("IntegrationTest")]
    [IntegrationTest]
    public class BlobArchiveTests
    {
        [Fact]
        public async Task Test_WriteToArchive()
        {
            // Arrange ------------------
            var archiveConn = "UseDevelopmentStorage=true;";
            var marketoplogs = "marketoplogs";
            var marketoplogsArchive = "marketoplogs-archive";

            var itemToMove = PersistenceTestHelper.CrateRandomBlobItem();
            await using var itemToMoveContentStream = new MemoryStream(Encoding.UTF8.GetBytes(itemToMove.Content));

            var blobServiceClientMarketoplogs = await PersistenceTestHelper.InitTestBlobStorageAsync(archiveConn, marketoplogs).ConfigureAwait(false);
            await PersistenceTestHelper.InitTestBlobStorageAsync(archiveConn, marketoplogsArchive).ConfigureAwait(false);
            var containerClient = blobServiceClientMarketoplogs.GetBlobContainerClient(marketoplogs);
            var blobClient = containerClient.GetBlobClient(itemToMove.Name);

            // Act -----------------------
            var options = new BlobUploadOptions { Tags = itemToMove.IndexTags, Metadata = itemToMove.MetaData };

            // First Upload to storage
            await blobClient.UploadAsync(itemToMoveContentStream, options).ConfigureAwait(false);

            // Find blobs to Process and move
            var blobReader = new BlobReader(archiveConn, marketoplogs);
            var blobsForProcessing = await blobReader.GetBlobsReadyForProcessingAsync().ConfigureAwait(false);

            var firstItem = blobsForProcessing.First(e => e.Name.Equals(itemToMove.Name));

            // Now move to archive
            var blobArchive = new BlobArchive(archiveConn, marketoplogs, marketoplogsArchive);
            var resultUri = await blobArchive.MoveToArchiveAsync(firstItem).ConfigureAwait(false);

            // Assert -------------
            Assert.NotNull(resultUri);
            Assert.EndsWith(itemToMove.Name, resultUri.AbsolutePath);
        }
    }
}
