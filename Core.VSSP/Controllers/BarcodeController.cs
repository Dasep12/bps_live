using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Core.VSSP.Services;
using System.Drawing;
using System.Drawing.Imaging;
using ZXing;
using ZXing.QrCode;
using ZXing.QrCode.Internal;
using System.Windows.Media.Imaging; // for SaveJpeg extension

namespace Core.VSSP.Controllers
{
    public class BarcodeController : Controller
    {
        // GET: Barcode
        public byte[] GenerateQRCode(string d)
        {
            byte[] result = { 0 };
            BarcodeWriter qrCodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Margin = 1,
                    Height = 600,
                    Width = 600,
                    ErrorCorrection = ErrorCorrectionLevel.Q,
                },
            };
            var writeableBitmap = qrCodeWriter.Write(d);
            var memoryStream = new MemoryStream();
            //writeableBitmap.Save(memoryStream, writeableBitmap.Width, writeableBitmap.Height, 0, 100);
            //return File(memoryStream.GetBuffer(), "image/jpeg");
            return result;
        }
    }
}