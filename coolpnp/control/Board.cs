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
    static Tuple<decimal, decimal> rotateVect(decimal x, decimal y, decimal angle)
    {
      decimal ca = (decimal)Math.Cos((double) angle * Math.PI / 180);
      decimal sa = (decimal)Math.Sin((double) angle * Math.PI / 180);
      decimal rx = x * ca + y * sa;
      decimal ry = y * ca - x * sa;
      return new Tuple<decimal, decimal>(rx, ry);
    }

    static Tuple<decimal, decimal> boardPosToPnpPos(decimal x, decimal y)
    {
      var bx = rotateVect(board.sizeX, 0, board.rotation);
      var by = rotateVect(0, board.sizeY, board.rotation);
      decimal zx = 0, zy = 0;
      switch (settings.boardJigRotation)
      {
        case 90:
          zx = board.sizeY;
          break;
        case 180:
          zx = board.sizeX;
          zy = board.sizeY;
          break;
        case 270:
          zy = board.sizeX;
          break;
      }
      decimal rx = zx + x / board.sizeX * bx.Item1 + y / board.sizeY * bx.Item2;
      decimal ry = zy + x / board.sizeX * by.Item1 + y / board.sizeY * by.Item2;
      rx += settings.boardZeroX - settings.downCamera.xOffset;
      ry += settings.boardZeroY - settings.downCamera.yOffset;
      return new Tuple<decimal, decimal>(rx, ry);
    }

    static void updateBoard()
    {
      if (board == null || board.steps == null || board.steps.Count == 0 || board.fiducials == null || board.fiducials.Count == 0)
        return;
      if (state.lastStep <= 0)
        state.lastStep = 0;
      if (state.lastStep >= board.steps.Count - 1)
        state.lastStep = board.steps.Count - 1;
      if (state.lastFid <= 0)
        state.lastFid = 0;
      if (state.lastFid >= board.fiducials.Count - 1)
        state.lastFid = board.fiducials.Count - 1;

      Placement pl = board.steps[state.lastStep];
      Native.DrawText(820, 275, 1, 0, pl.reference + "  " + pl.posX + "  " + pl.posY + "  " + pl.angle + "            ");
      Fiducial f = board.fiducials[state.lastFid];
      Native.DrawText(820, 300, 1, 0, f.id + "  " + f.posX + "  " + f.posY + "  " + f.realX + "  " + f.realY + "            ");
    }

    static decimal Distance(Placement p, Fiducial f)
    {
      decimal x = f.posX - p.posX;
      decimal y = f.posY - p.posY;
      return x * x + y * y;
    }

    static decimal Distance(Fiducial f1, Fiducial f2)
    {
      decimal x = f1.realX - f2.realX;
      decimal y = f1.realY - f2.realY;
      return x * x + y * y;
    }

    static Tuple<decimal, decimal> CalcPartRelative()
    {
      Placement pl = board.steps[state.lastStep];
      var fds = (from f in board.fiducials select f).OrderBy((xf) => Distance(pl, xf)).ToList();
      if (fds.Count > 3)
        fds.RemoveRange(3, fds.Count - 3);
      if (fds.Count == 2) {
        if (fds[0].posX > fds[1].posX) {
          Fiducial tmp = fds[0];
          fds[0] = fds[1];
          fds[1] = tmp;
        }
        Fiducial fe = new Fiducial();
        fe.posX = fds[0].posX - (fds[1].posY - fds[0].posY);
        fe.posY = fds[0].posY + (fds[1].posX - fds[0].posX);
        fe.realX = fds[0].realX + fds[1].realY - fds[0].realY;
        fe.realY = fds[0].realY + fds[1].realX - fds[0].realX;
        fds.Add(fe);
      }
      var d1 = Distance(fds[0], fds[1]);
      var d2 = Distance(fds[0], fds[2]);
      var d3 = Distance(fds[2], fds[1]);
      Fiducial fc, fx, fy;
      if (d1 > d2) {
        if (d1 > d3) {
          fx = fds[0];
          fy = fds[1];
          fc = fds[2];
        } else {
          fx = fds[2];
          fy = fds[1];
          fc = fds[0];
        }
      } else {
        if (d2 > d3) {
          fx = fds[0];
          fy = fds[2];
          fc = fds[1];
        } else {
          fx = fds[2];
          fy = fds[1];
          fc = fds[0];
        }
      }
      if (Math.Abs(fy.posX - fc.posX) > Math.Abs(fx.posX - fc.posX)) {
        Fiducial ftmp = fx;
        fx = fy;
        fy = ftmp;
      }
      decimal vx = fx.posX - fc.posX;
      decimal vy = fy.posY - fc.posY;
      decimal px = board.steps[state.lastStep].posX - fc.posX;
      decimal py = board.steps[state.lastStep].posY - fc.posY;
      decimal cx = px / vx;
      decimal cy = py / vy;
      decimal x = fc.realX + cx * (fx.realX - fc.realX) + cy * (fy.realX - fc.realX);
      decimal y = fc.realY + cx * (fx.realY - fc.realY) + cy * (fy.realY - fc.realY);
      Native.DrawText(820, 530, 1, 0, Decimal.Round(x - state.lastX, 2) + "  " + Decimal.Round(y - state.lastY, 2));
      return new Tuple<decimal, decimal>(x, y);
    }

    static void MoveToPartRelative2()
    {
      Placement pl = board.steps[state.lastStep];
      var fds = (from f in board.fiducials select f).OrderBy((xf) => Distance(pl, xf)).ToList();
      var d1 = Math.Sqrt((double) Distance(fds[0], fds[1]));
      var d2 = Math.Sqrt((double) Distance(fds[0], fds[2]));
      var d3 = Math.Sqrt((double) Distance(fds[2], fds[1]));
      var df1 = (double) Distance(pl, fds[0]);
      var df2 = (double) Distance(pl, fds[1]);
      var df3 = (double) Distance(pl, fds[2]);

      var a = (df1 - df2 + d1 * d1) / (2 * d1);
      var h = Math.Sqrt(df1 - a * a);
      var x2 = (double) fds[0].realX + a * (double) (fds[1].realX - fds[0].realX) / d1;
      var y2 = (double) fds[0].realY + a * (double) (fds[1].realY - fds[0].realY) / d1;
      var x31 = x2 + h * (double) (fds[1].realY - fds[0].realY) / d1;
      var y31 = y2 - h * (double) (fds[1].realX - fds[0].realX) / d1;
      var x32 = x2 - h * (double) (fds[1].realY - fds[0].realY) / d1;
      var y32 = y2 + h * (double) (fds[1].realX - fds[0].realX) / d1;
      var d31 = Math.Abs(((double)fds[2].realX - x31) * ((double)fds[2].realX - x31) + ((double)fds[2].realY - y31) * ((double)fds[2].realY - y31) - df3);
      var d32 = Math.Abs(((double)fds[2].realX - x32) * ((double)fds[2].realX - x32) + ((double)fds[2].realY - y32) * ((double)fds[2].realY - y32) - df3);
      if (d32 < d31) {
        x31 = x32;
        y31 = y32;
      }
      Native.DrawText(820, 530, 1, 0, Decimal.Round((decimal)x31 - state.lastX, 2) + "  " + Decimal.Round((decimal)y31 - state.lastY, 2) + "              ");

      channel.MoveTo(x: Decimal.Round((decimal)x31, 2), y: Decimal.Round((decimal)y31, 2), s: settings.speed);
    }

    static void AlignUpCam()
    {
      int dx = (int) (settings.downCamera.xPixelPerMm * 1.0m);
      Native.Vec2 retval = new Native.Vec2();
      Native.findcheck(0, settings.markXOffset, settings.markYOffset, dx, dx, dx * 4, dx * 4, retval);
      decimal mx = Decimal.Round(retval.x / settings.downCamera.xPixelPerMm, 2);
      decimal my = Decimal.Round(retval.y / settings.downCamera.yPixelPerMm, 2);
      Native.DrawText(820, 560, 1, 0, "Camera Got: " + mx + "  " + my);
      if (Math.Abs(mx) > 0.8m || Math.Abs(my) > 0.8m) {
        Error("Camera correction too big: " + mx + ", " + my);
        return;
      }
      channel.Move(x: mx, y: my, s: 1000);
      updateState();
      settings.upCamera.xOffset = channel.posX;
      settings.upCamera.yOffset = channel.posY;
    }

    static void CorrectUpCam()
    {
      int dx = (int) (settings.downCamera.xPixelPerMm * 1.0m);
      Native.Vec2 retval = new Native.Vec2();
      Native.findcheck(0, settings.markXOffset, settings.markYOffset, dx, dx, dx * 4, dx * 4, retval);
      decimal mx = Decimal.Round(retval.x / settings.downCamera.xPixelPerMm, 2);
      decimal my = Decimal.Round(retval.y / settings.downCamera.yPixelPerMm, 2);
      Native.DrawText(820, 560, 1, 0, "Camera Got: " + mx + "  " + my);
      if (Math.Abs(mx) > 0.8m || Math.Abs(my) > 0.8m) {
        Error("Camera correction too big: " + mx + ", " + my);
        return;
      }
      channel.Move(x: mx, y: my, s: 1000);
      channel.SetCoords(x: settings.upCamera.xOffset, y: settings.upCamera.yOffset);
      updateState();
    }

    static void MoveToFid(int fi)
    {
      Fiducial f = board.fiducials[fi];
      var tf = boardPosToPnpPos(f.posX, f.posY);
      channel.MoveTo(x: tf.Item1, y: tf.Item2, s: settings.speed);
      updateState();
    }

    static void MoveToFidReal(int fi)
    {
      FiduState fs = state.fiducials[fi];
      if (fs.realX == 0)
        return;
      UpLightOn();
      channel.MoveTo(x: fs.realX, y: fs.realY, s: settings.speed);
      updateState();
    }

    static void AlignFid(int fi)
    {
      int fx = (int) (settings.downCamera.xPixelPerMm / 2 * 1.0m);
      var retval = new Native.Vec2();
      Native.findcircle(0, fx / 2, 12, fx * 4, fx * 4, settings.downCamera.threshold1, settings.downCamera.threshold2, retval);
      decimal mx = Decimal.Round(retval.x / settings.downCamera.xPixelPerMm, 2);
      decimal my = Decimal.Round(retval.y / settings.downCamera.yPixelPerMm, 2);
      Native.DrawText(820, 500, 1, 0, "Got: " + mx + "  " + my);
      if (Math.Abs(mx) < 0.5m && Math.Abs(my) < 0.5m) {
        channel.Move(x: mx, y: my, s: 1000);
        updateState();
        state.fiducials[fi].realX = channel.posX;
        state.fiducials[fi].realY = channel.posY;
        board.fiducials[fi].realX = channel.posX;
        board.fiducials[fi].realY = channel.posY;
        return;
      }
      Error("Fiducial correction too big: " + mx + ", " + my);
    }

    static void BoardCallback(long data)
    {
      if (board == null || board.steps == null || board.steps.Count == 0 || board.fiducials == null || board.fiducials.Count == 0)
        return;
      switch (data&255)
      {
      case 1:
        state.lastStep--;
        if (state.lastStep <= 0)
          state.lastStep = 0;
        break;
      case 2:
        state.lastStep++;
        if (state.lastStep >= board.steps.Count - 1)
          state.lastStep = board.steps.Count - 1;
        break;
      case 3:
        Placement pl = board.steps[state.lastStep];
        var tp = boardPosToPnpPos(pl.posX, pl.posY);
        channel.MoveTo(x: tp.Item1, y: tp.Item2, s: settings.speed);
        updateState();
        break;
      case 4:
        state.lastFid--;
        if (state.lastFid <= 0)
          state.lastFid = 0;
        break;
      case 5:
        state.lastFid++;
        if (state.lastFid >= board.fiducials.Count - 1)
          state.lastFid = board.fiducials.Count - 1;
        break;
      case 6:
        Run(() => MoveToFid(state.lastFid));
        break;
      case 7:
        Run(() => AlignFid(state.lastFid));
        break;
      case 8:
        var xy = CalcPartRelative();
        channel.MoveTo(x: xy.Item1, y: xy.Item2, s: settings.speed);
        updateState();
        break;
      case 9:
        Fiducial f = board.fiducials[state.lastFid];
        var tf = boardPosToPnpPos(f.posX, f.posY);
        settings.boardZeroX += state.lastX - tf.Item1;
        settings.boardZeroY += state.lastY - tf.Item2;
        break;
      case 10:
        Run(() => MoveToFidReal(state.lastFid));
        break;
      case 11:
        Native.Vec2 retval = new Native.Vec2();
        Native.findsymmetry(0, 50, 50, 100, 100, retval);
        Native.findsymmetry(1, 50, 50, 100, 100, retval);
        Native.save_screen("test.bmp");
        break;
      case 20:
        Run(AlignUpCam);
        break;
      case 21:
        Run(CorrectUpCam);
        break;
      }
      updateBoard();
    }

    static void AddBoardPanel(int x, int y)
    {
      if (state.fiducials.Count != board.fiducials.Count) {
        state.fiducials.Clear();
        for (int i = 0; i < board.fiducials.Count; i++)
          state.fiducials.Add(new FiduState());
      } else {
        for (int i = 0; i < board.fiducials.Count; i++) {
          board.fiducials[i].realX = state.fiducials[i].realX;
          board.fiducials[i].realY = state.fiducials[i].realY;
        }
      }

      Native.add_button("Prev", x, y, 50, 25, 1, boardCallback);
      Native.add_button("Next", x + 60, y, 50, 25, 2, boardCallback);
      Native.add_button("Move", x + 120, y, 50, 25, 3, boardCallback);
      Native.add_button("Prev", x, y + 30, 50, 25, 4, boardCallback);
      Native.add_button("Next", x + 60, y + 30, 50, 25, 5, boardCallback);
      Native.add_button("Move", x + 120, y + 30, 50, 25, 6, boardCallback);
      Native.add_button("Real", x + 180, y + 30, 50, 25, 10, boardCallback);
      Native.add_button("Align Fid", x, y + 60, 110, 25, 7, boardCallback);
      Native.add_button("MoveX", x + 120, y + 60, 110, 25, 8, boardCallback);
      Native.add_button("Set Zero", x, y + 90, 110, 25, 9, boardCallback);
      Native.add_button("Align UpCam", x + 120, y + 90, 110, 25, 20, boardCallback);
      Native.add_button("Save Image", x, y + 120, 110, 25, 11, boardCallback);
      Native.add_button("Correct", x + 120, y + 120, 110, 25, 21, boardCallback);
    }
  }
}
