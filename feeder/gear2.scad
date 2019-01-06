$fn=100;
n = 24;     // teeth
dn = 4;     // distance between
td = 1;      // tooth depth
tl = 2.0;      // tooth length
tw = 1.4;     // tooth width
c = n * dn;
r = c / (2 * 3.1415926536);

difference(){
union(){
translate([0,0,-2.4+3])
cylinder(r=7,h=6,center=true);
cylinder(r=r - td,h=1.2,center=true);

for (i = [0 : n - 1])
rotate([0,0,360 * i / n])
translate([r - td + tl / 2,0,0])
//cube([tl,tw,1],center=true);
rotate([45,0,0])
rotate([0,90,0])
cylinder(r1=1/2*1.2,r2=0.2,h=tl+0.2,$fn=24,center=true);
}
difference(){
cylinder(r=2.6,h=50,center=true);
translate([0,2.65,0])
cube([20,2,20],center=true);
translate([0,-2.65,0])
cube([20,2,20],center=true);
}
}
