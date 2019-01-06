$fn=100;
n=10;
w=15;
l=72;

difference(){
union(){
cube([n*w+2,l,2+6+1],center=true);

for (i = [0 : n - 1]) {
s=(i%2)*2-1;
translate([0,s*(10+16),0])
translate([(i-n/2)*w+w/2,0,4.5])
cylinder(r=4.9,h=4,$fn=6);
}
}

for (i = [0 : n - 1]) {
translate([(i-n/2)*w+w/2,0,0])
cube([14.4,l+0.1,7],center=true);
}
}
