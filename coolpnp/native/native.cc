#include <stdio.h>
#include <stdlib.h>
#include <stdarg.h>
#include <string.h>
#include <pthread.h>
#include <sys/time.h>

#include "SDL2/SDL.h"
#include "SDL2/SDL_ttf.h"
#include "SDL2/SDL_hints.h"
#include <string>
#include <mutex>

#include "common.h"

struct button {
  char text[256];
  int x;
  int y;
  int w;
  int h;
  int active;
  int64 data;
  void (*callback)(int64 data);
  SDL_Color fc;
  SDL_Color bc;
};

std::mutex uimtx;
int quit = 0;
int dirty = 0;
SDL_Window *window = NULL;
SDL_Renderer *renderer = NULL;
SDL_Texture *texture = NULL;
SDL_Surface *surface = NULL;
unsigned char *pixels = (unsigned char *)malloc(WINDOW_WIDTH * WINDOW_HEIGHT * 4);
SDL_Surface *savesurf = NULL;
unsigned char *savebuf = (unsigned char *)malloc(WINDOW_WIDTH * WINDOW_HEIGHT * 4);
TTF_Font* fontsans14;
TTF_Font* fontmono14;
SDL_Color whiteColor = { 255, 255, 255 };
SDL_Color blackColor = { 0, 0, 0 };
SDL_Color grayColor = { 128, 128, 128 };
SDL_Color redColor = { 192, 0, 0 };
SDL_Color greenColor = { 24, 240, 32 };
struct button buttons[128];
int butcnt = 0;
SDL_Color palette[] = { blackColor, whiteColor, grayColor, redColor, greenColor };

extern "C" int add_button(const char *text, int x, int y, int w, int h, int64 data, void (*callback)(int64))
{
  uimtx.lock();
  struct button *b = &buttons[butcnt++];

  strcpy(b->text, text);
  b->x = x;
  b->y = y;
  b->w = w;
  b->h = h;
  b->active = 1;
  b->data = data;
  b->callback = callback;
  b->fc = blackColor;
  b->bc = grayColor;
  int rb = butcnt - 1;
  uimtx.unlock();

  return rb;
}

extern "C" uint64 gettick()
{
  struct timeval tv;

  gettimeofday(&tv, NULL);

  return tv.tv_sec * 1000ULL + tv.tv_usec / 1000;
}

extern "C" void sleep_ms(uint32 ms)
{
  SDL_Delay(ms);
}

extern "C" void set_pixel(int x, int y, int color)
{
  uint8 *ptr = pixels + (WINDOW_WIDTH * y + x) * 4;
  SDL_Color c = palette[color];
  ptr[0] = c.b;
  ptr[1] = c.g;
  ptr[2] = c.r;
}

extern "C" void draw_line(int x1, int y1, int x2, int y2, int color)
{
  uimtx.lock();
  if (x1 < 0)
    x1 = 0;
  if (x1 > WINDOW_WIDTH - 1)
    x1 = WINDOW_WIDTH - 1;
  if (x2 < 0)
    x2 = 0;
  if (x2 > WINDOW_WIDTH - 1)
    x2 = WINDOW_WIDTH - 1;
  if (y1 < 0)
    y1 = 0;
  if (y1 > WINDOW_HEIGHT - 1)
    y1 = WINDOW_HEIGHT - 1;
  if (y2 < 0)
    y2 = 0;
  if (y2 > WINDOW_HEIGHT - 1)
    y2 = WINDOW_HEIGHT - 1;
  const bool steep = (abs(y2 - y1) > abs(x2 - x1));
  if(steep)
  {
    std::swap(x1, y1);
    std::swap(x2, y2);
  }

  if(x1 > x2)
  {
    std::swap(x1, x2);
    std::swap(y1, y2);
  }

  const float dx = x2 - x1;
  const float dy = fabs(y2 - y1);

  float error = dx / 2.0f;
  const int ystep = (y1 < y2) ? 1 : -1;
  int y = y1;
  const int maxX = x2;

  for(int x=x1; x<maxX; x++)
  {
    if(steep)
    {
      set_pixel(y, x, color);
    }
    else
    {
      set_pixel(x, y, color);
    }
    error -= dy;
    if(error < 0)
    {
      y += ystep;
      error += dx;
    }
  }
  uimtx.unlock();
}

extern "C" void draw_text(int x, int y, int f, int b, const char *str) {
  uimtx.lock();
  SDL_Surface* textSurface = TTF_RenderText_Shaded(fontsans14, str, palette[f], palette[b]);
  if (textSurface == NULL) {
    uimtx.unlock();
    return;
  }
  // Pass zero for width and height to draw the whole surface 
  SDL_Rect textLocation = { (short)x, (short)y, 0, 0 };
  SDL_BlitSurface(textSurface, NULL, surface, &textLocation);
  SDL_FreeSurface(textSurface);
  dirty = 1;
  uimtx.unlock();
}

void drawtext(int x, int y, SDL_Color fc, SDL_Color bc, const char *str, ...) {
  char temp[1024];
  va_list args;

  va_start(args, str);
  vsprintf(temp, str, args);

  SDL_Surface* textSurface = TTF_RenderText_Shaded(fontsans14, temp, fc, bc);
  if (textSurface == NULL)
    return;
  // Pass zero for width and height to draw the whole surface 
  SDL_Rect textLocation = { (short)x, (short)y, 0, 0 };
  SDL_BlitSurface(textSurface, NULL, surface, &textLocation);
  SDL_FreeSurface(textSurface);
  va_end(args);
}

unsigned char *so(int x, int y) {
  return pixels + (WINDOW_WIDTH * y + x) * 4;
}

void drawbox(int x, int y, int w, int h, SDL_Color c) {
  SDL_Rect rect = { (short)x, (short)y, (unsigned short)w, (unsigned short)h };
  SDL_FillRect(surface, &rect, SDL_MapRGB(surface->format, c.r, c.g, c.b));
}

void draw_button(struct button *b, int pushed)
{
  uimtx.lock();
  SDL_Color fc = b->fc, bc = b->bc;

  if (pushed)
    fc = b->bc, bc = b->fc;

  SDL_Surface* textSurface = TTF_RenderText_Shaded(fontsans14, b->text, fc, bc);
  if (textSurface == NULL) {
    uimtx.unlock();
    return;
  }
  int px = b->x + (b->w - textSurface->w) / 2;
  int py = b->y + (b->h - textSurface->h) / 2;
  drawbox(b->x, b->y, b->w, b->h, bc);
  SDL_Rect textLocation = { (short)px, (short)py, 0, 0 };
  SDL_BlitSurface(textSurface, NULL, surface, &textLocation);
  SDL_FreeSurface(textSurface);
  dirty = 1;
  uimtx.unlock();
}

int find_button(int x, int y)
{
  for (int i = 0; i < butcnt; i++)
  {
    struct button *b = &buttons[i];
    if (x > b->x && y > b->y && x < (b->x + b->w) && y < (b->y + b->h))
      return i;
  }
  return -1;
}

void draw_ui()
{
  for (int i = 0; i < butcnt; i++)
    draw_button(&buttons[i], 0);
}

char relpath[1024];
char tmppath[1024];

char *getpath(const char *filename) {
  strcpy(tmppath, relpath);
  strcat(tmppath, filename);
}

extern "C" int ui_loop(){
  SDL_Event event;
  uint64 tick = gettick();
  int pushidx = -1;

  draw_ui();
  while (!quit) {
    while(SDL_PollEvent(&event)) {
      if (event.type == SDL_QUIT) {
        quit = true;
        break;
      }
      if (event.type == SDL_KEYDOWN && !event.key.repeat) {
        if (event.key.keysym.sym == SDLK_ESCAPE)
          quit = true;
        continue;
      }
      if (event.type == SDL_KEYUP && !event.key.repeat) {
        continue;
      }
      if (event.type == SDL_MOUSEBUTTONDOWN) {
        switch (event.button.button) {
          case SDL_BUTTON_LEFT:
            pushidx = find_button(event.button.x, event.button.y);
            if (pushidx >= 0)
            {
              draw_button(&buttons[pushidx], 1);
              if (buttons[pushidx].callback != NULL)
                buttons[pushidx].callback(buttons[pushidx].data);
            }
            break;
          case SDL_BUTTON_RIGHT:
            break;
        }
        continue;
      }
      if (event.type == SDL_MOUSEBUTTONUP) {
        switch (event.button.button) {
          case SDL_BUTTON_LEFT:
            if (pushidx >= 0)
            {
              draw_button(&buttons[pushidx], 0);
              pushidx = -1;
            }
            break;
          case SDL_BUTTON_RIGHT:
            break;
        }
        continue;
      }
    }
    if (quit)
      break;

    if (!dirty && (gettick() - tick) < 20) {
      SDL_Delay(5);
      continue;
    }
    dirty = 0;
    tick = gettick();

    SDL_UpdateTexture(texture, NULL, pixels, WINDOW_WIDTH * 4);
    SDL_RenderClear(renderer);
    SDL_RenderCopy(renderer, texture, NULL, NULL);
    SDL_RenderPresent(renderer);
  }
  return 0;
}

void save_bwcopy(uint8 *ybuf, int x, int y, int stride)
{
  SDL_FillRect(savesurf, NULL, 0x000000);
  uint8 *dst = savebuf;
  for (int i = 0; i < y; i++) {
    uint8 *ptr = dst;
    for (int j = 0; j < x; j++) {
      uint8 p = *ybuf++;
      *ptr++ = p;
      *ptr++ = p;
      *ptr++ = p;
      *ptr++ = 0;
    }
    ybuf += stride - x;
    dst += WINDOW_WIDTH * 4;
  }
}

extern "C" void save_bitmap(char *filename)
{
  uimtx.lock();
  SDL_SaveBMP(savesurf, filename);
  uimtx.unlock();
}

extern "C" void save_screen(char *filename)
{
  uimtx.lock();
  memcpy(savebuf, pixels, WINDOW_WIDTH * WINDOW_HEIGHT * 4);
  SDL_SaveBMP(savesurf, filename);
  uimtx.unlock();
}

extern "C" int ui_init(const char *binpath, const char *title)
{
  strcpy(relpath, binpath);

  if( SDL_Init( SDL_INIT_EVERYTHING ) == -1 ) {
    return 1;
  }
  TTF_Init();

  window = SDL_CreateWindow(title, SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, WINDOW_WIDTH, WINDOW_HEIGHT, SDL_WINDOW_SHOWN);
  renderer = SDL_CreateRenderer(window, -1, 0);
  texture = SDL_CreateTexture(renderer, SDL_PIXELFORMAT_ARGB8888, SDL_TEXTUREACCESS_STREAMING, WINDOW_WIDTH, WINDOW_HEIGHT);
  surface = SDL_CreateRGBSurfaceFrom(pixels, WINDOW_WIDTH, WINDOW_HEIGHT, 32, WINDOW_WIDTH * 4, 0x00FF0000, 0x0000FF00, 0x000000FF, 0xFF000000);
  savesurf = SDL_CreateRGBSurfaceFrom(savebuf, WINDOW_WIDTH, WINDOW_HEIGHT, 32, WINDOW_WIDTH * 4, 0x00FF0000, 0x0000FF00, 0x000000FF, 0xFF000000);

  fontsans14 = TTF_OpenFont(getpath("OpenSans-Regular.ttf"), 16);
  fontmono14 = TTF_OpenFont(getpath("SourceCodePro-Regular.otf"), 14);
}

extern "C" void ui_close()
{
  SDL_Delay(1000);
  TTF_CloseFont(fontsans14); 
  TTF_CloseFont(fontmono14); 
  TTF_Quit();   
  SDL_Quit();
}
