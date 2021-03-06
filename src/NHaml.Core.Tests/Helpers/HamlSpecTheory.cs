using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using NUnit.Core.Extensibility;
using System.Reflection;

namespace NHaml.Core.Tests.Helpers
{
    public class HamlSpecTheory
    {
        private readonly string _specName;

        public HamlSpecTheory(string specName)
        {
            if (specName == null)
                throw new ArgumentNullException("specName");
            _specName = specName;
        }

        private string _jsonTests;

        public IEnumerable<ParameterSet> GetTestCasesFor()
        {
            var resourceName = "NHaml.Core.Tests" + "." + _specName;
            var resourceStream = typeof(HamlSpecTheory).Assembly.GetManifestResourceStream(resourceName);

            if (resourceStream == null)
                throw new FileNotFoundException("resource " + resourceName + " not found");

            using (var reader = new StreamReader(resourceStream, Encoding.UTF8, false))
            {
                _jsonTests = reader.ReadToEnd();
            }

            var testGroups = (Hashtable)JSON.JsonDecode(_jsonTests);

            foreach (DictionaryEntry testGroup in testGroups)
            {
                var groupName = (string)testGroup.Key;
                var tests = (Hashtable)testGroup.Value;

                foreach (DictionaryEntry test in tests)
                {
                    var testName = (string)test.Key;
                    var testValues = (Hashtable)test.Value;

                    var haml = (string)testValues["haml"];
                    var html = (string)testValues["html"];

                    var config = (Hashtable)testValues["config"];
                    var format = config != null ? config["format"] : null;

                    var locals = (Hashtable)testValues["locals"];
                    var localsArr = new List<SpecLocal>();

                    if (locals != null)
                        foreach (DictionaryEntry local in locals)
                            localsArr.Add(new SpecLocal
                            {
                                Name = local.Key.ToString(),
                                Value = local.Value.ToString()
                            });

                    html = html.Replace("\n", Environment.NewLine);

                    var fullName = string.Format("{1} ({0})", groupName, testName);
                    ParameterSet s = new ParameterSet();
                    s.Arguments = new[] { fullName, haml, html, format, localsArr.ToArray() };
                    yield return s;
                    //yield return new TheoryCommand(method, fullName, );
                }
            }
        }
    }
}