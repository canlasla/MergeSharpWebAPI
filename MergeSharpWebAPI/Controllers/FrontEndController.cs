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

using MergeSharpWebAPI.Hubs.Clients;
using MergeSharpWebAPI.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MergeSharpWebAPI.Models;

namespace MergeSharpWebAPI.Controllers;
public class FrontEndController : Controller
{
    public IHubContext<FrontEndHub, IFrontEndClient> _strongFrontEndHubContext { get; }

    public FrontEndController(IHubContext<FrontEndHub, IFrontEndClient> FrontEndHubContext)
    {
        _strongFrontEndHubContext = FrontEndHubContext;
    }

    public async Task SendMessage(FrontEndMessage message)
    {
        Console.WriteLine(message);
        await _strongFrontEndHubContext.Clients.All.ReceiveMessage(message);
    }
}
