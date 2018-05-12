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
    delegate void StepDelegate();
    static StepDelegate[] steps = null;
    static int stepIdx = 0;
    static string error = null;
    static decimal partOffsetX = 0;
    static decimal partOffsetY = 0;

    static void Error(string err)
    {
      error = err;
      Native.DrawText(20, 860, 3, 0, err);
    }

    static void BackgroundTask()
    {
      try {
        for (stepIdx = 0; stepIdx < steps.Length; stepIdx++) {
          Native.DrawText(20, 860, 1, 0, "Running: " + steps[stepIdx].Method.Name + "                                                                       ");
          steps[stepIdx]();
          updateState();
          channel.WaitForDone();
          if (error != null) {
            break;
          }
        }
      } catch (Exception e) {
        Error("Internal error.");
        Console.WriteLine(e.Message);
        Console.WriteLine(e.Source);
        Console.WriteLine(e.StackTrace);
      }
      if (error == null)
        Native.DrawText(20, 860, 4, 0, "Ready.                                                                                                  ");
      updateState();
      steps = null;
    }

    static void Run(params StepDelegate[] s)
    {
      if (steps != null) {
        Error("Task running, please wait.");
        return;
      }
      error = null;
      steps = s;
      channel.WaitForDone();
      Thread t = new Thread(new ThreadStart(BackgroundTask));
      t.Start();
    }

    static void Loop()
    {
      stepIdx = -1;
    }

    static void Delay2s()
    {
      Thread.Sleep(2000);
    }

    static void Delay1s()
    {
      Thread.Sleep(1000);
    }

    static void Delay500ms()
    {
      Thread.Sleep(500);
    }

    static void Delay100ms()
    {
      Thread.Sleep(100);
    }

    static int FindTape(string package, string value)
    {
      int idx = tapes.FindIndex((x) => x.packageName == package && x.value == value);
      if (idx < 0) {
        Error("Tape " + package + " - " + value + " not found.");
        return 0;
      }
      return idx;
    }

    static void PickDown()
    {
      Tape t = tapes[state.lastTape];
      if (state.lastNozzle < 0 || t.nozzle != settings.nozzles[state.lastNozzle].name) {
        Error("Wrong type of nozzle, needed: " + t.nozzle);
        return;
      }
      if ((t.packageName != board.steps[state.lastStep].packageName || t.value != board.steps[state.lastStep].value) && t.value != "0") {
        Error("Wrong tape, needed: " + board.steps[state.lastStep].packageName + " " + board.steps[state.lastStep].value);
        return;
      }
      channel.RotationOn();
      channel.ExhaustValveOn();
      channel.VacuumValveOff();
      partOffsetX = 0;
      partOffsetY = 0;
      channel.SetCoords(e: 0);
      channel.WaitForDone();
      channel.Move(x: settings.downCamera.xOffset + t.xOffset, y: settings.downCamera.yOffset + t.yOffset, s: settings.speed);
      channel.MoveTo(z: tapes[state.lastTape].zOffset + 2, s: settings.zSpeed);
      channel.MoveTo(z: tapes[state.lastTape].zOffset, s: 200);
      channel.VacuumValveOn();
      channel.ExhaustValveOff();
      tapes[state.lastTape].lastPartX = channel.posX - t.xOffset - settings.downCamera.xOffset + t.lastDeltaX;
      tapes[state.lastTape].lastPartY = channel.posY - t.yOffset - settings.downCamera.yOffset + t.lastDeltaY;
      updateState();
      updateTape();
    }

    static decimal PartAngleToPnpAngle(decimal angle)
    {
      while (angle > 180)
        angle -= 360;
      while (angle < -180)
        angle += 360;
      return angle * 40 / 360;
    }

    static void PickUp()
    {
      channel.MoveTo(z: tapes[state.lastTape].zOffset + 5, s: 100);
      channel.MoveTo(z: settings.upCamera.zOffset, s: settings.zSpeed);
      UpLightOn();
      channel.MoveTo(x: settings.upCamera.xOffset, y: settings.upCamera.yOffset, s: settings.speed);
      channel.MoveTo(e: PartAngleToPnpAngle(board.steps[state.lastStep].angle + tapes[state.lastTape].rotation), s: 10000);
      updateState();
    }

    static void PrePlace()
    {
      decimal h = settings.boardZeroZ + tapes[state.lastTape].height - 1;
      channel.MoveTo(z: h + 2, s: settings.zSpeed);
      updateState();
    }

    static void PlaceDown()
    {
      decimal h = settings.boardZeroZ + tapes[state.lastTape].height - 1;
      channel.MoveTo(z: h, s: 500);
      channel.ExhaustValveOn();
      channel.VacuumValveOff();
      updateState();
    }

    static void PlaceUp()
    {
      decimal h = settings.boardZeroZ + tapes[state.lastTape].height - 1;
      channel.MoveTo(z: h + 3, s: 300);
      channel.MoveTo(z: 12, s: settings.zSpeed);
      channel.RotationOff();
      updateState();
      if (state.lastStep == board.steps.Count) {
        Error("Placement completed.");
        return;
      }
      state.lastStep++;
      updateBoard();
    }

    static void MoveToPart()
    {
      var xy = CalcPartRelative();
      channel.MoveTo(x: xy.Item1 + settings.downCamera.xOffset + partOffsetX, y: xy.Item2 + settings.downCamera.yOffset + partOffsetY, s: settings.speed);
      updateState();
    }

    static void NextPart()
    {
      channel.MoveTo(x: tapes[state.lastTape].lastPartX, y: tapes[state.lastTape].lastPartY, s: settings.speed);
      UpLightOn();
      updateState();
      updateBoard();
      updateTape();
    }

    static void NextTape()
    {
      state.lastTape = FindTape(board.steps[state.lastStep].packageName, board.steps[state.lastStep].value);
      updateTape();
      updateCameras();
    }

    static void CheckNozzle()
    {
      Tape t = tapes[state.lastTape];
      if (state.lastNozzle >= 0 && t.nozzle == settings.nozzles[state.lastNozzle].name) {
        return;
      }
      MoveToUpCamera();
      CorrectUpCam();
      int idx = settings.nozzles.FindIndex((n) => n.name == t.nozzle);
      if (idx < 0) {
        Error("Nozzle not found: " + t.nozzle);
        return;
      }
      channel.WaitForDone();
      ChangeNozzle(idx);
      channel.WaitForDone();
      MoveToUpCamera();
      Delay1s();
      CorrectUpCam();
    }

    static void AlignPart()
    {
      channel.SetCoords(e: 0);
      Native.MinRect mr = new Native.MinRect();
      Native.MinRect mr2 = new Native.MinRect();
      Camera u = settings.upCamera;
      Tape t = tapes[state.lastTape];
      decimal w = Math.Max(t.partRects[0].width, t.partRects[0].height);
      int partsize = (int)(t.partRects[0].width * t.partRects[0].height * u.xPixelPerMm * u.yPixelPerMm);
      int ww = (int)((w * 1.4m + 1) * u.xPixelPerMm);
      Native.get_minrect(1, ww, ww, t.threshold1, t.threshold2, t.gauss1, t.gauss2, 25, partsize, 5, mr);
      int angle = mr.angle < -45 ? (mr.angle + 90) : mr.angle;
      if (angle > 17 || angle < -17) {
        Error("Part angle too big: " + angle);
        return;
      }
      if ((w > 2 && angle != settings.zeroAngle) || angle > 3 || angle < -3) {
        channel.MoveTo(e: PartAngleToPnpAngle(-angle + settings.zeroAngle), s: 10000);
        channel.WaitForDone();
        Native.get_minrect(1, ww, ww, t.threshold1, t.threshold2, t.gauss1, t.gauss2, 25, partsize, 5, mr2);
      } else {
        mr2 = mr;
      }
      decimal sx = Decimal.Round((mr2.xc - u.resolutionX / 2) / u.xPixelPerMm, 2);
      decimal sy = -Decimal.Round((mr2.yc - u.resolutionY / 2) / u.yPixelPerMm, 2);
      Native.DrawText(420, 810, 1, 0, "Angle: " + angle + " (" + sx + "," + sy + ")  " + mr.size + "                  ");
      Native.DrawLine(mr.x1 / 2 + 200, mr.y1 / 2 + 150, mr.x2 /2 + 200, mr.y2 / 2 + 150, 3);
      Native.DrawLine(mr.x3 / 2 + 200, mr.y3 / 2 + 150, mr.x2 /2 + 200, mr.y2 / 2 + 150, 3);
      Native.DrawLine(mr.x3 / 2 + 200, mr.y3 / 2 + 150, mr.x4 /2 + 200, mr.y4 / 2 + 150, 3);
      Native.DrawLine(mr.x1 / 2 + 200, mr.y1 / 2 + 150, mr.x4 /2 + 200, mr.y4 / 2 + 150, 3);
      if (Math.Abs(sx) > 0.6m || Math.Abs(sy) > 0.6m) {
        Error("Part misalignment too big: " + sx + ", " + sy);
        return;
      }
      partOffsetX = sx + settings.fineOffsetX + t.visionXOffset;
      partOffsetY = sy + settings.fineOffsetY + t.visionYOffset;
    }

    static void PnpCallback(long data)
    {
      switch (data & 255)
      {
        case 1:
          channel.PumpOn();
          break;
        case 2:
          channel.PumpOff();
          break;
        case 3:
          channel.VacuumValveOn();
          break;
        case 4:
          channel.VacuumValveOff();
          break;
        case 5:
          channel.ExhaustValveOn();
          break;
        case 6:
          channel.ExhaustValveOff();
          break;
        case 21:
          Run(PickDown, Delay1s, PickUp);
          break;
        case 22:
          Run(CorrectUpCam, UpLightOff, DownLightOn, AlignPart, DownLightOff);
          break;
        case 23:
          Run(MoveToPart);
          break;
        case 24:
          Run(PrePlace, PlaceDown, Delay500ms, PlaceUp);
          break;
        case 25:
          Run(NextPart, CenterPart);
          break;
        case 30:
          Run(NextPart, CenterPart, PickDown, Delay1s, PickUp, Delay500ms, CorrectUpCam, UpLightOff, DownLightOn, AlignPart, DownLightOff);
          break;
        case 31:
          Run(MoveToPart, PrePlace, PlaceDown, Delay500ms, PlaceUp);
          break;
        case 32:
          Run(NextPart, CenterPart, PickDown, Delay1s, PickUp, CorrectUpCam, UpLightOff, DownLightOn, AlignPart, DownLightOff, MoveToPart, PrePlace, PlaceDown, Delay500ms, PlaceUp);
          break;
        case 33:
          Run(NextTape, CheckNozzle);
          break;
        case 40:
          Run(MoveToPart, PrePlace);
          break;
        case 41:
          Run(PlaceDown, Delay500ms, PlaceUp);
          break;
        case 42:
          Error("Stopped by user.");
          break;
        case 50:
          Run(MoveToZero, HomeZ, MoveToUpCamera, UpLightOn, AlignUpCam, AlignUpCam, AlignUpCam, UpLightOff);
          break;
        case 51:
          channel.MoveTo(z: 16, s: settings.zSpeed);
          UpLightOn();
          var sl = new List<StepDelegate>();
          for (int j = 0; j < 2; j++) {
            for (int i = 0; i < board.fiducials.Count; i++) {
              sl.Add(MoveToUpCameraFast);
              sl.Add(CorrectUpCam);
              int k = i;
              sl.Add(() => MoveToFid(k));
              sl.Add(() => AlignFid(k));
              sl.Add(() => AlignFid(k));
            }
          }
          sl.Add(UpLightOff);
          Run(sl.ToArray());
          break;
        case 52:
          Run(NextTape, CheckNozzle, NextPart, CenterPart, PickDown, Delay1s, PickUp, CorrectUpCam, UpLightOff, DownLightOn, AlignPart, DownLightOff, MoveToPart, PrePlace, PlaceDown, Delay500ms, PlaceUp, Loop);
          break;
      }
    }

    static void AddPnpPanel(int x, int y)
    {
      Native.add_button("Pump On", x, y, 110, 25, 1, pnpCallback);
      Native.add_button("Pump Off", x, y + 30, 110, 25, 2, pnpCallback);
      Native.add_button("Vacuum On", x, y + 60, 110, 25, 3, pnpCallback);
      Native.add_button("Vacuum Off", x, y + 90, 110, 25, 4, pnpCallback);
      Native.add_button("Exhaust On", x, y + 120, 110, 25, 5, pnpCallback);
      Native.add_button("Exhaust Off", x, y + 150, 110, 25, 6, pnpCallback);
      Native.add_button("Pick", x + 120, y, 110, 25, 21, pnpCallback);
      Native.add_button("Align Part", x + 120, y + 30, 110, 25, 22, pnpCallback);
      Native.add_button("Position", x + 120, y + 60, 110, 25, 23, pnpCallback);
      Native.add_button("Place", x + 120, y + 90, 110, 25, 24, pnpCallback);
      Native.add_button("Next Part", x + 120, y + 120, 110, 25, 25, pnpCallback);
    }

    static void AddRunPanel(int x, int y)
    {
      Native.add_button("Pick Run", x, y, 110, 25, 30, pnpCallback);
      Native.add_button("Place Run", x, y + 30, 110, 25, 31, pnpCallback);
      Native.add_button("Part Run", x, y + 60, 110, 25, 32, pnpCallback);
      Native.add_button("Tape Run", x, y + 90, 110, 25, 33, pnpCallback);
      Native.add_button("Pre-place", x + 120, y, 110, 25, 40, pnpCallback);
      Native.add_button("Finish place", x + 120, y + 30, 110, 25, 41, pnpCallback);
      Native.add_button("Stop", x + 120, y + 60, 110, 25, 42, pnpCallback);
      Native.add_button("Start Run", x + 240, y, 110, 25, 50, pnpCallback);
      Native.add_button("Board Run", x + 240, y + 30, 110, 25, 51, pnpCallback);
      Native.add_button("Place All", x + 240, y + 60, 110, 25, 52, pnpCallback);
    }
  }
}
