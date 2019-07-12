using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Hors.Dict
{
    public static class Morph
    {
        private static readonly Dictionary<string, (string normalForm, byte plural)> Storage = new Dictionary<string, (string, byte)>();
        private static bool _loaded;
        private static string _lastNormalForm = "";
        
        public static void Load()
        {
            var assembly = Assembly.GetCallingAssembly();
            var file = assembly.GetManifestResourceStream("Hors.Dict.time_words.txt");

            if (file != null)
            {
                LoadFromStreamReader(new StreamReader(file, Encoding.UTF8));
            }
            _loaded = true;
        }

        public static void AddDictionary(string fileName)
        {
            using (var file = new StreamReader(fileName))
            {
                LoadFromStreamReader(file);
            }
        }

        private static void LoadFromStreamReader(StreamReader file)
        {
            while (!file.EndOfStream)
            {
                LoadLine(file.ReadLine());
            }
        }

        private static void LoadLine(string line)
        {
            if (line == string.Empty)
            {
                // new normal form
                _lastNormalForm = "";
                return;
            }
                    
            // read line data
            var tokens = line.Split('|');
            var word = tokens[0];
            var plural = byte.Parse(tokens[1]);

            if (_lastNormalForm == "")
            {
                _lastNormalForm = word;
            }

            Storage[word] = (_lastNormalForm, plural);

        }

        public static string GetNormalForm(string rawWord, LemmaSearchOptions option = LemmaSearchOptions.All)
        {
            if (!_loaded) Load();
            if (!Storage.ContainsKey(rawWord)) return null;

            var (normalForm, plural) = Storage[rawWord];
            if (
                option == LemmaSearchOptions.All || plural == 0
                || option == LemmaSearchOptions.OnlySingular && plural == 1
                || option == LemmaSearchOptions.OnlyPlural && plural == 2
                )
            {
                return normalForm;
            }

            return null;
        }

        public static bool HasLemma(string rawWord, string rawLemma, LemmaSearchOptions option = LemmaSearchOptions.All)
        {
            if (rawWord.ToLower() == rawLemma) return true;
            var lemma = GetNormalForm(rawWord, option);
            return lemma != null && lemma == rawLemma;
        }

        public static bool HasOneOfLemmas(string rawWord, params string[] lemmas)
        {
            return lemmas.Any(x => HasLemma(rawWord, x));
        }
        
        public static bool HasOneOfLemmas(string rawWord, params string[][] lemmas)
        {
            return lemmas.Any(x => HasOneOfLemmas(rawWord, x));
        }

        public enum LemmaSearchOptions
        {
            All,
            OnlySingular,
            OnlyPlural
        }
    }
}
