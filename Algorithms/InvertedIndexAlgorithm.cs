using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OrganizationChecker.Utils;

namespace OrganizationChecker.Algorithms
{
    /// <summary>
    /// Алгоритм с использованием инвертированного индекса
    /// Строит индекс слов запрещенных организаций для быстрого поиска
    /// </summary>
    public class InvertedIndexAlgorithm : IAlgorithm
    {
        // Инвертированный индекс: слово -> множество организаций, содержащих это слово
        private Dictionary<string, HashSet<string>> _wordIndex = new();
        private HashSet<string> _allBannedOrganizations = new();
        private List<string> _normalizedBannedOrganizations = new();
        private bool _isInitialized = false;
        
        public string Name => "Инвертированный индекс + фильтрация";
        
        /// <summary>
        /// Инициализация инвертированного индекса
        /// </summary>
        private void Initialize(List<string> bannedOrganizations)
        {
            if (_isInitialized) return;
            
            _wordIndex.Clear();
            _allBannedOrganizations.Clear();
            _normalizedBannedOrganizations.Clear();
            
            foreach (var org in bannedOrganizations)
            {
                var normalizedOrg = StringNormalizer.Normalize(org);
                if (string.IsNullOrWhiteSpace(normalizedOrg))
                    continue;
                    
                _allBannedOrganizations.Add(normalizedOrg);
                _normalizedBannedOrganizations.Add(normalizedOrg);
                
                // Разбиваем организацию на слова
                var words = normalizedOrg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var word in words.Distinct())
                {
                    // Очистка от лишних символов
                    var cleanWord = CleanWord(word);
                    if (string.IsNullOrEmpty(cleanWord))
                        continue;
                        
                    if (!_wordIndex.ContainsKey(cleanWord))
                    {
                        _wordIndex[cleanWord] = new HashSet<string>();
                    }
                    _wordIndex[cleanWord].Add(normalizedOrg);
                }
            }
            
            _isInitialized = true;
        }
        
        /// <summary>
        /// Очистка слова от символов по краям
        /// </summary>
        private string CleanWord(string word)
        {
            // Убираем  пунктуацию с начала и конца слова
            // Исключение: дефисы
            return word.Trim('"', '\'', '«', '»', '(', ')', '[', ']', '{', '}', '.', ',', ';', ':', '!', '?');
        }
        
        public bool CheckBlock(List<string> block, List<string> bannedOrganizations)
        {
            Initialize(bannedOrganizations);
            
            foreach (var line in block)
            {
                if (CheckLine(line, bannedOrganizations))
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// Проверка строки
        /// </summary>
        public bool CheckLine(string line, List<string> bannedOrganizations)
        {
            Initialize(bannedOrganizations);
            
            var normalizedLine = StringNormalizer.Normalize(line);
            if (string.IsNullOrWhiteSpace(normalizedLine))
                return false;
            
            // Прямая проверка
            foreach (var bannedOrg in _normalizedBannedOrganizations)
            {
                if (normalizedLine.Contains(bannedOrg))
                {
                    if (StringNormalizer.IsExactMatch(normalizedLine, bannedOrg))
                    {
                        return true;
                    }
                }
            }
            
            // Если прямая проверка не сработала, используем инвертированный индекс
            var words = normalizedLine.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(w => CleanWord(w))
                .Where(w => !string.IsNullOrEmpty(w))
                .Distinct()
                .ToList();
            
            if (words.Count == 0)
                return false;
            
            // Собираем организации-кандидаты по словам из строки
            var candidates = new HashSet<string>();
            foreach (var word in words)
            {
                if (_wordIndex.TryGetValue(word, out var orgs))
                {
                    foreach (var org in orgs)
                    {
                        if (org.Length <= normalizedLine.Length)
                        {
                            candidates.Add(org);
                        }
                    }
                }
            }
            
            if (candidates.Count == 0)
                return false;
            
            // Проверяем на точное совпадение
            foreach (var candidate in candidates)
            {
                if (normalizedLine.Contains(candidate))
                {
                    if (StringNormalizer.IsExactMatch(normalizedLine, candidate))
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
    }
}