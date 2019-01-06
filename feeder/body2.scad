$fn=100;
tt=1.2;     // tape thickness
tw=8;      // tape width
td=4;       // tape depth
lm=0.8;    // left margin
rm=2.4;    // right margin
mp2=25;    // motor position
mp=-25;
fl=100;     // feeder length
fw=15;     // feeder width
fh=38;      // feeder height

bml=30;    // back mount length
bmh=2;    // back mount height
fml=10;     // front mount length
fmh=2;      // front mount height

ttt = td+tt+6;

difference(){
union(){
translate([0,0,fw/2])
difference(){
cube([fl,fh,fw],center=true);
if(mp < 0){
translate([-fl/2-1,-(fh/2-(fh-ttt)/2),fw/4+2])
cube([fl,fh-ttt+2,fw/2+0.1],center=true);
} else {
translate([fl/2+1,-(fh/2-(fh-ttt)/2),fw/4+2])
cube([fl,fh-ttt+2,fw/2+0.1],center=true);
}
}

// entrance cover
//difference(){
//translate([-45,18.5,4])
//cube([20,1,8],center=true);
//translate([-35,18.5,4.75])
//rotate([0,0,-45])
//cube([10,1,5.5],center=true);
//}

translate([mp,0,2]){
translate([0,4-10,0]){
rotate([0,0,-30]){
translate([21,0,3.8])
cylinder(r=5,h=7.6,center=true);
translate([-21,0,3.8])
cylinder(r=5,h=7.6,center=true);
}
}
}

// back mount
translate([fl/2+bml/2,fh/2-6-bmh/2,fw/2])
cube([bml,bmh,fw],center=true);
translate([fl/2+bml/2,fh/2-3-bmh/2,1])
cube([bml,6,2],center=true);
translate([fl/2+bml/2,fh/2-3-bmh/2,fw-1])
cube([bml,6,2],center=true);

// front mount
//translate([-fl/2-fml/2,fh/2-6-fmh/2,fw/2])
//cube([fml,fmh,fw],center=true);
translate([-fl/2-fml/2,fh/2-4,fw/2])
cube([fml,6,fw],center=true);
translate([-fl/2-fml/2,fh/2-3-fmh/2,0.6])
cube([fml,6,1.2],center=true);
translate([-fl/2-fml/2,fh/2-3-fmh/2,fw-1])
cube([fml,6,2],center=true);

// top cover
translate([5,fh/2-0.6,fw/2])
cube([fl+30,1.2,fw],center=true);

}
// ------------------------------------------

// top cover
//translate([10,fh/2-0.6,fw/2])
//cube([fl+20,1.2,fw],center=true);
translate([7+10,fh/2-0.6,(tw+0.4)/2+2-rm+2+(rm-lm)/2])
cube([fl-7+20,tt,tw+0.4-rm-lm],center=true);

// tape guide
translate([-9-9,fh/2-2+tt/2,(tw+0.4)/2+2-rm+2])
cube([fl+0,tt,tw+0.4],center=true);
// part guide
translate([0,fh/2-2-td/2+0.2,(tw+0.4)/2+2-rm+2+(rm-lm)/2])
cube([fl+20,td,tw+0.4-rm-lm],center=true);
// sloped tape exit
translate([fl/2-18,-6.8,(tw+0.4)/2+2-rm+2])
intersection(){
union(){
difference()
{
cylinder(r=25,h=tw+0.4,center=true);
cylinder(r=25-tt-td,h=tw+1,center=true);
}
//translate([0,0,(tw+0.4)/2-(tw+0.4-rm-lm)/2-lm])
//difference(){
//cylinder(r=25-0.2,h=tw+0.4-rm-lm,center=true);
//cylinder(r=25-tt-td,h=tw+1,center=true);
//}
}
translate([0,0,-(tw+1)/2])
cube([26,26,tw+1]);
}

translate([mp,0,2]){
translate([5,4,0])
cylinder(r=17.5,h=10,center=true);

translate([0,4-10,0])
rotate([0,0,-30]){
translate([21,0,0])
cylinder(r=1.8,h=40,center=true);
translate([-21,0,0])
cylinder(r=1.8,h=40,center=true);
}
translate([0,4-10,0])
cylinder(r=18.2,h=30,center=true);
}

translate([mp2,0,2]){
translate([0,4-10,0])
cylinder(r=19,h=30,center=true);

translate([0,4-10,0])
rotate([0,0,-30]){
translate([21,0,0])
cylinder(r=5,h=40,center=true);
translate([-21,0,0])
cylinder(r=2.5,h=40,center=true);
}
}

// tape opening
translate([-fl/2+10,fh/2,4.75+2])
cube([6,2,5.5],center=true);

//mounting holes
translate([-fl/2-fml/2,fh/2-6-fmh/2,fw/2])
rotate([90,0,0])
cylinder(r=3,h=10,center=true);
translate([fl/2+bml-5,fh/2-6-bmh/2,fw/2])
rotate([90,0,0])
cylinder(r=3,h=10,center=true);

}
// fix hole above driving motor
translate([0,16,-2+(4-rm)/2+2])
cube([fl,6,4-rm],center=true);
