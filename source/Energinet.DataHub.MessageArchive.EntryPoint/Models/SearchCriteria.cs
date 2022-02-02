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

namespace Energinet.DataHub.MessageArchive.EntryPoint.Models
{
    public sealed record SearchCriteria
    {
        public SearchCriteria(
            string? messageId,
            string? messageType,
            string? processId,
            long? dateTimeFrom,
            long? dateTimeTo,
            string? senderId,
            string? businessReasonCode)
        {
            MessageId = messageId;
            MessageType = messageType;
            ProcessId = processId;
            DateTimeFrom = dateTimeFrom;
            DateTimeTo = dateTimeTo;
            SenderId = senderId;
            BusinessReasonCode = businessReasonCode;
        }

        public string? MessageId { get; }
        public string? MessageType { get; set; }
        public string? ProcessId { get; set; }
        public long? DateTimeFrom { get; set; }
        public long? DateTimeTo { get; set; }
        public string? SenderId { get; set; }
        public string? BusinessReasonCode { get; set; }
    }
}