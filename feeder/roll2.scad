$fn=100;
cr=5.6/2;     // core radius
it=4.0;        // inside thickness
tr=4;
or=15;    // outside radius
tw=5.8;     // tape width
ww=1.0;       // wall width

cw = tw+ww;

module top(){
difference(){
union(){
translate([0,0,-(tw+ww)/2])
cylinder(r=or,h=ww,center=true);
}

difference(){
cylinder(r=cr,h=cw+2,center=true);

translate([it/2+2,0,0])
cube([4,10,10],center=true);
translate([-it/2-2,0,0])
cube([4,10,10],center=true);
}
}
}

module bottom(){
difference(){
union(){
translate([0,0,(tw+ww)/2])
cylinder(r=or,h=ww,center=true);

cylinder(r=tr,h=cw,center=true);
}

difference(){
cylinder(r=cr,h=cw+2,center=true);

translate([it/2+2,0,0])
cube([4,10,10],center=true);
translate([-it/2-2,0,0])
cube([4,10,10],center=true);
}

xr=tr+3;
for (i = [0 : 7])
rotate([0,0,360 * i / 8])
translate([xr+(or-xr)/2,0,(tw+ww)/2])
cube([(or-xr)+0.1,0.2,ww+1],center=true);
}
}

top();
bottom();
