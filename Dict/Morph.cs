using System.Collections.Generic;
using System.Linq;

namespace Hors.Dict
{
    public static class Morph
    {
        private static readonly Dictionary<string, string[]> NormalForms = new Dictionary<string, string[]>();
        private static bool _loaded = false;

        public static void Load()
        {
            AddDictionary("time_words.txt");
        }

        public static void AddDictionary(string filename)
        {
            
        }

        public static string[] GetNormalForm(string rawWord)
        {
            if (!_loaded) Load();
            return NormalForms.ContainsKey(rawWord) ? NormalForms[rawWord] : null;
        }

        public static bool HasLemma(string rawWord, string lemma, bool checkItself = false)
        {
            var lemmas = GetNormalForm(rawWord);
            return lemmas != null && lemmas.Any(x => x == lemma);
        }

        public static bool HasOneOfLemmas(string rawWord, params string[] lemmas)
        {
            return lemmas.Any(x => HasLemma(rawWord, x));
        }
        
        public static bool HasOneOfLemmas(string rawWord, params string[][] lemmas)
        {
            return lemmas.Any(x => HasOneOfLemmas(rawWord, x));
        }
    }
}
