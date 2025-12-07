using System.Collections.Generic;

namespace OrganizationChecker.Algorithms
{
    /// <summary>
    /// Интерфейс алгоритма поиска запрещенных организаций
    /// </summary>
    public interface IAlgorithm
    {
        string Name { get; } // Название алгоритма для отображения
        
        /// <summary>
        /// Проверка блока строк на наличие запрещенных организаций
        /// </summary>
        bool CheckBlock(List<string> block, List<string> bannedOrganizations);
        
        /// <summary>
        /// Проверка одной строки на наличие запрещенных организаций
        /// </summary>
        bool CheckLine(string line, List<string> bannedOrganizations);
    }
}