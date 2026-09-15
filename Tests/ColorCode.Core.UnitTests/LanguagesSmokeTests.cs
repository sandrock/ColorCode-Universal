// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace ColorCode.Core.UnitTests
{
    /// <summary>
    /// Smoke tests over the built-in language registry: every language is retrievable and
    /// all of its rule patterns are valid, compilable regular expressions. A malformed pattern
    /// then fails here at test time rather than at a consumer's runtime (issue #13).
    /// </summary>
    public class LanguagesSmokeTests
    {
        public static IEnumerable<object[]> AllLanguageIds =>
            Languages.All.Select(language => new object[] { language.Id });

        [Fact]
        public void Registry_exposes_languages()
        {
            Assert.NotEmpty(Languages.All);
        }

        [Theory]
        [MemberData(nameof(AllLanguageIds))]
        public void Language_has_identity_and_is_findable_by_id(string id)
        {
            var language = Languages.FindById(id);

            Assert.NotNull(language);
            Assert.False(string.IsNullOrWhiteSpace(language.Id));
            Assert.False(string.IsNullOrWhiteSpace(language.Name));
            Assert.Equal(id, language.Id);
        }

        [Theory]
        [MemberData(nameof(AllLanguageIds))]
        public void Language_rule_patterns_compile(string id)
        {
            var language = Languages.FindById(id);

            Assert.NotNull(language.Rules);
            Assert.NotEmpty(language.Rules);

            foreach (var rule in language.Rules)
            {
                // Constructing the Regex validates the pattern; an invalid pattern throws and
                // fails the test, naming the offending language id and pattern.
                var exception = Record.Exception(() => new Regex(rule.Regex));
                Assert.True(
                    exception == null,
                    $"Language '{id}' has an invalid rule pattern: {rule.Regex}\n{exception}");
            }
        }
    }
}
