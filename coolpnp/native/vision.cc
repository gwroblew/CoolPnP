#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <mutex>

#include "common.h"

#include "opencv2/imgproc/imgproc.hpp"
#include <opencv2/highgui/highgui.hpp>

using namespace cv;
using namespace std;

mutex vmtx;

typedef struct {
  int x;
  int y;
} vec2;

typedef struct {
  int x1;
  int y1;
  int x2;
  int y2;
  int x3;
  int y3;
  int x4;
  int y4;
  int xc;
  int yc;
  int angle;
  int size;
} minrect;

extern struct camera ca[4];

int calchsymerr(int idx, int x, int y, int dx, int dy) {
  uint8 *sy = ca[idx].ybuf + y * ca[idx].width + x;
  uint8 *ry = ca[idx].ybuf + (y + dy - 1) * ca[idx].width + x;
  int diff = 0;
  for (int i = 0; i < dy / 2; i++) {
    uint8 *y1 = sy, *y2 = ry;
    for (int j = 0; j < dx; j++)
      diff += abs(*y2++ - *y1++);
    sy += ca[idx].width;
    ry -= ca[idx].width;
  }
  return diff;
}

int calcvsymerr(int idx, int x, int y, int dx, int dy) {
  uint8 *sy = ca[idx].ybuf + y * ca[idx].width + x;
  uint8 *ry = ca[idx].ybuf + y * ca[idx].width + x + dx - 1;
  int diff = 0;
  for (int i = 0; i < dy; i++) {
    uint8 *y1 = sy, *y2 = ry;
    for (int j = 0; j < dx / 2; j++)
      diff += abs(*y2-- - *y1++);
    sy += ca[idx].width;
    ry += ca[idx].width;
  }
  return diff;
}

int calcavgpixel(int idx, int x, int y, int dx, int dy)
{
  uint8 *ptr = ca[idx].ybuf + y * ca[idx].width + x;
  int total = 0;
  for (int i = 0; i < dy; i++) {
    uint8 *pt = ptr;
    for (int j = 0; j < dx; j++)
      total += *pt++;
    ptr += ca[idx].width;
  }
  return total / (dx * dy);
}

void invert(int idx, int x, int y, int dx, int dy)
{
  uint8 *ptr = ca[idx].ybuf + y * ca[idx].width + x;
  for (int i = 0; i < dy; i++) {
    uint8 *pt = ptr;
    for (int j = 0; j < dx; j++)
      *pt++ = *pt ^ 255;
    ptr += ca[idx].width;
  }
}

extern "C" void findsymmetry(int idx, int dx, int dy, int rx, int ry, vec2 *retval) {
  vmtx.lock();
  getcapture(idx);
  int min = 1000000000, miny = 0, minx = 0;
  for (int i = 0; i < ry - dy; i++) {
    for (int j = 0; j < rx - dx; j++) {
      int px = ca[idx].width / 2 - rx / 2 + j;
      int py = ca[idx].height / 2 - ry / 2 + i;
      if (calcavgpixel(idx, px, py, dx, dy) < 40)
        continue;
      int diff = calchsymerr(idx, px, py, dx, dy);
      diff += calcvsymerr(idx, px, py, dx, dy);
      if (diff < min) {
        min = diff;
        miny = i + dy / 2;
        minx = j + dx / 2;
      }
    }
  }
  minx -= rx / 2;
  miny -= ry / 2;
  retval->x = minx;
  retval->y = miny;
  showcapture(idx, ca[idx].width / 2, ca[idx].height / 2);
  showrectangle(idx, -rx / 2, -ry / 2, rx / 2, ry / 2);
  showrectangle(idx, -dx / 2 + minx, -dy / 2 + miny, dx / 2 + minx, dy / 2 + miny);
  vmtx.unlock();
}

int sumrect(int idx, int x, int y, int dx, int dy)
{
  uint8 *ptr = ca[idx].ybuf + y * ca[idx].width + x;
  int total = 0;
  for (int i = 0; i < dy; i++) {
    uint8 *pt = ptr;
    for (int j = 0; j < dx; j++)
      total += *pt++;
    ptr += ca[idx].width;
  }
  return total;
}

int calcrect(int idx, int x, int y, int dx, int dy, int bx, int by)
{
  long long bs = sumrect(idx, x, y, dx + bx * 2, by) + sumrect(idx, x, y + by + dy, dx + bx * 2, by)
         + sumrect(idx, x, y + by, bx, dy) + sumrect(idx, x + bx + dx, y + by, bx, dy);
  long long rs = sumrect(idx, x + bx, y + by, dx, dy);
  int bc = 2 * (dx + bx * 2) * by + 2 * bx * dy;
  int rc = dx * dy;
  bs = (bs * 1000) / bc;
  rs = (rs * 1000) / rc;
  int rv = (int)(bs - rs);
  if (rv < 0)
    return -rv;
  return rv;
}

extern "C" void findrectangle(int idx, int dx, int dy, int bx, int by, int rx, int ry, vec2 *retval) {
  vmtx.lock();
  getcapture(idx);
  int min = 0, miny = 0, minx = 0;
  for (int i = 0; i < ry - dy - by * 2; i++) {
    for (int j = 0; j < rx - dx - bx * 2; j++) {
      int px = ca[idx].width / 2 - rx / 2 + j;
      int py = ca[idx].height / 2 - ry / 2 + i;
      int diff = calcrect(idx, px, py, dx, dy, bx, by);
      if (diff > min) {
        min = diff;
        miny = i + dy / 2 + by;
        minx = j + dx / 2 + bx;
      }
    }
  }
  minx -= rx / 2;
  miny -= ry / 2;
  retval->x = minx;
  retval->y = miny;
  showcapture(idx, ca[idx].width / 2, ca[idx].height / 2);
  showrectangle(idx, -rx / 2, -ry / 2, rx / 2, ry / 2);
  showrectangle(idx, -dx / 2 + minx, -dy / 2 + miny, dx / 2 + minx, dy / 2 + miny);
  dx += bx * 2;
  dy += by * 2;
  showrectangle(idx, -dx / 2 + minx, -dy / 2 + miny, dx / 2 + minx, dy / 2 + miny);
  vmtx.unlock();
}

int calccheck(int idx, int x, int y, int dx, int dy)
{
  dx = dx / 2;
  dy = dy / 2;
  int pcnt = dx * dy;
  long long ws = sumrect(idx, x, y + dy, dx, dy) + sumrect(idx, x + dx, y, dx, dy);
  long long bs = sumrect(idx, x, y, dx, dy) + sumrect(idx, x + dx, y + dy, dx, dy);
  ws = ((ws - bs) * 1000) / (4 * pcnt);
  return (int)ws;
}

extern "C" void findcheck(int idx, int x, int y, int dx, int dy, int rx, int ry, vec2 *retval) {
  vmtx.lock();
  getcapture(idx);
  int min = 0, miny = 0, minx = 0;
  for (int i = 0; i < ry - dy; i++) {
    for (int j = 0; j < rx - dx; j++) {
      int px = ca[idx].width / 2 - rx / 2 + j + x;
      int py = ca[idx].height / 2 - ry / 2 + i + y;
      int diff = calccheck(idx, px, py, dx, dy);
      if (diff > min) {
        min = diff;
        miny = i + dy / 2;
        minx = j + dx / 2;
      }
    }
  }
  minx -= rx / 2;
  miny -= ry / 2;
  retval->x = minx;
  retval->y = miny;
  showcapture(idx, ca[idx].width / 2 + x, ca[idx].height / 2 + y);
  showrectangle(idx, -rx / 2, -ry / 2, rx / 2, ry / 2);
  showrectangle(idx, -dx / 2 + minx, -dy / 2 + miny, dx / 2 + minx, dy / 2 + miny);
  vmtx.unlock();
}

int calccircle(int idx, int x, int y, int r, int rb)
{
  int rt = r + rb;
  int isum = 0, osum = 0, icnt = 0, ocnt = 0;
  uint8 *ptr, *src = ca[idx].ybuf + y * ca[idx].width + x;
  for (int i = -rt; i <= rt; i++) {
    int ow = (int)sqrt((rt + 0.5) * rt - i * i);
    ptr = src + (i + rt) * ca[idx].width + rt - ow;
    if (abs(i) > r) {
      for (int j = 0; j < ow * 2; j++)
        osum += *ptr++;
      ocnt += ow * 2;
      continue;
    }
    int iw = (int)sqrt((r + 0.5) * r - i * i);
    int bp = ow - iw;
    for (int j = 0; j < bp; j++)
      osum += *ptr++;
    for (int j = 0; j < iw * 2; j++)
      isum += *ptr++;
    for (int j = 0; j < bp; j++)
      osum += *ptr++;
    ocnt += bp * 2;
    icnt += iw * 2;
  }
  int bs = (osum * 1000) / ocnt;
  int rs = (isum * 1000) / icnt;
  int rv = (int)(bs - rs);
  if (rv < 0)
    return -rv;
  return rv;
}

extern "C" void findcircle(int idx, int r, int rb, int rx, int ry, int thrs1, int thrs2, vec2 *retval) {
  vmtx.lock();
  getcapture(idx);
  uint8 *ptr = ca[idx].ybuf;
  for (int i = 0; i < ca[idx].width * ca[idx].height; i++) {
    uint8 p = *ptr;
    if (p < thrs1)
      p = 0;
    if (p > thrs2)
      p = 255;
    *ptr++ = p;
  }
  int min = 0, miny = 0, minx = 0, rt = r + rb;
  for (int i = 0; i < ry - rt * 2; i++) {
    for (int j = 0; j < rx - rt * 2; j++) {
      int px = ca[idx].width / 2 - rx / 2 + j;
      int py = ca[idx].height / 2 - ry / 2 + i;
      int diff = calccircle(idx, px, py, r, rb);
      if (diff > min) {
        min = diff;
        miny = i + rt;
        minx = j + rt;
      }
    }
  }
  minx -= rx / 2;
  miny -= ry / 2;
  retval->x = minx;
  retval->y = miny;
  showcapture(idx, ca[idx].width / 2, ca[idx].height / 2);
  showrectangle(idx, -rx / 2, -ry / 2, rx / 2, ry / 2);
  showrectangle(idx, -r + minx, -r + miny, r + minx, r + miny);
  showrectangle(idx, -rt + minx, -rt + miny, rt + minx, rt + miny);
  vmtx.unlock();
}

extern "C" void mvtest() {
  Mat i = imread("/home/greg/pnp/coolpnp/control/test.bmp", CV_LOAD_IMAGE_COLOR);
  Mat image, image2;
  RNG rng(12345);
  cvtColor(i, image, CV_BGR2GRAY);
  GaussianBlur(image, image2, Size(7,7), 0, 0);
  GaussianBlur(image, image, Size(21,21), 0, 0);
  image -= image2;
  threshold(image, image2, 10, 255, THRESH_BINARY);
  vector<vector<Point> > contours;
  findContours(image2, contours, CV_RETR_EXTERNAL, CV_CHAIN_APPROX_SIMPLE);
  Mat drawing = Mat::zeros( image2.size(), CV_8UC3 );
  for( int i = 0; i< contours.size(); i++ )
  {
    Scalar color = Scalar( rng.uniform(0, 255), rng.uniform(0,255), rng.uniform(0,255) );
    drawContours( drawing, contours, i, color, 2);
  }
  //Canny(image, image2, 80, 240, 3);
  imwrite("/home/greg/pnp/coolpnp/control/test2.bmp", drawing);
  imwrite("/home/greg/pnp/coolpnp/control/test3.bmp", image2);
}

typedef struct {
  vector<Point> pts;
  Point minpt;
  int mindist;
} Blob;

extern "C" void get_minrect(int idx, int dx, int dy, int thrs1, int thrs2, int gauss1, int gauss2, int minarea, int partsize, int sizedelta, minrect *rect) {
  vmtx.lock();
  getcapture(idx);
  memset(ca[idx].ybuf, 0, (ca[idx].height / 2 - dy / 2) * ca[idx].width);
  for (int i = 0; i < dy; i++) {
    memset(ca[idx].ybuf + (ca[idx].height / 2 - dy / 2 + i) * ca[idx].width, 0, ca[idx].width / 2 - dx / 2);
    memset(ca[idx].ybuf + (ca[idx].height / 2 - dy / 2 + i) * ca[idx].width + ca[idx].width / 2 + dx / 2, 0, ca[idx].width / 2 - dx / 2);
  }
  memset(ca[idx].ybuf + (ca[idx].height / 2 + dy / 2) * ca[idx].width, 0, (ca[idx].height / 2 - dy / 2) * ca[idx].width);
  Mat src_gray(ca[idx].height, ca[idx].width, CV_8UC1, ca[idx].ybuf);
  Mat image, threshold_input, threshold_output;
  vector<vector<Point> > contours;
  vector<Blob> blobs;
  vector<Point> contour;
  RotatedRect minRect;
  vector<Point> points;
  int cx = ca[idx].width / 2, cy = ca[idx].height / 2;
  try {
    GaussianBlur(src_gray, image, Size(gauss1, gauss1), 0, 0);
    threshold(image, threshold_input, thrs1, 255, THRESH_BINARY);
    GaussianBlur(threshold_input, image, Size(gauss2, gauss2), 0, 0);
    threshold(image, threshold_output, thrs2, 255, THRESH_BINARY);
    findContours(threshold_output, contours, CV_RETR_EXTERNAL, CV_CHAIN_APPROX_SIMPLE);
    memcpy(ca[idx].ybuf, threshold_output.data, ca[idx].width * ca[idx].height);

    for(int i = 0; i < contours.size(); i++) {
      if (contourArea(contours[i]) < minarea)
        continue;
      Blob b;
      b.pts = contours[i];
      b.minpt = *min_element(contours[i].begin(), contours[i].end(),
          [cx, cy](const Point &a, const Point &b)
          {
            int ax = a.x - cx, ay = a.y - cy;
            int bx = b.x - cx, by = b.y - cy;
            return (ax * ax + ay * ay) < (bx * bx + by * by);
          });
      int mx = b.minpt.x - cx;
      int my = b.minpt.y - cy;
      b.mindist = mx * mx + my * my;
      blobs.push_back(b);
    }
    sort(blobs.begin(), blobs.end(),
        [](const Blob &a, const Blob &b)
        {
          return a.mindist < b.mindist;
        });

    int pslb = (partsize * (100 - sizedelta)) / 100;
    int pshb = (partsize * (100 + 5)) / 100;
    for(int i = 0; i < blobs.size(); i++) {
      int ptscnt = points.size();
      points.insert(points.end(), blobs[i].pts.begin(), blobs[i].pts.end());
      convexHull(Mat(points), contour);
      minRect = minAreaRect(Mat(contour));
      int size = (int)(minRect.size.width * minRect.size.height);
      if (size >= pslb && size <= pshb)
        break;
      if (size < pslb)
        continue;
      int ptsrange = points.size() - ptscnt;
      for (int step = 19; step >= 0; step--) {
        points.resize(ptscnt + (ptsrange * step) / 20);
        convexHull(Mat(points), contour);
        minRect = minAreaRect(Mat(contour));
        size = (int)(minRect.size.width * minRect.size.height);
        if (size <= pshb)
          break;
      }
      break;
    }

    Point2f vtx[4];
    minRect.points(vtx);
    rect->x1 = (int)vtx[0].x;
    rect->y1 = (int)vtx[0].y;
    rect->x2 = (int)vtx[1].x;
    rect->y2 = (int)vtx[1].y;
    rect->x3 = (int)vtx[2].x;
    rect->y3 = (int)vtx[2].y;
    rect->x4 = (int)vtx[3].x;
    rect->y4 = (int)vtx[3].y;
    rect->xc = (int)minRect.center.x;
    rect->yc = (int)minRect.center.y;
    rect->angle = (int)minRect.angle;
    rect->size = (int)(minRect.size.width * minRect.size.height);
    showcapture2(idx, ca[idx].width / 2, ca[idx].height / 2);
  } catch (...) {
    rect->size = 0;
    fprintf(stderr, "error\n");
  }
  vmtx.unlock();
}
