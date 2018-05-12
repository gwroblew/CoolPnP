// base thickness
b=1.2;
// wall height
d=1.2;
// separator width
s=1;

module tray(tw,th,w,h){
c=floor(tw/w);
r=floor(th/h);
difference(){
cube([tw,th,b+d]);

for(i=[0:r-1])
for(j=[0:c-1])
translate([j*(w+s)+s,i*(h+s)+s,b])
cube([w,h,d*2]);

for(i = [0:4])
translate([tw/5 * i + tw/20,-10,-0.6])
cube([3,200,1.2]);
}
}

//tray(169,49,23,23);

tray(105,29,14,14);
translate([0,29,0])
tray(105,21,10,10);
translate([0,21+29,0])
tray(105,15,7,7);
translate([0,15+21+29,0])
tray(105,11,4,4);
translate([105-s,0,0])
cube([s,11+15+21+29,b+d]);
