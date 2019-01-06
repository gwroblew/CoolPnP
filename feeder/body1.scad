$fn=100;
tt=1.2;     // tape thickness
tw=8;      // tape width
lm=0.8;    // left margin
rm=2.4;    // right margin
mp=-20;    // motor position
mp2=30;

difference(){
union(){
cube([110,38,4],center=true);
translate([0,8.5,8])
cube([110,21,2],center=true);
translate([0,3.5,4])
cube([110,11,6],center=true);
translate([0,10,4])
cube([110,5,6],center=true);

difference(){
translate([-45,18.5,4])
cube([20,1,8],center=true);
translate([-35,18.5,4.75])
rotate([0,0,-45])
cube([10,1,5.5],center=true);
}

translate([45,18.5,4])
cube([20,1,8],center=true);

translate([mp,0,0]){
translate([21,4-10,3.8])
cylinder(r=5,h=7.6,center=true);
translate([-21,4-10,3.8])
cylinder(r=5,h=7.6,center=true);
}
}
translate([-10+0.1,17+tt/2,(tw+0.4)/2+2-rm])
cube([100,tt,tw+0.4],center=true);

translate([mp,0,0]){
translate([0,4,0])
cylinder(r=16,h=10,center=true);

translate([21,4-10,0])
cylinder(r=1.8,h=40,center=true);
translate([-21,4-10,0])
cylinder(r=1.8,h=40,center=true);
translate([0,4-10,0])
cylinder(r=18.2,h=30,center=true);
}

translate([mp2,0,0]){
translate([0,4-10,0])
cylinder(r=18.2,h=30,center=true);
}

translate([-34,19,4.75])
cube([3,2,5.5],center=true);


translate([40,-1.75,(tw+0.4)/2-0.5])
intersection(){
difference()
{
cylinder(r=20,h=tw+0.4,center=true);
cylinder(r=20-tt-4.5,h=tw+1,center=true);
}
translate([0,0,-(tw+1)/2])
cube([21,21,tw+1]);
}
}
translate([0,16,-2+(4-rm)/2])
cube([110,6,4-rm],center=true);
