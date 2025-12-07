using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OrganizationChecker.Algorithms;
using OrganizationChecker.Models;

namespace OrganizationChecker.Services
{
    /// <summary>
    /// Сервис для форматированного вывода результатов
    /// </summary>
    public static class ResultPrinter
    {
        /// <summary>
        /// Выводит сравнительную таблицу результатов
        /// </summary>
        public static void PrintResultsTable(List<AlgorithmResult> results, 
            Dictionary<(string FileName, int BlockNumber), BlockDetectionInfo> detectedBlocks)
        {
            Console.WriteLine("\n=== СРАВНЕНИЕ АЛГОРИТМОВ ===");
            Console.WriteLine(new string('-', 90));
            Console.WriteLine($"| {"Алгоритм", -40} | {"Время (мс)", -12} | {"Блоков", -8} | {"Найдено", -8} |");
            Console.WriteLine(new string('-', 90));
            
            // Вывод для каждого алгоритма
            foreach (var result in results)
            {
                Console.WriteLine($"| {result.AlgorithmName, -40} | {result.ExecutionTime.TotalMilliseconds, 10:F2} | {result.TotalBlocks, 8} | {result.FoundBlocksCount, 8} |");
            }
            
            Console.WriteLine(new string('-', 90));
            
            // Расхождения между алгоритмами
            Console.WriteLine("\nАНАЛИЗ РЕЗУЛЬТАТОВ:");
            
            var blocksWithDiscrepancies = detectedBlocks.Where(kv => 
                kv.Value.DetectionCount < results.Count && kv.Value.DetectionCount > 0).ToList();
            
            if (blocksWithDiscrepancies.Any())
            {
                Console.WriteLine($"Блоки, где алгоритмы дали разные результаты: {blocksWithDiscrepancies.Count}");
                
                // Группировка по количеству алгоритмов, нашедших блок
                var groups = blocksWithDiscrepancies
                    .GroupBy(kv => kv.Value.DetectionCount)
                    .OrderBy(g => g.Key);
                    
                foreach (var group in groups)
                {
                    Console.WriteLine($"  Найдено {group.Key} алгоритмами: {group.Count()} блоков");
                }
            }
            else if (detectedBlocks.Any())
            {
                Console.WriteLine("Все алгоритмы нашли одинаковые блоки.");
            }
        }

        /// <summary>
        /// Выводит детальную информацию о найденных блоках
        /// </summary>
        public static void PrintDetailedResultsWithAlgorithms(
            Dictionary<(string FileName, int BlockNumber), BlockDetectionInfo> detectedBlocks,
            List<IAlgorithm> algorithms,
            List<string> bannedOrganizations)
        {
            Console.WriteLine($"\n=== ДЕТАЛЬНЫЕ РЕЗУЛЬТАТЫ ===");
            Console.WriteLine($"Найдено блоков с запрещенными организациями: {detectedBlocks.Count}");
            Console.WriteLine("Запрещенные организации выделены ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("красным цветом");
            Console.ResetColor();
            
            Console.WriteLine(".");
            Console.WriteLine(new string('=', 100));

            // Сортировка блоков по имени файла и номеру
            var sortedBlocks = detectedBlocks.Values
                .OrderBy(b => b.FileName)
                .ThenBy(b => b.BlockNumber)
                .ToList();

            foreach (var blockInfo in sortedBlocks)
            {
                Console.WriteLine($"\nФайл: {Path.GetFileName(blockInfo.FileName)}");
                Console.WriteLine($"Блок №{blockInfo.BlockNumber}");
                
                // Вывод информации об обнаруживших алгоритмах
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Найден алгоритмами: ");
                var algorithmNames = blockInfo.DetectedByAlgorithms.OrderBy(a => a).ToList();
                for (int i = 0; i < algorithmNames.Count; i++)
                {
                    if (i > 0) Console.Write(", ");
                    Console.Write(algorithmNames[i]);
                }
                Console.WriteLine($" ({blockInfo.DetectionCount} из {algorithms.Count})");
                Console.ResetColor();
                
                Console.WriteLine(new string('-', 100));
                Console.WriteLine("Организации в блоке:");
                
                int bannedCount = 0;
                foreach (var org in blockInfo.Organizations.Distinct())
                {
                    var foundBannedOrg = BlockAnalyzer.FindBannedOrganizationInText(org, bannedOrganizations);
                    
                    if (!string.IsNullOrEmpty(foundBannedOrg))
                    {
                        bannedCount++;
                        // Подсветка запрещенной организации
                        HighlightBannedOrganizationInText(org, foundBannedOrg);
                    }
                    else
                    {
                        Console.WriteLine($"  • {org}");
                    }
                }
                
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\nВ этом блоке найдено запрещенных организаций: {bannedCount}");
                Console.ResetColor();
                Console.WriteLine(new string('-', 100));
            }
        }

        /// <summary>
        /// Подсвечивает запрещенную организацию
        /// </summary>
        public static void HighlightBannedOrganizationInText(string text, string bannedOrg)
        {
            // Поиск позиции запрещенной организации
            int index = text.IndexOf(bannedOrg, StringComparison.OrdinalIgnoreCase);
            
            if (index >= 0)
            {
                Console.Write("  • ");
                if (index > 0)
                {
                    // Вывод части текста до запрещенной организации
                    Console.Write(text.Substring(0, index));
                }
                
                // Вывод запрещенной организации красным цветом
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(text.Substring(index, bannedOrg.Length));
                Console.ResetColor();
                
                // Вывод части текста после запрещенной организации
                if (index + bannedOrg.Length < text.Length)
                {
                    Console.Write(text.Substring(index + bannedOrg.Length));
                }
                Console.WriteLine();
            }
            else
            {
                // Если точная позиция не найдена
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  • {text}");
                Console.ResetColor();
            }
        }
    }
}