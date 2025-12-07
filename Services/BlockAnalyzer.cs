using System.Collections.Generic;
using System.Linq;
using OrganizationChecker.Algorithms;
using OrganizationChecker.Models;
using OrganizationChecker.Utils;

namespace OrganizationChecker.Services
{
    public static class BlockAnalyzer
    {
        public static List<BlockResult> ProcessBlocksWithAlgorithm(
            List<FileBlock> allBlocks, 
            List<string> bannedOrganizations,
            IAlgorithm algorithm)
        {
            var foundBlocks = new List<BlockResult>();

            foreach (var block in allBlocks)
            {
                if (algorithm.CheckBlock(block.Organizations, bannedOrganizations))
                {
                    foundBlocks.Add(new BlockResult
                    {
                        FileName = block.FileName,
                        BlockNumber = block.BlockNumber,
                        Organizations = block.Organizations
                    });
                }
            }

            return foundBlocks;
        }

        public static Dictionary<(string FileName, int BlockNumber), BlockDetectionInfo> CollectDetectionInfo(
            List<AlgorithmResult> algorithmResults)
        {
            var allDetectedBlocks = new Dictionary<(string FileName, int BlockNumber), BlockDetectionInfo>();

            foreach (var algorithmResult in algorithmResults)
            {
                foreach (var block in algorithmResult.FoundBlocks)
                {
                    var key = (block.FileName, block.BlockNumber);
                    if (!allDetectedBlocks.ContainsKey(key))
                    {
                        allDetectedBlocks[key] = new BlockDetectionInfo
                        {
                            FileName = block.FileName,
                            BlockNumber = block.BlockNumber,
                            Organizations = block.Organizations.Distinct().ToList()
                        };
                    }
                    
                    if (!allDetectedBlocks[key].DetectedByAlgorithms.Contains(algorithmResult.AlgorithmName))
                    {
                        allDetectedBlocks[key].DetectedByAlgorithms.Add(algorithmResult.AlgorithmName);
                    }
                }
            }

            return allDetectedBlocks;
        }

        public static string? FindBannedOrganizationInText(string text, List<string> bannedOrganizations)
        {
            var normalizedText = StringNormalizer.Normalize(text);
            
            foreach (var bannedOrg in bannedOrganizations)
            {
                var normalizedBannedOrg = StringNormalizer.Normalize(bannedOrg);
                
                if (StringNormalizer.IsExactMatch(normalizedText, normalizedBannedOrg))
                {
                    return bannedOrg;
                }
            }
            
            return null;
        }
    }
}