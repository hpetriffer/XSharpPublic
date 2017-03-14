﻿//
// Copyright (c) XSharp B.V.  All Rights Reserved.  
// Licensed under the Apache License, Version 2.0.  
// See License.txt in the project root for license information.
//
using LanguageService.CodeAnalysis.XSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using LanguageService.SyntaxTree;

namespace XSharpModel
{
    public class SourceWalker
    {

        static XSharpCommandLineParser xsCmdLineparser;

        static SourceWalker()
        {
            xsCmdLineparser = XSharpCommandLineParser.Default;
        }

        private IClassificationType _xsharpIdentifierType;
        private IClassificationType _xsharpBraceOpenType;
        private IClassificationType _xsharpBraceCloseType;
        private IClassificationType _xsharpRegionStartType;
        private IClassificationType _xsharpRegionStopType;
        private ITextSnapshot _snapshot;
        private string _source;
        private string _fullPath;
        private List<ClassificationSpan> _tags;
        private string[] _args;
        private XSharpParseOptions xsparseoptions;

        private XFile _file;

        private LanguageService.SyntaxTree.ITokenStream _TokenStream;
        private LanguageService.CodeAnalysis.XSharp.SyntaxParser.XSharpParser.SourceContext _xTree;
        private bool _treeInit;

        public ITextSnapshot Snapshot
        {
            set
            {
                _snapshot = value;
                _source = _snapshot.GetText();
            }
        }


        public string FullPath
        {
            set
            {
                _fullPath = value;
                this._file = XSolution.FindFullPath(value);
            }
        }

        public XFile File
        {
            set
            {
                this._file = value;
                if (this._file != null)
                {
                    this._fullPath = this._file.FullPath;
                    if (this._source == null)
                    {
                        this._source = System.IO.File.ReadAllText(this._file.FullPath);
                    }
                }
            }

            get
            {
                return this._file;
            }
        }

        public List<ClassificationSpan> Tags
        {
            get
            {
                return _tags;
            }

        }

        public ITokenStream TokenStream
        {
            get
            {
                return _TokenStream;
            }

        }

        public string Source
        {
            get
            {
                return _source;
            }

            set
            {
                _source = value;
            }
        }

        public SourceWalker()
        {
            //
            _tags = new List<ClassificationSpan>();
        }

        public SourceWalker(IClassificationTypeRegistryService registry):this()
        {
            if (registry != null)
            {
                _xsharpIdentifierType = registry.GetClassificationType("identifier");
                _xsharpBraceOpenType = registry.GetClassificationType("punctuation");
                _xsharpBraceCloseType = registry.GetClassificationType("punctuation");
                _xsharpRegionStartType = registry.GetClassificationType(ColorizerConstants.XSharpRegionStartFormat);
                _xsharpRegionStopType = registry.GetClassificationType(ColorizerConstants.XSharpRegionStopFormat);
            }
            //
        }

        public void InitParse()
        {
            _treeInit = false;
            //
            if (this.File == null || this.File.Project == null || this.File.Project.ProjectNode == null)
                return;

            try
            {
                // this gets at least the default include path     
                // so we can process Vulcan and XSharp include files           
                // get command line args and compare with old args
                var args = this.File.Project.ProjectNode.CommandLineArgs;
                if (args != _args || xsparseoptions == null)
                {
                    _args = args;
                    var cmdlineopts = xsCmdLineparser.Parse(args, "", "", "");
                    xsparseoptions = cmdlineopts.ParseOptions;
                }
                LanguageService.CodeAnalysis.SyntaxTree tree = XSharpSyntaxTree.ParseText(_source, xsparseoptions, _fullPath);
                if ( this.File != null )
                {
                    // Put a Hash Tag on the File
                    this.File.HashCode = _source.GetHashCode();
                }
                var syntaxRoot = tree.GetRoot();

                var prjNode = File.Project.ProjectNode;

                ShowErrorsAsync(syntaxRoot);

                 // Get the antlr4 parse tree root
                _xTree = ((LanguageService.CodeAnalysis.XSharp.Syntax.CompilationUnitSyntax)syntaxRoot).XSource;
                _TokenStream = ((LanguageService.CodeAnalysis.XSharp.Syntax.CompilationUnitSyntax)syntaxRoot).XTokenStream;
                //
                _treeInit = true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        IEnumerable<LanguageService.CodeAnalysis.Diagnostic> errors = null;
        object _gate = new object();
        void ShowErrorsAsync(LanguageService.CodeAnalysis.SyntaxNode syntaxRoot)
        {
            // To list errors: But how to add to errorlist from here ?
            
            var prjNode = File.Project.ProjectNode;
            lock (_gate)
            {
                errors = syntaxRoot.GetDiagnostics();
            }
            if (errors == null)
                return;
            //var thread = new System.Threading.Thread(delegate ()
            //{
                // wait 2 seconds to allow continuous typing. The error may have disappeared in 2 seconds
                //System.Threading.Thread.Sleep(2000);
                IEnumerable<LanguageService.CodeAnalysis.Diagnostic> current;
                lock (_gate)
                {
                    current = errors;
                    string path = File.FullPath;
                    prjNode.ClearIntellisenseErrors(path);
                    if (current != null && prjNode.IsDocumentOpen(path))
                    {
                        foreach (var diag in current)
                        {
                            var span = diag.Location.GetLineSpan();
                            var loc = span.StartLinePosition;
                            var length = span.Span.End.Character - span.Span.Start.Character + 1;
                            prjNode.AddIntellisenseError(path, loc.Line + 1, loc.Character + 1,length ,diag.Id, diag.GetMessage(), diag.Severity);
                        }
                    }
                    prjNode.ShowIntellisenseErrors();
                }
            //});
            //thread.Start();
        }


        public void BuildRegionTagsOnly()
        {
            var discover = new XSharpModelRegionDiscover();
            discover.File = this._file;
            discover.BuildRegionTags = true;
            discover.BuildModel = false;
            //
            if ( _treeInit && ( _snapshot != null ) )
            {
                var walker = new LanguageService.SyntaxTree.Tree.ParseTreeWalker();
                //
                discover.Snapshot = _snapshot;
                discover.xsharpBraceCloseType = _xsharpBraceCloseType;
                discover.xsharpBraceOpenType = _xsharpBraceOpenType;
                discover.xsharpIdentifierType = _xsharpIdentifierType;
                discover.xsharpRegionStartType = _xsharpRegionStartType;
                discover.xsharpRegionStopType = _xsharpRegionStopType;
                // Walk the tree. The TreeDiscover class will collect the tags.
                walker.Walk(discover, _xTree);
            }
            //
            _tags = discover.tags;
        }

        public void BuildModelOnly()
        {
            // abort when the project is unloaded
            if (!_file.Project.Loaded)
                return;

            //
            var discover = new XSharpModelRegionDiscover();
            discover.File = this._file;
            discover.BuildRegionTags = false;
            discover.BuildModel = true;
            if (_file != null)
            {
                if (_file.Project.Loaded)
                {
                    discover.BuildModel = !_file.Parsed;
                }
            }
            //
            if (_treeInit )
            {
                var walker = new LanguageService.SyntaxTree.Tree.ParseTreeWalker();
                //
                // Walk the tree. The TreeDiscover class will build the model.
                walker.Walk(discover, _xTree);
            }
        }

        public void BuildModelAndRegionTags()
        {
            //
            var discover = new XSharpModelRegionDiscover();
            discover.File = this._file;
            discover.BuildRegionTags = (_snapshot != null);
            discover.BuildModel = false;
            if (this._file != null)
            {
                if ( _file.Project.Loaded )
                {
                    discover.BuildModel = ! _file.Parsed;
                }
            }
            //
            if (_treeInit)
            {
                var walker = new LanguageService.SyntaxTree.Tree.ParseTreeWalker();
                //
                if (_snapshot != null)
                {
                    discover.Snapshot = _snapshot;
                    discover.xsharpBraceCloseType = _xsharpBraceCloseType;
                    discover.xsharpBraceOpenType = _xsharpBraceOpenType;
                    discover.xsharpIdentifierType = _xsharpIdentifierType;
                    discover.xsharpRegionStartType = _xsharpRegionStartType;
                    discover.xsharpRegionStopType = _xsharpRegionStopType;
                }
                // Walk the tree. The TreeDiscover class will build the model.
                walker.Walk(discover, _xTree);
            }
            if ( discover.BuildRegionTags )
            {
                _tags = discover.tags;
            }
        }


    }
}