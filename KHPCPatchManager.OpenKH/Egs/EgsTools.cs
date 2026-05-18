//using KHPCPatchManager.OpenKH.Common;
//using KHPCPatchManager.OpenKH.Kh1;
using KHPCPatchManager.OpenKH.KH2;
using KHPCPatchManager.OpenKH.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using KHPCPatchManager.OpenKH.Xe.BinaryMapper;

namespace KHPCPatchManager.OpenKH.Egs;

public class EgsTools
{
    private const string RAW_FILES_FOLDER_NAME = "raw";
    private const string ORIGINAL_FILES_FOLDER_NAME = "original";
    private const string REMASTERED_FILES_FOLDER_NAME = "remastered";

    #region MD5 names

    private static readonly IEnumerable<string> KH2Names = EgsHdAsset.KH2Names
        .Concat(EgsHdAsset.KH2Names.Where(x => x.Contains("anm/")).SelectMany(x => new string[]
        {
                    x.Replace("anm/", "anm/jp/"),
                    x.Replace("anm/", "anm/us/"),
                    x.Replace("anm/", "anm/fm/")
        }))
        .Concat(KH2.Constants.Languages.SelectMany(lang =>
            KH2.Constants.WorldIds.SelectMany(world =>
                Enumerable.Range(0, 64).Select(index => Path.Combine("ard", lang).Replace('\\', '/') + $"/{world}{index:D02}.ard"))))
        .Concat(KH2.Constants.Languages.SelectMany(lang =>
            KH2.Constants.WorldIds.SelectMany(world =>
                Enumerable.Range(0, 64).Select(index => Path.Combine("map", lang).Replace('\\', '/') + $"/{world}{index:D02}.map"))))
        .Concat(KH2.Constants.Languages.SelectMany(lang =>
            KH2.Constants.WorldIds.SelectMany(world =>
                Enumerable.Range(0, 64).Select(index => Path.Combine("map", lang).Replace('\\', '/') + $"/{world}{index:D02}.bar"))))
        .Concat(EgsHdAsset.KH2Names.Where(x => x.StartsWith("bgm/")).Select(x => x.Replace(".bgm", ".win32.scd")))
        .Concat(EgsHdAsset.KH2Names.Where(x => x.StartsWith("se/")).Select(x => x.Replace(".seb", ".win32.scd")))
        .Concat(EgsHdAsset.KH2Names.Where(x => x.StartsWith("vagstream/")).Select(x => x.Replace(".vas", ".win32.scd")))
        .Concat(EgsHdAsset.KH2Names.Where(x => x.StartsWith("gumibattle/se/")).Select(x => x.Replace(".seb", ".win32.scd")))
        .Concat(EgsHdAsset.KH2Names.Where(x => x.StartsWith("voice/")).Select(x => x
            .Replace(".vag", ".win32.scd")
            .Replace(".vsb", ".win32.scd")))
        .Concat(new string[]
        {
                    "item-011.imd",
                    "KH2.IDX",
                    "ICON/ICON0.PNG",
                    "ICON/ICON0_EN.png",
        });

    private static readonly Dictionary<string, string> Names = KH2Names
        //.Concat(Idx1Name.Names)
        .Concat(EgsHdAsset.DddNames)
        .Concat(EgsHdAsset.BbsNames)
        .Concat(EgsHdAsset.RecomNames)
        .Concat(EgsHdAsset.MareNames)
        .Concat(EgsHdAsset.SettingMenuNames)
        .Concat(EgsHdAsset.TheaterNames)
        .Concat(EgsHdAsset.Kh1AdditionalNames)
        .Concat(EgsHdAsset.Launcher28Names)
        .Concat(EgsHdAsset.CustomNames)
        .Concat(new string[] { "dummy.txt" })
        .Distinct()
        .ToDictionary(x => Helpers.ToString(KHPCPatchManager.OpenKH.Extensions.Extensions.GetHashData(Encoding.UTF8.GetBytes(x))), x => x);

    #endregion

    #region Extract

    public static void Extract(string inputHed, string output, bool doNotExtractAgain = false)
    {
        var outputDir = output ?? Path.GetFileNameWithoutExtension(inputHed);
        using var hedStream = File.OpenRead(inputHed);
        using var img = File.OpenRead(Path.ChangeExtension(inputHed, "pkg"));

        foreach (var entry in Hed.Read(hedStream))
        {
            var hash = Helpers.ToString(entry.MD5);
            if (!Names.TryGetValue(hash, out var fileName))
                fileName = $"{hash}.dat";

            var outputFileName = Path.Combine(outputDir, ORIGINAL_FILES_FOLDER_NAME, fileName);

            if (doNotExtractAgain && File.Exists(outputFileName))
                continue;

            Console.WriteLine(outputFileName);
            CreateDirectoryForFile(outputFileName);

            var hdAsset = new EgsHdAsset(img.SetPosition(entry.Offset));

            File.Create(outputFileName).Using(stream => stream.Write(hdAsset.OriginalData));

            outputFileName = Path.Combine(outputDir, REMASTERED_FILES_FOLDER_NAME, fileName);

            foreach (var asset in hdAsset.Assets)
            {
                var outputFileNameRemastered = Path.Combine(GetHDAssetFolder(outputFileName), asset);

                Console.WriteLine(outputFileNameRemastered);
                CreateDirectoryForFile(outputFileNameRemastered);

                var assetData = hdAsset.RemasteredAssetsDecompressedData[asset];
                File.Create(outputFileNameRemastered).Using(stream => stream.Write(assetData));
            }
        }
    }

    public static void ExtractRAW(string inputHed, string output, bool doNotExtractAgain = false)
    {
        var outputDir = output ?? Path.GetFileNameWithoutExtension(inputHed);
        using var hedStream = File.OpenRead(inputHed);
        using var img = File.OpenRead(Path.ChangeExtension(inputHed, "pkg"));

        foreach (var entry in Hed.Read(hedStream))
        {
            var hash = Helpers.ToString(entry.MD5);
            if (!Names.TryGetValue(hash, out var fileName))
                fileName = $"{hash}.dat";

            var outputFileName = Path.Combine(outputDir, RAW_FILES_FOLDER_NAME, fileName);

            if (doNotExtractAgain && File.Exists(outputFileName))
                continue;

            Console.WriteLine(outputFileName);
            CreateDirectoryForFile(outputFileName);

            byte[] rawData = img.ReadBytes(entry.DataLength);
            File.Create(outputFileName).Using(stream => stream.Write(rawData));
        }
    }

    private static string GetHDAssetFolder(string assetFile)
    {
        var parentFolder = Directory.GetParent(assetFile).FullName;
        var assetFolderName = Path.Combine(parentFolder, $"{Path.GetFileName(assetFile)}");

        return assetFolderName;
    }

    private static void CreateDirectoryForFile(string fileName)
    {
        var directoryName = Path.GetDirectoryName(fileName);
        if (!Directory.Exists(directoryName))
            Directory.CreateDirectory(directoryName);
    }

    #endregion

    #region Patch

    public static void Patch(string pkgFile, string inputFolder, string outputFolder, MyBackgroundWorker bgw1 = null)
    {
        // Get files to inject in the PKG to detect if we want to include new files or not
        // We only get the original files as for me it doesn't make sense to include
        // new "remastered" asset since it must be linked to an original one
        var patchFiles = new List<string>();
        if (ZipManager.DirectoryExists(Path.Combine(inputFolder, ORIGINAL_FILES_FOLDER_NAME)))
            patchFiles.AddRange(ZipManager.GetFiles(Path.Combine(inputFolder, ORIGINAL_FILES_FOLDER_NAME)).ToList());

        if (ZipManager.DirectoryExists(Path.Combine(inputFolder, RAW_FILES_FOLDER_NAME)))
            patchFiles.AddRange(ZipManager.GetFiles(Path.Combine(inputFolder, RAW_FILES_FOLDER_NAME)).ToList());

        var filenames = new List<string>();

        var remasteredFilesFolder = Path.Combine(inputFolder, REMASTERED_FILES_FOLDER_NAME);

        var outputDir = outputFolder ?? Path.GetFileNameWithoutExtension(pkgFile);

        var hedFile = Path.ChangeExtension(pkgFile, "hed");
        using var hedStream = File.OpenRead(hedFile);
        using var pkgStream = File.OpenRead(pkgFile);

        var hedHeaders = Hed.Read(hedStream).ToList();

        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        File.WriteAllText("custom_hd_assets.txt", "");

        using var patchedHedStream = File.Create(Path.Combine(outputDir, Path.GetFileName(hedFile)));
        using var patchedPkgStream = File.Create(Path.Combine(outputDir, Path.GetFileName(pkgFile)));

        foreach (var hedHeader in hedHeaders)
        {
            if (bgw1 != null) bgw1.ReportProgress(0, bgw1.PKG + ": " + (hedHeaders.IndexOf(hedHeader) + 1) + "/" + hedHeaders.Count);
            var hash = Helpers.ToString(hedHeader.MD5);
            bool isNameUnknown = false;

            // We don't know this filename, we ignore it
            if (!Names.TryGetValue(hash, out var filename))
            {
                Console.WriteLine($"Unknown filename (hash: {hash})");
                var tempname = patchFiles.Find(x => Helpers.CreateMD5(x) == hash);
                if (tempname != null)
                {
                    filename = tempname;
                    Console.WriteLine($"Wait, actually I found it in your patch: {filename}");
                    File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/custom_filenames.txt"), filename + "\n");
                }
                else
                {
                    isNameUnknown = true;
                    //continue;
                }
            }

            if (patchFiles.Contains(filename))
            {
                patchFiles.Remove(filename);
            }

            filenames.Add(filename);

            var asset = new EgsHdAsset(pkgStream.SetPosition(hedHeader.Offset));

            if (hedHeader.DataLength > 0)
            {
                ReplaceFile(inputFolder, filename, patchedHedStream, patchedPkgStream, asset, hedHeader, isNameUnknown);
            }
            else
            {
                Console.WriteLine($"Skipped: {filename}");
            }
        }

        // Add all files that are not in the original HED file and inject them in the PKG stream too
        foreach (var filename in patchFiles)
        {
            AddFile(inputFolder, filename, patchedHedStream, patchedPkgStream);
            Console.WriteLine($"Added a new file: {filename}");
        }
    }

    private static Hed.Entry AddFile(string inputFolder, string filename, FileStream hedStream, FileStream pkgStream, bool shouldCompressData = false, bool shouldEncryptData = false)
    {
        var completeFilePath = Path.Combine(inputFolder, ORIGINAL_FILES_FOLDER_NAME, filename);
        var completeRawFilePath = Path.Combine(inputFolder, RAW_FILES_FOLDER_NAME, filename);
        var offset = pkgStream.Position;
        int actualLength = 0;

        #region Data
        if (ZipManager.FileExists(completeFilePath))
        {
            using var newFileStream = ZipManager.FileReadStream(completeFilePath);
            actualLength = (int)newFileStream.Length;

            bool RemasterExist = false;
            string RemasteredPath = completeFilePath.Replace("\\original\\", "\\remastered\\");
            if (ZipManager.DirectoryExists(RemasteredPath))
                RemasterExist = true;

            var header = new EgsHdAsset.Header()
            {
                // CompressedLenght => -2: no compression and encryption, -1: no compression 
                CompressedLength = !shouldCompressData ? !shouldEncryptData ? -2 : -1 : 0,
                DecompressedLength = (int)newFileStream.Length,
                RemasteredAssetCount = 0,
                CreationDate = -1
            };

            var decompressedData = newFileStream.ReadAllBytes();
            // Make sure to align asset data on 16 bytes
            if (decompressedData.Length % 0x10 != 0)
            {
                int diff = 16 - (decompressedData.Length % 0x10);
                byte[] paddedData = new byte[decompressedData.Length + diff];
                decompressedData.CopyTo(paddedData, 0);
                Enumerable.Repeat((byte)0xCD, diff).ToArray().CopyTo(paddedData, decompressedData.Length);
                decompressedData = paddedData;
            }

            var compressedData = decompressedData.ToArray();

            if (shouldCompressData)
            {
                compressedData = Helpers.CompressData(decompressedData);
                header.CompressedLength = compressedData.Length;
            }

            SDasset sdasset = new SDasset(filename, decompressedData, RemasterExist);
            RemasterExist = false;

            if (sdasset != null && !sdasset.Invalid) header.RemasteredAssetCount = sdasset.AssetCount;

            // Encrypt and write current file data in the PKG stream
            // The seed used for encryption is the original data header
            var seed = new MemoryStream();
            BinaryMapping.WriteObject<EgsHdAsset.Header>(seed, header);

            var encryptionSeed = seed.ReadAllBytes();
            var encryptedData = header.CompressedLength > -2 ? EgsEncryption.Encrypt(compressedData, encryptionSeed) : compressedData;

            // Write original file header
            BinaryMapping.WriteObject<EgsHdAsset.Header>(pkgStream, header);

            if (header.RemasteredAssetCount > 0)
            {
                // Create an "Asset" to pass to ReplaceRemasteredAssets
                EgsHdAsset asset = new EgsHdAsset(header, decompressedData, encryptedData, encryptionSeed);
                ReplaceRemasteredAssets(inputFolder, filename, asset, pkgStream, encryptionSeed, encryptedData, sdasset);
            }
            else
            {
                // Make sure to write the original file after remastered assets headers
                pkgStream.Write(encryptedData);
            }
        }
        else if (ZipManager.FileExists(completeRawFilePath))
        {
            var newFileStream = ZipManager.FileReadAllBytes(completeRawFilePath);
            actualLength = BitConverter.ToInt32(newFileStream, 0);

            pkgStream.Write(newFileStream);
        }

        #endregion

        // Write a new entry in the HED stream
        var hedHeader = new Hed.Entry()
        {
            MD5 = Helpers.ToBytes(Helpers.CreateMD5(filename)),
            ActualLength = actualLength,
            DataLength = (int)(pkgStream.Position - offset),
            Offset = offset
        };

        if (!Names.TryGetValue(Helpers.ToString(hedHeader.MD5), out var existingfilename))
        {
            File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/custom_filenames.txt"), filename + "\n");
        }

        BinaryMapping.WriteObject<Hed.Entry>(hedStream, hedHeader);

        return hedHeader;
    }

    private static Hed.Entry ReplaceFile(
        string inputFolder,
        string filename,
        FileStream hedStream,
        FileStream pkgStream,
        EgsHdAsset asset,
        Hed.Entry originalHedHeader = null,
        bool isNameUnknown = false)
    {
        var completeFilePath = Path.Combine(inputFolder, ORIGINAL_FILES_FOLDER_NAME, filename);
        var completeRawFilePath = Path.Combine(inputFolder, RAW_FILES_FOLDER_NAME, filename);

        var offset = pkgStream.Position;
        var originalHeader = asset.OriginalAssetHeader;

        // Clone the original asset header
        var header = new EgsHdAsset.Header()
        {
            CompressedLength = originalHeader.CompressedLength,
            DecompressedLength = originalHeader.DecompressedLength,
            RemasteredAssetCount = originalHeader.RemasteredAssetCount,
            CreationDate = originalHeader.CreationDate
        };

        // Use the base original asset data by default
        var decompressedData = asset.OriginalData;
        var encryptedData = asset.OriginalRawData;
        var encryptionSeed = asset.Seed;

        int actualLength = 0;

        SDasset sdasset = null;
        // We want to replace the original file
        if (ZipManager.FileExists(completeFilePath))
        {
            bool RemasterExist = false;

            Console.WriteLine($"Replacing original: {filename}!");
            string RemasteredPath = completeFilePath.Replace("\\original\\", "\\remastered\\");
            if (ZipManager.DirectoryExists(RemasteredPath))
            {
                Console.WriteLine($"Remastered Folder Exists! Path: {RemasteredPath}");
                RemasterExist = true;
            }

            using var newFileStream = ZipManager.FileReadStream(completeFilePath);
            decompressedData = newFileStream.ReadAllBytes();
            // Make sure to align asset data on 16 bytes
            if (decompressedData.Length % 0x10 != 0)
            {
                int diff = 16 - (decompressedData.Length % 0x10);
                byte[] paddedData = new byte[decompressedData.Length + diff];
                decompressedData.CopyTo(paddedData, 0);
                Enumerable.Repeat((byte)0xCD, diff).ToArray().CopyTo(paddedData, decompressedData.Length);
                decompressedData = paddedData;
            }

            sdasset = new SDasset(filename, decompressedData, RemasterExist);

            if (sdasset != null && !sdasset.Invalid) header.RemasteredAssetCount = sdasset.AssetCount;

            var compressedData = decompressedData.ToArray();
            var compressedDataLenght = originalHeader.CompressedLength;

            // CompressedLenght => -2: no compression and encryption, -1: no compression 
            if (originalHeader.CompressedLength > -1)
            {
                compressedData = Helpers.CompressData(decompressedData);
                compressedDataLenght = compressedData.Length;
            }

            header.CompressedLength = compressedDataLenght;
            header.DecompressedLength = decompressedData.Length;
            // Encrypt and write current file data in the PKG stream

            // The seed used for encryption is the original data header
            var seed = new MemoryStream();
            BinaryMapping.WriteObject<EgsHdAsset.Header>(seed, header);

            encryptionSeed = seed.ReadAllBytes();
            encryptedData = header.CompressedLength > -2 ? EgsEncryption.Encrypt(compressedData, encryptionSeed) : compressedData;
        }

        if (ZipManager.FileExists(completeRawFilePath))
        {
            var rawFileStream = ZipManager.FileReadAllBytes(completeRawFilePath);
            actualLength = BitConverter.ToInt32(rawFileStream, 0);

            pkgStream.Write(rawFileStream);
        }
        else
        {
            // Write original file header
            BinaryMapping.WriteObject<EgsHdAsset.Header>(pkgStream, header);

            var remasteredHeaders = new List<EgsHdAsset.RemasteredEntry>();

            // Is there remastered assets?
            if (header.RemasteredAssetCount > 0)
            {
                remasteredHeaders = ReplaceRemasteredAssets(inputFolder, filename, asset, pkgStream, encryptionSeed, encryptedData, sdasset);
            }
            else
            {
                // Make sure to write the original file after remastered assets headers
                pkgStream.Write(encryptedData);
            }
            actualLength = decompressedData.Length;
        }

        // Write a new entry in the HED stream
        var hedHeader = new Hed.Entry()
        {
            MD5 = Helpers.ToBytes(isNameUnknown ? filename : Helpers.CreateMD5(filename)),
            ActualLength = actualLength,
            DataLength = (int)(pkgStream.Position - offset),
            Offset = offset
        };

        // For unknown reason, some files have a data length of 0
        if (originalHedHeader.DataLength == 0)
        {
            Console.WriteLine($"{filename} => {originalHedHeader.ActualLength} ({originalHedHeader.DataLength})");

            hedHeader.ActualLength = originalHedHeader.ActualLength;
            hedHeader.DataLength = originalHedHeader.DataLength;
        }

        BinaryMapping.WriteObject<Hed.Entry>(hedStream, hedHeader);

        return hedHeader;
    }

    private static List<EgsHdAsset.RemasteredEntry> ReplaceRemasteredAssets(string inputFolder, string originalFile, EgsHdAsset asset, FileStream pkgStream, byte[] seed, byte[] originalAssetData, SDasset sdasset)
    {
        var newRemasteredHeaders = new List<EgsHdAsset.RemasteredEntry>();
        var oldRemasteredHeaders = new List<EgsHdAsset.RemasteredEntry>();
        var relativePath = Helpers.GetRelativePath(originalFile, Path.Combine(inputFolder, ORIGINAL_FILES_FOLDER_NAME));
        var remasteredAssetsFolder = Path.Combine(inputFolder, REMASTERED_FILES_FOLDER_NAME, relativePath);

        var assetConfig = new AssetConfig(remasteredAssetsFolder);
        int assetCount = assetConfig.ForceAssetCount == -1 ? sdasset != null && !sdasset.Invalid ? sdasset.AssetCount : 0 : assetConfig.ForceAssetCount;

        var allRemasteredAssetsData = new MemoryStream();

        foreach (var remasteredAssetHeader in asset.RemasteredAssetHeaders.Values)
        {
            oldRemasteredHeaders.Add(remasteredAssetHeader);
        }

        //At the moment this only applies on fresh PKGs (or ones that haven't been patched with this modded MDLX before, otherwise we'd neet to analyse ALL MDLX files)
        if (sdasset != null && !sdasset.Invalid && assetConfig.UpdateAssetCountFromOriginal)
        {
            File.AppendAllText("custom_hd_assets.txt", "HD assets for: " + originalFile + "\n");
            while (oldRemasteredHeaders.Count > assetCount)
            {
                File.AppendAllText("custom_hd_assets.txt", "Removing: -" + (oldRemasteredHeaders.Count - 1) + ".dds\n");
                oldRemasteredHeaders.RemoveAt(oldRemasteredHeaders.Count - 1);
            }
            while (oldRemasteredHeaders.Count < assetCount)
            {
                var newRemasteredAssetHeader = new EgsHdAsset.RemasteredEntry()
                {
                    CompressedLength = 0,
                    DecompressedLength = 0,
                    Name = "-" + oldRemasteredHeaders.Count + ".dds",
                    Offset = 0,
                    OriginalAssetOffset = 0
                };
                File.AppendAllText("custom_hd_assets.txt", "Adding: -" + oldRemasteredHeaders.Count + ".dds\n");
                oldRemasteredHeaders.Add(newRemasteredAssetHeader);
            }
            File.AppendAllText("custom_hd_assets.txt", "\n");
        }

        // 0x30 is the size of this header
        var totalRemasteredAssetHeadersSize = oldRemasteredHeaders.Count() * 0x30;
        // This offset is relative to the original asset data
        var offset = totalRemasteredAssetHeadersSize + 0x10 + asset.OriginalAssetHeader.DecompressedLength;

        List<string> remasteredNames = new List<string>();

        //if (sdasset != null && !sdasset.Invalid && sdasset.NamesAudio != null && sdasset.NamesAudio.Count > 0) remasteredNames.AddRange(sdasset.NamesAudio);

        if (asset.RemasteredAssetHeaders.Values.Count == 0 || offset != asset.RemasteredAssetHeaders.Values.First().Offset) remasteredNames.Clear();
        //grab list of full file paths from current remasteredAssetsFolder path and add them to a list.
        //we use this list later to correctly add the file names to the PKG.
        if (ZipManager.DirectoryExists(remasteredAssetsFolder) && ZipManager.GetFiles(remasteredAssetsFolder).ToList().Count > 0) //only do this if there are actually file in it.
        {
            remasteredNames.AddRange(ZipManager.GetFiles(remasteredAssetsFolder).ToList());

            for (int l = 0; l < remasteredNames.Count; l++) //fix names
            {
                remasteredNames[l] = remasteredNames[l].Replace(remasteredAssetsFolder + "/", "").Replace(@"\", "/");
                if (Path.GetExtension(remasteredNames[l]) != "")
                    remasteredNames[l] = Path.ChangeExtension(remasteredNames[l], Path.GetExtension(remasteredNames[l]).ToLower());
            }

            if (assetConfig.SortOrder)
            {
                //Make a sorted list tempremasteredNames
                List<string> tempremasteredNamesD = new List<string>();
                List<string> tempremasteredNamesP = new List<string>();
                List<string> tempremasteredNames = new List<string>(remasteredNames);
                for (int i = 0; i < remasteredNames.Count; i++)
                {
                    var filename = "-" + i.ToString();
                    //Console.WriteLine("TEST for " + filename + ".dds/.png");
                    if (remasteredNames.Contains(filename + ".dds"))
                    {
                        //Console.WriteLine(filename + ".dds" + "FOUND!");
                        tempremasteredNamesD.Add(filename + ".dds");
                        tempremasteredNames.Remove(filename + ".dds");
                    }
                    else if (remasteredNames.Contains(filename + ".png"))
                    {
                        //Console.WriteLine(filename + ".png" + "FOUND!");
                        tempremasteredNamesP.Add(filename + ".png");
                        tempremasteredNames.Remove(filename + ".png");
                    }
                }
                //Add the image files at the end
                //DDS list first, PNG list 2nd, everything else after
                tempremasteredNamesD.AddRange(tempremasteredNamesP);
                tempremasteredNamesD.AddRange(tempremasteredNames);
                //Add the sorted list back to remasteredNames
                remasteredNames = tempremasteredNamesD;
            }
        }

        for (int i = 0; i < oldRemasteredHeaders.Count; i++)
        {
            var remasteredAssetHeader = oldRemasteredHeaders[i];
            var filename = remasteredAssetHeader.Name;

            //get actual file names ONLY if the remastered asset count is greater than 0 and ONLY if the number of files in the 
            //remastered folder for the SD asset is equal to or greater than what the total count is from what was gotten in SDasset.
            //if those criteria aren't met then do the old method.
            if (sdasset != null && !sdasset.Invalid && remasteredNames.Count >= oldRemasteredHeaders.Count && remasteredNames.Count > 0)
            {
                filename = remasteredNames[i];
            }

            var assetFilePath = Path.Combine(remasteredAssetsFolder, filename);

            // Use base remastered asset data
            var assetData = asset.RemasteredAssetsDecompressedData.ContainsKey(filename) ? asset.RemasteredAssetsDecompressedData[filename] : new byte[] { };
            var decompressedLength = remasteredAssetHeader.DecompressedLength;
            var originalAssetOffset = remasteredAssetHeader.OriginalAssetOffset;
            if (ZipManager.FileExists(assetFilePath))
            {
                Console.WriteLine($"Replacing remastered file: {relativePath}/{filename}");

                assetData = ZipManager.FileReadAllBytes(assetFilePath);
                decompressedLength = assetData.Length;
                assetData = remasteredAssetHeader.CompressedLength > -1 ? Helpers.CompressData(assetData) : assetData;
                assetData = remasteredAssetHeader.CompressedLength > -2 ? EgsEncryption.Encrypt(assetData, seed) : assetData;
                if (sdasset != null && !sdasset.Invalid) originalAssetOffset = sdasset.Offsets[i];
            }
            else
            {
                Console.WriteLine($"Keeping remastered file: {relativePath}/{filename}");
                // The original file have been replaced, we need to encrypt all remastered asset with the new key
                if (!seed.SequenceEqual(asset.Seed))
                {
                    assetData = remasteredAssetHeader.CompressedLength > -1 ? Helpers.CompressData(assetData) : assetData;
                    assetData = remasteredAssetHeader.CompressedLength > -2 ? EgsEncryption.Encrypt(assetData, seed) : assetData;
                    if (sdasset != null && !sdasset.Invalid && sdasset.AssetCount >= i) originalAssetOffset = sdasset.Offsets[i];
                }
                else
                {
                    assetData = asset.RemasteredAssetsCompressedData.ContainsKey(filename) ? asset.RemasteredAssetsCompressedData[filename] : new byte[] { };
                }
            }
            var compressedLength = remasteredAssetHeader.CompressedLength > -1 ? assetData.Length : remasteredAssetHeader.CompressedLength;

            var newRemasteredAssetHeader = new EgsHdAsset.RemasteredEntry()
            {
                CompressedLength = compressedLength,
                DecompressedLength = decompressedLength,
                Name = filename,
                Offset = offset,
                OriginalAssetOffset = originalAssetOffset
            };

            newRemasteredHeaders.Add(newRemasteredAssetHeader);

            // Write asset header in the PKG stream
            BinaryMapping.WriteObject<EgsHdAsset.RemasteredEntry>(pkgStream, newRemasteredAssetHeader);

            // Don't write into the PKG stream yet as we need to write
            // all HD assets header juste after original file's data
            allRemasteredAssetsData.Write(assetData);

            // Make sure to align remastered asset data on 16 bytes
            if (assetData.Length % 0x10 != 0)
            {
                allRemasteredAssetsData.Write(Enumerable.Repeat((byte)0xCD, 16 - (assetData.Length % 0x10)).ToArray());
            }

            offset += decompressedLength;
        }

        pkgStream.Write(originalAssetData);
        pkgStream.Write(allRemasteredAssetsData.ReadAllBytes());

        return newRemasteredHeaders;
    }

    #endregion

    #region List

    public static void List(string inputHed)
    {
        using var hedStream = File.OpenRead(inputHed);
        var entries = Hed.Read(hedStream);

        foreach (var entry in entries)
        {
            var hash = Helpers.ToString(entry.MD5);
            if (!Names.TryGetValue(hash, out var fileName))
                fileName = $"{hash}.dat";

            Console.WriteLine(fileName);
        }
    }

    #endregion
}

