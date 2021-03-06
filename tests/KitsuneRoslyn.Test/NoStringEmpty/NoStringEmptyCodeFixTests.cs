﻿// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.Diagnostics.TestHelpers.CodeFixTestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlackFox.Roslyn.Diagnostics.NoStringEmpty
{
    [TestClass]
    public class NoStringEmptyCodeFixTests
    {
        [TestMethod]
        public void Fix_on_fully_qualified_call()
        {
            CheckSingleFixAsync(
                "class Foo{void Bar(){var x = global::System.String.Empty;}}",
                "global::System.String.Empty",
                "class Foo{void Bar(){var x = \"\";}}",
                "Use \"\"",
                new ReplaceStringEmptyWithEmptyLiteral(),
                NoStringEmptyAnalyzer.Descriptor).Wait();
        }

        [TestMethod]
        public void Fix_on_namespace_qualified_call()
        {
            CheckSingleFixAsync(
                "class Foo{void Bar(){var x = System.String.Empty;}}",
                "System.String.Empty",
                "class Foo{void Bar(){var x = \"\";}}",
                "Use \"\"",
                new ReplaceStringEmptyWithEmptyLiteral(),
                NoStringEmptyAnalyzer.Descriptor).Wait();
        }

        [TestMethod]
        public void Fix_on_standard_call()
        {
            CheckSingleFixAsync(
                "using System;class Foo{void Bar(){var x = String.Empty;}}",
                "String.Empty",
                "using System;class Foo{void Bar(){var x = \"\";}}",
                "Use \"\"",
                new ReplaceStringEmptyWithEmptyLiteral(),
                NoStringEmptyAnalyzer.Descriptor).Wait();
        }

        [TestMethod]
        public void Fix_on_method_call()
        {
            CheckSingleFixAsync(
                "using System;class Foo{void Bar(){var x = FooBar(String.Empty);} void FooBar(string s) {}}",
                "String.Empty",
                "using System;class Foo{void Bar(){var x = FooBar(\"\");} void FooBar(string s) {}}",
                "Use \"\"",
                new ReplaceStringEmptyWithEmptyLiteral(),
                NoStringEmptyAnalyzer.Descriptor).Wait();
        }
    }
}
