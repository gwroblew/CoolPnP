include <threads.scad>;

$fn=100;

difference(){
cylinder(r=7,h=22,center=true);
translate([0,0,-2.1])
cylinder(r=2.5,h=15);
translate([0,0,-12])
cylinder(r=2.4,h=10);
translate([0,-1,5])
rotate([90,0,0])
metric_thread(diameter=4,pitch=1,length=6,internal=true);
translate([0,1,5])
rotate([-90,0,0])
metric_thread(diameter=4,pitch=1,length=6,internal=true);

translate([0,0,-8])
rotate([0,0,30])
cylinder(r=4.5,h=3.2,$fn=6);
translate([0,10,-8+1.6])
cube([7.8,20,3.2],center=true);
}
