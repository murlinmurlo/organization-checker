using System.Collections.Generic;

namespace OrganizationChecker.Models
{
    /// <summary>
    /// Результат проверки одного блока
    /// </summary>
    public class BlockResult
    {
        public string FileName { get; set; } = string.Empty;     // Имя файла
        public int BlockNumber { get; set; }                     // Номер блока
        public List<string> Organizations { get; set; } = new List<string>(); // Список организаций в блоке
    }
}