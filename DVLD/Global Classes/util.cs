using DVLD.Global_Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DVLD.Classes
{
    public class clsUtil
    {
        public static string GenerateGUID()
        {

            // Generate a new GUID
            Guid newGuid = Guid.NewGuid();

            // convert the GUID to a string
            return newGuid.ToString();
            
        }

        public static async Task<bool> DeleteOldImageFromDistDiskAsync(string ImagePath)
        {
            try
            {
                // التحقق من وجود الملف
                if (File.Exists(ImagePath))
                {
                    // حذف الملف
                    File.Delete(ImagePath);
                    return true; // تم الحذف بنجاح
                }
                else
                {
                    // الملف غير موجود
                    return false;
                }
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ أو عرض رسالة للمستخدم
                MessageBox.Show($"Failed to delete image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                await clsEventLogger.LogEvent(ex.Message, System.Diagnostics.EventLogEntryType.Error);
                return false; // فشل الحذف
            }
        }

        public static async Task<bool> CreateFolderIfDoesNotExistAsync(string FolderPath)
        {
         
            // Check if the folder exists
            if (!Directory.Exists(FolderPath))
            {
                try
                {
                    // If it doesn't exist, create the folder
                    Directory.CreateDirectory(FolderPath);
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error creating folder: " + ex.Message);
                    await clsEventLogger.LogEvent(ex.Message, System.Diagnostics.EventLogEntryType.Error);
                    return false;
                }
            }

            return true;
            
        }
     
        public static string ReplaceFileNameWithGUID(string sourceFile)
        {
            // Full file name. Change your file name   
            string fileName = sourceFile;
            FileInfo fi = new FileInfo(fileName);
            string extn = fi.Extension;
            return GenerateGUID() + extn;

        }
        public static async Task<string> ReplaceFileNameWithGUIDAsync(string sourceFile)
        {
            string fileName = sourceFile;
            string extn = await Task.Run(() => {
                FileInfo fi = new FileInfo(fileName); // عملية I/O محتملة
                return fi.Extension;
            });

            // GenerateGUID() يفترض أنها عملية سريعة مرتبطة بالـ CPU
            return GenerateGUID() + extn;
        }

        public static  async Task<(bool Success, string NewFilePath)> CopyImageToProjectImagesFolderAsync(string  sourceFile)
        {
            // this funciton will copy the image to the
            // project images foldr after renaming it
            // with GUID with the same extention, then it will update the sourceFileName with the new name.

            string DestinationFolder = @"C:\Users\msam1\source\repos\Projects\DVLD Project Final\People_Images\";
            if (!await CreateFolderIfDoesNotExistAsync(DestinationFolder))
            {
                return (Success: false, NewFilePath: null);
            }
            
            
            string destinationFile = DestinationFolder + await ReplaceFileNameWithGUIDAsync(sourceFile);
            try
            {
                File.Copy(sourceFile, destinationFile, true);

            }
            catch (IOException iox)
            {
                MessageBox.Show (iox.Message,"Error",MessageBoxButtons.OK, MessageBoxIcon.Error);
                await clsEventLogger.LogEvent(iox.Message, System.Diagnostics.EventLogEntryType.Error);
                return (Success: false, NewFilePath: null);
            }
            
            sourceFile = destinationFile;
            return (Success: true, NewFilePath: sourceFile);
        }


    }
}
