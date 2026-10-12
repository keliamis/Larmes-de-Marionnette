"""Create clean aged-paper UI strips with broad, quiet torn edges."""
from pathlib import Path
from PIL import Image, ImageDraw
import random
ROOT=Path(__file__).resolve().parents[1]/'Assets/Backstage/Resources'
SOURCE=Path(__file__).resolve().parent/'indie_art_sources'
for hover,name in ((False,'PaperButton'),(True,'PaperButtonHover')):
    w,h=960,144;rng=random.Random(8392)
    im=Image.new('RGBA',(w,h));mask=Image.new('L',(w,h));d=ImageDraw.Draw(mask)
    top=[(8,18),(118,16),(217,20),(340,17),(478,19),(602,15),(735,18),(841,16),(952,19)]
    bottom=[(952,125),(861,128),(742,124),(614,128),(478,124),(330,127),(212,125),(107,128),(8,124)]
    points=top+[(950,49),(954,79),(950,108)]+bottom+[(10,101),(6,74),(11,41)]
    d.polygon(points,fill=255)
    px=im.load()
    for x in range(w):
        for y in range(h):
            a=mask.getpixel((x,y))
            if a:
                v=y/h;grain=rng.gauss(0,.7);light=6 if hover else 0
                px[x,y]=(int(239+light-17*v+grain),int(220+light-22*v+grain),int(179+light-24*v+grain),255)
    d=ImageDraw.Draw(im,'RGBA')
    d.line(top,fill=(164,111,69,110),width=2)
    d.line(bottom,fill=(146,94,60,110),width=2)
    # Small folded tips imply age without speckles or dark edge bands.
    d.polygon([(8,18),(26,19),(10,36)],fill=(194,156,112,105))
    d.polygon([(952,125),(934,126),(951,109)],fill=(174,131,94,85))
    im.save(ROOT/(name+'.bytes'),format='PNG')
    im.save(SOURCE/(name+'.png'))
