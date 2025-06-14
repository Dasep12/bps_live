using System;
using System.Collections.Generic;
using System.Linq;

using System.IO;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Xobject;
using ZXing;
using ZXing.Common;
using System.Drawing;
using System.Text.RegularExpressions;
using Core.VSSP.WorkEntity;
using ZXing.OneD;



namespace Core.VSSP.Services
{
    public class PdfExtractorService
    {
        vssp_entity vssp_db = new vssp_entity();
        public class PartData
        {
            public string No { get; set; }
            public string PartNumber { get; set; }
            public string Model { get; set; }
            public string Unique { get; set; }
            public string UniqueCustomer { get; set; }
            public string PartName { get; set; }
            public string QtyKanban { get; set; }
            public string KanbanDelivery { get; set; }
            public string QtyDelivery { get; set; }
            public string Units { get; set; }
        }


        // Extract DN Data For TBINA-ADM
        public (string Barcode, List<PartData>) ExtractData(string pdfPath, int pageNumber)
        {
            string barcodeValue = null;
            List<PartData> tableData = new List<PartData>();

            using (var pdfReader = new PdfReader(pdfPath))
            using (var pdfDocument = new PdfDocument(pdfReader))
            {
                if (pageNumber > pdfDocument.GetNumberOfPages() || pageNumber < 1)
                    throw new ArgumentException("Halaman tidak valid!");

                var page = pdfDocument.GetPage(pageNumber);

                // 1. Ekstrak teks dari halaman PDF
                var text = PdfTextExtractor.GetTextFromPage(page);

                // 2. Ekstrak Barcode menggunakan ZXing
                barcodeValue = ExtractBarcodeFromPDF(pdfPath, pageNumber);

                // 3. Ekstrak dan filter data tabel
                tableData = ExtractTableData(text);
            }

            return (barcodeValue, tableData);
        }

        private string ExtractBarcodeFromPDF(string pdfPath, int pageNumber)
        {
            using (var pdfReader = new PdfReader(pdfPath))
            using (var pdfDocument = new PdfDocument(pdfReader))
            {
                var page = pdfDocument.GetPage(pageNumber);

                // Render halaman PDF ke gambar
                using (var bitmap = RenderPdfPageToBitmap(pdfDocument, pageNumber))
                {
                    // Inisialisasi ZXing reader
                    var barcodeReader = new BarcodeReader
                    {
                        AutoRotate = true,
                        Options = new DecodingOptions
                        {
                            TryHarder = true,
                            PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.CODE_128, BarcodeFormat.QR_CODE, BarcodeFormat.PDF_417 }
                        }
                    };

                    // Coba membaca barcode dari gambar
                    var result = barcodeReader.Decode(bitmap);
                    return result?.Text;
                }
            }
        }

        private Bitmap RenderPdfPageToBitmap(PdfDocument pdfDocument, int pageNumber)
        {
            var page = pdfDocument.GetPage(pageNumber);

            // Konversi PDF ke gambar
            using (var memoryStream = new MemoryStream())
            {
                var image = new Bitmap(800, 1200); // Ukuran bisa disesuaikan
                using (var graphics = Graphics.FromImage(image))
                {
                    graphics.Clear(Color.White);
                }
                return image;
            }
        }

        private List<PartData> ExtractTableData(string text)
        {
            var tableData = new List<PartData>();
            var lines = text.Split('\n');

            bool isTableSection = false;
            foreach (var line in lines)
            {
                if (line.Contains("No Material No Job No")) // Awal tabel
                {
                    isTableSection = true;
                    continue;
                }

                if (isTableSection)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.Contains("Page :")) // Akhir tabel
                        break;

                    var columns = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    // Hilangkan baris yang tidak relevan
                    if (columns.Contains("APPROVED") || columns.Contains("PREPARED") || columns.Contains("LOGISTIC") || columns.Contains("Printed"))
                        continue;

                    // Pastikan hanya tabel data yang valid yang disimpan
                    if (columns.Length >= 6)
                    {
                        string no = columns[0]; // No
                        string partNumber = columns[1]; // Part Number
                        string unique = columns[2]; // Unique ID

                        // Identifikasi posisi angka terakhir yang merupakan QtyKanban, Kanban, dan Qty
                        int qtyKanbanIndex = columns.Length - 3;
                        int kanbanIndex = columns.Length - 2;
                        int qtyIndex = columns.Length - 1;

                        string qtyKanban = columns[qtyKanbanIndex]; // QtyKanban
                        string kanban = columns[kanbanIndex]; // Kanban
                        string qty = columns[qtyIndex]; // Qty

                        // Gabungkan Part Name dengan memastikan tidak ada Qty di dalamnya
                        string partName = string.Join(" ", columns.Skip(3).Take(qtyKanbanIndex - 3));

                        // Cek apakah Part Name diakhiri dengan angka, jika ya, hapus angka tersebut
                        partName = Regex.Replace(partName, @"\d+$", "").Trim();

                        var FinishGoods = (from a in vssp_db.Vw_MST_PartFinishGoods
                                           where a.Actived == true && (a.PartNumber.Contains(partNumber) || a.UniqueNumber.Contains(unique))
                                           orderby a.CustomerId, a.PartNumber
                                           select new { a.FinishGoodKey, a.CustomerId, a.CustomerName, a.CustomerUnitModel, a.PartNumber, a.PartNumberCustomer, a.UniqueNumber, a.PartName, a.CategoryId, a.PackingId, a.AreaId, a.LocationId, a.UnitLevel1, a.UnitLevel2, a.UnitQty, a.Price, a.EndDate, a.Expired, a.PassThrough, a.Actived, IsActived = a.Actived, a.UserId, a.EditDate }).FirstOrDefault();

                        tableData.Add(new PartData
                        {
                            No = no,
                            PartNumber = partNumber,
                            Model = FinishGoods.CustomerUnitModel,
                            Unique = FinishGoods.UniqueNumber,
                            UniqueCustomer = unique,
                            PartName = partName,
                            QtyKanban = qtyKanban,
                            KanbanDelivery = kanban,
                            QtyDelivery = qty,
                            Units = FinishGoods.UnitLevel2,
                        });
                    }
                }
            }

            return tableData;
        }
        // End


        // Extract Kanban TBINA Regular
        public List<string> ExtractBarcodesFromKanbanPDF(string pdfPath)
        {
            List<string> barcodeList = new List<string>();
            using (var pdfReader = new PdfReader(pdfPath))
            using (var pdfDocument = new PdfDocument(pdfReader))
            {
                for (int page = 1; page <= pdfDocument.GetNumberOfPages(); page++)
                {
                    List<Bitmap> images = ExtractAllImagesFromKanbanPDF(pdfDocument, page);
                    foreach (Bitmap image in images)
                    {
                        string barcodeText = DecodeBarcode(image);
                        if (!string.IsNullOrEmpty(barcodeText))
                        {
                            barcodeList.Add(barcodeText);
                        }
                    }
                }
            }
            return barcodeList;
        }

        private List<Bitmap> ExtractAllImagesFromKanbanPDF(PdfDocument pdfDocument, int pageNumber)
        {
            List<Bitmap> images = new List<Bitmap>();
            var page = pdfDocument.GetPage(pageNumber);
            var resources = page.GetResources();
            var xObjects = resources.GetResource(PdfName.XObject);

            if (xObjects != null)
            {
                foreach (var entry in xObjects.KeySet())
                {
                    var obj = xObjects.Get(entry);
                    if (obj is PdfStream stream)
                    {
                        try
                        {
                            var imageObj = new PdfImageXObject(stream);
                            using (var ms = new MemoryStream(imageObj.GetImageBytes()))
                            {
                                images.Add(new Bitmap(ms));
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error decoding image: " + ex.Message);
                        }
                    }
                }
            }
            return images;
        }

        private string DecodeBarcode(Bitmap bitmap)
        {
            BarcodeReader reader = new BarcodeReader
            {
                AutoRotate = true,
                Options = new ZXing.Common.DecodingOptions
                {
                    TryHarder = true,
                    PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE, BarcodeFormat.CODE_128, BarcodeFormat.PDF_417 }
                }
            };

            var result = reader.Decode(bitmap);
            return result?.Text;
        }

        // End
    }
}