// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;

namespace BVNetwork.NotFound.Core.CustomRedirects
{
    public interface IRedirectHandler
    {
        /// <summary>
        /// Returns custom redirect for the not found url
        /// </summary>
        /// <param name="urlNotFound"></param>
        /// <returns></returns>
        CustomRedirect Find(Uri urlNotFound);
    }
}