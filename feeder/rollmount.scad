$fn=100;

difference(){
cube([20,25,100+20+20],center=true);

translate([0,5,-60])
cube([20.1,20.1,20.1],center=true);

translate([0,0,60])
rotate([0,90,0])
cylinder(r=6.5,h=21,center=true);

translate([0,0,-60])
rotate([90,0,0])
cylinder(r=2,h=31,center=true);
translate([0,-12.5+1.5,-60])
rotate([90,0,0])
cylinder(r1=2,r2=4,h=3.1,center=true);
}
