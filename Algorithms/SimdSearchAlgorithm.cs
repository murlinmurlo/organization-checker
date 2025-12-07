using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using OrganizationChecker.Utils;

namespace OrganizationChecker.Algorithms
{
    /// <summary>
    /// SIMD-ускоренный алгоритм поиска
    /// Использует векторные инструкции процессора
    /// </summary>
    public class SimdSearchAlgorithm : IAlgorithm
    {
        // Байтевые представления запрещенных организаций
        private List<byte[]> _normalizedBannedBytes = new();
        private List<string> _normalizedBannedOrganizations = new();
        private bool _isInitialized = false;
        
        public string Name => "SIMD-ускоренный поиск";
        
        /// <summary>
        /// Подготовка байтовых представлений
        /// </summary>
        private void Initialize(List<string> bannedOrganizations)
        {
            if (_isInitialized) return;
            
            _normalizedBannedOrganizations = bannedOrganizations
                .Select(StringNormalizer.Normalize)
                .Where(org => !string.IsNullOrWhiteSpace(org))
                .Distinct()
                .ToList();
            
            // Конвертируем строки в байты для SIMD операций
            _normalizedBannedBytes = _normalizedBannedOrganizations
                .Select(org => Encoding.UTF8.GetBytes(org))
                .ToList();
            
            _isInitialized = true;
        }
        
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
        /// Проверка строки с использованием SIMD инструкций
        /// </summary>
        public bool CheckLine(string line, List<string> bannedOrganizations)
        {
            Initialize(bannedOrganizations);
            
            var normalizedLine = StringNormalizer.Normalize(line);
            if (string.IsNullOrWhiteSpace(normalizedLine))
                return false;
            
            var lineBytes = Encoding.UTF8.GetBytes(normalizedLine);
            
            // Проверяем каждую организацию с SIMD ускорением
            for (int i = 0; i < _normalizedBannedBytes.Count; i++)
            {
                var bannedBytes = _normalizedBannedBytes[i];
                var bannedOrg = _normalizedBannedOrganizations[i];
                
                // SIMD поиск + проверка границ слов
                if (ContainsBytesSimd(lineBytes, bannedBytes) && 
                    StringNormalizer.IsExactMatch(normalizedLine, bannedOrg))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Поиск байтового паттерна в тексте с использованием SIMD
        /// </summary>
        private unsafe bool ContainsBytesSimd(byte[] textBytes, byte[] patternBytes)
        {
            if (patternBytes.Length == 0 || textBytes.Length < patternBytes.Length)
                return false;
            
            // Для коротких паттернов используем обычный поиск
            if (patternBytes.Length < 16 || textBytes.Length < patternBytes.Length)
            {
                return ContainsBytesSimple(textBytes, patternBytes);
            }
            
            // Для длинных паттернов используем SIMD
            return ContainsBytesSimdExact(textBytes, patternBytes);
        }
        
        /// <summary>
        /// Поиск с использованием SIMD инструкций
        /// </summary>
        private unsafe bool ContainsBytesSimdExact(byte[] textBytes, byte[] patternBytes)
        {
            fixed (byte* textPtr = textBytes)
            fixed (byte* patternPtr = patternBytes)
            {
                int patternLength = patternBytes.Length;
                int textLength = textBytes.Length;
                
                // Используем AVX2 (256-битные векторы) для паттернов >= 32 байт
                if (Avx2.IsSupported && patternLength >= 32)
                {
                    return ContainsBytesAvx2Exact(textPtr, patternPtr, textLength, patternLength);
                }
                // Используем SSE2 (128-битные векторы) для паттернов >= 16 байт
                else if (Sse2.IsSupported && patternLength >= 16)
                {
                    return ContainsBytesSse2Exact(textPtr, patternPtr, textLength, patternLength);
                }
                else
                {
                    return ContainsBytesSimple(textBytes, patternBytes);
                }
            }
        }
        
        /// <summary>
        /// Поиск с использованием AVX2 инструкций (256 бит = 32 байта)
        /// </summary>
        private unsafe bool ContainsBytesAvx2Exact(byte* textPtr, byte* patternPtr, int textLength, int patternLength)
        {
            var patternVec = Avx2.LoadVector256(patternPtr);
            
            // Проверяем все позиции в тексте
            for (int i = 0; i <= textLength - patternLength; i++)
            {
                var textVec = Avx2.LoadVector256(textPtr + i);
                var equalMask = Avx2.CompareEqual(textVec, patternVec);
                
                // Проверяем, что все 32 байта совпали
                if (Avx2.MoveMask(equalMask) == -1)
                {
                    // Если паттерн длиннее 32 байт, проверяем остаток
                    if (patternLength > 32)
                    {
                        if (CheckRemainingBytes(textPtr + i + 32, patternPtr + 32, patternLength - 32))
                            return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Поиск с использованием SSE2 инструкций (128 бит = 16 байт)
        /// </summary>
        private unsafe bool ContainsBytesSse2Exact(byte* textPtr, byte* patternPtr, int textLength, int patternLength)
        {
            var patternVec = Sse2.LoadVector128(patternPtr);
            
            for (int i = 0; i <= textLength - patternLength; i++)
            {
                var textVec = Sse2.LoadVector128(textPtr + i);
                var equalMask = Sse2.CompareEqual(textVec, patternVec);
                
                // Проверяем, что все 16 байт совпали
                if (Sse2.MoveMask(equalMask) == 0xFFFF)
                {
                    // Если паттерн длиннее 16 байт, проверяем остаток
                    if (patternLength > 16)
                    {
                        if (CheckRemainingBytes(textPtr + i + 16, patternPtr + 16, patternLength - 16))
                            return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Проверка оставшихся байт после SIMD-сравнения
        /// </summary>
        private unsafe bool CheckRemainingBytes(byte* textPtr, byte* patternPtr, int length)
        {
            for (int i = 0; i < length; i++)
            {
                if (*(textPtr + i) != *(patternPtr + i))
                    return false;
            }
            return true;
        }
        
        /// <summary>
        /// Линейный поиск для коротких паттернов
        /// </summary>
        private bool ContainsBytesSimple(byte[] textBytes, byte[] patternBytes)
        {
            if (patternBytes.Length == 0) return false;
            
            for (int i = 0; i <= textBytes.Length - patternBytes.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < patternBytes.Length; j++)
                {
                    if (textBytes[i + j] != patternBytes[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match) return true;
            }
            return false;
        }
    }
}