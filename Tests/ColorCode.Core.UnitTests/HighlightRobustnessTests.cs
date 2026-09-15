// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ColorCode.Core.UnitTests
{
    /// <summary>
    /// Robustness guard: highlighting malformed / partial / oversized input for any language must
    /// complete quickly and never hang. Generalizes the JSON regression (issue #45) across the whole
    /// language set, so a future catastrophic-backtracking rule is caught by the suite (issue #13).
    /// </summary>
    public class HighlightRobustnessTests
    {
        // Structurally awkward inputs that a real document can contain: empty / whitespace, unbalanced
        // delimiters, oversized tokens, unterminated comments, and the partial-JSON array that used to
        // send Json.cs into exponential-time matching (#45). None of these should take measurable time.
        public static readonly string[] MalformedInputs =
        {
            "",
            "   \n\t\r  ",
            new string('{', 500),
            new string('a', 5000),
            "// " + new string('x', 3000),
            "/* " + new string('y', 3000),
            "<" + new string('a', 2000),
            "[\n  { \"field\": \"" + new string('a', 60) + "\n",
            string.Join(" ", Enumerable.Repeat("a=b,c;", 500)),
        };

        public static IEnumerable<object[]> LanguageAndInput =>
            from language in Languages.All
            from index in Enumerable.Range(0, MalformedInputs.Length)
            select new object[] { language.Id, index };

        [Theory]
        [MemberData(nameof(LanguageAndInput))]
        public void Highlighting_malformed_input_completes_quickly(string id, int inputIndex)
        {
            var language = Languages.FindById(id);
            var input = MalformedInputs[inputIndex];

            // Bound the call: a pre-fix catastrophic rule would never return, so the test failing fast
            // (rather than hanging the whole run) IS the signal. A healthy rule finishes in milliseconds.
            string html = null;
            var task = Task.Run(() => html = new HtmlClassFormatter().GetHtmlString(input, language));

            Assert.True(
                task.Wait(TimeSpan.FromSeconds(5)),
                $"Highlighting malformed input did not finish in 5s (language '{id}', input #{inputIndex}, " +
                $"length {input.Length}) — likely catastrophic regex backtracking.");
            Assert.NotNull(html);
        }
    }
}
