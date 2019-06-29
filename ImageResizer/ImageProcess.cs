﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageResizer
{
    public class ImageProcess
    {
        /// <summary>
        /// 清空目的目錄下的所有檔案與目錄
        /// </summary>
        /// <param name="destPath">目錄路徑</param>
        public void Clean(string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }
            else
            {
                DirectoryInfo di = new DirectoryInfo(destPath);
                var files = di.GetFiles();
                Stopwatch sw = new Stopwatch();
                sw.Start();
                files.AsParallel().ForAll((f) => f.Delete());
                sw.Stop();
                Console.WriteLine($"刪檔花費時間: {sw.ElapsedMilliseconds} ms");
            }
        }

        /// <summary>
        /// 進行圖片的縮放作業
        /// </summary>
        /// <param name="sourcePath">圖片來源目錄路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        public async Task ResizeImagesAsync(string sourcePath, string destPath, double scale)
        {
            var allFiles = FindImages(sourcePath);
            Image[] imgPhotos = new Image[allFiles.Length];
            string[] imgNames = new string[allFiles.Length];
            allFiles.Select((file, index) => new { file, index }).AsParallel().ForAll(f => 
            {
                imgPhotos[f.index] = Image.FromFile(f.file);
                imgNames[f.index] = Path.GetFileNameWithoutExtension(f.file);
            });

            var imgs = imgPhotos.Select((imgPhoto, index) => new { imgPhoto, index });
            Task[] tasks = new Task[imgs.Count()];
            int idx = 0;
            foreach (var img in imgs)
            {
                tasks[idx] =  Task.Run(() =>
                {
                    int sourceWidth = img.imgPhoto.Width;
                    int sourceHeight = img.imgPhoto.Height;

                    int destionatonWidth = (int)(sourceWidth * scale);
                    int destionatonHeight = (int)(sourceHeight * scale);

                    Bitmap processedImage = processBitmapAsync((Bitmap)img.imgPhoto,
                        sourceWidth, sourceHeight,
                        destionatonWidth, destionatonHeight);

                    string destFile = Path.Combine(destPath, imgNames[img.index] + ".jpg");
                    processedImage.Save(destFile, ImageFormat.Jpeg);
                });
                idx++;
            }
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 找出指定目錄下的圖片
        /// </summary>
        /// <param name="srcPath">圖片來源目錄路徑</param>
        /// <returns></returns>
        public string[] FindImages(string srcPath)
        {
            List<string> files = new List<string>();
            DirectoryInfo di = new DirectoryInfo(srcPath);
            var fileInfos = di.GetFiles().AsParallel().Where(f => f.Extension == ".png" || f.Extension == ".jpg" || f.Extension == ".jpeg").Select(f=>f.FullName);
            files.AddRange(fileInfos);
            return files.ToArray();
        }

        /// <summary>
        /// 針對指定圖片進行縮放作業
        /// </summary>
        /// <param name="img">圖片來源</param>
        /// <param name="srcWidth">原始寬度</param>
        /// <param name="srcHeight">原始高度</param>
        /// <param name="newWidth">新圖片的寬度</param>
        /// <param name="newHeight">新圖片的高度</param>
        /// <returns></returns>
        Bitmap processBitmapAsync(Bitmap img, int srcWidth, int srcHeight, int newWidth, int newHeight)
        {
            Bitmap resizedbitmap = new Bitmap(newWidth, newHeight);
            Graphics g = Graphics.FromImage(resizedbitmap);
            g.InterpolationMode = InterpolationMode.High;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Clear(Color.Transparent);
            g.DrawImage(img,
                new Rectangle(0, 0, newWidth, newHeight),
                new Rectangle(0, 0, srcWidth, srcHeight),
                GraphicsUnit.Pixel);
            return resizedbitmap;
        }
    }
}
