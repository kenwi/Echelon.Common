using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Echelon.Common.Extensions
{
    internal static class HtmlDocumentEx
    {
        public static string? GetUserId(this HtmlDocument document)
            => document.DocumentNode
                ?.SelectSingleNode("//a[contains(@href, 'finduser')]")
                ?.Attributes["href"]
                ?.Value
                ?.Split("=")
                ?.Last();
    }
}
