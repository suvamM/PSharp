﻿//-----------------------------------------------------------------------
// <copyright file="DirectAccessAnalysis.cs">
//      Copyright (c) 2015 Pantazis Deligiannis (p.deligiannis@imperial.ac.uk)
// 
//      THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//      EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//      MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//      IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//      CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//      TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//      SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.PSharp.StaticAnalysis
{
    /// <summary>
    /// This analysis reports an error if a machine has any fields
    /// or methods that can be publicly accessed.
    /// </summary>
    public sealed class DirectAccessAnalysis
    {
        #region fields

        /// <summary>
        /// The analysis context.
        /// </summary>
        private AnalysisContext AnalysisContext;

        #endregion

        #region public API

        /// <summary>
        /// Creates a new direct access analysis pass.
        /// </summary>
        /// <param name="context">AnalysisContext</param>
        /// <returns>DirectAccessAnalysis</returns>
        public static DirectAccessAnalysis Create(AnalysisContext context)
        {
            return new DirectAccessAnalysis(context);
        }

        /// <summary>
        /// Runs the analysis.
        /// </summary>
        public void Run()
        {
            this.CheckFields();
            this.CheckMethods();
        }

        #endregion

        #region private methods

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">AnalysisContext</param>
        private DirectAccessAnalysis(AnalysisContext context)
        {
            this.AnalysisContext = context;
        }

        /// <summary>
        /// Checks the fields of each machine and report warnings if
        /// any field is not private or protected.
        /// </summary>
        private void CheckFields()
        {
            foreach (var machineDecl in AnalysisContext.Machines)
            {
                foreach (var field in machineDecl.ChildNodes().OfType<FieldDeclarationSyntax>())
                {
                    if (field.Modifiers.Any(SyntaxKind.PublicKeyword))
                    {
                        AnalysisErrorReporter.Report("Field '{0}' of machine '{1}' is declared as " +
                            "'public'.", field.Declaration.ToString(), machineDecl.Identifier.ValueText);
                    }
                    else if (field.Modifiers.Any(SyntaxKind.InternalKeyword))
                    {
                        AnalysisErrorReporter.Report("Field '{0}' of machine '{1}' is declared as " +
                            "'internal'.", field.Declaration.ToString(), machineDecl.Identifier.ValueText);
                    }
                }
            }
        }

        /// <summary>
        /// Checks the methods of each machine and report warnings if
        /// any method is directly accessed by anything else than the
        /// P# runtime.
        /// </summary>
        private void CheckMethods()
        {
            foreach (var machineDecl in AnalysisContext.Machines)
            {
                foreach (var method in machineDecl.ChildNodes().OfType<MethodDeclarationSyntax>())
                {
                    if (method.Modifiers.Any(SyntaxKind.PublicKeyword))
                    {
                        AnalysisErrorReporter.Report("Method '{0}' of machine '{1}' is " +
                            "declared as 'public'.", method.Identifier.ValueText,
                            machineDecl.Identifier.ValueText);
                    }
                    else if (method.Modifiers.Any(SyntaxKind.InternalKeyword))
                    {
                        AnalysisErrorReporter.Report("Method '{0}' of machine '{1}' is " +
                            "declared as 'internal'.", method.Identifier.ValueText,
                            machineDecl.Identifier.ValueText);
                    }
                }
            }
        }

        #endregion
    }
}