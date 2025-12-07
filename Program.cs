using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OrganizationChecker.Algorithms;
using OrganizationChecker.Models;
using OrganizationChecker.Services;

namespace OrganizationChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("=== ПРОВЕРКА ОРГАНИЗАЦИЙ ===");
                
                var dataPath = FileParser.FindDataDirectory();
                if (dataPath == null)
                {
                    Console.WriteLine("Папка Data не найдена. Создайте папку Data с файлами.");
                    WaitForExit();
                    return;
                }
                
                Console.WriteLine($"Используется папка данных: {dataPath}");
                
                var bannedOrganizations = LoadBannedOrganizations(dataPath);
                if (bannedOrganizations == null)
                {
                    WaitForExit();
                    return;
                }
                
                Console.WriteLine($"Загружено запрещенных организаций: {bannedOrganizations.Count}");

                var existingFiles = GetDocumentFiles(dataPath);
                if (existingFiles.Count == 0)
                {
                    Console.WriteLine("Нет доступных файлов для проверки.");
                    WaitForExit();
                    return;
                }

                var algorithms = CreateAlgorithms();
                var algorithmResults = new List<AlgorithmResult>();
                
                var allBlocks = FileParser.LoadAllBlocks(existingFiles);
                Console.WriteLine($"Всего блоков для проверки: {allBlocks.Count}");

                // Последовательный запуск каждого алгоритма для проверки блоков
                foreach (var algorithm in algorithms)
                {
                    var result = RunAlgorithm(algorithm, allBlocks, bannedOrganizations);
                    algorithmResults.Add(result);
                }

                var allDetectedBlocks = BlockAnalyzer.CollectDetectionInfo(algorithmResults);

                ResultPrinter.PrintResultsTable(algorithmResults, allDetectedBlocks);
                ResultPrinter.PrintDetailedResultsWithAlgorithms(allDetectedBlocks, algorithms, bannedOrganizations);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }
            
            WaitForExit();
        }

        /// <summary>
        /// Загружает список запрещенных организаций
        /// </summary>
        private static List<string>? LoadBannedOrganizations(string dataPath)
        {
            var bannedFilePath = Path.Combine(dataPath, "Запрещенные организации.txt");
            if (!File.Exists(bannedFilePath))
            {
                Console.WriteLine($"Файл не найден: {bannedFilePath}");
                return null;
            }
            
            return File.ReadAllLines(bannedFilePath)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();
        }

        /// <summary>
        /// Получает список файлов для проверки
        /// </summary>
        private static List<string> GetDocumentFiles(string dataPath)
        {
            var documentFiles = new[]
            {
                Path.Combine(dataPath, "Список документов ams.txt"),
                Path.Combine(dataPath, "Список документов arb.txt"),
                Path.Combine(dataPath, "Список документов r002.txt")
            };

            var existingFiles = new List<string>();
            foreach (var file in documentFiles)
            {
                if (File.Exists(file))
                {
                    existingFiles.Add(file);
                    Console.WriteLine($"Найден файл: {Path.GetFileName(file)}");
                }
                else
                {
                    Console.WriteLine($"ВНИМАНИЕ: Файл не найден: {Path.GetFileName(file)}");
                }
            }

            return existingFiles;
        }

        /// <summary>
        /// Создает экземпляры алгоритмов
        /// </summary>
        private static List<IAlgorithm> CreateAlgorithms()
        {
            return new List<IAlgorithm>
            {
                new AhoCorasickAlgorithm(),   // Алгоритм Ахо-Корасик
                new InvertedIndexAlgorithm(), // Алгоритм с инвертированным индексом
                new SimdSearchAlgorithm(),    // SIMD-ускоренный алгоритм поиска
            };
        }

        /// <summary>
        /// Запускает алгоритм проверки блоков и измеряет время выполнения
        /// </summary>
        private static AlgorithmResult RunAlgorithm(IAlgorithm algorithm, List<FileBlock> allBlocks, List<string> bannedOrganizations)
        {
            Console.WriteLine($"\n--- Проверка алгоритмом: {algorithm.Name} ---");
            
            // Измерение времени выполнения алгоритма
            var stopwatch = Stopwatch.StartNew();
            var foundBlocks = BlockAnalyzer.ProcessBlocksWithAlgorithm(allBlocks, bannedOrganizations, algorithm);
            stopwatch.Stop();
            
            Console.WriteLine($"Проверено блоков: {allBlocks.Count}");
            Console.WriteLine($"Найдено блоков с запрещенными организациями: {foundBlocks.Count}");
            Console.WriteLine($"Время выполнения: {stopwatch.ElapsedMilliseconds} мс");
            
            return new AlgorithmResult
            {
                AlgorithmName = algorithm.Name,
                ExecutionTime = stopwatch.Elapsed,
                TotalBlocks = allBlocks.Count,
                FoundBlocksCount = foundBlocks.Count,
                FoundBlocks = foundBlocks
            };
        }

        /// <summary>
        /// Завершение программы
        /// </summary>
        private static void WaitForExit()
        {
            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}