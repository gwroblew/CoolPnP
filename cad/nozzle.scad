$fn=100;

difference(){
union(){
cylinder(h=11,d=6.2);
translate([0,0,2.5])
cylinder(h=1,d=10.5);
translate([0,0,11])
cylinder(h=3.5,d1=2.5,d2=0.6);
}
cylinder(h=10,d=4.2);
cylinder(h=15,d=0.2);
translate([0,0,10])
cylinder(h=4,d1=1,d2=0.2);
}
