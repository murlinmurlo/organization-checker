using System;
using System.Collections.Generic;

namespace OrganizationChecker.Models
{
    /// <summary>
    /// Модель для хранения результатов работы алгоритма
    /// </summary>
    public class AlgorithmResult
    {
        public string AlgorithmName { get; set; } = string.Empty; // Название алгоритма
        public TimeSpan ExecutionTime { get; set; }               // Время выполнения
        public int TotalBlocks { get; set; }                      // Всего проверенных блоков
        public int FoundBlocksCount { get; set; }                 // Количество найденных блоков
        public List<BlockResult> FoundBlocks { get; set; } = new List<BlockResult>(); // Список найденных блоков
    }
}