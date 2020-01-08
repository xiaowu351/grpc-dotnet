using Client.ResponseProgress;
using Progress;
using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Core;
using Grpc.Net;

namespace Progress
{
    // Use a partial class to add an interface to the generated HistoryResponse type
    public sealed partial class HistoryResponse : IProgressMessage<HistoryResult, int>
    {
        bool IProgressMessage<HistoryResult, int>.IsProgress => ResponseTypeCase == ResponseTypeOneofCase.Progress;

        bool IProgressMessage<HistoryResult, int>.IsResult => ResponseTypeCase == ResponseTypeOneofCase.Result;


         
    }
}
