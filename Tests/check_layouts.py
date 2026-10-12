import math, collections
levels={1:[(-7,1),(6,-1),(-1,-3.2),(4.5,3)],2:[(-6,2),(7,0),(-3,-3.8),(2.7,3)],3:[(-8,1.5),(8,-1),(1,-3.5),(-3.5,3.2)]}
spawn={1:[(-10,3),(10,2),(9,-4.2),(-10,-4),(7.7,4)],2:[(-10,3.8),(10,-4.1),(9.4,4),(-9,-3.9),(3,4.9),(-8.2,-4.7)],3:[(-10,1),(10,2),(-8,4.7),(8,-4.5),(0,4.8),(9,-4)]}
def segdist(p,a,b):
 dx=b[0]-a[0];dy=b[1]-a[1];t=max(0,min(1,((p[0]-a[0])*dx+(p[1]-a[1])*dy)/(dx*dx+dy*dy)))
 return math.hypot(p[0]-a[0]-dx*t,p[1]-a[1]-dy*t)
def barriers(props,t,mode):
 ret=[]
 for i,p in enumerate(props):
  m=mode[1 if i in (1,3) else 0]
  if i==0: dx,dy=(1,-1) if m==0 else (1,.35)
  elif i==1:dx,dy=(-1,1) if m==0 else (-.45,-1)
  elif i==2:dx,dy=(1,-.25) if m==0 else (.15,1)
  else:dx,dy=(-.7,-1) if m==0 else (.55,-1)
  norm=math.hypot(dx,dy);ang=math.sin(t*.52+i*1.8)*.15
  dx,dy=dx/norm,dy/norm;dx,dy=dx*math.cos(ang)-dy*math.sin(ang),dx*math.sin(ang)+dy*math.cos(ang)
  k=3.7 if i==3 else 4.7
  ret.append((p,(p[0]+dx*k,p[1]+dy*k)))
 return ret
for lvl,props in levels.items():
 for m in [(0,0),(1,0),(0,1),(1,1)]:
  for time in [0,6,15]:
   walls=barriers(props,time,m)
   def free(p):
    x,y=p
    if abs(x)>12.6-.48 or y>5.75-.48 or y<-5.85+.48:return False
    if any(math.hypot(x-a,y-b)<1.03 for a,b in props):return False
    if any(segdist(p,*wall)<.83 for wall in walls):return False
    return True
   cell=.35; nx,ny=round(25/cell),round(11/cell)
   toxy=lambda ij:(-12+ij[0]*cell,-5.3+ij[1]*cell)
   index=lambda p:(round((p[0]+12)/cell),round((p[1]+5.3)/cell))
   starts=index((-4,-3.5));visited=set([starts]);q=collections.deque([starts]);
   while q:
    x,y=q.popleft()
    for dx,dy in ((0,1),(0,-1),(1,0),(-1,0)):
     n=(x+dx,y+dy)
     if n in visited or not (0<=n[0]<=nx and 0<=n[1]<=ny) or not free(toxy(n)):continue
     q.append(n);visited.add(n)
   targets=sum(1 for p in spawn[lvl] if index(p) in visited)
   if any(not free(p) for p in spawn[lvl]):
    print('SPAWN WARNING',lvl,m,time, [p for p in spawn[lvl] if not free(p)], [round(segdist((8,-4),*wall),2) for wall in walls] if lvl==3 else '')
   if not free((-4,-3.5)) or len(visited)<400:
    print('WARNING',lvl,m,time,'player start collision?',free((-4,-3.5)),'area',len(visited),'targets',targets)
 print('Level',lvl,'checked: all modes')
