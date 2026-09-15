// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;

namespace ColorCode.Core.UnitTests
{
    /// <summary>
    /// Output tests for <see cref="HtmlClassFormatter"/>: tokens are wrapped in spans carrying the
    /// expected CSS class, the container div carries the language class, source text is HTML-encoded,
    /// and a stylesheet is produced (issue #13). Assertions are structural, not exact-markup, so they
    /// do not couple the suite to incidental formatting.
    /// </summary>
    public class HtmlClassFormatterTests
    {
        [Fact]
        public void Json_keys_and_strings_get_their_scope_classes()
        {
            var html = new HtmlClassFormatter().GetHtmlString("{ \"name\": \"John\" }", Languages.FindById("json"));

            Assert.Contains("class=\"json\"", html);   // container div uses the language's CssClassName
            Assert.Contains("jsonKey", html);
            Assert.Contains("jsonString", html);
            Assert.Contains("John", html);
        }

        [Fact]
        public void CSharp_keywords_get_the_keyword_class()
        {
            var html = new HtmlClassFormatter().GetHtmlString("public class Foo { }", Languages.CSharp);

            Assert.Contains("class=\"keyword\"", html);
        }

        [Fact]
        public void Source_text_is_html_encoded()
        {
            var html = new HtmlClassFormatter().GetHtmlString("if (a < b && c) { }", Languages.CSharp);

            Assert.Contains("&lt;", html);
            Assert.Contains("&amp;", html);
        }

        [Fact]
        public void Css_stylesheet_is_produced()
        {
            var css = new HtmlClassFormatter().GetCSSString();

            Assert.False(string.IsNullOrWhiteSpace(css));
            Assert.Contains("{", css);
        }
    }
}
