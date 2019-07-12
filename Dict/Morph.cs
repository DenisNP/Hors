using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Hors.Dict
{
    public static class Morph
    {
        private static readonly Dictionary<string, (string normalForm, byte plural)> Storage = new Dictionary<string, (string, byte)>();
        private static bool _loaded;

        public static void Load()
        {
            AddDictionary("time_words.txt");
            _loaded = true;
        }

        public static void AddDictionary(string fileName)
        {
            using (var file = new StreamReader(fileName))
            {
                var lastNormalForm = "";
                
                while (!file.EndOfStream)
                {
                    var line = file.ReadLine();
                    if (line == string.Empty)
                    {
                        // new normal form
                        lastNormalForm = "";
                        continue;
                    }
                    
                    // read line data
                    var tokens = line.Split('|');
                    var word = tokens[0];
                    var plural = byte.Parse(tokens[1]);

                    if (lastNormalForm == "")
                    {
                        lastNormalForm = word;
                    }

                    Storage[word] = (lastNormalForm, plural);
                }
            }
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

        public static bool HasLemma(string rawWord, string rawLemma)
        {
            if (rawWord.ToLower() == rawLemma) return true;
            var lemma = GetNormalForm(rawWord);
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
