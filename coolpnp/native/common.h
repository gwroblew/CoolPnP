#ifndef __COMMON_H
#define __COMMON_H

#include <atomic>

#define PI 3.1415926537

typedef uint64_t uint64;
typedef int64_t int64;
typedef unsigned int uint32;
typedef int int32;
typedef unsigned short uint16;
typedef short int16;
typedef unsigned char uint8;
typedef signed char int8;

#define SPINLOCK(v)   std::atomic_flag v = ATOMIC_FLAG_INIT;
#define LOCK(v)       while (v.test_and_set(std::memory_order_acquire));
#define UNLOCK(v)     v.clear(std::memory_order_release);

extern int quit;
extern int dirty;

const int WINDOW_WIDTH = 1600;
const int WINDOW_HEIGHT = 900;

struct camera {
  int              state;
  int              capturing;
  char             error[256];
  int              fd;
  char             dev_name[256];
  void            *buffers[4];
  int              n_buffers;
  unsigned int     buf_size;
  unsigned int     width;
  unsigned int     height;
  uint8           *preview;
  int              preview_stride;
  uint8           *capture[4];
  uint8           *ybuf;
  uint8           *ubuf;
  uint8           *vbuf;
  int              auxRefX;
  int              auxRefY;
  uint8           *vision_view;
  int              vision_stride;

  void init();
  void print_error();
};

unsigned char *so(int x, int y);
void save_bwcopy(uint8 *ybuf, int x, int y, int stride);
void showcapture(int idx, int xo, int yo);
void showcapture2(int idx, int xo, int yo);
void showrectangle(int idx, int x1, int y1, int x2, int y2);
void getcapture(int idx);

extern "C" void mvtest();

extern "C" int ui_init(const char *binpath, const char *title);
extern "C" void ui_close();
extern "C" int ui_loop();
extern "C" uint64 gettick();
extern "C" void sleep_ms(uint32 ms);
extern "C" int add_button(const char *text, int x, int y, int w, int h, int64 data, void (*callback)(int64));
extern "C" void draw_text(int x, int y, int f, int b, const char *str);
extern "C" void draw_line(int x1, int y1, int x2, int y2, int color);

extern "C" int init_cameras();
extern "C" int open_device(int idx, const char *dev_name);
extern "C" int close_device(int idx);
extern "C" int init_device(int idx, int width, int height);
extern "C" int uninit_device(int idx);
extern "C" void set_preview(int idx, int x, int y);
extern "C" int start_capturing(int idx);
extern "C" int stop_capturing(int idx);
extern "C" void save_bitmap(char *filename);

#endif
