$fn=100;
module endstop(){
difference(){
cube([18,20,40],center=true);
translate([0,0,10])
rotate([90,0,0])
cylinder(r=2.7,h=24,center=true);
translate([0,0,-10])
rotate([90,0,0])
cylinder(r=2.7,h=24,center=true);
translate([0,-16,10])
rotate([90,0,0])
cylinder(r=5.5,h=20,center=true);
translate([0,-16,-10])
rotate([90,0,0])
cylinder(r=5.5,h=20,center=true);
}

translate([0,-(20-11)/2,-30])
rotate([0,90,0])
difference(){
cube([21,5+6,18],center=true);
translate([3.5+1.27,0,1-3])
rotate([90,0,0])
cylinder(r=1.3,h=22,center=true);
translate([-3.5-1.27,0,1-3])
rotate([90,0,0])
cylinder(r=1.3,h=22,center=true);
}
}

translate([12,0,0])
endstop();
translate([-12,0,0])
mirror([1,0,0])
endstop();
