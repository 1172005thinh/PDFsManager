using System;
using System.IO;
using iText.Kernel.Pdf;

namespace PDFsManager.Utils
{
    /// <summary>
    /// PDF file helper utilities for metadata extraction.
    /// Wraps iText7 library functionality.
    /// </summary>
    public static class PdfHelper
    {
        /// <summary>
        /// Extracts the creation date from a PDF file's metadata.
        /// </summary>
        /// <param name="filePath">Path to the PDF file.</param>
        /// <param name="creationDate">Output parameter for the creation date.</param>
        /// <returns>True if extraction succeeded, false otherwise.</returns>
        public static bool TryGetCreationDate(string filePath, out DateTime creationDate)
        {
            creationDate = DateTime.MinValue;

            try
            {
                using (PdfReader reader = new PdfReader(filePath))
                using (PdfDocument pdfDoc = new PdfDocument(reader))
                {
                    var docInfo = pdfDoc.GetDocumentInfo();
                    
                    // Try to get creation date from PDF metadata
                    string? creationDateStr = docInfo.GetMoreInfo(PdfName.CreationDate.GetValue());
                    
                    if (!string.IsNullOrEmpty(creationDateStr))
                    {
                        // Parse PDF date format (D:YYYYMMDDHHmmSSOHH'mm')
                        creationDate = ParsePdfDate(creationDateStr);
                        return creationDate != DateTime.MinValue;
                    }

                    // Fallback: try ModDate if CreationDate is not available
                    string? modDateStr = docInfo.GetMoreInfo(PdfName.ModDate.GetValue());
                    if (!string.IsNullOrEmpty(modDateStr))
                    {
                        creationDate = ParsePdfDate(modDateStr);
                        return creationDate != DateTime.MinValue;
                    }
                }
            }
            catch (Exception)
            {
                // Invalid PDF or metadata extraction failed
                return false;
            }

            // Fallback to file creation time if PDF metadata is not available
            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                creationDate = fileInfo.CreationTime;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Parses PDF date format string to DateTime.
        /// PDF date format: D:YYYYMMDDHHmmSSOHH'mm'
        /// </summary>
        private static DateTime ParsePdfDate(string pdfDateString)
        {
            try
            {
                // Remove "D:" prefix if present
                if (pdfDateString.StartsWith("D:"))
                    pdfDateString = pdfDateString.Substring(2);

                // Extract basic date components (minimum 14 chars: YYYYMMDDHHmmss)
                if (pdfDateString.Length >= 14)
                {
                    int year = int.Parse(pdfDateString.Substring(0, 4));
                    int month = int.Parse(pdfDateString.Substring(4, 2));
                    int day = int.Parse(pdfDateString.Substring(6, 2));
                    int hour = int.Parse(pdfDateString.Substring(8, 2));
                    int minute = int.Parse(pdfDateString.Substring(10, 2));
                    int second = int.Parse(pdfDateString.Substring(12, 2));

                    return new DateTime(year, month, day, hour, minute, second);
                }
            }
            catch
            {
                // Parsing failed
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// Checks if a file is a valid PDF file.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        /// <returns>True if valid PDF, false otherwise.</returns>
        public static bool IsValidPdf(string filePath)
        {
            try
            {
                using (PdfReader reader = new PdfReader(filePath))
                using (PdfDocument pdfDoc = new PdfDocument(reader))
                {
                    // If we can open it, it's valid
                    return pdfDoc.GetNumberOfPages() > 0;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
