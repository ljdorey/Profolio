
use <mcad/involute_gears.scad>

//Gear InfoSource
//https://khkgears.net/new/gear_knowledge/gear_technical_reference/gear_backlash.html


//TODO:
//Add holes / backlastspacing for gears... etc...
//change size /shape of driver bit / output of sun. 
//make the outside a hex
//make the outpuse a box


//spacings for when we split up the rings to print them. 
Spread=5; //Sun to Planets Spread
Spread2=20; //Outgear spread when split
Spread3=50; //Outgear 2 spread when split
Spread4=50; //Distance of suns from middle.
MarkSize=0.5; //how big the alignment marks are

testing=true; //turning might speed up preview rendering...
ShowSun=true;
ShowPlanets=true;
ShowOutterGear=true;
ShowMarks=true;
Viz=false;



SystemClearance=2; //distance between system1 and system 2

//System1
OutsideDiameter=75;
Height=10;



A=36;
M=1.2;   
B=45;

PZ=10;
SZ=15;
Np=5;

//Alignment fiddles
RF=2; //1 or 2
PRF=-0.3; 
ORF=-0.45;

//System2 Note M and NP set from system 1.
OutsideDiameter1=75;
Height1=10;

A1=36;
B1=45;
PZ1=15;
SZ1=20;

//Alignment fiddlers
RF1=2;
PRF1=-0.04;
ORF1=1.8;

//QualitySettings

//System1 Calcs
CZ=SZ+2*PZ;

At = atan(tan(A)/cos(B));
Ax = ((PZ+SZ)/(2*cos(B)))*M;

SD = SZ*M/(cos(B));
PD = PZ*M/(cos(B));
CD = PD*2+SD;

SDb=SD*cos(At);
PDb=PD*cos(At);
CDb=CD*cos(At);

SDa = SD + 2*M;
PDa = PD + 2*M;
CDa = CD + 2*M;

SDf = SD - 2.5 * M;
PDf = PD - 2.5 * M;
CDf = CD - 2.5 * M;

X = Height/2/tan(B);


    

//System2 Calcs
Np1=Np;
CZ1=SZ1+2*PZ1;
At1 = atan(tan(A1)/cos(B1));
Ax1 = Ax;
M1 = Ax / ((PZ1+SZ1)/(2*cos(B1)));

SD1 = SZ1*M1/(cos(B1));
PD1 = PZ1*M1/(cos(B1));
CD1 = PD1*2+SD1;

SDb1=SD1*cos(At1);
PDb1=PD1*cos(At1);
CDb1=CD1*cos(At1);

SDa1 = SD1 + 2*M1;
PDa1 = PD1 + 2*M1;
CDa1 = CD1 + 2*M1;

SDf1 = SD1 - 2.5 * M1;
PDf1 = PD1 - 2.5 * M1;
CDf1 = CD1 - 2.5 * M1;

X1 = Height1/2/tan(B1);

//Quality controlled by testing var
Slices= testing==true ?  2 : max(Height/2/0.1,Height1/2/0.1);
$fa= testing==true?5:0.1;


if(Viz)
visulize();
if(!Viz)
spreadforprinter();


module spreadforprinter()
{

translate([0,-Spread4,0])
sun(Height,X,SD,SZ,SDf,SDb,SDa,PRF,true,true);


translate([0,Spread4,0])
sun(Height1,X1,SD1,SZ1,SDf1,SDb1,SDa1,PRF1,true,true);    
    
SplitOuterGear(Height,X,CDf,CZ,OutsideDiameter,CD,CDf,CDb,CDa,RF,ORF,Spread2, true,true);
SplitOuterGear(Height1,X1,CDf1,CZ1,OutsideDiameter1,CD1,CDf1,CDb1,CDa1,RF1,ORF1,Spread3,true,true);

union()
  {
planets(Np,Ax,Height,X,PD,PZ,SZ,PDf,PDb,PDa,true,true);
planetConnectors(Np,Ax,SystemClearance,X,PD,PZ,SZ,PDf,PDb,PDa,Height/2, PDf,PDa1);
translate([0,0,Height+SystemClearance])
planets(Np1,Ax1,Height1,X1,PD1,PZ1,SZ1,PDf1,PDb1,PDa1,true, true);
  }

} 


module SplitOuterGear(height=Height,x=X,cdf=CDf,cz=CZ,od=OutsideDiameter,cd=CD,cdf=CDf,cdb=CDb,cda=CDa,rf=RF,orf=ORF,roa=Spread2,markbot=true, marktop=true)
{
    
    translate([roa, 0,0])    
    intersection()
    {
    translate([0,-1*(od/2+10),-height])
    cube([od+10,od+10,height*2]);
    outter(height,x,cdf,cz,od,cd,cdf,cdb,cda,rf,orf,markbot,marktop);
    };
    
    translate([-roa, 0,0])    
    intersection()
    {
    translate([-od-10,-1*(od/2+10/2),-height])
    cube([od+10,od+10,height*2]);
    outter(height,x,cdf,cz,od,cd,cdf,cdb,cda,rf,orf,markbot,marktop);
    };

}


module visulize()
{

//color("purple")
//marks(Np,-Height/2,OutsideDiameter/2+1);
    
outter(Height,X,CDf,CZ,OutsideDiameter,CD,CDf,CDb,CDa,RF,ORF);
sun(Height,X,SD,SZ,SDf,SDb,SDa,PRF);

translate([0,0,Height+SystemClearance])    
outter(Height1,X1,CDf1,CZ1,OutsideDiameter1,CD1,CDf1,CDb1,CDa1,RF1,ORF1);
translate([0,0,Height+SystemClearance])
sun(Height1,X1,SD1,SZ1,SDf1,SDb1,SDa1,PRF1);

union(){
translate([0,0,Height+SystemClearance])
planets(Np1,Ax1,Height1,X1,PD1,PZ1,SZ1,PDf1,PDb1,PDa1);    
planetConnectors(Np,Ax,SystemClearance,X,PD,PZ,SZ,PDf,PDb,PDa,Height/2, PDf,PDa1);
planets(Np,Ax,Height,X,PD,PZ,SZ,PDf,PDb,PDa);
    } 



}



module outter(height=Height,x=X,cdf=CDf,cz=CZ,od=OutsideDiameter,cd=CD,cdf=CDf,cdb=CDb,cda=CDa,rf=RF, orf=ORF,markbot=true, marktop=true)
{
if(ShowOutterGear)
difference()
{
color("yellow")
for( mir=[0,1] )
mirror([0,0,mir])
//linear_extrude( height=Height/2, center=false, convexity=10, twist=-360*Height/2/(2*PI*CD/2))
linear_extrude( height=height/2, center=false, convexity=10, twist=-asin(x/cdf)*2, slices=Slices)
rotate([0,0,360/CZ/rf+orf])
difference()
{
circle(r=od/2);    
gear_shape (
					number_of_teeth = cz,
					pitch_radius = cd/2,
					root_radius = cdf/2,
					base_radius = cdb/2,
					outer_radius = cda/2,
					half_thick_angle = 360/cz/4,
					involute_facets=0);

}
if(markbot)marks(height=-height/2);
if(marktop)marks(height=height/2);
}
}

module sun(height=Height, x=X, sd=SD,sz=SZ,sdf=SDf,sdb=SDb,sda=SDa, prf=PRF,markbot=true, marktop=true)
{
if(ShowSun)
color("blue")

for( mir=[0,1] )
mirror([0,0,mir])
difference()
{
    
    //linear_extrude( height=Height/2, center=false, convexity=10,twist=360*Height/2/(2*PI*SD/2))
    linear_extrude( height=height/2, center=false, convexity=10,twist=asin(x/sd)*2,slices=Slices)
    rotate([0,0,prf])
    gear_shape (
					number_of_teeth = sz,
					pitch_radius = sd/2,
					root_radius = sdf/2,
					base_radius = sdb/2,
					outer_radius = sda/2,
					half_thick_angle = 360/sz/4,
					involute_facets=0);
 
 color("green")
 translate([0,0,-1*height])
 linear_extrude(height=height*2, center=false)
 circle(r=3.3, $fn=6);
if(markbot)marks(height=-height/2);
if(marktop)marks(height=height/2);    
}
}
 
module planets(np=Np, ax=Ax, height=Height,x=X,pd=PD,pz=PZ,sz=SZ,pdf=PDf,pdb=PDb,pda=PDa,markbot=true, marktop=true)
{
if(ShowPlanets)

for(n=[0:np-1])
{
difference()
    {
color("green")
rotate([0,0,360/np*n])
translate([ax, 0,0])  
for( mir=[0,1] )
mirror([0,0,mir])
linear_extrude( height=height/2, center=false, convexity=10,twist=-asin(x/pd)*2,slices=Slices)
rotate([0,0,180+360/pz/2+360/np*n*sz/pz])
gear_shape (
					number_of_teeth = pz,
					pitch_radius = pd/2,
					root_radius = pdf/2,
					base_radius = pdb/2,
					outer_radius = pda/2,
					half_thick_angle = 360/pz/4,
					involute_facets=0);
if(markbot)marks(height=-height/2);
if(marktop)marks(height=height/2);
}



 
}
}

module planetConnectors(np=Np, ax=Ax, height=SystemClearance,x=X,pd=PD,pz=PZ,sz=SZ,pdf=PDf,pdb=PDb,pda=PDa, offset=Height/2, bd=PDf,td=PDa1)
{

if(ShowPlanets)
for(n=[0:np-1])
{
color("red")
union()
   {

rotate([0,0,360/np*n])
translate([ax, 0,0])
translate([0,0,offset])
linear_extrude(height=height,center=false,convexity=10,scale=td/pdf)
circle(bd/2);

rotate([0,0,360/np*n])
translate([ax, 0,0])  
translate([0,0,offset])
linear_extrude(height=height,center=false,convexity=10,scale=td/pda)
rotate([0,0,asin(x/pd)*2])
rotate([0,0,180+360/pz/2+360/np*n*sz/pz])    
gear_shape (
					number_of_teeth = pz,
					pitch_radius = pd/2,
					root_radius = pdf/2,
					base_radius = pdb/2,
					outer_radius = pda/2,
					half_thick_angle = 360/pz/4,
					involute_facets=0);
                }
   
    

}
}



module marks(height=-Height/2)
{
    if(ShowMarks)
    for(n=[1:Np])
    {
    rotate([0,0,360/Np*(n-1)-(n-1)*5/2])
    for(l=[1:n])
    {
       translate([0,0,height])
       rotate([0,0,(l-1)*5])
       rotate([0,90,0])
       linear_extrude(OutsideDiameter, center=false)
       circle(r=MarkSize,$fn=3);
    }

    }    
    
}
























































   


