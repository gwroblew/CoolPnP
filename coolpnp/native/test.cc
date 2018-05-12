#include "common.h"

void buttontest(int64 data)
{
  stop_capturing(1);
}

int main(int argc, char *argv[])
{
  mvtest();
  return 0;
  ui_init("/home/greg/pnp/coolpnp/native/", "CoolTest");
  add_button("test button", 30, 600, 300, 40, 1, buttontest);

  init_cameras();
  open_device(0, "/dev/video1");
  init_device(0, 1600, 1200);
  set_preview(0, 128, 128);
  start_capturing(0);
  open_device(1, "/dev/video2");
  init_device(1, 1600, 1200);
  set_preview(1, 628, 128);
  start_capturing(1);

  ui_loop();
  ui_close();
  stop_capturing(0);
  uninit_device(0);
  close_device(0);
  stop_capturing(1);
  uninit_device(1);
  close_device(1);
  return 0;
}
