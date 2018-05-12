using System;
using System.Runtime.InteropServices;
using System.Text;

namespace control {
  public class Native {

      public static Object nLock = new Object();

      public static void DrawText(int x, int y, int f, int b, string str)
      {
        lock(nLock) {
          draw_text(x, y, f, b, str);
        }
      }

      public static void DrawLine(int x1, int y1, int x2, int y2, int color)
      {
        lock(nLock) {
          draw_line(x1, y1, x2, y2, color);
        }
      }

      [DllImport("/home/greg/pnp/coolpnp/native/bin/native.so")]
      public static extern int ui_init(string binpath, string title);

      [DllImport("/home/greg/pnp/coolpnp/native/bin/native.so")]
      public static extern void ui_close();

      [DllImport("/home/greg/pnp/coolpnp/native/bin/native.so")]
      public static extern int ui_loop();

      [DllImport("/home/greg/pnp/coolpnp/native/bin/native.so")]
      public static extern ulong gettick();

      [DllImport("/home/greg/pnp/coolpnp/native/bin/native.so")]
      public static extern void sleep_ms(uint ms);

      public delegate void ButtonCallback(long data);

      [DllImport("/home/greg/pnp/coolpnp/native/bin/native.so")]
      public static extern int add_button(string text, int x, int y, int w, int h, long data, ButtonCallback callback);

      [DllImport("/home/greg/pnp/coolpnp/native/bin/native.so")]
      public static extern void draw_text(int x, int y, int f, int b, string str);

      [DllImport("/home/greg/pnp/coolpnp/native/bin/native.so")]
      public static extern void draw_line(int x1, int y1, int x2, int y2, int color);

      [DllImport("/home/greg/pnp/coolpnp/native/bin/native.so")]
      public static extern int init_cameras();

      [DllImport("/home/greg/pnp/coolpnp/native/bin/native.so")]
      public static extern int open_device(int idx, string dev_name);

      [DllImport("/home/greg/pnp/coolpnp/native/bin/native.so")]
      public static extern int close_device(int idx);

      [DllImport("/home/greg/pnp/coolpnp/native/bin/native.so")]
      public static extern int init_device(int idx, int width, int height);

      [DllImport("/home/greg/pnp/coolpnp/native/bin/native.so")]
      public static extern int uninit_device(int idx);

      [DllImport("/home/greg/pnp/coolpnp/native/bin/native.so")]
      public static extern int set_preview(int idx, int x, int y);

      [DllImport("/home/greg/pnp/coolpnp/native/bin/native.so")]
      public static extern int start_capturing(int idx);

      [DllImport("/home/greg/pnp/coolpnp/native/bin/native.so")]
      public static extern int stop_capturing(int idx);

      [DllImport("/home/greg/pnp/coolpnp/native/bin/native.so")]
      public static extern void set_camera(int idx, int exp, int bright, int contr, int sharp);

      [StructLayout(LayoutKind.Sequential)]
      public class Vec2 {
        public int x;
        public int y;
      }
      [DllImport("/home/greg/pnp/coolpnp/native/bin/native.so")]
      public static extern void findsymmetry(int idx, int dx, int dy, int rx, int ry, Vec2 retval);

      [DllImport("/home/greg/pnp/coolpnp/native/bin/native.so")]
      public static extern void findrectangle(int idx, int dx, int dy, int rx, int ry, int bx, int by, Vec2 retval);

      [DllImport("/home/greg/pnp/coolpnp/native/bin/native.so")]
      public static extern void findcircle(int idx, int r, int rb, int rx, int ry, int thrs1, int thrs2, Vec2 retval);

      [DllImport("/home/greg/pnp/coolpnp/native/bin/native.so")]
      public static extern void findcheck(int idx, int x, int y, int dx, int dy, int rx, int ry, Vec2 retval);

      [DllImport("/home/greg/pnp/coolpnp/native/bin/native.so", CharSet = CharSet.Ansi)]
      public static extern int get_device_info(int idx, StringBuilder card, StringBuilder businfo);

      [DllImport("/home/greg/pnp/coolpnp/native/bin/native.so")]
      public static extern void set_visionview(int idx, int x, int y);

      [DllImport("/home/greg/pnp/coolpnp/native/bin/native.so")]
      public static extern void set_auxref(int idx, int x, int y);

      [DllImport("/home/greg/pnp/coolpnp/native/bin/native.so")]
      public static extern void save_bw(int idx, string filename);

      [DllImport("/home/greg/pnp/coolpnp/native/bin/native.so")]
      public static extern void save_screen(string filename);

      [StructLayout(LayoutKind.Sequential)]
      public class MinRect {
        public int x1;
        public int y1;
        public int x2;
        public int y2;
        public int x3;
        public int y3;
        public int x4;
        public int y4;
        public int xc;
        public int yc;
        public int angle;
        public int size;
      }
      [DllImport("/home/greg/pnp/coolpnp/native/bin/native.so")]
      public static extern void get_minrect(int idx, int dx, int dy, int thrs1, int thrs2, int gauss1, int gauss2, int minarea, int partsize, int sizedelta, MinRect rect);
  }
}
