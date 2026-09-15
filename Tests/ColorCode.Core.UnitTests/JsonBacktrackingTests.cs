// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Xunit;

namespace ColorCode.Core.UnitTests
{
    /// <summary>
    /// Regression tests for the JSON language's <c>Regex_String</c> catastrophic backtracking
    /// (issue #45): malformed JSON must not send the highlighter into exponential-time matching.
    /// </summary>
    public class JsonBacktrackingTests
    {
        [Fact]
        public void Partial_json_does_not_catastrophically_backtrack()
        {
            // A JSON array whose last string value is unterminated. With the old Regex_String the
            // key rule explores O(2^n) ways to partition the string body and never returns; with
            // the fix it is linear. Bound the call so the pre-fix state fails fast (this test
            // running long IS the bug) instead of hanging the whole run.
            var partial = "[\n  { \"field\": \"" + new string('a', 40) + "\n";

            string html = null;
            var task = Task.Run(() => html = new HtmlClassFormatter().GetHtmlString(partial, Languages.FindById("json")));

            Assert.True(
                task.Wait(TimeSpan.FromSeconds(10)),
                "Highlighting partial JSON did not finish in 10s — Regex_String is backtracking catastrophically (issue #45).");
            Assert.NotNull(html);
        }

        [Fact]
        public void Valid_json_still_highlights_keys_strings_and_escapes()
        {
            // The fix must not change how valid JSON is tokenised: keys, string values, and strings
            // containing escaped quotes/backslashes all still colour correctly.
            var json = "{ \"name\": \"John Doe\", \"path\": \"C:\\\\a\\\"b\" }";

            var html = new HtmlClassFormatter().GetHtmlString(json, Languages.FindById("json"));

            Assert.Contains("jsonKey", html);
            Assert.Contains("jsonString", html);
            Assert.Contains("John Doe", html);
        }
    }
}
