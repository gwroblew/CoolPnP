$fn=100;
module screw(){
cylinder(r=1.9,h=30,center=true);
translate([0,0,7])
cylinder(r2=4,r1=1.9,h=2.1);
}

difference(){
cube([15*6+16,24,18],center=true);
for(x = [0:5]){
translate([x*15-15*2.5,0,2]){
cylinder(r=3.3,h=25,center=true);
translate([0,-8,0])
cube([6.6,16,25],center=true);
translate([0,0,4]){
cylinder(r=5.7,h=1.6,center=true);
translate([0,-8,0])
cube([11.4,16,1.6],center=true);
translate([0,-12,3])
cube([11.4,16,6],center=true);
}
if(x==5){
translate([0,0,-8]){
cylinder(r=5,h=10,center=true);
translate([0,-8,0])
cube([10,16,10],center=true);
}
}
}
}
translate([0,5,0])
screw();
translate([47,5,0])
screw();
translate([-47,5,0])
screw();
}
