using System;
using System.Collections.Generic;
using System.Linq;
using OrganizationChecker.Utils;

namespace OrganizationChecker.Algorithms
{
    /// <summary>
    /// Реализация алгоритма Ахо-Корасик для поиска множества подстрок
    /// </summary>
    public class AhoCorasickAlgorithm : IAlgorithm
    {
        /// <summary>
        /// Узел префиксного дерева
        /// </summary>
        private class TrieNode
        {
            public Dictionary<char, TrieNode> Children = new();
            public TrieNode? FailureLink = null; // Ссылка на состояние при неудачном переходе
            public List<string> Output = new();  // Список паттернов, завершающихся в этом узле
            public bool IsTerminal => Output.Count > 0; // Является ли узел терминальным
        }

        private TrieNode _root = new();
        private List<string> _normalizedBannedOrganizations = new();
        private bool _isInitialized = false;
        private bool _trieBuilt = false;
        
        public string Name => "Ахо-Корасик";
        
        /// <summary>
        /// Нормализация и сортировка паттернов
        /// </summary>
        private void Initialize(List<string> bannedOrganizations)
        {
            if (_isInitialized) return;
            
            // Нормализуем и сортируем по длине: от длинных к коротким
            _normalizedBannedOrganizations = bannedOrganizations
                .Select(StringNormalizer.Normalize)
                .Where(org => !string.IsNullOrWhiteSpace(org))
                .OrderByDescending(org => org.Length)
                .Distinct()
                .ToList();
            
            _isInitialized = true;
        }
        
        /// <summary>
        /// Построение префиксного дерева из паттернов
        /// </summary>
        private void BuildTrie()
        {
            if (_trieBuilt) return;
            
            _root = new TrieNode();
            
            // Добавление всех паттернов
            foreach (var org in _normalizedBannedOrganizations)
            {
                var currentNode = _root;
                foreach (var ch in org)
                {
                    if (!currentNode.Children.ContainsKey(ch))
                    {
                        currentNode.Children[ch] = new TrieNode();
                    }
                    currentNode = currentNode.Children[ch];
                }
                currentNode.Output.Add(org); // Отмечаем терминальный узел
            }
            
            // Построение failure links для обработки неудачных переходов
            BuildFailureLinks();
            _trieBuilt = true;
        }
        
        /// <summary>
        /// Построение failure links
        /// </summary>
        private void BuildFailureLinks()
        {
            var queue = new Queue<TrieNode>();
            
            // Инициализация failure links для детей корня
            foreach (var child in _root.Children.Values)
            {
                child.FailureLink = _root;
                queue.Enqueue(child);
            }
            
            while (queue.Count > 0)
            {
                var currentNode = queue.Dequeue();
                
                foreach (var child in currentNode.Children)
                {
                    var failureNode = currentNode.FailureLink;
                    
                    // Ищем самый длинный суффикс, который является префиксом
                    while (failureNode != null && !failureNode.Children.ContainsKey(child.Key))
                    {
                        failureNode = failureNode.FailureLink;
                    }
                    
                    child.Value.FailureLink = failureNode?.Children.GetValueOrDefault(child.Key) ?? _root;
                    
                    // Output из failure link
                    child.Value.Output.AddRange(child.Value.FailureLink!.Output);
                    
                    queue.Enqueue(child.Value);
                }
            }
        }
        
        /// <summary>
        /// Поиск всех паттернов в тексте
        /// </summary>
        /// <param name="text">Текст для поиска</param>
        /// <returns>Список найденных паттернов</returns>
        private List<string> Search(string text)
        {
            if (!_trieBuilt)
            {
                BuildTrie();
            }
            
            var results = new HashSet<string>();
            var currentNode = _root;
            
            for (int i = 0; i < text.Length; i++)
            {
                var ch = text[i];
                
                // Переход по failure links при неудаче
                while (currentNode != _root && !currentNode.Children.ContainsKey(ch))
                {
                    currentNode = currentNode.FailureLink!;
                }
                
                if (currentNode.Children.ContainsKey(ch))
                {
                    currentNode = currentNode.Children[ch];
                }
                
                // Добавление всех паттернов, найденных в текущем узле
                if (currentNode.IsTerminal)
                {
                    foreach (var match in currentNode.Output)
                    {
                        results.Add(match);
                    }
                }
            }
            
            return results.ToList();
        }
        
        /// <summary>
        /// Проверка блока на наличие запрещенных организаций
        /// </summary>
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
        /// Проверка строки на наличие запрещенных организаций
        /// </summary>
        public bool CheckLine(string line, List<string> bannedOrganizations)
        {
            Initialize(bannedOrganizations);
            
            var normalizedLine = StringNormalizer.Normalize(line);
            
            if (string.IsNullOrWhiteSpace(normalizedLine))
                return false;
            
            // Поиск всех совпадений в строке
            var matches = Search(normalizedLine);
            
            // Проверяем границы слов
            foreach (var match in matches)
            {
                if (StringNormalizer.IsExactMatch(normalizedLine, match))
                    return true;
            }
            
            return false;
        }
    }
}