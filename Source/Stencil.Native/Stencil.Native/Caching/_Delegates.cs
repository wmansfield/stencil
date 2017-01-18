using System;

namespace Stencil.Native.Caching
{
    public delegate void FetchedRequestDelegate<T>(RequestToken requestToken, bool isFreshData, T foundData);
    public delegate void FetchedDelegate<T>(bool isFreshData, T foundData);
}

