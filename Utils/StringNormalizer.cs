using System.Text.RegularExpressions;

namespace OrganizationChecker.Utils
{
    /// <summary>
    /// Приводит строки к единому формату для сравнения
    /// </summary>
    public static class StringNormalizer
    {
        private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);
        
        /// <summary>
        /// Нормализует строку: нижний регистр, удаление лишних пробелов
        /// </summary>
        public static string Normalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return WhitespaceRegex.Replace(input.ToLowerInvariant().Trim(), " ");
        }
        
        /// <summary>
        /// Проверяет точное совпадение паттерна в тексте с учетом границ слов
        /// </summary>
        public static bool IsExactMatch(string text, string pattern)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(pattern))
                return false;
            
            text = text.ToLowerInvariant();
            pattern = pattern.ToLowerInvariant();
            
            // Поиск всех вхождений паттерна в тексте
            int index = text.IndexOf(pattern);
            while (index != -1)
            {
                // Проверка границ: символы слева и справа должны быть не буквы и не цифры
                bool isLeftBoundaryValid = index == 0 || IsBoundaryChar(text[index - 1]);
                bool isRightBoundaryValid = index + pattern.Length == text.Length || 
                                          IsBoundaryChar(text[index + pattern.Length]);
                
                // Если обе границы допустимы - это валидное вхождение
                if (isLeftBoundaryValid && isRightBoundaryValid)
                {
                    return true;
                }
                
                // Продолжаем поиск со следующей позиции
                index = text.IndexOf(pattern, index + 1);
            }
            
            return false;
        }
        
        /// <summary>
        /// Проверяет, является ли символ границей слова
        /// Граница слова - не буква и не цифра
        /// </summary>
        private static bool IsBoundaryChar(char c)
        {
            return !char.IsLetterOrDigit(c);
        }
    }
}