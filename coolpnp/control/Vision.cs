using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;

namespace control
{
  public static partial class Program
  {
    static void FindPart()
    {
      Camera u = settings.upCamera;
      Native.MinRect mr = new Native.MinRect();
      Tape t = tapes[state.lastTape];
      decimal w = Math.Max(t.partRects[0].width, t.partRects[0].height);
      int partsize = (int)(t.partRects[0].width * t.partRects[0].height * u.xPixelPerMm * u.yPixelPerMm);
      int ww = (int)((w * 1.4m + 1) * u.xPixelPerMm);
      Native.get_minrect(1, ww, ww, t.threshold1, t.threshold2, t.gauss1, t.gauss2, 45, partsize, 5, mr);
      int angle = mr.angle < -45 ? (mr.angle + 90) : mr.angle;
      decimal sx = Decimal.Round((mr.xc - u.resolutionX / 2) / u.xPixelPerMm, 2);
      decimal sy = -Decimal.Round((mr.yc - u.resolutionY / 2) / u.yPixelPerMm, 2);
      Native.DrawText(420, 810, 1, 0, "Angle: " + angle + " (" + sx + "," + sy + ")  " + mr.size + "                  ");
      Native.DrawLine(mr.x1 / 2 + 200, mr.y1 / 2 + 150, mr.x2 /2 + 200, mr.y2 / 2 + 150, 3);
      Native.DrawLine(mr.x3 / 2 + 200, mr.y3 / 2 + 150, mr.x2 /2 + 200, mr.y2 / 2 + 150, 3);
      Native.DrawLine(mr.x3 / 2 + 200, mr.y3 / 2 + 150, mr.x4 /2 + 200, mr.y4 / 2 + 150, 3);
      Native.DrawLine(mr.x1 / 2 + 200, mr.y1 / 2 + 150, mr.x4 /2 + 200, mr.y4 / 2 + 150, 3);
    }

    static void updateCameras()
    {
      Camera u = settings.upCamera;
      Camera d = settings.downCamera;
      Tape t = tapes[state.lastTape];
      Native.DrawText(420, 610, 1, 0, "L: " + t.lightIntensity + "  E: " + u.exposure + "  B: " + u.brightness + "  C: " + u.contrast + "  T: " + t.threshold1 + " " + t.threshold2 + "  G: " + t.gauss1 + " " + t.gauss2 + "     ");
      Native.DrawText(20, 610, 1, 0, "L: " + d.lightIntensity + "  E: " + d.exposure + "  B: " + d.brightness + "  C: " + d.contrast + "  T: " + d.threshold1 + " " + d.threshold2 + "  G: " + d.gauss1 + " " + d.gauss2 + "     ");
    }

    static void DownLightOn()
    {
      channel.DownLightOn(tapes[state.lastTape].lightIntensity);
    }

    static void UpLightOn()
    {
      channel.UpLightOn(settings.downCamera.lightIntensity);
    }

    static void DownLightOff()
    {
      channel.DownLightOff();
    }

    static void UpLightOff()
    {
      channel.UpLightOff();
    }

    static void VisionCallback(long data)
    {
      Tape t = tapes[state.lastTape];
      switch (data & 255)
      {
        case 1:
          FindPart();
          break;
        case 100:
          DownLightOn();
          break;
        case 101:
          DownLightOff();
          break;
        case 102:
          if (t.lightIntensity < 30) {
            t.lightIntensity++;
            DownLightOn();
          }
          break;
        case 103:
          if (t.lightIntensity > 1) {
            t.lightIntensity--;
            DownLightOn();
          }
          break;
        case 104:
          if (settings.upCamera.exposure < 300) {
            settings.upCamera.exposure++;
            Native.set_camera(1, settings.upCamera.exposure, settings.upCamera.brightness, settings.upCamera.contrast, settings.upCamera.sharpness);
          }
          break;
        case 105:
          if (settings.upCamera.exposure > 10) {
            settings.upCamera.exposure--;
            Native.set_camera(1, settings.upCamera.exposure, settings.upCamera.brightness, settings.upCamera.contrast, settings.upCamera.sharpness);
          }
          break;
        case 106:
          if (settings.upCamera.brightness < 100) {
            settings.upCamera.brightness++;
            Native.set_camera(1, settings.upCamera.exposure, settings.upCamera.brightness, settings.upCamera.contrast, settings.upCamera.sharpness);
          }
          break;
        case 107:
          if (settings.upCamera.brightness > 0) {
            settings.upCamera.brightness--;
            Native.set_camera(1, settings.upCamera.exposure, settings.upCamera.brightness, settings.upCamera.contrast, settings.upCamera.sharpness);
          }
          break;
        case 108:
          if (settings.upCamera.contrast < 64) {
            settings.upCamera.contrast++;
            Native.set_camera(1, settings.upCamera.exposure, settings.upCamera.brightness, settings.upCamera.contrast, settings.upCamera.sharpness);
          }
          break;
        case 109:
          if (settings.upCamera.contrast > 0) {
            settings.upCamera.contrast--;
            Native.set_camera(1, settings.upCamera.exposure, settings.upCamera.brightness, settings.upCamera.contrast, settings.upCamera.sharpness);
          }
          break;
        case 120:
          if (t.threshold1 < 240)
            t.threshold1 += 10;
          break;
        case 121:
          if (t.threshold1 > 9)
            t.threshold1 -= 10;
          break;
        case 122:
          if (t.threshold2 < 240)
            t.threshold2 += 10;
          break;
        case 123:
          if (t.threshold2 > 9)
            t.threshold2 -= 10;
          break;
        case 124:
          if (t.gauss1 < 51)
            t.gauss1 += 2;
          break;
        case 125:
          if (t.gauss1 > 3)
            t.gauss1 -= 2;
          break;
        case 126:
          if (t.gauss2 < 51)
            t.gauss2 += 2;
          break;
        case 127:
          if (t.gauss2 > 3)
            t.gauss2 -= 2;
          break;
        case 200:
          UpLightOn();
          break;
        case 201:
          UpLightOff();
          break;
        case 202:
          if (settings.downCamera.lightIntensity < 30) {
            settings.downCamera.lightIntensity++;
            UpLightOn();
          }
          break;
        case 203:
          if (settings.downCamera.lightIntensity > 1) {
            settings.downCamera.lightIntensity--;
            UpLightOn();
          }
          break;
        case 204:
          if (settings.downCamera.exposure < 300) {
            settings.downCamera.exposure++;
            Native.set_camera(0, settings.downCamera.exposure, settings.downCamera.brightness, settings.downCamera.contrast, settings.downCamera.sharpness);
          }
          break;
        case 205:
          if (settings.downCamera.exposure > 10) {
            settings.downCamera.exposure--;
            Native.set_camera(0, settings.downCamera.exposure, settings.downCamera.brightness, settings.downCamera.contrast, settings.downCamera.sharpness);
          }
          break;
        case 206:
          if (settings.downCamera.brightness < 100) {
            settings.downCamera.brightness++;
            Native.set_camera(0, settings.downCamera.exposure, settings.downCamera.brightness, settings.downCamera.contrast, settings.downCamera.sharpness);
          }
          break;
        case 207:
          if (settings.downCamera.brightness > 0) {
            settings.downCamera.brightness--;
            Native.set_camera(0, settings.downCamera.exposure, settings.downCamera.brightness, settings.downCamera.contrast, settings.downCamera.sharpness);
          }
          break;
        case 208:
          if (settings.downCamera.contrast < 64) {
            settings.downCamera.contrast++;
            Native.set_camera(0, settings.downCamera.exposure, settings.downCamera.brightness, settings.downCamera.contrast, settings.downCamera.sharpness);
          }
          break;
        case 209:
          if (settings.downCamera.contrast > 0) {
            settings.downCamera.contrast--;
            Native.set_camera(0, settings.downCamera.exposure, settings.downCamera.brightness, settings.downCamera.contrast, settings.downCamera.sharpness);
          }
          break;
        case 220:
          if (settings.downCamera.threshold1 < 240)
            settings.downCamera.threshold1 += 10;
          break;
        case 221:
          if (settings.downCamera.threshold1 > 9)
            settings.downCamera.threshold1 -= 10;
          break;
        case 222:
          if (settings.downCamera.threshold2 < 240)
            settings.downCamera.threshold2 += 10;
          break;
        case 223:
          if (settings.downCamera.threshold2 > 9)
            settings.downCamera.threshold2 -= 10;
          break;
        case 224:
          if (settings.downCamera.gauss1 < 51)
            settings.downCamera.gauss1 += 2;
          break;
        case 225:
          if (settings.downCamera.gauss1 > 3)
            settings.downCamera.gauss1 -= 2;
          break;
        case 226:
          if (settings.downCamera.gauss2 < 51)
            settings.downCamera.gauss2 += 2;
          break;
        case 227:
          if (settings.downCamera.gauss2 > 3)
            settings.downCamera.gauss2 -= 2;
          break;
      }
      updateCameras();
    }

    static void AddCameraPanel(int x, int y, int ci)
    {
      Native.add_button("Light On", x, y, 110, 25, ci, visionCallback);
      Native.add_button("Light Off", x, y + 30, 110, 25, ci + 1, visionCallback);
      Native.add_button("Light+", x + 120, y, 50, 25, ci + 2, visionCallback);
      Native.add_button("Light-", x + 120, y + 30, 50, 25, ci + 3, visionCallback);
      Native.add_button("Exp+", x + 180, y, 50, 25, ci + 4, visionCallback);
      Native.add_button("Exp-", x + 180, y + 30, 50, 25, ci + 5, visionCallback);
      Native.add_button("Brigh+", x + 240, y, 50, 25, ci + 6, visionCallback);
      Native.add_button("Brigh-", x + 240, y + 30, 50, 25, ci + 7, visionCallback);
      Native.add_button("Contr+", x + 300, y, 50, 25, ci + 8, visionCallback);
      Native.add_button("Contr-", x + 300, y + 30, 50, 25, ci + 9, visionCallback);
      Native.add_button("Thr1+", x, y + 60, 50, 25, ci + 20, visionCallback);
      Native.add_button("Thr1-", x, y + 90, 50, 25, ci + 21, visionCallback);
      Native.add_button("Thr2+", x + 60, y + 60, 50, 25, ci + 22, visionCallback);
      Native.add_button("Thr2-", x + 60, y + 90, 50, 25, ci + 23, visionCallback);
      Native.add_button("Gau1+", x + 120, y + 60, 50, 25, ci + 24, visionCallback);
      Native.add_button("Gau1-", x + 120, y + 90, 50, 25, ci + 25, visionCallback);
      Native.add_button("Gau2+", x + 180, y + 60, 50, 25, ci + 26, visionCallback);
      Native.add_button("Gau2-", x + 180, y + 90, 50, 25, ci + 27, visionCallback);
    }

    static void AddVisionPanel(int x, int y)
    {
      Native.add_button("Find Part", x + 400, y + 120, 110, 25, 1, visionCallback);
      AddCameraPanel(x, y, 200);
      AddCameraPanel(x + 400, y, 100);
    }
  }
}
