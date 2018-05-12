$fn=100;
module endstop(){
difference(){
cube([20,6,20],center=true);
translate([3,0,0])
rotate([90,0,0])
cylinder(r=2.7,h=24,center=true);
translate([3,-3,0])
rotate([90,0,0])
cylinder(r=5.5,h=2,center=true);

x = 10 - 6 - 0.65;

translate([-x, 0, 3.5+1.27])
rotate([90,0,0])
cylinder(r=1.3,h=22,center=true);
translate([-x, 0, -3.5-1.27])
rotate([90,0,0])
cylinder(r=1.3,h=22,center=true);

translate([-x, 3, 3.5+1.27])
rotate([90,0,0])
cylinder(r=2.1,h=3.8,center=true);
translate([-x, 3, -3.5-1.27])
rotate([90,0,0])
cylinder(r=2.1,h=3.8,center=true);
}
}

endstop();
