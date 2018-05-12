module roundedcube(x,y,z,r){
 hull(){
  translate([r/2,r/2,0]) cylinder(h=z,d=r);
  translate([-(r/2)+x,r/2,0]) cylinder(h=z,d=r);
  translate([(r/2),-(r/2)+y,0]) cylinder(h=z,d=r);
  translate([-(r/2)+x,-(r/2)+y,0]) cylinder(h=z,d=r);
 }
}

difference(){
translate([0,-20,0])
roundedcube(93.5, 142+6, 6, 20);

translate([29.5+1.5,20.5+1.5,-1])
cylinder(r=1.55,h=10);
translate([93.5-(29.5+1.5),20.5+1.5,-1])
cylinder(r=1.55,h=10);

translate([24.5-4.5,122-(16.1-4.5),-1])
cylinder(r=6,h=3);
translate([24.5-4.5,122-(76.1-4.5),-1])
cylinder(r=6,h=3);
translate([93.5-(24.5-4.5),122-(16.1-4.5),-1])
cylinder(r=6,h=3);
translate([93.5-(24.5-4.5),122-(76.1-4.5),-1])
cylinder(r=6,h=3);

translate([93.5/2-(3.1+1.9),122-(9.6-1.9),-1])
cylinder(r=1.9,h=10);
translate([93.5/2+(3.1+1.9),122-(9.6-1.9),-1])
cylinder(r=1.9,h=10);
}
translate([93.5/2, 122+3, 24]) {
difference(){
cube([70, 6.5, 48], center=true);
translate([0,8,0])
rotate([90,0,0])
cylinder(r=23/2,h=16);
translate([14.25,8,14.25])
rotate([90,0,0])
cylinder(r=1.6,h=16);
translate([14.25,8,-14.25])
rotate([90,0,0])
cylinder(r=1.6,h=16);
translate([-14.25,8,14.25])
rotate([90,0,0])
cylinder(r=1.6,h=16);
translate([-14.25,8,-14.25])
rotate([90,0,0])
cylinder(r=1.6,h=16);
}}
difference(){
translate([93.5/2, -20+3, 16])
cube([70, 6, 32], center=true);
translate([93.5/2, -20+3, 24])
translate([0,8,0])
rotate([90,0,0])
cylinder(r=4,h=16);
translate([93.5/2, -20+3, 24])
translate([0,3.2,0])
rotate([90,0,0])
cylinder(r=8,h=3);
}
difference(){
hull(){
translate([10,-10,5]) cylinder(r=10,h=43);
translate([10,118,5]) cylinder(r=10,h=43);
}
translate([20-4.5,-20,42])
rotate([0,45,0])
cube([6.4,120,6.4]);
}
difference(){
hull(){
translate([93.5-10,-10,5]) cylinder(r=10,h=43);
translate([93.5-10,118,5]) cylinder(r=10,h=43);
}
translate([93.5-20-4.5,-20,42])
rotate([0,45,0])
cube([6.4,120,6.4]);
}
