using System;
using System.Collections.Generic;
using Agitprop.Core.Enums;

namespace Agitprop.Core.Contracts
{
    public record ScrapingJobDescription
    {
        public Uri Url { get; init; }
        public PageContentType Type { get; init; }
        public List<string> Sinks { get; init; }
    }
}