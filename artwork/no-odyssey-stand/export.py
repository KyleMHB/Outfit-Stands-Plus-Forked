from pathlib import Path
from PIL import Image, ImageDraw, ImageChops, ImageFont, ImageFilter
import importlib.util
root=Path(__file__).resolve().parents[2]
art=root/'artwork/no-odyssey-stand'; out=art/'export'; out.mkdir(parents=True,exist_ok=True)
spec=importlib.util.spec_from_file_location('stand',root/'scripts/draw-no-odyssey-art.py'); mod=importlib.util.module_from_spec(spec); spec.loader.exec_module(mod)

def fit(im, box, area):
    im=im.crop(box); a=im.getchannel('A'); im=im.crop(a.point(lambda v: 255 if v>16 else 0).getbbox())
    x,y,w,h=area; s=min(w/im.width,h/im.height)
    im=im.resize((round(im.width*s),round(im.height*s)), Image.Resampling.LANCZOS)
    result=Image.new('RGBA',(128,128)); result.alpha_composite(im,(round(x+(w-im.width)/2),round(y+(h-im.height)/2)))
    return result

files={}; previews={}
for view in ('south','north','east'):
    src=Image.open(root/f'artwork/no-odyssey-stand/sources/{view}-master.png').convert('RGBA')
    # The head covers the torso in the source. Preserve that overlap; crop the base at its upper edge.
    head_src=src.copy(); hm=Image.new('L',src.size,0); hd=ImageDraw.Draw(hm)
    if view=='east': hd.polygon([(440,330),(445,260),(480,190),(550,140),(630,125),(710,145),(775,190),(815,270),(830,350),(810,430),(775,485),(720,520),(650,540),(575,525),(510,490),(470,450),(450,400)],fill=255)
    else: hd.polygon([(425,325),(435,250),(470,190),(530,145),(600,125),(660,125),(730,145),(785,190),(815,250),(825,330),(810,410),(780,470),(730,515),(680,545),(625,560),(565,545),(510,515),(470,470),(440,410)],fill=255)
    head_src.putalpha(ImageChops.multiply(src.getchannel('A'),hm))
    head=fit(head_src,(400,110,850,560),(42,40,44,49))
    torso=src.copy(); a=torso.getchannel('A'); mask=Image.new('L',src.size,255); md=ImageDraw.Draw(mask)
    if view=='east': md.polygon([(0,0),(1254,0),(1254,440),(810,440),(790,490),(750,520),(690,540),(610,535),(550,515),(490,480),(460,440),(0,440)],fill=0)
    else: md.polygon([(0,0),(1254,0),(1254,450),(790,450),(770,490),(730,520),(680,545),(625,558),(565,545),(510,520),(470,490),(450,450),(0,450)],fill=0)
    if view=='east': md.polygon([(0,900),(450,900),(455,980),(660,980),(675,900),(1254,900),(1254,1254),(0,1254)],fill=0); md.polygon([(455,900),(675,900),(660,980),(455,980)],fill=255)
    else: md.polygon([(0,900),(460,900),(475,980),(775,980),(790,900),(1254,900),(1254,1254),(0,1254)],fill=0); md.polygon([(460,900),(790,900),(775,980),(475,980)],fill=255)
    torso.putalpha(ImageChops.multiply(a,mask))
    body=fit(torso,(410,450,830,1000),(39,43,50,65))
    child=fit(torso,(410,450,830,1000),((46 if view=='east' else 49),43,(36 if view=='east' else 30),51))
    base_src=Image.open(art/'sources/base-component.png').convert('RGBA')
    base=fit(base_src,(0,0,*base_src.size),(34,83,58,35))
    # Short support connects the uniformly scaled child body to the plinth.
    box=child.getchannel('A').getbbox(); cx=(box[0]+box[2])//2
    draw=ImageDraw.Draw(child); draw.rounded_rectangle((cx-3,box[3]-2,cx+3,95),radius=2,fill=(35,35,35,255)); draw.rectangle((cx-1,box[3]-1,cx+1,94),fill=(180,180,180,255))
    # One monotonic greyscale curve keeps dark contours while raising fills for stuff tint.
    lut=[v if v<=45 else round(45+(v-45)*171/89) if v<=134 else round(216+(v-134)*14/46) if v<=180 else min(245,round(230+(v-180)*15/50)) for v in range(256)]
    def levels(im):
        grey=im.convert('L').point(lut); return Image.merge('RGBA',(grey,grey,grey,im.getchannel('A')))
    head,body,child,base=map(levels,(head,body,child,base))
    def contour(im):
        alpha=im.getchannel('A'); ring=ImageChops.subtract(alpha,alpha.filter(ImageFilter.MinFilter(5)))
        rgb=im.convert('RGB'); rgb.paste((35,35,35),mask=ring); rgb.putalpha(alpha); return rgb
    head,body,child,base=map(contour,(head,body,child,base))
    if view=='north':
        body=ImageChops.offset(body,0,-2); child=ImageChops.offset(child,0,-2)
    for item in (body,child):
        neck=Image.new('RGBA',(128,128)); ImageDraw.Draw(neck).ellipse((53,42,75,60),fill=(35,35,35,255)); neck.alpha_composite(item); item.paste(neck)
    for kind,im in [('Base',base),('Body',body),('BodyChild',child),('Head',head)]: files[f'Things/Building/OutfitStand/OutfitStand_{kind}_{view}.png']=im
    for kid, b in [(False,body),(True,child)]:
        preview=mod.stand_preview(base,b,head,view)
        previews[(view,kid)]=preview
        crop=preview.crop(preview.getbbox()); h=49 if kid else 58; crop=crop.resize((round(crop.width*h/crop.height),h),Image.Resampling.LANCZOS)
        icon=Image.new('RGBA',(64,64)); icon.alpha_composite(crop,((64-crop.width)//2,61-h))
        files[f'Things/Building/OutfitStand/{"Kid" if kid else ""}OutfitStand_MenuIcon_{view}.png']=icon
src=Image.open(art/'sources/swap-source.png').convert('RGBA'); im=src.crop(src.getchannel('A').getbbox()); im.thumbnail((116,116),Image.Resampling.LANCZOS); icon=Image.new('RGBA',(128,128)); icon.alpha_composite(im,((128-im.width)//2,(128-im.height)//2)); files['UI/Commands/SwapOutfits.png']=icon
for name,im in files.items():
    p=out/name;p.parent.mkdir(parents=True,exist_ok=True);im.save(p)
# Review uses the actual existing stand_preview compositor. Tint is a CPU multiplication approximation.
sheet=Image.new('RGB',(1600,1800),(105,105,105)); d=ImageDraw.Draw(sheet)
colors={'untinted':(255,255,255),'wood':(133,97,67),'steel':(170,174,178),'marble':(240,240,234)}
for row,(view,kid) in enumerate(previews):
    d.text((5,row*300+4),f'{view} {"kid" if kid else "adult"}',fill='white')
    for col,(name,color) in enumerate(colors.items()):
        img=previews[(view,kid)]; rgb=ImageChops.multiply(img.convert('RGB'),Image.new('RGB',img.size,color)); rgb.putalpha(img.getchannel('A'))
        for z in (1,2):
            side=round(64*2.2*z); mini=rgb.resize((side,side),Image.Resampling.LANCZOS); x=100+col*325+(z-1)*145; y=row*300; sheet.paste(mini,(x,y),mini)
        if row==0:d.text((100+col*325,0),name,fill='white')
d.text((5,1780),'Each material: 1x at left, 2x at right; CPU tint approximation.',fill='white')
sheet.save(art/'review.png')
print(len(files))
