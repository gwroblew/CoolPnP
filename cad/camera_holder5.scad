$fn=100;
difference(){
union(){
cylinder(r=32,h=80);
translate([0,0,78])
cylinder(r=40,h=2);
}
translate([0,0,-10])
cylinder(r=27,h=192);
translate([-2.5,-40,30])
cube([5,80,110]);
translate([-40,-2.5,30])
cube([80,5,110]);
}
rotate([0,0,45]){
translate([-7.5,-30,0])
cube([15,14,5]);
//translate([-7.5,18,0])
//cube([15,14,5]);
translate([-30,-7.5,0])
cube([14,15,5]);
translate([16,-7.5,0])
cube([14,15,5]);
}
translate([0,0,5])
rotate([0,180,45])
difference(){
union(){
translate([0,0,2.5])
cube([32,32,5],center=true);
translate([16-1.9,16-1.9,-1])
cube([3.8,3.8,2],center=true);
translate([-16+1.9,16-1.9,-1])
cube([3.8,3.8,2],center=true);
translate([16-1.9,-16+1.9,-1])
cube([3.8,3.8,2],center=true);
translate([-16+1.9,-16+1.9,-1])
cube([3.8,3.8,2],center=true);
}
translate([16-1.9,16-1.9,-1])
cylinder(d=2.2,h=30,center=true);
translate([-16+1.9,16-1.9,-1])
cylinder(d=2.2,h=30,center=true);
translate([16-1.9,-16+1.9,-1])
cylinder(d=2.2,h=30,center=true);
translate([-16+1.9,-16+1.9,-1])
cylinder(d=2.2,h=30,center=true);
translate([16-5.5-5.5,16-2.5,0])
cube([11,5,20],center=true);
}
