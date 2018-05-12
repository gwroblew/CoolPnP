using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Concurrent;

namespace control
{
  class GCodeTcp
  {
    public const int maxX = 840;
    public const int maxY = 340;
    public const int maxZ = 25;
    public const int maxE = 40;
    public bool homed = false;
    public bool pumpOn = false;
    public bool vacuumOn = false;
    public bool exhaustOn = false;
    public decimal lastBacklashX = 0;
    public static decimal xBacklash = 0.24m;
    public static decimal xBacklash2 = xBacklash / 2;
    public static decimal xFineBacklash = 0.16m;

    public TcpClient client = new TcpClient();
    public NetworkStream networkStream;
    public StreamWriter writer;
    public StreamReader reader;
    public decimal posX = 0;
    public decimal posY = 0;
    public decimal posZ = 0;
    public decimal posE = 0;
    public ConcurrentQueue<string> writeQueue = new ConcurrentQueue<string>();
    public ConcurrentQueue<string> readQueue = new ConcurrentQueue<string>();
    public Thread writeThread;
    public Thread readThread;

    public GCodeTcp(IPAddress ipAddress, int port)
    {
      try
      {
        client.Connect(ipAddress, port); // connect to the server
        
        networkStream = client.GetStream();
        writer = new StreamWriter(networkStream);
        reader = new StreamReader(networkStream);

        writer.AutoFlush = true;

        Console.WriteLine(reader.ReadLine());
        SetCoords(0, 0, 0, 0);
        Move(0, 0, 0, 0, 10000);
        PumpOff();
        VacuumValveOff();
        ExhaustValveOff();
        UpLightOff();
        DownLightOff();
        RotationOff();
        writeThread = new Thread(new ThreadStart(Sender));
        readThread = new Thread(new ThreadStart(Reader));
        writeThread.Start();
        readThread.Start();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        System.Environment.Exit(-1);
      }
    }

    public void Close()
    {
      writeQueue.Clear();
      readQueue.Clear();
      writeThread.Interrupt();
      readThread.Interrupt();
      client.Close();
    }

    public void Sender()
    {
      while(true)
      {
        if (writeQueue.Count == 0)
        {
          Thread.Sleep(20);
          continue;
        }
        try
        {
          string gcode = "";
          if(!writeQueue.TryPeek(out gcode))
            continue;
          writer.WriteLine(gcode);
          readQueue.Enqueue(gcode);
          //Console.WriteLine(gcode);
          writeQueue.TryDequeue(out gcode);
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
        }
      }
    }

    public void Reader()
    {
      while(true)
      {
        if (readQueue.Count == 0)
        {
          Thread.Sleep(20);
          continue;
        }
        try
        {
          string gcode = "";
          if (!readQueue.TryPeek(out gcode))
            continue;
          string response = reader.ReadLine();
          if (response.ToLower() != "ok")
          {
            Console.WriteLine(gcode + " -> " + response);
          }
          readQueue.TryDequeue(out gcode);
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
        }
      }
    }

    public bool IsDone()
    {
      return readQueue.Count == 0;
    }

    public void WaitForDone()
    {
      SendCommand("M400");
      while(writeQueue.Count != 0)
        Thread.Sleep(50);
      while(!IsDone())
        Thread.Sleep(50);
    }

    public void SendCommand(string gcode)
    {
      writeQueue.Enqueue(gcode);
    }

    public void SteppersOn()
    {
      SendCommand("M17");
    }

    public void SteppersOff()
    {
      SendCommand("M18");
    }

    public void RotationOn()
    {
      SendCommand("M17A");
    }

    public void RotationOff()
    {
      SendCommand("M18A");
    }

    public void PumpOn()
    {
      SendCommand("M42");
      pumpOn = true;
    }

    public void PumpOff()
    {
      SendCommand("M43");
      pumpOn = false;
    }

    public void ExhaustValveOn()
    {
      SendCommand("M44");
      exhaustOn = true;
    }

    public void ExhaustValveOff()
    {
      SendCommand("M45");
      exhaustOn = false;
    }

    public void VacuumValveOn()
    {
      SendCommand("M46");
      vacuumOn = true;
    }

    public void VacuumValveOff()
    {
      SendCommand("M47");
      vacuumOn = false;
    }

    public void UpLightOn(int duty)
    {
      duty = Math.Min(Math.Max(duty, 0), 30);
      SendCommand("M48 S" + duty);
    }

    public void UpLightOff()
    {
      SendCommand("M49");
    }

    public void DownLightOn(int duty)
    {
      duty = Math.Min(Math.Max(duty, 0), 30);
      SendCommand("M50 S" + duty);
    }

    public void DownLightOff()
    {
      SendCommand("M51");
    }

    public void HomeX()
    {
      SendCommand("G28 X");
      WaitForDone();
      SetCoords(x: 0);
      posX = 0;
    }

    public void HomeY()
    {
      SendCommand("G28 Y");
      WaitForDone();
      SetCoords(y: 0);
      posY = 0;
    }

    public void HomeZ()
    {
      SendCommand("G28 Z");
      WaitForDone();
      SetCoords(z: 0);
      posZ = 0;
    }

    private string FormatPos(string prefix, decimal value)
    {
      if (value == decimal.MaxValue)
        return "";
      return string.Format(prefix + "{0:0.00}", value);
    }

    private decimal Round(decimal d)
    {
      if (d == decimal.MaxValue)
        return d;
      return Decimal.Round(d, 2);
    }

    public void MoveTo(decimal x = decimal.MaxValue, decimal y = decimal.MaxValue, decimal z = decimal.MaxValue, decimal e = decimal.MaxValue, int s = 0, bool wait = false)
    {
      if (homed)
      {
        if (x != decimal.MaxValue)
          x = Math.Min(Math.Max(0, x), maxX);
        if (y != decimal.MaxValue)
          y = Math.Min(Math.Max(0, y), maxY);
        if (z != decimal.MaxValue)
          z = Math.Min(Math.Max(0, z), maxZ);
      }
      x = Round(x);
      y = Round(y);
      z = Round(z);
      e = Round(e);
      decimal cx = x;
      if (x != decimal.MaxValue && x != posX) {
        decimal dx = x - posX;
        dx = Math.Max(Math.Min(dx * xFineBacklash, xBacklash), -xBacklash);
        decimal mx = dx > 0 ? xBacklash2 : -xBacklash2;
        if (mx != lastBacklashX) {
          if (dx > 0) {
            cx += (lastBacklashX + dx) > xBacklash2 ? (xBacklash2 - lastBacklashX) : dx;
          } else {
            cx += (lastBacklashX + dx) < -xBacklash2 ? (-xBacklash2 - lastBacklashX) : dx;
          }
          lastBacklashX = Math.Max(Math.Min(lastBacklashX + dx, xBacklash2), -xBacklash2);
        }
        //Console.WriteLine("XC: " + (cx - x));
      }
      SendCommand("G1" + FormatPos(" X", cx) + FormatPos(" Y", y) + FormatPos(" Z", z) + FormatPos(" E", e) + (s == 0 ? "" : (" F" + s)));
      if (cx != x) {
        SetCoords(x: x);
      }
      posX = x == decimal.MaxValue ? posX : x;
      posY = y == decimal.MaxValue ? posY : y;
      posZ = z == decimal.MaxValue ? posZ : z;
      posE = e == decimal.MaxValue ? posE : e;
      if (wait)
        WaitForDone();
    }

    public void Move(decimal x = 0, decimal y = 0, decimal z = 0, decimal e = 0, int s = 0)
    {
      MoveTo(
        x == 0 ? decimal.MaxValue : posX + x,
        y == 0 ? decimal.MaxValue : posY + y,
        z == 0 ? decimal.MaxValue : posZ + z,
        e == 0 ? decimal.MaxValue : posE + e, s);
    }

    public void SetCoords(decimal x = decimal.MaxValue, decimal y = decimal.MaxValue, decimal z = decimal.MaxValue, decimal e = decimal.MaxValue)
    {
      x = Round(x);
      y = Round(y);
      z = Round(z);
      e = Round(e);
      SendCommand("G92" + FormatPos(" X", x) + FormatPos(" Y", y) + FormatPos(" Z", z) + FormatPos(" E", e));
      posX = x == decimal.MaxValue ? posX : x;
      posY = y == decimal.MaxValue ? posY : y;
      posZ = z == decimal.MaxValue ? posZ : z;
      posE = e == decimal.MaxValue ? posE : e;
    }
  }
}
