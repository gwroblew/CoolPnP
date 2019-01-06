$fn=100;
cr=1.4;     // core radius
ir=2.3;      // inside radius
it=2;        // inside thickness
or=15;    // outside radius
tw=5.8;     // tape width
ww=1.2;       // wall width

cw = tw+ww;
tr=ir+it;

module top(){
difference(){
union(){
cylinder(r=tr,h=cw,center=true);

translate([0,0,-(tw+ww)/2])
cylinder(r=or,h=ww,center=true);
}

cylinder(r=cr,h=cw+2,center=true);

for (i = [0 : 5])
rotate([0,0,360 * i / 6])
translate([cr,0,0])
cube([(ir-cr)*2+0.3,1.1,cw+2],center=true);
}
}

module bottom(){
difference(){
union(){
translate([0,0,(tw+ww)/2])
cylinder(r=or,h=ww,center=true);

translate([0,0,cw/4]){
cylinder(r=cr,h=cw/2,center=true);

for (i = [0 : 5])
rotate([0,0,360 * i / 6])
translate([cr,0,0])
cube([(ir-cr)*2+0.1,0.95,cw/2],center=true);
}
}

xr=tr+3;
for (i = [0 : 7])
rotate([0,0,360 * i / 8])
translate([xr+(or-xr)/2,0,(tw+ww)/2])
cube([(or-xr)+0.1,0.2,ww+1],center=true);
}
}

//top();
bottom();
