$fn=100;
module cube_at(pos,dim){
translate(pos) cube(dim,center=true);
}

difference(){
union(){
cylinder(r=19,h=19+4);
}
translate([0,0,-1])
cylinder(r=14,h=192);
}
difference(){
cube_at([0,25,2+19],[10,20,4]);
translate([0,27,19-1])
cylinder(r=2.5,h=10);
}
difference(){
cube_at([0,-25,2+19],[10,20,4]);
translate([0,-27,19-1])
cylinder(r=2.5,h=10);
}
difference(){
cube_at([25,0,2+19],[20,10,4]);
translate([27,0,19-1])
cylinder(r=2.5,h=10);
}
translate([0,0,19+4])
rotate_extrude(convexity=100){
translate([19-2.5,0,0])
circle(r=2.5);
}
