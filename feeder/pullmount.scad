$fn=100;
up=0;

difference(){
translate([0,0,-up/2])
translate([0.5,0,1])
cube([14,20,22+up],center=true);

translate([0,0,-up]){
translate([0,0,-10+2.2])
cylinder(r=5.2,h=4.41,center=true,$fn=6);
}

translate([0,0,10-5])
cube([15.1,8.1,10.1],center=true);

translate([7.5-2.5,0,5])
rotate([0,90,0])
cylinder(r=5.1,h=5.01,center=true);

translate([7.5-2.5,0,7.5])
cube([5.01,10.2,5.01],center=true);
}
