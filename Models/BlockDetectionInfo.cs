using System.Collections.Generic;

namespace OrganizationChecker.Models
{
    /// <summary>
    /// Информация о детекции блока
    /// Агрегирует результаты от нескольких алгоритмов
    /// </summary>
    public class BlockDetectionInfo
    {
        public string FileName { get; set; } = string.Empty;     // Имя файла
        public int BlockNumber { get; set; }                     // Номер блока в файле
        public List<string> Organizations { get; set; } = new List<string>(); // Список организаций в блоке
        public List<string> DetectedByAlgorithms { get; set; } = new List<string>(); // Алгоритмы, нашедшие блок
        public int DetectionCount => DetectedByAlgorithms.Count; // Количество алгоритмов, нашедших блок
    }
}