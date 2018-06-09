﻿using System;
using System.IO;
using System.Runtime.CompilerServices;
using Assent;
using Assent.Namers;

namespace ExpressionToCodeTest
{
    static class ApprovalTest
    {
        public static void Verify(string text, [CallerFilePath] string filepath = null, [CallerMemberName] string membername = null)
        {
            var filename = Path.GetFileNameWithoutExtension(filepath);
            var filedir = Path.GetDirectoryName(filepath) ?? throw new InvalidOperationException("path " + filepath + " has no directory");
            var config = new Configuration().UsingNamer(new FixedNamer(Path.Combine(filedir, filename + "." + membername)));
            // ReSharper disable once ExplicitCallerInfoArgument
            "bla".Assent(text, config, membername, filepath);
            //var writer = WriterFactory.CreateTextWriter(text);
            //var namer = new SaneNamer { Name = filename + "." + membername, SourcePath = filedir };
            //var reporter = new DiffReporter();
            //Approver.Verify(new FileApprover(writer, namer, true), reporter);
        }

        //public class SaneNamer : IApprovalNamer
        //{
        //    public string SourcePath { get; set; }
        //    public string Name { get; set; }
        //}
    }
}
