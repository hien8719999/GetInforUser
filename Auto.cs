using KAutoHelper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template_MinSoftware
{
    public class Auto
    {

        public static bool ClickControlImageFind(string windowName,string mainScreen, string pathImage, int typeMouse = 0)
        {
            try
            {
                ///typemouse=0 left, typemouse=1 right, typemouse=2 doubleleft, typemouse=3 double right
                IntPtr hWnd = IntPtr.Zero;
                hWnd = AutoControl.FindWindowHandle(null, windowName);
                var screen = CaptureHelper.CaptureWindow(hWnd);
                screen.Save(mainScreen);
                var subBitmap = ImageScanOpenCV.GetImage(pathImage);
                var point = ImageScanOpenCV.FindOutPoint((Bitmap)screen, subBitmap);
                if (point != null)
                {
                    //resBitmap.Save("res.PNG");
                    var pointToClick = AutoControl.GetGlobalPoint(hWnd, 190, 185);
                    AutoControl.MouseClick(pointToClick.X, pointToClick.Y);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool ClickControlNoMouseImageFind(string windowName, string cl, string txt, string pathImage)
        {
            ///typemouse=0 left, typemouse=1 right, typemouse=2 doubleleft, typemouse=3 double right
            IntPtr hWnd = IntPtr.Zero;
            hWnd = AutoControl.FindWindowHandle(null, windowName);
            var childhWnd = IntPtr.Zero;
            childhWnd = AutoControl.FindHandle(hWnd, cl, txt);

            var screen = CaptureHelper.CaptureWindow(hWnd);
            screen.Save("mainScreen.PNG");
            var subBitmap = ImageScanOpenCV.GetImage(pathImage);
            var point = ImageScanOpenCV.FindOutPoint((Bitmap)screen, subBitmap);
            if (point != null)
            {
                AutoControl.SendClickOnPosition(childhWnd, point.Value.X + 15, point.Value.Y + 15);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool ImageFind(string windowName, string pathImage)
        {
            IntPtr hWnd = IntPtr.Zero;
            hWnd = AutoControl.FindWindowHandle(null, windowName);
            var screen = CaptureHelper.CaptureWindow(hWnd);
            screen.Save("mainScreen.PNG");
            var subBitmap = ImageScanOpenCV.GetImage(pathImage);
            var point = ImageScanOpenCV.FindOutPoint((Bitmap)screen, subBitmap);
            if (point != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool ClickControlImageFind(string windowName, string pathImage, int typeMouse = 0, int addX = 0, int addY = 0)
        {
            ///typemouse=0 left, typemouse=1 right, typemouse=2 doubleleft, typemouse=3 double right
            IntPtr hWnd = IntPtr.Zero;
            hWnd = AutoControl.FindWindowHandle(null, windowName);
            var screen = CaptureHelper.CaptureWindow(hWnd);
            screen.Save("mainScreen.PNG");
            var subBitmap = ImageScanOpenCV.GetImage(pathImage);
            var point = ImageScanOpenCV.FindOutPoint((Bitmap)screen, subBitmap);
            if (point != null)
            {
                //resBitmap.Save("res.PNG");
                var pointToClick = AutoControl.GetGlobalPoint(hWnd, point.Value.X, point.Value.Y);
                AutoControl.MouseClick(pointToClick.X + addX, pointToClick.Y + addY, (EMouseKey)typeMouse);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void ClickControlPoint(string windowName,string cl,string txt,int x=0, int y = 0,int typeMouse=0)
        {
            ///typemouse=0 left, typemouse=1 right, typemouse=2 doubleleft, typemouse=3 double right
            IntPtr hWnd = IntPtr.Zero;
            hWnd = AutoControl.FindWindowHandle(null, windowName);
            var childhWnd = IntPtr.Zero;
            // Tìm ra handle con mà thỏa điều kiện text và class y chang
            //childhWnd = AutoControl.FindWindowExFromParent(hWnd, txt, cl);

            //Tìm ra handle con mà thỏa text hoặc class giống
            childhWnd = AutoControl.FindHandle(hWnd, cl, txt);
            //AutoControl.SendClickOnControlByHandle(childhWnd);
            AutoControl.SendClickOnPosition(childhWnd, x, y);
            ////// lấy ra tọa độ trên màn hình của tọa độ bên trong cửa sổ
            //var pointToClick = AutoControl.GetGlobalPoint(childhWnd, x, y);

            //EMouseKey mouseKey = EMouseKey.LEFT;
            //switch (typeMouse)
            //{
            //    case 0:
            //        mouseKey = EMouseKey.LEFT;
            //        break;
            //    case 1:
            //        mouseKey = EMouseKey.RIGHT;
            //        break;
            //    case 2:
            //        mouseKey = EMouseKey.DOUBLE_LEFT;
            //        break;
            //    case 3:
            //        mouseKey = EMouseKey.RIGHT;
            //        break;
            //}
            //AutoControl.BringToFront(hWnd);
            //AutoControl.MouseClick(pointToClick, mouseKey);
        }
    }
}
