
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>

#include <fcntl.h>              /* low-level i/o */
#include <unistd.h>
#include <errno.h>
#include <sys/stat.h>
#include <sys/types.h>
#include <sys/mman.h>
#include <sys/ioctl.h>
#include <pthread.h>

#include <linux/videodev2.h>

#include "common.h"
#include "video/video.h"

#define CLEAR(x) memset(&(x), 0, sizeof(x))

struct camera ca[4];

void camera::init()
{
  state = 0;
  capturing = 0;
  preview = NULL;
  error[0] = 0;
  capture[0] = NULL;
  capture[1] = NULL;
  capture[2] = NULL;
  capture[3] = NULL;
  ybuf = NULL;
  auxRefX = 0;
  auxRefY = 0;
  vision_view = NULL;
}

void camera::print_error()
{
  if (error[0] != 0)
  {
    printf("%s\n", error);
  }
}

static void errno_exit(int idx, const char *s)
{
  sprintf(ca[idx].error, "%s error %d, %s\n", s, errno, strerror(errno));
  ca[idx].state = 0;
  ca[idx].capturing = 0;
  fprintf(stderr, "%s", ca[idx].error);
}

static int xioctl(int fh, int request, void *arg)
{
  int r;

  do {
    r = ioctl(fh, request, arg);
  } while (-1 == r && EINTR == errno);

  return r;
}

void hline(uint8 *buf, int stride, int x1, int x2, int y)
{
  buf += y * stride + x1 * 4;
  for (int i = x1; i <= x2; i++)
  {
    buf[0] = buf[0] ^ 255;
    buf[1] = 255;
    buf[2] = buf[2] ^ 255;
    buf += 4;
  }
}

void vline(uint8 *buf, int stride, int x, int y1, int y2)
{
  buf += y1 * stride + x * 4;
  for (int i = y1; i <= y2; i++)
  {
    buf[0] = buf[0] ^ 255;
    buf[1] = 255;
    buf[2] = buf[2] ^ 255;
    buf += stride;
  }
}

int previewX = 400;
int previewY = 300;

static void read_frame(int idx)
{
  struct v4l2_buffer buf;
  unsigned int i;

  CLEAR(buf);

  buf.type = V4L2_BUF_TYPE_VIDEO_CAPTURE;
  buf.memory = V4L2_MEMORY_MMAP;

  if (-1 == xioctl(ca[idx].fd, VIDIOC_DQBUF, &buf)) {
    switch (errno) {
    case EAGAIN:
      return;

    case EIO:
      /* Could ignore EIO, see spec. */
      /* fall through */

    default:
      errno_exit(idx, "VIDIOC_DQBUF");
    }
  }

  uint8 *src = (unsigned char *)ca[idx].buffers[buf.index];
  if (ca[idx].preview != NULL)
  {
    int scale = 4;
    ca[idx].capture[3] = ca[idx].capture[2];
    ca[idx].capture[2] = ca[idx].capture[1];
    ca[idx].capture[1] = ca[idx].capture[0];
    ca[idx].capture[0] = src;
    if (previewX * 4 <= ca[idx].width && previewY * 4 <= ca[idx].height) {
      YUY2ToARGBScaleDown4(src, ca[idx].preview, previewX * 4, previewY * 4, ca[idx].width * 2, ca[idx].preview_stride);
    } else {
      scale = 2;
      uint8 *ptr = src + (ca[idx].height / 2 - previewY) * ca[idx].width * 2 + (ca[idx].width / 2 - previewX) * 2;
      YUY2ToARGBScaleDown2(ptr, ca[idx].preview, previewX * 2, previewY * 2, ca[idx].width * 2, ca[idx].preview_stride);
    }
    int cx = previewX / 2, cy = previewY / 2;
    hline(ca[idx].preview, ca[idx].preview_stride, cx - 20, cx + 20, cy - 1);
    hline(ca[idx].preview, ca[idx].preview_stride, cx - 20, cx + 20, cy);
    vline(ca[idx].preview, ca[idx].preview_stride, cx - 1, cy - 20, cy + 20);
    vline(ca[idx].preview, ca[idx].preview_stride, cx, cy - 20, cy + 20);
    if (ca[idx].auxRefX != 0) {
      int x = cx + ca[idx].auxRefX / scale, y = cy + ca[idx].auxRefY / scale;
      hline(ca[idx].preview, ca[idx].preview_stride, x - 10, x + 10, y);
      vline(ca[idx].preview, ca[idx].preview_stride, x, y - 10, y + 10);
    }
    dirty = 1;
  }

  if (-1 == xioctl(ca[idx].fd, VIDIOC_QBUF, &buf))
    errno_exit(idx, "VIDIOC_QBUF");
}

void showcapture(int idx, int xo, int yo)
{
  if (ca[idx].ybuf == NULL || ca[idx].vision_view == NULL)
    return;
  xo -= previewX / 2;
  yo -= previewY / 2;
  xo = xo < 0 ? 0 : xo;
  yo = yo < 0 ? 0 : yo;
  int wo = xo + previewX, ho = yo + previewY;
  xo = wo > ca[idx].width ? (ca[idx].width - previewX)  : xo;
  yo = ho > ca[idx].height ? (ca[idx].height - previewY) : yo;
  int o = yo * ca[idx].width + xo;
  int s = ca[idx].width;
  I444ToARGB(ca[idx].ybuf + o, s, ca[idx].ubuf + o, s, ca[idx].vbuf + o, s, ca[idx].vision_view, ca[idx].vision_stride, previewX, previewY);
  int cx = previewX / 2, cy = previewY / 2;
  hline(ca[idx].vision_view, ca[idx].vision_stride, cx - 10, cx + 10, cy);
  vline(ca[idx].vision_view, ca[idx].vision_stride, cx, cy - 10, cy + 10);
}

void showcapture2(int idx, int xo, int yo)
{
  if (ca[idx].ybuf == NULL || ca[idx].vision_view == NULL)
    return;
  xo -= previewX;
  yo -= previewY;
  xo = xo < 0 ? 0 : xo;
  yo = yo < 0 ? 0 : yo;
  int wo = xo + previewX * 2, ho = yo + previewY * 2;
  xo = wo > ca[idx].width ? (ca[idx].width - previewX * 2)  : xo;
  yo = ho > ca[idx].height ? (ca[idx].height - previewY * 2) : yo;
  int o = yo * ca[idx].width + xo;
  int s = ca[idx].width;
  I444ToARGBScaleDown2(ca[idx].ybuf + o, s, ca[idx].ubuf + o, s, ca[idx].vbuf + o, s, ca[idx].vision_view, ca[idx].vision_stride, previewX, previewY);
  int cx = previewX / 2, cy = previewY / 2;
  hline(ca[idx].vision_view, ca[idx].vision_stride, cx - 10, cx + 10, cy);
  vline(ca[idx].vision_view, ca[idx].vision_stride, cx, cy - 10, cy + 10);
}

void showrectangle(int idx, int x1, int y1, int x2, int y2)
{
  if (ca[idx].ybuf == NULL || ca[idx].vision_view == NULL)
    return;
  x1 += previewX / 2;
  x2 += previewX / 2;
  y1 += previewY / 2;
  y2 += previewY / 2;
  if (x1 < 0)
    x1 = 0;
  if (x1 > ca[idx].width - 1)
    x1 = ca[idx].width - 1;
  if (x2 < 0)
    x2 = 0;
  if (x2 > ca[idx].width - 1)
    x2 = ca[idx].width - 1;
  if (y1 < 0)
    y1 = 0;
  if (y1 > ca[idx].height - 1)
    y1 = ca[idx].height - 1;
  if (y2 < 0)
    y2 = 0;
  if (y2 > ca[idx].height - 1)
    y2 = ca[idx].height - 1;
  hline(ca[idx].vision_view, ca[idx].vision_stride, x1, x2, y1);
  hline(ca[idx].vision_view, ca[idx].vision_stride, x1, x2, y2);
  vline(ca[idx].vision_view, ca[idx].vision_stride, x1, y1, y2);
  vline(ca[idx].vision_view, ca[idx].vision_stride, x2, y1, y2);
}

void getcapture(int idx)
{
  if (ca[idx].ybuf == NULL) {
    ca[idx].ybuf = (uint8 *)malloc(ca[idx].width * ca[idx].height);
    ca[idx].ubuf = (uint8 *)malloc(ca[idx].width * ca[idx].height);
    ca[idx].vbuf = (uint8 *)malloc(ca[idx].width * ca[idx].height);
  }
  uint8 *old = ca[idx].capture[0];
  int cnt = 0;
  while (ca[idx].capture[0] == old && ++cnt != 10)
    sleep_ms(100);
  old = ca[idx].capture[0];
  cnt = 0;
  while (ca[idx].capture[0] == old && ++cnt != 10)
    sleep_ms(100);
  old = ca[idx].capture[0];
  cnt = 0;
  while (ca[idx].capture[0] == old && ++cnt != 10)
    sleep_ms(100);
  uint8 *temp = ca[idx].preview;
  ca[idx].preview = NULL;
  YUY2ToI444(ca[idx].capture[0], ca[idx].ybuf, ca[idx].ubuf, ca[idx].vbuf, ca[idx].width, ca[idx].height, ca[idx].width * 2);
  ca[idx].preview = temp;
}

extern "C" void save_bw(int idx, char *filename)
{
  if (ca[idx].ybuf == NULL)
    getcapture(idx);
  save_bwcopy(ca[idx].ybuf + ca[idx].width / 2 - previewX / 2 + (ca[idx].height / 2 - previewY / 2) * ca[idx].width,
              previewX, previewY, ca[idx].width);
  save_bitmap(filename);
}

static void *mainloop(void *)
{
  unsigned int count = 0;

  while(quit != true) {
    fd_set fds;
    struct timeval tv;
    int r, max_fd = 0;

    FD_ZERO(&fds);
    for (int i = 0; i < 4; i++)
    {
      if (ca[i].capturing != 0)
      {
        FD_SET(ca[i].fd, &fds);
        if (ca[i].fd > max_fd)
          max_fd = ca[i].fd;
      }
    }
    if (max_fd == 0)
    {
      sleep_ms(100);
      continue;
    }

    /* Timeout. */
    tv.tv_sec = 10;
    tv.tv_usec = 0;

    r = select(max_fd + 1, &fds, NULL, NULL, &tv);

    if (-1 == r) {
      if (EINTR == errno)
        continue;
      fprintf(stderr, "select\n");
      exit(EXIT_FAILURE);
    }

    if (0 == r) {
      fprintf(stderr, "select timeout\n");
      exit(EXIT_FAILURE);
    }

    for (int i = 0; i < 4; i++)
    {
      if (ca[i].capturing != 0 && FD_ISSET(ca[i].fd, &fds))
        read_frame(i);
    }
  }
}

extern "C" int stop_capturing(int idx)
{
  ca[idx].capturing = 0;

  enum v4l2_buf_type type;

  type = V4L2_BUF_TYPE_VIDEO_CAPTURE;
  if (-1 == xioctl(ca[idx].fd, VIDIOC_STREAMOFF, &type))
    errno_exit(idx, "VIDIOC_STREAMOFF");

  return ca[idx].state;
}

extern "C" int start_capturing(int idx)
{
  unsigned int i;
  enum v4l2_buf_type type;

  for (i = 0; i < ca[idx].n_buffers; ++i) {
    struct v4l2_buffer buf;

    CLEAR(buf);
    buf.type = V4L2_BUF_TYPE_VIDEO_CAPTURE;
    buf.memory = V4L2_MEMORY_MMAP;
    buf.index = i;

    if (-1 == xioctl(ca[idx].fd, VIDIOC_QBUF, &buf))
    {
      errno_exit(idx, "VIDIOC_QBUF");
      return ca[idx].state;
    }
  }
  type = V4L2_BUF_TYPE_VIDEO_CAPTURE;
  if (-1 == xioctl(ca[idx].fd, VIDIOC_STREAMON, &type))
  {
    errno_exit(idx, "VIDIOC_STREAMON");
    return ca[idx].state;
  }

  ca[idx].capturing = 1;
  return ca[idx].state;
}

extern "C" int uninit_device(int idx)
{
  unsigned int i;

  for (i = 0; i < ca[idx].n_buffers; ++i)
    if (-1 == munmap(ca[idx].buffers[i], ca[idx].buf_size))
      errno_exit(idx, "munmap");

  return ca[idx].state;
}

static int init_mmap(int idx)
{
  struct v4l2_requestbuffers req;

  CLEAR(req);

  req.count = 4;
  req.type = V4L2_BUF_TYPE_VIDEO_CAPTURE;
  req.memory = V4L2_MEMORY_MMAP;

  if (-1 == xioctl(ca[idx].fd, VIDIOC_REQBUFS, &req)) {
    if (EINVAL == errno) {
      errno_exit(idx, "Does not support memory mapping");
    } else {
      errno_exit(idx, "VIDIOC_REQBUFS");
    }
    return ca[idx].state;
  }

  if (req.count < 2) {
    errno_exit(idx, "Insufficient buffer memory");
    return ca[idx].state;
  }

  for (ca[idx].n_buffers = 0; ca[idx].n_buffers < req.count; ++ca[idx].n_buffers) {
    struct v4l2_buffer buf;

    CLEAR(buf);

    buf.type        = V4L2_BUF_TYPE_VIDEO_CAPTURE;
    buf.memory      = V4L2_MEMORY_MMAP;
    buf.index       = ca[idx].n_buffers;

    if (-1 == xioctl(ca[idx].fd, VIDIOC_QUERYBUF, &buf))
    {
      errno_exit(idx, "VIDIOC_QUERYBUF");
      return ca[idx].state;
    }

    ca[idx].buf_size = buf.length;
    ca[idx].buffers[ca[idx].n_buffers] =
      mmap(NULL /* start anywhere */,
           buf.length,
           PROT_READ | PROT_WRITE /* required */,
           MAP_SHARED /* recommended */,
           ca[idx].fd, buf.m.offset);

    if (MAP_FAILED == ca[idx].buffers[ca[idx].n_buffers]) {
      errno_exit(idx, "mmap");
      return ca[idx].state;
    }
  }
  return ca[idx].state;
}

void set_ctrl(int idx, int id, int value)
{
  v4l2_control c;
  c.id = id;
  c.value = value;
  xioctl(ca[idx].fd, VIDIOC_S_CTRL, &c);
}

extern "C" int init_device(int idx, int width, int height)
{
  struct v4l2_capability cap;
  struct v4l2_cropcap cropcap;
  struct v4l2_crop crop;
  struct v4l2_format fmt;
  unsigned int min;
  int fd = ca[idx].fd;

  ca[idx].width = width;
  ca[idx].height = height;

  if (-1 == xioctl(fd, VIDIOC_QUERYCAP, &cap)) {
    if (EINVAL == errno) {
      errno_exit(idx, "Is no V4L2 device");
    } else {
      errno_exit(idx, "VIDIOC_QUERYCAP");
    }
    return ca[idx].state;
  }

  if (!(cap.capabilities & V4L2_CAP_VIDEO_CAPTURE)) {
    errno_exit(idx, "Is no video capture device");
    return ca[idx].state;
  }

  if (!(cap.capabilities & V4L2_CAP_STREAMING)) {
    errno_exit(idx, "%s does not support streaming i/o");
    return ca[idx].state;
  }

  /* Select video input, video standard and tune here. */
  CLEAR(cropcap);
  cropcap.type = V4L2_BUF_TYPE_VIDEO_CAPTURE;

  if (0 == xioctl(fd, VIDIOC_CROPCAP, &cropcap)) {
    crop.type = V4L2_BUF_TYPE_VIDEO_CAPTURE;
    crop.c = cropcap.defrect; /* reset to default */

    if (-1 == xioctl(fd, VIDIOC_S_CROP, &crop)) {
      switch (errno) {
      case EINVAL:
        /* Cropping not supported. */
        break;
      default:
        /* Errors ignored. */
        break;
      }
    }
  } else {
    /* Errors ignored. */
  }

  CLEAR(fmt);

  fmt.type = V4L2_BUF_TYPE_VIDEO_CAPTURE;
  fmt.fmt.pix.width       = width;
  fmt.fmt.pix.height      = height;
  fmt.fmt.pix.pixelformat = V4L2_PIX_FMT_YUYV;

  if (-1 == xioctl(fd, VIDIOC_S_FMT, &fmt)) {
    errno_exit(idx, "VIDIOC_S_FMT");
    return ca[idx].state;
  }

  /* Note VIDIOC_S_FMT may change width and height. */
  /* Buggy driver paranoia. */
  min = fmt.fmt.pix.width * 2;
  if (fmt.fmt.pix.bytesperline < min)
    fmt.fmt.pix.bytesperline = min;
  min = fmt.fmt.pix.bytesperline * fmt.fmt.pix.height;
  if (fmt.fmt.pix.sizeimage < min)
    fmt.fmt.pix.sizeimage = min;

  struct v4l2_streamparm parm;
  memset(&parm, 0, sizeof(struct v4l2_streamparm));

  parm.type = V4L2_BUF_TYPE_VIDEO_CAPTURE;

  parm.parm.capture.timeperframe.numerator = 1;
  parm.parm.capture.timeperframe.denominator = 5;

  xioctl(fd, VIDIOC_S_PARM, &parm);

  set_ctrl(idx, V4L2_CID_EXPOSURE_AUTO, V4L2_EXPOSURE_MANUAL);
  set_ctrl(idx, V4L2_CID_EXPOSURE_AUTO_PRIORITY, 0);
  set_ctrl(idx, V4L2_CID_AUTO_WHITE_BALANCE, 0);
  set_ctrl(idx, V4L2_CID_POWER_LINE_FREQUENCY, V4L2_CID_POWER_LINE_FREQUENCY_60HZ);
  set_ctrl(idx, V4L2_CID_BACKLIGHT_COMPENSATION, 0);

  return init_mmap(idx);
}

extern "C" void set_camera(int idx, int exp, int bright, int contr, int sharp)
{
  set_ctrl(idx, V4L2_CID_EXPOSURE_ABSOLUTE, exp);
  set_ctrl(idx, V4L2_CID_EXPOSURE, exp);
  set_ctrl(idx, V4L2_CID_BRIGHTNESS, bright);
  set_ctrl(idx, V4L2_CID_CONTRAST, contr);
  set_ctrl(idx, V4L2_CID_SHARPNESS, sharp);
}

extern "C" int close_device(int idx)
{
  if (-1 == close(ca[idx].fd))
    errno_exit(idx, "close");
  return ca[idx].state;
}

extern "C" int open_device(int idx, const char *dev_name)
{
  struct stat st;

  strcpy(ca[idx].dev_name, dev_name);
  ca[idx].state = 1;

  if (-1 == stat(dev_name, &st)) {
    errno_exit(idx, "Cannot identify device name: %d, %s");
    return ca[idx].state;
  }

  if (!S_ISCHR(st.st_mode)) {
    errno_exit(idx, "Is no device");
    return ca[idx].state;
  }

  ca[idx].fd = open(dev_name, O_RDWR /* required */ | O_NONBLOCK, 0);

  if (-1 == ca[idx].fd) {
    errno_exit(idx, "Cannot open device: %d, %s\n");
    return ca[idx].state;
  }
  return ca[idx].state;
}

extern "C" int get_device_info(int idx, char *card, char *businfo)
{
  char tn[256];
  struct stat st;
  int fd;
  struct v4l2_capability cap;
  sprintf(tn, "/dev/video%d", idx);
  card[0] = 0;
  businfo[0] = 0;
  if (-1 == stat(tn, &st)) {
    return -1;
  }
  if (!S_ISCHR(st.st_mode)) {
    return -2;
  }
  fd = open(tn, O_RDWR | O_NONBLOCK, 0);
  if (-1 == fd) {
    return -3;
  }
  if (-1 == xioctl(fd, VIDIOC_QUERYCAP, &cap)) {
    close(fd);
    return -4;
  }
  close(fd);
  strncpy(card, (const char *)cap.card, 32);
  strncpy(businfo, (const char *)cap.bus_info, 32);
  return 0;
}

extern "C" int init_cameras()
{
  for (int i = 0; i < 4; i++)
    ca[i].init();

  pthread_t capture_thread;

  if (pthread_create(&capture_thread, NULL, mainloop, NULL)) {
    return 0;
  }
  return 1;
}

extern "C" void set_preview(int idx, int x, int y)
{
  if (x < 0)
  {
    ca[idx].preview = NULL;
    return;
  }

  ca[idx].preview = so(x, y);
  ca[idx].preview_stride = WINDOW_WIDTH * 4;
}

extern "C" void set_auxref(int idx, int x, int y)
{
  ca[idx].auxRefX = x;
  ca[idx].auxRefY = y;
}

extern "C" void set_visionview(int idx, int x, int y)
{
  ca[idx].vision_view = so(x, y);
  ca[idx].vision_stride = WINDOW_WIDTH * 4;
}
