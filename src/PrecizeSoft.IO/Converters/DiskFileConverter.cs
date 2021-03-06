﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PrecizeSoft.IO.Converters
{
    public abstract class DiskFileConverter : IFileConverter
    {
        protected string destinationFileExtension;

        public DiskFileConverter(string destinationFileExtension)
        {
            if (!PathHelper.IsValidExtension(destinationFileExtension))
                throw new FormatException(destinationFileExtension);

            this.destinationFileExtension = destinationFileExtension;
        }

        public abstract IEnumerable<string> SupportedFormatCollection
        {
            get;
        }

        protected abstract void InternalConvert(string sourceFileName, string destinationFileName);

        public void Convert(string sourceFileName, string destinationFileName)
        {
            new FileConverterValidator(this.SupportedFormatCollection)
                .ValidateConvertParameters(sourceFileName, destinationFileName);

            this.InternalConvert(sourceFileName, destinationFileName);
        }

        public Stream Convert(Stream sourceStream, string fileExtension)
        {
            new FileConverterValidator(this.SupportedFormatCollection)
                .ValidateConvertParameters(sourceStream, fileExtension);

            string sourceTempFileName = null;
            string destinationTempFileName = Path.GetTempFileName() + destinationFileExtension;

            MemoryStream destinationMemoryStream = null;

            try
            {
                #region Converting file
                if (sourceStream is FileStream)
                {
                    this.InternalConvert(((FileStream)sourceStream).Name, destinationTempFileName);
                }
                else
                {
                    sourceTempFileName = Path.GetTempFileName() + fileExtension;
                    using (FileStream sourceFileStream = File.OpenWrite(sourceTempFileName))
                    {
                        sourceStream.CopyTo(sourceFileStream);
                    }
                    this.InternalConvert(sourceTempFileName, destinationTempFileName);
                }
                #endregion

                destinationMemoryStream = new MemoryStream();

                #region Copying result file to memory stream
                using (FileStream destinationFileStream = File.OpenRead(destinationTempFileName))
                {
                    destinationFileStream.CopyTo(destinationMemoryStream);
                }
                #endregion
            }
            finally
            {
                this.DeleteTempFiles(sourceTempFileName, destinationTempFileName);
            }

            return destinationMemoryStream;
        }

        public byte[] Convert(byte[] sourceBytes, string fileExtension)
        {
            new FileConverterValidator(this.SupportedFormatCollection)
                .ValidateConvertParameters(sourceBytes, fileExtension);

            string fileNameWithoutExtension = Guid.NewGuid().ToString();
            string sourceTempFileName = Path.Combine(Path.GetTempPath(), fileNameWithoutExtension + fileExtension);
            string destinationTempFileName = Path.Combine(Path.GetTempPath(), fileNameWithoutExtension + destinationFileExtension);

            byte[] result;

            try
            {
                File.WriteAllBytes(sourceTempFileName, sourceBytes);

                this.InternalConvert(sourceTempFileName, destinationTempFileName);

                result = File.ReadAllBytes(destinationTempFileName);
            }
            finally
            {
                this.DeleteTempFiles(sourceTempFileName, destinationTempFileName);
            }

            return result;
        }

        protected void DeleteTempFiles(string sourceTempFileName, string destinationTempFileName)
        {
            GC.WaitForPendingFinalizers();
            GC.Collect();

            if (!string.IsNullOrEmpty(sourceTempFileName))
            {
                try
                {
                    File.Delete(sourceTempFileName);
                }
                catch
                {
                    //Add manual resource disposing
                }
            }

            if (!string.IsNullOrEmpty(destinationTempFileName))
            {
                try
                {
                    File.Delete(destinationTempFileName);
                }
                catch
                {
                    //Add manual resource disposing
                }
            }
        }
    }
}
