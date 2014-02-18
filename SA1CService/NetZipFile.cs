using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace SA1CService
{
    /// <summary>
    /// Класс для работы с файлами
    /// </summary>
    //TODO: заменить GZipStream обычным zip, т.к. ограничение на 4 Гб и имя файла = имени архива
    public static class ZipFile
    {

    	/// <summary>
    	/// Архивировать файл
    	/// </summary>
    	/// <param name="fi">Описание файла подлежащего архивации</param>
    	/// <returns>имя архива</returns>
        public static string Compress(FileInfo fi)
        {
        	string ZipFileName;
            // Get the stream of the source file.
            using (FileStream inFile = fi.OpenRead())
            {
                // Prevent compressing hidden and 
                // already compressed files.
                if ((File.GetAttributes(fi.FullName)
                    & FileAttributes.Hidden)
                    != FileAttributes.Hidden & fi.Extension != ".zip")
                {
                    // Create the compressed file.
                    using (FileStream outFile =
                                File.Create(fi.FullName + ".zip"))
                    {
                        using (GZipStream Compress =
                            new GZipStream(outFile,
                            CompressionMode.Compress))
                        {
                            // Copy the source file into 
                            // the compression stream.
                            inFile.CopyTo(Compress,4096);

                        }
                    }
                }
                ZipFileName = fi.FullName + ".zip";            	   
            }
            return ZipFileName;
        }

        public static void Decompress(FileInfo fi)
        {
            // Get the stream of the source file.
            using (FileStream inFile = fi.OpenRead())
            {
                // Get original file extension, for example
                // "doc" from report.doc.gz.
                string curFile = fi.FullName;
                string origName = curFile.Remove(curFile.Length -
                        fi.Extension.Length);

                //Create the decompressed file.
                using (FileStream outFile = File.Create(origName))
                {
                    using (GZipStream Decompress = new GZipStream(inFile,
                            CompressionMode.Decompress))
                    {
                        // Copy the decompression stream 
                        // into the output file.
                        Decompress.CopyTo(outFile,4096);

                    }
                }
            }
        }

        /// <summary>
        /// Получает из сжатого потока и сохраняет на диске распакованный файл
        /// </summary>
        /// <param name="tempStream">сжатый поток</param>
        /// <param name="name">имя файла на диске</param>
        //public static void Unzip(string pathToZipFile, string pathToUnzipFile)
        //{
        //    FileStream tempFile = File.Create(name);
        //    GZipStream zip = new GZipStream(tempStream, CompressionMode.Decompress);

        //    int numByteRead = 0;
        //    byte[] buffer = new byte[1000];
        //    int b = zip.ReadByte();
        //    while (b != -1)
        //    {
        //        tempFile.WriteByte((Byte)b);

        //        numByteRead = zip.Read(buffer, 0, 1000);
        //        tempFile.Write(buffer, 0, numByteRead);

        //        b = zip.ReadByte();
        //    }

        //    tempFile.Flush();
        //    tempFile.Close();
        //    zip.Close();
        //    //tempStream.Close();
        //}

        /// <summary>
        /// Создаем поток из сжатого файла
        /// </summary>
        /// <param name="name">имя файла на диске</param>
        /// <returns>поток</returns>
        //public static Stream Zip(string pathToUnzipFile, string pathToZipFile, bool deleteSource)
        //{
        //    FileStream tempFile;
        //    //ждем пока файл освободит другой процесс
        //    while (true)
        //    {
        //        try
        //        {
        //            tempFile = File.Open(name, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        //            break;
        //        }
        //        catch
        //        {
        //            Thread.Sleep(1000);
        //        }
        //    }
        //    FileStream ziptempFile = File.Open("tempStream.tmp", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        //    GZipStream zip = new GZipStream(ziptempFile, CompressionMode.Compress);

        //    int numByteRead = 0;
        //    byte[] buffer = new byte[1000];
        //    int b = tempFile.ReadByte();
        //    while (b != -1)
        //    {
        //        zip.WriteByte((Byte)b);

        //        numByteRead = tempFile.Read(buffer, 0, 1000);
        //        zip.Write(buffer, 0, numByteRead);
        //        zip.Flush();
                
        //        b = tempFile.ReadByte();
        //    }

        //    zip.Flush();
        //    zip.Close();
        //    tempFile.Close();
        //    return File.Open("tempStream.tmp", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        //}

    }
}
