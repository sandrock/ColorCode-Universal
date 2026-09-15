// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Xunit;

namespace ColorCode.Core.UnitTests
{
    /// <summary>
    /// Regression tests for catastrophic backtracking in the Koka language's string rules. The
    /// normal string, verbatim string, and char-literal patterns each used a nested quantifier
    /// (<c>(?:X+|...)*</c>) that went exponential on an unterminated literal; the unrolled-loop
    /// form fixes it. Same class of bug as the JSON one (issue #45).
    /// </summary>
    public class KokaBacktrackingTests
    {
        private static ILanguage Koka => Languages.FindById("koka");

        [Theory]
        [InlineData("\"")]  // unterminated normal string
        [InlineData("@\"")] // unterminated verbatim string
        [InlineData("'")]   // unterminated char literal
        public void Unterminated_literal_does_not_catastrophically_backtrack(string opener)
        {
            // An unterminated literal followed by a long run of ordinary characters. With the old
            // nested-quantifier rules this never returns; with the unrolled form it is linear. Bound
            // the call so the pre-fix state fails fast instead of hanging the whole run.
            var input = opener + new string('a', 200) + "\n";

            string html = null;
            var task = Task.Run(() => html = new HtmlClassFormatter().GetHtmlString(input, Koka));

            Assert.True(
                task.Wait(TimeSpan.FromSeconds(5)),
                $"Highlighting an unterminated Koka literal (opener '{opener}') did not finish in 5s — catastrophic backtracking.");
            Assert.NotNull(html);
        }

        [Fact]
        public void Valid_string_still_highlights_with_the_string_scope()
        {
            // The fix must not change how a valid string tokenises.
            var html = new HtmlClassFormatter().GetHtmlString("\"hello world\"", Koka);

            Assert.Contains("class=\"string\"", html);
            Assert.Contains("hello world", html);
        }
    }
}
