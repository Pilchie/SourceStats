using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.MSBuild;

namespace SourceStats
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 1 || !File.Exists(args[0]))
            {
                Console.WriteLine("Usage: SourceStats.exe <solution file name>");
                return 1;
            }

            var ws = MSBuildWorkspace.Create(new Dictionary<string, string> { { "DelaySign", "true" } });
            AnalyzeSolutionAsync(ws, args[0]).Wait();

            return 0;
        }

        private static async Task AnalyzeSolutionAsync(MSBuildWorkspace workspace, string solutionFilename)
        {
            var documents = 0;
            var lines = 0;
            var solution = await workspace.OpenSolutionAsync(solutionFilename);
            foreach (var project in solution.Projects)
            {
                var linesInProject = 0;
                foreach (var document in project.Documents)
                {
                    documents++;
                    var text = await document.GetTextAsync();
                    var linesInDocument = text.Lines.Count;
                    linesInProject += linesInDocument;
                    lines += linesInDocument;
                }

                Console.WriteLine($"Project '{project.Name}' with '{project.DocumentIds.Count:N0}' documents, and '{linesInProject:N0}' lines.");
            }

            var projects = solution.ProjectIds.Count;
            Console.WriteLine($"Solution '{solutionFilename}' has '{projects:N0}' projects, '{documents:N0}' documents, and '{lines:N0}' lines of code");
            Console.WriteLine($"Averages: '{documents/projects:N0}' documents/project, '{lines/documents:N0}' lines/document, '{lines/projects:N0}' lines/project");
        }
    }
}
