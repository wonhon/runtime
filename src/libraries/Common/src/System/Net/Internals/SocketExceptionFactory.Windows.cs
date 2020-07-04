// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.Sockets;

namespace System.Net.Internals
{
    internal static partial class SocketExceptionFactory
    {
        public static SocketException CreateSocketException(SocketError errorCode, int platformError)
        {
            return new SocketException((int)errorCode);
        }
    }
}
