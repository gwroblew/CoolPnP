$fn=100;
difference(){
union(){
cylinder(r=32,h=80);
translate([0,0,78])
cylinder(r=40,h=2);
}
translate([0,0,-10])
cylinder(r=27,h=192);
translate([-0.2,0,-20])
cube([0.4,80,110]);
translate([-40,-0.2,-20])
cube([80,0.4,110]);
translate([-37,37,40])
rotate([90,0,45])
translate([0,0,-10])
cylinder(r=6+6,h=82);
}
translate([-40,40,40])
rotate([90,0,45])
difference(){
union(){
cylinder(r=10,h=30);
translate([-2.5,-30,6])
cube([5,30,26]);
}
translate([0,0,-10])
cylinder(r=6,h=92);
translate([-0.5,0,-5])
cube([1,20,40]);
}
