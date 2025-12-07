using System.Collections.Generic;

namespace OrganizationChecker.Models
{
    /// <summary>
    /// Модель блока данных из файла
    /// </summary>
    public class FileBlock
    {
        public string FileName { get; set; } = string.Empty;     // Имя исходного файла
        public int BlockNumber { get; set; }                     // Порядковый номер блока в файле
        public List<string> Organizations { get; set; } = new List<string>(); // Список строк в блоке
    }
}