$fn=100;
nx=1;
ny=3;
dx=26.5;
dy=30.5;
hd=2.7;
m=7;

tx=(dx+m)*nx;
ty=(dy+m)*ny;

difference(){
cube([tx,ty,2],center=true);
for (y = [0 : ny - 1]){
by=y*(dy+m)+(dy+m)/2-ty/2;
translate([0,by,-1])
cube([100,8,1.2],center=true);
}
}

for (x = [0 : nx - 1]) {
for (y = [0 : ny - 1]) {
bx=x*(dx+m)+(dx+m)/2-tx/2;
by=y*(dy+m)+(dy+m)/2-ty/2;
translate([bx-dx/2,by-dy/2,0])
cylinder(r=hd/2,h=5);
translate([bx-dx/2,by+dy/2,0])
cylinder(r=hd/2,h=5);
translate([bx+dx/2,by-dy/2,0])
cylinder(r=hd/2,h=5);
translate([bx+dx/2,by+dy/2,0])
cylinder(r=hd/2,h=5);
}
}
