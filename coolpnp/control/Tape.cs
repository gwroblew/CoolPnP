using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Xml.Serialization;

namespace control
{
  public static partial class Program
  {
    static void updateTape()
    {
      if (tapes == null || tapes.Count == 0)
        return;
      if (state.lastTape <= 0)
        state.lastTape = 0;
      if (state.lastTape >= tapes.Count - 1)
        state.lastTape = tapes.Count - 1;

      Tape tp = tapes[state.lastTape];
      Native.DrawText(1200, 275, 1, 0, tp.packageName + "  " + tp.value + "  " + tp.nozzle + "  " + tp.lastPartX + "  " + tp.lastPartY + "            ");
      Native.DrawText(1200, 300, 1, 0, tp.x1 + "  " + tp.y1 + "  " + tp.x2 + "  " + tp.y2 + "            ");
    }

    static void CenterPart()
    {
      Tape t = tapes[state.lastTape];
      var retval = new Native.Vec2();
      if (t.xOffset == 0 && t.yOffset == 0) {
        int fx = (int) (settings.downCamera.xPixelPerMm * t.partRects[0].width * 1.2m);
        int fy = (int) (settings.downCamera.yPixelPerMm * t.partRects[0].height * 1.2m);
        Native.findrectangle(0, fx, fy, 20, 20, fx * 4, fy * 4, retval);
      } else {
        int fx = (int) (settings.downCamera.xPixelPerMm * 1.6m);
        int fy = (int) (settings.downCamera.yPixelPerMm * 1.6m);
        Native.findcircle(0, fx / 2, 12, fx * 2, fy * 2, settings.downCamera.threshold1, settings.downCamera.threshold2, retval);
      }
      decimal mx = Decimal.Round(retval.x / settings.downCamera.xPixelPerMm, 2);
      decimal my = Decimal.Round(retval.y / settings.downCamera.yPixelPerMm, 2);
      Native.DrawText(1200, 810, 1, 0, "Got: " + mx + "  " + my);
      if (Math.Abs(mx) > 1 || Math.Abs(my) > 1) {
        Error("Part alignment too big: " + mx + ", " + my);
        return;
      }
      channel.Move(x: mx, y: my, s: 1000);
      updateState();
    }

    static void TapeCallback(long data)
    {
      if (tapes == null || tapes.Count == 0)
        return;
      Tape t = tapes[state.lastTape];
      switch (data&255)
      {
      case 1:
        state.lastTape--;
        if (state.lastTape <= 0)
          state.lastTape = 0;
        updateCameras();
        break;
      case 2:
        state.lastTape++;
        if (state.lastTape >= tapes.Count - 1)
          state.lastTape = tapes.Count - 1;
        updateCameras();
        break;
      case 3:
        if (t.lastPartX == 0) {
          if (t.x1 == 0)
            return;
          t.lastPartX = t.x1;
          t.lastPartY = t.y1;
        }
        channel.MoveTo(x: t.lastPartX, y: t.lastPartY, s: settings.speed);
        updateState();
        break;
      case 4:
        t.x1 = state.lastX;
        t.y1 = state.lastY;
        break;
      case 5:
        t.x2 = state.lastX;
        t.y2 = state.lastY;
        break;
      case 6:
        t.lastPartX = state.lastX;
        t.lastPartY = state.lastY;
        break;
      case 7:
        t.lastDeltaX = state.lastX - t.x1;
        t.lastDeltaY = state.lastY - t.y1;
        break;
      case 8:
        t.zOffset = state.lastZ;
        break;
      case 9:
        CenterPart();
        break;
      case 10:
        if (t.x1 == 0) {
          return;
        }
        channel.MoveTo(x: t.x1, y: t.y1, s: settings.speed);
        updateState();
        break;
      case 11:
        channel.Move(x: -t.lastDeltaX, y: -t.lastDeltaY, s: settings.speed);
        updateState();
        break;
      case 12:
        channel.Move(x: t.lastDeltaX, y: t.lastDeltaY, s: settings.speed);
        updateState();
        break;
      case 20:
        Native.Vec2 retval = new Native.Vec2();
        int sx = (int) (settings.downCamera.xPixelPerMm * 1.5m);
        int sy = (int) (settings.downCamera.yPixelPerMm * 1.5m);
        //Native.findrectangle(0, sx, sy, 10, 10, sx * 2, sy * 4, retval);
        Native.findcircle(0, sx / 2, 12, sx * 2, sy * 4, settings.downCamera.threshold1, settings.downCamera.threshold2, retval);
        decimal mx = Decimal.Round(retval.x / settings.downCamera.xPixelPerMm, 2);
        decimal my = Decimal.Round(retval.y / settings.downCamera.yPixelPerMm, 2);
        Native.DrawText(1200, 810, 1, 0, "Got: " + mx + "  " + my);
        if (Math.Abs(mx) < 1 && Math.Abs(my) < 4) {
          channel.Move(x: mx, y: my, s: 1000);
          updateState();
        }
        break;
      }
      updateTape();
    }

    static void AddTapePanel(int x, int y)
    {
      Native.add_button("Prev", x, y, 50, 25, 1, tapeCallback);
      Native.add_button("Next", x + 60, y, 50, 25, 2, tapeCallback);
      Native.add_button("Move to Last", x + 120, y, 110, 25, 3, tapeCallback);
      Native.add_button("Move to Start", x, y + 30, 110, 25, 10, tapeCallback);
      Native.add_button("Prev", x + 120, y + 30, 50, 25, 11, tapeCallback);
      Native.add_button("Next", x + 180, y + 30, 50, 25, 12, tapeCallback);
      Native.add_button("Set Start", x, y + 60, 110, 25, 4, tapeCallback);
      Native.add_button("Set End", x + 120, y + 60, 110, 25, 5, tapeCallback);
      Native.add_button("Set Last", x, y + 90, 110, 25, 6, tapeCallback);
      Native.add_button("Set Delta", x + 120, y + 90, 110, 25, 7, tapeCallback);
      Native.add_button("Set Z Offset", x, y + 120, 110, 25, 8, tapeCallback);
      Native.add_button("Center", x + 120, y + 120, 110, 25, 9, tapeCallback);
      Native.add_button("Sync", x, y + 150, 110, 25, 20, tapeCallback);
    }
  }
}
