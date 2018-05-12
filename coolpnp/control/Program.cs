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
  public class FiduState
  {
    public decimal realX;
    public decimal realY;
  }

  public class State
  {
    public decimal lastX;
    public decimal lastY;
    public decimal lastZ;

    public int lastNozzle;

    public string lastBoard;
    public int lastStep;
    public int lastFid;
    public int lastTape;

    public List<FiduState> fiducials = new List<FiduState>();
  }

  public class Fiducial
  {
    public string id;
    public decimal posX;
    public decimal posY;
    public decimal innerRadius;
    public decimal outerRadius;
    [XmlIgnore]
    public decimal realX;
    [XmlIgnore]
    public decimal realY;
  }

  public class Placement
  {
    public string reference;
    public decimal posX;
    public decimal posY;
    public decimal angle;
    public string packageName;
    public string value;
  }

  public class PnpBoard
  {
    public List<Fiducial> fiducials;
    public List<Placement> steps;
    public decimal sizeX;
    public decimal sizeY;
    public int rotation;
  }

  public class PartRect
  {
    public decimal offsetX;
    public decimal offsetY;
    public decimal width;
    public decimal height;
  }

  public class Tape
  {
    public string packageName;
    public string value;
    public string nozzle;
    public decimal height;
    public decimal rotation;
    public List<PartRect> partRects;

    public decimal x1;
    public decimal y1;
    public decimal x2;
    public decimal y2;

    public decimal lastPartX;
    public decimal lastPartY;
    public decimal lastDeltaX;
    public decimal lastDeltaY;
    public decimal xOffset;
    public decimal yOffset;
    public decimal zOffset;

    public int lightIntensity;
    public int threshold1;
    public int threshold2;
    public int gauss1;
    public int gauss2;
    public decimal visionXOffset;
    public decimal visionYOffset;
  }

  public class Nozzle
  {
    public string name;
    public decimal innerDiameter;
    public decimal outerDiameter;
    public decimal zOffset;
    public decimal restX;
    public decimal restY;
  }

  public class Camera
  {
    public string card;
    public string businfo;
    public int resolutionX;
    public int resolutionY;
    public decimal xPixelPerMm;
    public decimal yPixelPerMm;
    public decimal depthRatio;
    public bool zFixed;
    public decimal xOffset;
    public decimal yOffset;
    public decimal zOffset;
    public int lightIntensity;
    public int exposure;
    public int brightness;
    public int contrast;
    public int sharpness;
    public int threshold1;
    public int threshold2;
    public int gauss1;
    public int gauss2;
  }

  public class Settings
  {
    public int speed;
    public int zSpeed;
    public Camera downCamera;
    public Camera upCamera;

    public decimal zProbeX;
    public decimal zProbeY;
    public decimal zProbeOffset;

    public List<Nozzle> nozzles;
    public decimal changeStartZ;
    public decimal changePickUpZ;
    public decimal changeSlideZ;
    public decimal changeReturnZ;
    public decimal changeVectorX;
    public decimal changeVectorY;

    public decimal boardZeroX;
    public decimal boardZeroY;
    public decimal boardZeroZ;
    public int boardJigRotation;

    public int markXOffset;
    public int markYOffset;
    public decimal markPixelsPerMm;
    public decimal fineOffsetX;
    public decimal fineOffsetY;
    public int zeroAngle;
  }

  public static partial class Program
  {
    static string stateFile = "state.xml";
    static string tapesFile = "tapes.xml";
    static string settingsFile = "settings.xml";

    static GCodeTcp channel = new GCodeTcp(new IPAddress(new byte[]{ 192, 168, 3, 222}), 23);
    public static State state;
    public static List<Tape> tapes;
    public static Settings settings;
    public static PnpBoard board;

    public static Native.ButtonCallback moveCallback = new Native.ButtonCallback(MoveCallback);
    public static Native.ButtonCallback moveToCallback = new Native.ButtonCallback(MoveToCallback);
    public static Native.ButtonCallback homeCallback = new Native.ButtonCallback(HomeCallback);
    public static Native.ButtonCallback boardCallback = new Native.ButtonCallback(BoardCallback);
    public static Native.ButtonCallback tapeCallback = new Native.ButtonCallback(TapeCallback);
    public static Native.ButtonCallback pnpCallback = new Native.ButtonCallback(PnpCallback);
    public static Native.ButtonCallback visionCallback = new Native.ButtonCallback(VisionCallback);

    public static void WriteToXmlFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
    {
        TextWriter writer = null;
        try
        {
            var serializer = new XmlSerializer(typeof(T));
            writer = new StreamWriter(filePath, append);
            serializer.Serialize(writer, objectToWrite);
        }
        finally
        {
            if (writer != null)
                writer.Close();
        }
    }

    public static T ReadFromXmlFile<T>(string filePath) where T : new()
    {
        TextReader reader = null;
        try
        {
            var serializer = new XmlSerializer(typeof(T));
            reader = new StreamReader(filePath);
            return (T)serializer.Deserialize(reader);
        }
        catch(Exception)
        {
        }
        finally
        {
            if (reader != null)
                reader.Close();
        }
        return new T();
    }

    public static void updateState()
    {
      state.lastX = channel.posX;
      state.lastY = channel.posY;
      state.lastZ = channel.posZ;

      string ns = state.lastNozzle < 0 ? "<EMPTY>" : settings.nozzles[state.lastNozzle].name;

      Native.DrawText(820, 5, 1, 0, "X: " + state.lastX + "  Y: " + state.lastY + "  Z: " + state.lastZ + "  " + ns + "         ");
    }

    static long DirCode(int dir, int value)
    {
      return ((value + 1000) << 8) + dir;
    }

    static long DirValue(long code)
    {
      return (code >> 8) - 1000;
    }

    static void MoveCallback(long data)
    {
      switch(data&255)
      {
      case 1:
          channel.Move(x: DirValue(data) * 0.1m, s: settings.speed);
          break;
      case 2:
          channel.Move(y: DirValue(data) * 0.1m, s: settings.speed);
          break;
      case 3:
          channel.Move(z: DirValue(data) * 0.1m, s: settings.zSpeed);
          break;
      case 4:
          channel.Move(e: DirValue(data), s: 1500);
          break;
      }
      updateState();
    }

    static void AxisPanel(string axis, int x, int y, int code, int value1, int value2, int value3, string label1, string label2, string label3)
    {
      Native.DrawText(x + 10, y, 1, 0, axis);
      Native.add_button("-" + label3, x, y + 30, 50, 25, DirCode(code, -value3), moveCallback);
      Native.add_button("-" + label2, x, y + 60, 50, 25, DirCode(code, -value2), moveCallback);
      Native.add_button("-" + label1, x, y + 90, 50, 25, DirCode(code, -value1), moveCallback);
      Native.add_button(label1, x, y + 120, 50, 25, DirCode(code, value1), moveCallback);
      Native.add_button(label2, x, y + 150, 50, 25, DirCode(code, value2), moveCallback);
      Native.add_button(label3, x, y + 180, 50, 25, DirCode(code, value3), moveCallback);
    }

    static void HomeZ()
    {
      channel.MoveTo(x: settings.zProbeX, y: settings.zProbeY, s: settings.zSpeed);
      channel.HomeZ();
      channel.SetCoords(z: settings.zProbeOffset);
      channel.MoveTo(z: settings.zProbeOffset + 3, s: settings.zSpeed);
    }

    static void HomeCallback(long data)
    {
      if (data == 0)
        channel.HomeX();
      else if (data == 1)
        channel.HomeY();
      else {
        Run(HomeZ);
      }
      updateState();
    }

    static void AddControlPanel(int x, int y)
    {
      AxisPanel("X", x, y, 1, 1, 10, 100, "0.1", "1", "10");
      AxisPanel("Y", x + 60, y, 2, 1, 10, 100, "0.1", "1", "10");
      AxisPanel("Z", x + 120, y, 3, 1, 10, 20, "0.1", "1", "2");
      AxisPanel("R", x + 180, y, 4, 1, 10, 20, "9'", "90'", "180'");
      Native.add_button("Home", x, y + 210, 50, 25, 0, homeCallback);
      Native.add_button("Home", x + 60, y + 210, 50, 25, 1, homeCallback);
      Native.add_button("Home", x + 120, y + 210, 50, 25, 2, homeCallback);
    }

    static void UnloadNozzle()
    {
      if (state.lastNozzle < 0)
        return;
      Nozzle n = settings.nozzles[state.lastNozzle];
      channel.MoveTo(z: settings.changeStartZ, s: settings.zSpeed);
      channel.WaitForDone();
      channel.MoveTo(x: n.restX + settings.changeVectorX, y: n.restY + settings.changeVectorY, s: settings.zSpeed);
      channel.MoveTo(z: settings.changeReturnZ + 2, s: settings.zSpeed, wait: true);
      channel.MoveTo(x: n.restX + settings.changeVectorX / 2, y: n.restY + settings.changeVectorY / 2, s: 300);
      channel.MoveTo(z: settings.changeReturnZ, s: 300, wait: true);
      channel.MoveTo(x: n.restX, y: n.restY, s: 300);
      channel.MoveTo(z: settings.changeStartZ - 2, s: 300, wait: true);
      state.lastNozzle = -1;
      HomeZ();
    }

    static void ChangeNozzle(int ni)
    {
      if (state.lastNozzle >= 0) {
        UnloadNozzle();
      }
      Nozzle n = settings.nozzles[ni];
      channel.MoveTo(z: settings.changeStartZ, s: settings.zSpeed);
      channel.MoveTo(x: n.restX, y: n.restY, s: settings.speed, wait: true);
      channel.MoveTo(z: settings.changePickUpZ, s: 300);
      channel.MoveTo(z: settings.changeSlideZ, s: 300, wait: true);
      channel.MoveTo(x: n.restX + settings.changeVectorX / 2, y: n.restY + settings.changeVectorY / 2, s: 300);
      channel.MoveTo(z: settings.changePickUpZ, s: 300);
      channel.MoveTo(z: settings.changeReturnZ, s: 300);
      channel.MoveTo(x: n.restX + settings.changeVectorX, y: n.restY + settings.changeVectorY, s: 300, wait: true);
      channel.MoveTo(z: 12, s: 1000);
      state.lastNozzle = ni;
    }

    static void MoveToZero()
    {
      channel.MoveTo(z: 20, s: settings.zSpeed);
      channel.WaitForDone();
      channel.MoveTo(x: 5, y: 5, s: settings.speed);
      channel.HomeX();
      channel.HomeY();
    }

    static void MoveToUpCamera()
    {
      channel.MoveTo(z: 20, s: settings.zSpeed);
      channel.MoveTo(x: settings.upCamera.xOffset, y: settings.upCamera.yOffset, s: settings.speed);
      UpLightOn();
      channel.MoveTo(z: settings.upCamera.zOffset, s: settings.zSpeed);
    }

    static void MoveToUpCameraFast()
    {
      channel.MoveTo(x: settings.upCamera.xOffset, y: settings.upCamera.yOffset, s: settings.speed);
      UpLightOn();
    }

    static void MoveToCallback(long data)
    {
      switch(data&255)
      {
      case 1:
        Run(MoveToUpCamera);
        break;
      case 2:
        channel.Move(x: settings.downCamera.xOffset, y: settings.downCamera.yOffset, s: settings.speed);
        break;
      case 3:
        channel.Move(x: -settings.downCamera.xOffset, y: -settings.downCamera.yOffset, s: settings.speed);
        break;
      case 4:
        channel.MoveTo(x: settings.boardZeroX, y: settings.boardZeroY, s: settings.speed);
        break;
      case 5:
        Run(MoveToZero);
        break;
      case 100:
      case 101:
      case 102:
      case 103:
      case 104:
      case 105:
      case 106:
      case 107:
      case 108:
      case 109:
        int i = (int)(data&255) - 100;
        Run(() => ChangeNozzle(i));
        break;
      case 200:
        Run(UnloadNozzle);
        break;
      case 201:
        state.lastNozzle = -1;
        break;
      }
      updateState();
    }

    static void AddMovesPanel(int x, int y)
    {
      Native.add_button("-> Up Cam", x, y, 110, 25, 1, moveToCallback);
      Native.add_button("<- Down Cam", x, y + 30, 110, 25, 2, moveToCallback);
      Native.add_button("-> Down Cam", x, y + 60, 110, 25, 3, moveToCallback);
      Native.add_button("-> Board", x, y + 90, 110, 25, 4, moveToCallback);
      Native.add_button("-> Zero", x, y + 120, 110, 25, 5, moveToCallback);
    }

    static void AddNozzlePanel(int x, int y)
    {
      Native.add_button("Unload", x, y, 110, 25, 200, moveToCallback);
      Native.add_button("Empty", x, y + 30, 110, 25, 201, moveToCallback);
      for (int i = 0; i < settings.nozzles.Count; i++)
        Native.add_button("-> " + settings.nozzles[i].name, x, y + 60 + 30 * i, 110, 25, 100 + i, moveToCallback);
    }

    static void Main(string[] args)
    {
      // Load state and settings.
      string homePath = (Environment.OSVersion.Platform == PlatformID.Unix || 
                        Environment.OSVersion.Platform == PlatformID.MacOSX)
          ? Environment.GetEnvironmentVariable("HOME")
          : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
      homePath = Path.Combine(homePath, "pcb");
      stateFile = Path.Combine(homePath, stateFile);
      tapesFile = Path.Combine(homePath, tapesFile);
      settingsFile = Path.Combine(homePath, settingsFile);

      state = ReadFromXmlFile<State>(stateFile);
      settings = ReadFromXmlFile<Settings>(settingsFile);
      tapes = ReadFromXmlFile<List<Tape>>(tapesFile);
      foreach (Tape t in tapes) {
        if (t.lightIntensity == 0) {
          t.lightIntensity = settings.upCamera.lightIntensity;
          t.threshold1 = settings.upCamera.threshold1;
          t.threshold2 = settings.upCamera.threshold2;
          t.gauss1 = settings.upCamera.gauss1;
          t.gauss2 = settings.upCamera.gauss2;
        }
      }

      // Find cameras.
      string upcam = null, downcam = null;

      for (int i = 0; i < 10; i++) {
        var cardb = new StringBuilder(100);
        var businfob = new StringBuilder(100);
        if (Native.get_device_info(i, cardb, businfob) < 0)
          continue;
        var card = cardb.ToString();
        var businfo = businfob.ToString();
        Console.WriteLine(card + ", " + businfo);
        if (card.StartsWith(settings.upCamera.card)) {
          upcam = "/dev/video" + i;
          continue;
        }
        if (card.StartsWith(settings.downCamera.card)) {
          downcam = "/dev/video" + i;
          continue;
        }
      }
      if (upcam == null || downcam == null) {
        Console.WriteLine("Camera not found!");
        return;
      }

      if (state.lastBoard != null)
        board = ReadFromXmlFile<PnpBoard>(state.lastBoard);
      channel.SteppersOn();
      channel.SetCoords(state.lastX, state.lastY, state.lastZ);
      Native.ui_init("/home/greg/pnp/coolpnp/native/", "CoolPNP");
      AddControlPanel(820, 30);
      AddMovesPanel(1080, 30);
      AddNozzlePanel(1200, 30);
      AddPnpPanel(1320, 30);
      AddBoardPanel(820, 330);
      AddTapePanel(1200, 330);
      AddRunPanel(1200, 600);
      AddVisionPanel(20, 640);
      updateState();
      updateBoard();
      updateTape();
      updateCameras();

      Native.init_cameras();
      Native.open_device(0, downcam);
      Native.init_device(0, settings.downCamera.resolutionX, settings.downCamera.resolutionY);
      Native.set_preview(0, 0, 0);
      Native.set_visionview(0, 0, 300);
      Native.set_auxref(0, settings.markXOffset, settings.markYOffset);
      Native.set_camera(0, settings.downCamera.exposure, settings.downCamera.brightness, settings.downCamera.contrast, settings.downCamera.sharpness);
      Native.start_capturing(0);
      Native.open_device(1, upcam);
      Native.init_device(1, settings.upCamera.resolutionX, settings.upCamera.resolutionY);
      Native.set_preview(1, 400, 0);
      Native.set_visionview(1, 400, 300);
      Native.set_camera(1, settings.upCamera.exposure, settings.upCamera.brightness, settings.upCamera.contrast, settings.upCamera.sharpness);
      Native.start_capturing(1);

      Native.ui_loop();

      channel.SteppersOff();
      channel.PumpOff();
      channel.VacuumValveOff();
      channel.ExhaustValveOff();
      channel.UpLightOff();
      channel.DownLightOff();

      WriteToXmlFile<State>(stateFile, state);
      WriteToXmlFile<List<Tape>>(tapesFile, tapes);
      WriteToXmlFile<Settings>(settingsFile, settings);

      Native.ui_close();
      Native.stop_capturing(0);
      Native.uninit_device(0);
      Native.close_device(0);
      Native.stop_capturing(1);
      Native.uninit_device(1);
      Native.close_device(1);
      channel.Close();
    }
  }
}
