using System.Collections.Generic;
using System.IO;
using System.Linq;
using OrganizationChecker.Models;

namespace OrganizationChecker.Services
{
    /// <summary>
    /// Сервис для парсинга файлов
    /// </summary>
    public static class FileParser
    {
        /// <summary>
        /// Находит папку Data с файлами для обработки
        /// </summary>
        public static string? FindDataDirectory()
        {
            // Проверяем текущую директорию
            var currentDir = Directory.GetCurrentDirectory();
            var dataPath = Path.Combine(currentDir, "Data");
            
            if (Directory.Exists(dataPath))
                return dataPath;
            
            // Проверяем родительскую директорию
            var parentDir = Directory.GetParent(currentDir)?.FullName;
            if (parentDir != null)
            {
                dataPath = Path.Combine(parentDir, "Data");
                if (Directory.Exists(dataPath))
                    return dataPath;
            }
            
            // Создаем папку Data в текущей директории
            Directory.CreateDirectory(dataPath);
            return dataPath;
        }

        /// <summary>
        /// Загружает все блоки из файлов
        /// </summary>
        public static List<FileBlock> LoadAllBlocks(List<string> fileNames)
        {
            var allBlocks = new List<FileBlock>();
            
            foreach (var fileName in fileNames)
            {
                var blocks = ParseFileBlocks(fileName);
                for (int i = 0; i < blocks.Count; i++)
                {
                    allBlocks.Add(new FileBlock
                    {
                        FileName = fileName,
                        BlockNumber = i + 1,
                        Organizations = blocks[i]
                    });
                }
            }
            
            return allBlocks;
        }

        /// <summary>
        /// Парсит файл на блоки
        /// </summary>
        public static List<List<string>> ParseFileBlocks(string fileName)
        {
            var blocks = new List<List<string>>();
            var currentBlock = new List<string>();
            
            var lines = File.ReadAllLines(fileName);
            
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    // Пустая строка - завершение текущего блока
                    if (currentBlock.Any())
                    {
                        blocks.Add(currentBlock);
                        currentBlock = new List<string>();
                    }
                }
                else
                {
                    // Добавление строки в текущий блок
                    currentBlock.Add(line.Trim());
                }
            }
            
            // Добавление последнего блока, если он не пустой
            if (currentBlock.Any())
            {
                blocks.Add(currentBlock);
            }
            
            return blocks;
        }
    }
}