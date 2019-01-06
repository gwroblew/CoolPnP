$fn=100;
n=10;
w=15;

difference(){
union(){
cube([n*w,10,1],center=true);

for (i = [0 : n - 1]) {
translate([(i-n/2)*w+w/2,0,0])
cylinder(r=2.9,h=2.5);
}
}

for (i = [0 : n - 1]) {
translate([(i-n/2)*w+w/2,0,-0.3])
cube([5,10.1,0.41],center=true);
}

/*translate([0,0,-1.5])
cylinder(r1=3.3/2,r2=6.4/2,h=2);
translate([60,0,-1.5])
cylinder(r1=3.3/2,r2=6.4/2,h=2);
translate([-60,0,-1.5])
cylinder(r1=3.3/2,r2=6.4/2,h=2);*/
}
