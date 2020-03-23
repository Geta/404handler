// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

namespace BVNetwork.NotFound.Core.Logging
{
    public interface IRequestLogger
    {
        void LogRequest(string oldUrl, string referrer);
    }
}