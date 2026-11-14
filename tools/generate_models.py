import math

class Obj:
    def __init__(s, name):
        s.name=name; s.v=[]; s.n=[]; s.f=[]; s.mats={}
    def mat(s, m, color): s.mats[m]=color
    def _add(s, verts, norms, faces, m):
        o=len(s.v); on=len(s.n)
        s.v+=verts; s.n+=norms
        s.f.append(('usemtl',m))
        for f in faces:
            s.f.append(('f',[(o+a+1,on+a+1) for a in f]))
    def box(s, c, size, m):
        cx,cy,cz=c; sx,sy,sz=[k/2 for k in size]
        faces=[((0,0,1),[(-1,-1,1),(1,-1,1),(1,1,1),(-1,1,1)]),((0,0,-1),[(1,-1,-1),(-1,-1,-1),(-1,1,-1),(1,1,-1)]),
               ((1,0,0),[(1,-1,1),(1,-1,-1),(1,1,-1),(1,1,1)]),((-1,0,0),[(-1,-1,-1),(-1,-1,1),(-1,1,1),(-1,1,-1)]),
               ((0,1,0),[(-1,1,1),(1,1,1),(1,1,-1),(-1,1,-1)]),((0,-1,0),[(-1,-1,-1),(1,-1,-1),(1,-1,1),(-1,-1,1)])]
        V=[];N=[];F=[]
        for n,qs in faces:
            b=len(V)
            for q in qs: V.append((cx+q[0]*sx,cy+q[1]*sy,cz+q[2]*sz)); N.append(n)
            F.append([b,b+1,b+2,b+3])
        s._add(V,N,F,m)
    def sphere(s, c, r, m, seg=16):
        rx,ry,rz=r if isinstance(r,tuple) else (r,r,r)
        V=[];N=[];F=[]
        for i in range(seg+1):
            t=math.pi*i/seg
            for j in range(seg*2+1):
                p=2*math.pi*j/(seg*2)
                d=(math.sin(t)*math.cos(p),math.cos(t),math.sin(t)*math.sin(p))
                V.append((c[0]+rx*d[0],c[1]+ry*d[1],c[2]+rz*d[2])); N.append(d)
        w=seg*2+1
        for i in range(seg):
            for j in range(seg*2):
                a=i*w+j;b=a+w
                F.append([a,a+1,b+1,b])
        s._add(V,N,F,m)
    def cyl(s, c, h, r0, r1, m, seg=16):
        V=[];N=[];F=[]
        for k,(y,r) in enumerate(((-h/2,r0),(h/2,r1))):
            for j in range(seg+1):
                p=2*math.pi*j/seg
                V.append((c[0]+r*math.cos(p),c[1]+y,c[2]+r*math.sin(p))); N.append((math.cos(p),0.2,math.sin(p)))
        for j in range(seg):
            a=j;b=j+seg+1
            F.append([a,a+1,b+1,b])
        s._add(V,N,F,m)
        for y,r,ny in ((h/2,r1,1),(-h/2,r0,-1)):
            if r<=0: continue
            b=len(V); V2=[(c[0],c[1]+y,c[2])]+[(c[0]+r*math.cos(2*math.pi*j/seg),c[1]+y,c[2]+r*math.sin(2*math.pi*j/seg)) for j in range(seg)]
            F2=[[0,1+(j+1)%seg,1+j] if ny>0 else [0,1+j,1+(j+1)%seg] for j in range(seg)]
            s._add(V2,[(0,ny,0)]*len(V2),F2,m)
    def save(s):
        with open(s.name+'.mtl','w') as f:
            for m,c in s.mats.items():
                f.write(f"newmtl {m}\nKd {c[0]/255:.3f} {c[1]/255:.3f} {c[2]/255:.3f}\nKa 0 0 0\nKs 0.05 0.05 0.05\nNs 10\n\n")
        with open(s.name+'.obj','w') as f:
            f.write(f"mtllib {s.name}.mtl\no {s.name}\n")
            for v in s.v: f.write("v %.4f %.4f %.4f\n"%v)
            for n in s.n: f.write("vn %.4f %.4f %.4f\n"%n)
            for t,x in s.f:
                if t=='usemtl': f.write(f"usemtl {x}\n")
                else: f.write("f "+" ".join(f"{a}//{b}" for a,b in x)+"\n")

# ---------- المهرج (يواجه -Z) ----------
c=Obj('Clown')
for m,col in {'skin':(235,230,220),'suit':(190,20,35),'suit2':(240,240,235),'nose':(255,10,10),'hair':(30,170,60),
              'hat':(100,30,140),'shoe':(230,200,20),'black':(10,10,10),'mouth':(120,0,10),'teeth':(235,225,180),'pants':(40,60,170)}.items(): c.mat(m,col)
c.sphere((0,1.15,0),(0.55,0.7,0.5),'suit')                       # جسم
for i,y in enumerate((1.45,1.15,0.85)): c.sphere((0,y,-0.47),0.07,'black',8)   # أزرار
c.sphere((0,1.72,0),(0.45,0.12,0.45),'suit2')                    # ياقة
c.sphere((0,2.15,0),(0.3,0.36,0.3),'skin')                       # رأس طويل مخيف
c.sphere((0,2.12,-0.28),0.09,'nose',10)                          # أنف
c.sphere((-0.12,2.28,-0.26),(0.06,0.09,0.04),'black',8); c.sphere((0.12,2.28,-0.26),(0.06,0.09,0.04),'black',8)  # عيون
c.box((0,1.97,-0.27),(0.34,0.09,0.05),'mouth')                   # فم واسع
for x in (-0.12,-0.04,0.04,0.12): c.box((x,1.97,-0.30),(0.04,0.06,0.02),'teeth')  # أسنان
c.sphere((-0.34,2.3,0),0.17,'hair',10); c.sphere((0.34,2.3,0),0.17,'hair',10)
c.sphere((-0.3,2.42,0),0.13,'hair',10); c.sphere((0.3,2.42,0),0.13,'hair',10)
c.cyl((0,2.68,0),0.42,0.12,0.28,'hat'); c.cyl((0,2.5,0),0.04,0.36,0.36,'hat')     # قبعة
c.cyl((-0.65,1.2,0),0.9,0.1,0.12,'suit'); c.cyl((0.65,1.2,0),0.9,0.1,0.12,'suit')  # أذرع
c.sphere((-0.65,0.7,0),0.14,'skin',10); c.sphere((0.65,0.7,0),0.14,'skin',10)      # أيدي
c.cyl((-0.22,0.4,0),0.8,0.11,0.11,'pants'); c.cyl((0.22,0.4,0),0.8,0.11,0.11,'pants')  # أرجل
c.sphere((-0.22,0.07,-0.12),(0.15,0.08,0.3),'shoe'); c.sphere((0.22,0.07,-0.12),(0.15,0.08,0.3),'shoe')  # أحذية كبيرة
c.save()

# ---------- البيت (نفس تصميم الكود) ----------
h=Obj('House')
for m,col in {'floor':(60,40,30),'ceil':(30,30,30),'wall':(95,85,78),'wood':(90,55,35),'trim':(50,35,28)}.items(): h.mat(m,col)
h.box((0,-0.1,0),(20,0.2,20),'floor'); h.box((0,3.1,0),(20,0.2,20),'ceil')
def wall(x1,z1,x2,z2):
    t=0.3; h.box(((x1+x2)/2,1.5,(z1+z2)/2),(max(abs(x2-x1),t),3,max(abs(z2-z1),t)),'wall')
for w in [(-10,-10,10,-10),(-10,10,10,10),(-10,-10,-10,10),(10,-10,10,10),(-10,0,-2,0),(2,0,10,0),(-10,-5,-3,-5),(3,-5,10,-5),(-4,0,-4,4),(4,5,4,10)]: wall(*w)
for z in (-9.85,9.85):                                              # زوايا زخرفية
    h.box((0,0.1,z),(20,0.2,0.1),'trim')
h.box((-8,0.4,6),(1.5,0.8,1),'wood'); h.box((7,0.5,-2),(1,1,2),'wood'); h.box((-6,0.5,-7),(2,1,1),'wood')
h.box((-8,0.9,6),(1.2,0.2,0.8),'trim')
h.save()

# ---------- مفتاح وباب ----------
k=Obj('Key'); k.mat('gold',(255,210,20))
k.cyl((0,0.12,0),0.04,0.1,0.1,'gold',12); k.cyl((0,-0.05,0),0.3,0.025,0.025,'gold',8); k.box((0.05,-0.15,0),(0.1,0.04,0.03),'gold'); k.box((0.05,-0.08,0),(0.1,0.04,0.03),'gold')
k.save()
d=Obj('ExitDoor'); d.mat('door',(20,200,60)); d.mat('frame',(40,30,25))
d.box((0,1.2,0),(1.6,2.4,0.15),'door'); d.box((0,2.45,0),(1.9,0.15,0.2),'frame'); d.box((-0.87,1.2,0),(0.15,2.4,0.2),'frame'); d.box((0.87,1.2,0),(0.15,2.4,0.2),'frame')
d.sphere((0.55,1.1,-0.1),0.06,'frame',8)
d.save()
print('ok')
