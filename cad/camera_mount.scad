$fn=100;
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
translate([8,-4,0])
cylinder(d=11.8,h=50);
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
translate([8,-4,45]){
rotate([0,90,0])
cylinder(r=1.2,h=20,center=true);
rotate([-20,0,0])
cube([5,20,16],center=true);
}
}
