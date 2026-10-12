"""Render an exact, readable theatrical title as a transparent game asset."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import math
R=Path(__file__).resolve().parents[1]/'Assets/Backstage/Resources'
W,H,S=1080,360,2
im=Image.new('RGBA',(W*S,H*S));d=ImageDraw.Draw(im)
font='/usr/share/fonts/opentype/urw-base35/P052-Roman.otf'
def text(x,y,t,size):
 f=ImageFont.truetype(font,size*S)
 d.text((int(x*S+2),int(y*S+3)),t,font=f,fill=(48,27,25,160),anchor='lt')
 d.text((int(x*S),int(y*S)),t,font=f,fill=(239,195,126,255),anchor='lt')
text(92,4,'L',163);text(196,46,'armes',115);text(536,71,'de',61)
text(108,156,'M',183);text(283,197,'arionnette',109)
# One quiet filigree below the second line, not dense ornament.
col=(217,167,97,240)
def line(points,width=2):d.line([(int(x*S),int(y*S)) for x,y in points],fill=col,width=width*S)
for side in (-1,1):
 pts=[]
 for i in range(181):
  t=i/180
  x=542+side*(8+270*t)
  y=332+9*math.sin(t*math.pi*2)*math.sin(t*math.pi)
  pts.append((x,y))
 line(pts)
for side in (-1,1):
 pts=[]
 for i in range(121):
  a=i/120*math.pi*2.2;r=19*(1-i/150)
  pts.append((542+side*278+side*r*math.cos(a),330+r*.46*math.sin(a)))
 line(pts)
d.polygon([(542*S,320*S),(556*S,330*S),(542*S,340*S),(528*S,330*S)],outline=col,width=2*S)
im=im.resize((W,H),Image.Resampling.LANCZOS)
im.save(R/'TitleWordmark.bytes',format='PNG')
im.save(Path(__file__).resolve().parent/'indie_art_sources'/'TitleWordmark.png')
