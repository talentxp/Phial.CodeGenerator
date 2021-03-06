﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ModelCodeC
{
    class Program
    {
        static readonly ModelParser ModelParser = new ModelParser();
        static readonly ModelGenerator ModelGenerator = new ModelGenerator();
        static void Main(string[] args)
        {
            if (args.Length < 5)
            {
                Console.WriteLine("wrong args");
                Console.WriteLine("[usage:]");
                Console.WriteLine("assemblyFile formaterPath clientFilePath serverFilePath dataAccessFilePath");

                return;
            }

            var assemblyFile = args[0];
            var formaterPath = args[1];
            var clientFilePath = args[2];
            var serverFilePath = args[3];
            var dataAccessFilePath = args[4];

            try
            {
                var assembly = Assembly.LoadFrom(assemblyFile);

                var test = assembly.GetTypes().Where(t=>t.IsEnum);

                var ast = ModelParser.ParseSerialize(assembly).ToList();
                var ast2 = ModelParser.ParseDataAccess(assembly);

                GenerateFile(clientFilePath, ModelGenerator.GenClientSerialize(ast, test));
                GenerateFile(serverFilePath, ModelGenerator.GenServerSerialize(ast, test));
                GenerateFile(dataAccessFilePath, ModelGenerator.GenDataAccess(ast2));

                FormatFile(formaterPath, clientFilePath);
                FormatFile(formaterPath, serverFilePath);
                FormatFile(formaterPath, dataAccessFilePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static void GenerateFile(string filePath, string content)
        {
            var fileStream = new FileStream(filePath, FileMode.Create);
            var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);

            streamWriter.Write(content);
            streamWriter.Flush();

            streamWriter.Close();
            fileStream.Close();
        }

        static void FormatFile(string toolPath, string toFormatFilePath)
        {
            var arg = Path.GetFullPath(toFormatFilePath);
            var startInfo = new ProcessStartInfo(toolPath, string.Format("-f {0}", arg));

            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;

            Process.Start(startInfo);
        }
    }
}
