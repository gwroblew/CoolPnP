$fn=100;
module roundedcube(x,y,z,r){
 hull(){
  translate([r/2,r/2,0]) cylinder(h=z,d=r);
  translate([-(r/2)+x,r/2,0]) cylinder(h=z,d=r);
  translate([(r/2),-(r/2)+y,0]) cylinder(h=z,d=r);
  translate([-(r/2)+x,-(r/2)+y,0]) cylinder(h=z,d=r);
 }
}

module main(){
difference(){
union(){
difference(){
union(){
translate([0,-20,0])
roundedcube(93.5, 142+6, 6, 20);
difference(){
hull(){
translate([10,-10,5]) cylinder(r=10,h=27);
translate([10,118,5]) cylinder(r=10,h=27);
}
translate([20,120,-10])
cube([5,100,50],center=true);
}
hull(){
translate([93.5-10,-10,5]) cylinder(r=10,h=27);
translate([93.5-10,118,5]) cylinder(r=10,h=27);
}}

translate([24.5-4.5,122-(16.1-4.5),-1])
cylinder(r=6,h=3);
translate([24.5-4.5,122-(76.1-4.5),-1])
cylinder(r=6,h=3);
translate([93.5-(24.5-4.5),122-(16.1-4.5),-1])
cylinder(r=6,h=3);
translate([93.5-(24.5-4.5),122-(76.1-4.5),-1])
cylinder(r=6,h=3);

translate([93.5/2-(3.1+1.9),122-(9.6-1.9),-1])
cylinder(r=1.9+0.1,h=15,center=true);
translate([93.5/2+(3.1+1.9),122-(9.6-1.9),-1])
cylinder(r=1.9+0.1,h=15,center=true);

translate([7.7,12,0])
rotate([0,0,90])
cylinder(r=2.7,h=160,center=true);
translate([93.5-7.7,12,0])
rotate([0,0,90])
cylinder(r=2.7,h=160,center=true);
translate([7.7,55,0])
rotate([0,0,90])
cylinder(r=2.7,h=160,center=true);
translate([93.5-7.7,55,0])
rotate([0,0,90])
cylinder(r=2.7,h=160,center=true);

translate([93.5,37,25])
cube([18,12,16],center=true);
}
translate([93.5-12,37,46])
rotate([-90,0,0])
difference(){
translate([2.5,3,0])
cube([21,23,20],center=true);
translate([0,0,-15])
cylinder(r=6.1,h=30);
translate([8,0,0])
cube([15,1,22],center=true);
translate([9,0,0])
rotate([90,0,0])
cylinder(r=2.7,h=55,center=true);
}

translate([93.5/2, 122+2, 24]) {
difference(){
cube([70, 8, 48], center=true);
translate([-11,0,0]){
translate([0,8,0])
rotate([90,0,0])
cylinder(r=11.25,h=16);
translate([15.3,8,15.3])
rotate([90,0,0])
cylinder(r=1.7,h=16);
translate([15.3,8,-15.3])
rotate([90,0,0])
cylinder(r=1.7,h=16);
translate([-15.3,8,15.3])
rotate([90,0,0])
cylinder(r=1.7,h=16);
translate([-15.3,8,-15.3])
rotate([90,0,0])
cylinder(r=1.7,h=16);
}
translate([11, 8, 0])
rotate([90,0,0])
cylinder(r=4.1,h=16);
translate([35,0,24])
rotate([90,0,0])
cylinder(r=12,h=20,center=true);
}}

difference(){
translate([93.5/2, -20+3, 18])
cube([70, 6, 36], center=true);
translate([-11,0,0]){
translate([93.5/2, -20+3, 24])
translate([0,8,0])
rotate([90,0,0])
cylinder(r=4.1,h=16);
translate([93.5/2, -20+3, 24])
translate([0,3.2,0])
rotate([90,0,0])
cylinder(r=11.1,h=3);
}
translate([11,0,0]){
translate([93.5/2, -20+3, 24])
translate([0,8,0])
rotate([90,0,0])
cylinder(r=4.1,h=7);
}}
}

translate([93.5/2-11, 122+2, 24]) {
translate([-15.3,-4-2,-15.3])
rotate([90,0,0])
cylinder(r=3,h=3.98,center=true);
translate([15.3,-4-2,-15.3])
rotate([90,0,0])
cylinder(r=3,h=3.98,center=true);
}}}

module carriage(){
color("green"){
difference(){
translate([93.5/2+11+1,50,23])
cube([22,50,24],center=true);
translate([93.5/2+11+1,65,14])
cube([30,4,1.5],center=true);
translate([93.5/2+11, 128, 24])
rotate([90,0,0])
cylinder(r=7.5,h=148);
translate([93.5/2+11+1+6,50,24])
cube([12,52,1],center=true);
}
translate([93.5/2-11,25+25,24])
difference(){
cube([24,20,22],center=true);
translate([5.6,0,5.6])
rotate([90,0,0])
cylinder(r=1.7,h=30,center=true);
translate([5.6,0,-5.6])
rotate([90,0,0])
cylinder(r=1.7,h=30,center=true);
translate([-5.6,0,-5.6])
rotate([90,0,0])
cylinder(r=1.7,h=30,center=true);
translate([-5.6,0,5.6])
rotate([90,0,0])
cylinder(r=1.7,h=30,center=true);
rotate([90,0,0])
cylinder(r=4.2,h=30,center=true);
}
translate([93.5/2+11+1-23,25+7.5+5,24+22])
cube([24,25,22],center=true);
translate([93.5/2+11+1,25+5,24+22])
difference(){
cube([22,10,22], center=true);
rotate([90,0,0])
cylinder(r=7.85,h=20,center=true);
translate([8,0,8])
rotate([90,0,0])
cylinder(r=1.15,h=20,center=true);
translate([8,0,-8])
rotate([90,0,0])
cylinder(r=1.15,h=20,center=true);
translate([-8,0,-8])
rotate([90,0,0])
cylinder(r=1.15,h=20,center=true);
translate([-8,0,8])
rotate([90,0,0])
cylinder(r=1.15,h=20,center=true);
}
}
}

//main();
carriage();
/*color("blue")
translate([93.5/2-11, 128, 24])
rotate([90,0,0])
cylinder(r=4,h=148);

/*color("blue")
translate([93.5/2+11, 128, 24])
rotate([90,0,0])
cylinder(r=4,h=148);
*/
