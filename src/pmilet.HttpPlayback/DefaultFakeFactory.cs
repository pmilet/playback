// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using Microsoft.AspNetCore.Http;
using pmilet.HttpPlayback.Core;
using System;

namespace pmilet.HttpPlayback
{
    internal class DefaultFakeFactory : IFakeFactory
    {
        public bool GenerateFakeResponse(HttpContext context)
        {
            return false;
        }
    }
}