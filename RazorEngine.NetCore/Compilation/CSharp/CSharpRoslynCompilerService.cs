﻿#if RAZOR4
#else
using System.Web.Razor.Parser;
#endif
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RazorEngine.Compilation;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;
#if !RAZOR4
using System.CodeDom.Compiler;
using System.Web.Razor;
using System.IO;
using System.Globalization;
#endif
using RazorEngine.Compilation.ReferenceResolver;

namespace RazorEngine.Roslyn.CSharp
{
    /// <summary>
    /// A concrete <see cref="ICompilerService"/> implementation for C# by using the Roslyn compiler.
    /// </summary>
    [SecurityCritical]
    public class CSharpRoslynCompilerService : RoslynCompilerServiceBase
    {
#if !RAZOR4
        /// <summary>
        /// We need a CodeDom instance as pre Razor4 uses CodeDom 
        /// internally and we need to generate the source code file...
        /// </summary>
        private Microsoft.CSharp.CSharpCodeProvider _codeDomProvider; 
#endif
        /// <summary>
        /// Creates a new CSharpRoslynCompilerService instance.
        /// </summary>
        /// <param name="strictMode"></param>
        /// <param name="markupParserFactory"></param>
        [SecurityCritical]
        public CSharpRoslynCompilerService(bool strictMode = true)
            : base()
        {
#if !RAZOR4
            _codeDomProvider = new Microsoft.CSharp.CSharpCodeProvider(); 
#endif
        }

        /// <summary>
        /// Returns "cs".
        /// </summary>
        public override string SourceFileExtension
        {
            [SecuritySafeCritical]
            get { return "cs"; }
        }

#if !RAZOR4
        /// <summary>
        /// Inspects the GeneratorResults and returns the source code.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [SecurityCritical]
        public override string InspectSource(GeneratorResults results, TypeContext context)
        {
            string generatedCode;
            var builder = new StringBuilder();
            using (var writer = new StringWriter(builder, CultureInfo.InvariantCulture))
            {
                _codeDomProvider.GenerateCodeFromCompileUnit(results.GeneratedCode, writer, new CodeGeneratorOptions());
                generatedCode = builder.ToString();
            }
            return generatedCode;
        }
#endif

        /// <summary>
        /// Build a C# typename.
        /// </summary>
        /// <param name="templateType"></param>
        /// <param name="modelType"></param>
        /// <returns></returns>
        [SecuritySafeCritical]
        public override string BuildTypeName(Type templateType, Type modelType)
        {
            if (templateType == null)
                throw new ArgumentNullException("templateType");

            var modelTypeName = CompilerServicesUtility.ResolveCSharpTypeName(modelType);
            return CompilerServicesUtility.CSharpCreateGenericType(templateType, modelTypeName, false);
        }

        /// <summary>
        /// Creates a CSharpSyntaxTree instance.
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <param name="sourceCodePath"></param>
        /// <returns></returns>
        [SecurityCritical]
        public override Microsoft.CodeAnalysis.SyntaxTree GetSyntaxTree(string sourceCode, string sourceCodePath)
        {
            return CSharpSyntaxTree.ParseText(sourceCode, path: sourceCodePath, encoding: System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// Creates a CSharpCompilation instance
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        [SecurityCritical]
        public override Microsoft.CodeAnalysis.Compilation GetEmptyCompilation(string assemblyName)
        {
            return CSharpCompilation.Create(assemblyName);
        }
        /// <summary>
        /// Creates a CSharpCompilationOptions instance.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [SecurityCritical]
        public override CompilationOptions CreateOptions(TypeContext context)
        {
            return
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithUsings(context.Namespaces);
        }

        /// <summary>
        /// Returns a set of assemblies that must be referenced by the compiled template.
        /// </summary>
        /// <returns>The set of assemblies.</returns>
        [SecuritySafeCritical]
        public override IEnumerable<CompilerReference> IncludeReferences()
        {
            // Ensure the Microsoft.CSharp assembly is referenced to support dynamic typing.
            return new[] { CompilerReference.From(typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly) };
        }
    }
}
