using EFCore.Scaffolding.Extension;
using System;
using System.IO;
using System.Linq;

namespace EFCore.CodeGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
            var scaffoldingFile = di.Parent.Parent.Parent.Parent.GetFiles(".Scaffolding.xml", SearchOption.AllDirectories).FirstOrDefault();
            ScaffoldingHelper.Scaffolding("Entities", " QuartzDbContext", scaffoldingFile.Directory.FullName);
        }
    }
}
