using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadCsvIntoTables
{
    class Program
    {
        static void Main(string[] args)
        {
            var directory = @"C:\temp\";

            if (Directory.Exists(directory))
            {
                foreach (var file in Directory.EnumerateFiles(directory))
                {
                    var fileContents = "";

                    using (var reader = new StreamReader(File.OpenRead(file)))
                    {
                        fileContents = reader.ReadToEnd();
                    }

                    var csvList = new List<string[]>();

                    var fileLines = fileContents.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    foreach (string fileLine in fileLines)
                    {
                        csvList.Add(fileLine.Split(";".ToCharArray(), StringSplitOptions.None));
                    }

                    var tableName = Path.GetFileNameWithoutExtension(file);
                    BuildSqlScriptAndExecute(csvList, tableName);
                }
            }
            else
            {
                Console.WriteLine("directory does not exist...");
            }
            Console.WriteLine("/**--Done! Press [ENTER] --**/");
            Console.ReadLine(); 
        }

        private static void BuildSqlScriptAndExecute(List<string[]> csvList, string tableName)
        {
            var numberOfColumns = csvList[0].Length;
            var sqlScript = new StringBuilder();

            foreach (var row in csvList.ToList())
            {
                if (row.Length != numberOfColumns)
                {
                    Console.WriteLine("{0} has a row with {1} columns, instead of {2} headercolumns ", tableName, row.Length, numberOfColumns);
                    csvList.Remove(row);
                }
            }

            csvList.RemoveAt(0);

            foreach (var row in csvList.ToList())
            {
                var commaSeperatedValues = string.Empty;

                foreach (var column in row)
                {
                    commaSeperatedValues += "'" + System.Text.RegularExpressions.Regex.Replace(column, @"\s{2,}", " ").Replace("'", string.Empty) + "',";
                }
                commaSeperatedValues = commaSeperatedValues.TrimEnd(',');

                sqlScript.AppendFormat("insert into Stage{0} values({1}) " + Environment.NewLine, tableName, commaSeperatedValues.Replace('"'.ToString(), string.Empty));//.Replace("''", "null");
            }

            Console.WriteLine("--Executing insert statements for {0}", tableName);
            Console.WriteLine(sqlScript.ToString());

            WriteToLog(tableName, sqlScript.ToString());

        }

        private static void WriteToLog(string tableName, string sqlScript)
        {
            using (var sw = File.CreateText("ErrorLog_" + tableName + ".log"))
            {
                sw.WriteLine(sqlScript.ToString());
            }
        }
    }
}
