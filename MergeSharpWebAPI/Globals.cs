// Copyright 2022 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License").
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Globalization;
using MergeSharpWebAPI.Services;
using Microsoft.AspNetCore.SignalR.Client;

namespace MergeSharpWebAPI;

public static class Globals
{
    public static readonly LWWSetService<int> myLWWSetService = new();
    public static readonly TPTPGraphService myTPTPGraphService = new();
    public static readonly GraphService myGraphService = new();
    //dictionary from guids to ints
    public static readonly Dictionary<Guid, int> IDMapping = new();

    //public static IDMapping = new Dictionary<Guid, int>() { { Guid.NewGuid(), 1}, { Guid.NewGuid(), 2 } };

    private const string PropagationMessageServer = "https://serverwebapi20230125175127.azurewebsites.net/hubs/propagationmessage";
    // private const string PropagationMessageServer = "https://localhost:7106/hubs/propagationmessage";
    // private const string PropagationMessageServer = "http://localhost:5106/hubs/propagationmessage";

    public static readonly HubConnection connection = new HubConnectionBuilder()
                    .WithUrl(PropagationMessageServer)
                    .WithAutomaticReconnect()
                    .Build();
}
