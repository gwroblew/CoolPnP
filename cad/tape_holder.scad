// length of tray
l=180;
// thickness of base
b=1.2;
// width of lane
w=8.2;
// number of lanes
n=4;
// height of groove
g=2.2;
// width of spacer
s=1.8;

module strip(){
rotate([90,0,0])
linear_extrude(height=l, center=true)
polygon([[0,0],[0,0.8],[0.6,0.8],[0.6,0.6]]);
}

module holder(){
cube([w,l,b],center=true);
translate([-w/2,-l/2,-b/2])
cube([0.6+1.8,l,b+g-0.8]);
translate([w/2-0.6,-l/2,-b/2])
cube([0.6,l,b+g-0.8]);
translate([-w/2,0,b/2+g])
strip();
mirror(0,1,0)
translate([-w/2,0,b/2+g])
strip();
}

module spacer(){
translate([0,0,(g+0.8)/2])
cube([s,l,b+g+0.8],center=true);
}

difference(){
union(){
for(i = [0:n-1]){
translate([i * (w+s) - (n*(w+s))/2,0,0]){
translate([-(w/2)-s/2,0,0])
spacer();
holder();
}}
translate([(w/2)+s/2 + (n-1)*(w+s) - (n*(w+s))/2,0,0])
spacer();
}
for(i = [-5:5])
translate([0,(l-20)/10 * i,-b/2])
cube([200,3,1.2],center=true);
}
