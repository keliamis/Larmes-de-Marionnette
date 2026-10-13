"""Carved wooden theater signs, painted in the game's amber palette."""
from pathlib import Path
from PIL import Image, ImageDraw
import math
R=Path(__file__).resolve().parents[1]/'Assets/Backstage/Resources'
S=Path(__file__).resolve().parent/'indie_art_sources'
W,H,K=960,152,3

def cubic(a,b,c,d,n=30):
 return [((1-t)**3*a[0]+3*(1-t)**2*t*b[0]+3*(1-t)*t*t*c[0]+t**3*d[0],(1-t)**3*a[1]+3*(1-t)**2*t*b[1]+3*(1-t)*t*t*c[1]+t**3*d[1]) for t in [i/n for i in range(n+1)]]
pts=[(78,24),(882,24)]
for a,b,c,d in [((882,24),(897,24),(894,42),(910,46)),((910,46),(928,52),(928,66),(912,73)),((912,73),(902,78),(894,88),(897,100)),((897,100),(897,116),(884,126),(867,126))]:pts+=cubic(a,b,c,d)
pts.append((93,126))
for a,b,c,d in [((93,126),(76,126),(63,117),(65,100)),((65,100),(68,88),(58,78),(48,73)),((48,73),(32,66),(32,52),(50,46)),((50,46),(66,42),(63,24),(78,24))]:pts+=cubic(a,b,c,d)
for hover,name in [(False,'PaperButton'),(True,'PaperButtonHover')]:
 mask=Image.new('L',(W,H));ImageDraw.Draw(mask).polygon(pts,fill=255)
 im=Image.new('RGBA',(W,H));pix=im.load();m=mask.load()
 for x in range(W):
  for y in range(H):
   if m[x,y]:
    v=y/H;g=2*math.sin(x*.025+y*.013)+math.sin(x*.043-y*.02);light=9 if hover else 0
    pix[x,y]=(int(231+light-28*v+g),int(197+light-34*v+g),int(145+light-36*v+g),255)
 im=im.resize((W*K,H*K),Image.Resampling.LANCZOS);d=ImageDraw.Draw(im,'RGBA')
 def line(p,color,width=1):d.line([(round(x*K),round(y*K)) for x,y in p],fill=color,width=width*K,joint='curve')
 line(pts+[pts[0]],(99,62,35,235),2)
 inner=[(480+(x-480)*.965,75+(y-75)*.83) for x,y in pts]
 line(inner+[inner[0]],(157,105,59,185),1)
 line(cubic((90,28),(270,25),(700,25),(872,28)),(255,231,172,230),2)
 line(cubic((94,123),(290,125),(690,124),(866,123)),(104,67,40,170),2)
 for y in (39,46,108,116):
  line(cubic((114,y),(310,y-4),(632,y+5),(846,y-1)),(123,77,43,35),1)
 for side in (-1,1):
  center=480+side*394
  for yc in (47,102):
   spiral=[]
   for i in range(100):
    t=i/99;a=t*math.pi*2.2;r=14*(1-t*.8)
    spiral.append((center+side*r*math.cos(a),yc+r*.62*math.sin(a)))
   line(spiral,(118,77,43,180),2)
   line([(x,y-1) for x,y in spiral],(240,205,143,165),1)
 im=im.resize((W,H),Image.Resampling.LANCZOS)
 im.save(R/(name+'.bytes'),format='PNG');im.save(S/(name+'.png'))
print('Carved wooden button signs rendered')
