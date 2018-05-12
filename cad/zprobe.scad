$fn=100;
module screw(){
cylinder(r=1.9,h=30,center=true);
translate([0,0,7])
cylinder(r2=4,r1=1.9,h=2.1);
}

module switch(){
difference(){
translate([0,0,1.5])
cube([21,10,15],center=true);
translate([3.5+1.27,0,1])
rotate([90,0,0])
cylinder(r=1.3,h=12,center=true);
translate([-3.5-1.27,0,1])
rotate([90,0,0])
cylinder(r=1.3,h=12,center=true);
cylinder(r=3,h=25,center=true);
}}

module base(){
difference(){
cube([23,10.2,3],center=true);
translate([-7,0,-7])
screw();
translate([7,0,-7])
screw();
}
translate([0,6.6,5-1.5])
cube([23,3,10],center=true);
cylinder(r=2.6,h=12);
}

switch();
//base();
