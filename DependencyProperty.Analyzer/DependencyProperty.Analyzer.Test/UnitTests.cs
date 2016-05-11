using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using DependencyProperty.Analyzer;

namespace DependencyProperty.Analyzer.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void TestMethod1()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void TestMethod2()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace DependencyProperty.Analyzer.Test
{
    
    public class Dummy : DependencyObject
    {
        public static readonly System.Windows.DependencyProperty IsAnyGoodProperty = System.Windows.DependencyProperty.Register(
            ""IsAnyGood"", typeof (bool), typeof (Dummy), new PropertyMetadata(default(bool)));

        public bool IsAnyGood
        {
            get { return (bool)GetValue(IsAnyGoodProperty); }
            set { SetValue(IsAnyGoodProperty, value); }
        }
    }
}

";
            var expected = new DiagnosticResult
            {
                Id = "DependencyPropertyAnalyzer",
                Message = $"DependencyProperty '{"IsAnyGood"}' contains string literal.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 14, 86)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void TestMethod3()
        {
            var test = @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace DependencyProperty.Analyzer.Test
{
    
    public class Dummy : DependencyObject
    {
        public static readonly System.Windows.DependencyProperty IsAnyGoodProperty = System.Windows.DependencyProperty.Register(
            nameof(IsAnyGood), typeof (bool), typeof (Dummy), new PropertyMetadata(default(bool)));

        public bool IsAnyGood
        {
            get { return (bool)GetValue(IsAnyGoodProperty); }
            set { SetValue(IsAnyGoodProperty, value); }
        }
    }
}

";

            VerifyCSharpDiagnostic(test);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DependencyPropertyAnalyzerAnalyzer();
        }
    }
}