"""Pack the deterministic Three.js export into bounded native Unity streaming cells."""
import collections, gzip, hashlib, json, math, struct, shutil
from pathlib import Path
import numpy as np

src=Path('outputs/UnityPort/OpenWorld')
out=Path('outputs/UnityPort/WorldPacked')
for folder in ('Geometry','Textures'): (out/folder).mkdir(parents=True,exist_ok=True)
data=json.loads((src/'world.json').read_text())
bounds={}; total_vertices=total_triangles=0
def write_mesh(path,p,n,uv,c,indices,groups):
    with gzip.open(path,'wb',compresslevel=2) as f:
        f.write(struct.pack('<iii',len(p),len(indices),len(groups)))
        for array,dtype in ((p,'<f4'),(n,'<f4'),(uv,'<f4'),(c,'u1'),(indices,'<i4'),(groups,'<i4')):
            f.write(np.asarray(array,dtype=dtype).tobytes())
for i in range(data['geometryCount']):
    d=json.loads((src/f'geometry_{i}.json').read_text())
    p=np.asarray(d['p'],dtype=np.float32).reshape(-1,3);p[:,2]*=-1
    n=np.asarray(d['n'],dtype=np.float32).reshape(-1,3)
    if len(n)!=len(p): n=np.zeros_like(p);n[:,1]=1
    else:n[:,2]*=-1
    uv=np.asarray(d['uv'],dtype=np.float32).reshape(-1,2)
    if len(uv)!=len(p):uv=np.zeros((len(p),2),dtype=np.float32)
    color=np.full((len(p),4),255,dtype=np.uint8)
    if len(d['c'])==len(p)*3:color[:,:3]=np.clip(np.asarray(d['c']).reshape(-1,3)*255,0,255).astype(np.uint8)
    ix=np.asarray(d['indices'],dtype=np.int32).reshape(-1,3)[:,[0,2,1]].reshape(-1)
    groups=[[g['start'],g['count'],g.get('materialIndex',0)] for g in d['groups']] or [[0,len(ix),0]]
    write_mesh(out/'Geometry'/f'g{i}.bytes',p,n,uv,color,ix,groups)
    bounds[i]=(p.min(axis=0),p.max(axis=0));total_vertices+=len(p);total_triangles+=len(ix)//3
    if i%100==0:print('Packed geometry',i,flush=True)
textures={}; txids={}
for i in range(data['textureCount']):
    file=src/f'texture_{i}.png'; digest=hashlib.sha256(file.read_bytes()).hexdigest()
    if digest not in textures:
        textures[digest]=f't{len(textures)}';shutil.copy2(file,out/'Textures'/(textures[digest]+'.png'))
    txids[i]=textures[digest]
mats=[]
for m in data['materials']:
    m=dict(m);m['texturePath']=txids.get(m['texture'],'');mats.append(m)
chunks={}
def cell(key,low,high):
    if key not in chunks:chunks[key]={'id':'cell'+str(len(chunks)),'nodes':[],'min':low.copy(),'max':high.copy(),'layer':'','collider':''}
    ch=chunks[key];ch['min']=np.minimum(ch['min'],low);ch['max']=np.maximum(ch['max'],high);return ch
S=np.diag([1,1,-1,1])
for node in data['nodes']:
    # The approved vault scene already replaces the original vault interior.
    if node['group'].startswith('map_vault'):continue
    low,high=bounds[node['geometry']]
    mat=S@np.asarray(node['matrix']).reshape(4,4,order='F')@S
    corners=np.array([[x,y,z,1] for x in (low[0],high[0]) for y in (low[1],high[1]) for z in (low[2],high[2])])@mat.T
    lo=corners[:,:3].min(axis=0);hi=corners[:,:3].max(axis=0);center=(lo+hi)*.5
    group=node['group']; layer='';lodx=lodz=0
    if group.startswith('map_city/chunk_'):
        layer,x,z=group.split('chunk_')[1].split('|');lodx=(int(x)+.5)*240;lodz=-(int(z)+.5)*240
        key=group
    else:key='objects:'+str(math.floor(center[0]/120))+':'+str(math.floor(center[2]/120))
    ch=cell(key,lo,hi);ch['layer']=layer;ch['lodCenter']=[lodx,0,lodz]
    ch['nodes'].append({'name':node['name'],'geometry':node['geometry'],'materials':node['materials'],'matrix':mat.flatten(order='F').tolist()})
# Preserve the authored collision boxes, combined per cell to avoid 48k collider objects.
boxes=collections.defaultdict(list)
for c in data['colliders']:
    p=c['center'];size=c['size']
    if not all(math.isfinite(v) for v in p+size) or min(size)<=0:continue
    if 3950<p[0]<4050 and -110<p[2]<100:continue
    boxes[(math.floor(p[0]/120),math.floor(-p[2]/120))].append(c)
cube=np.array([[-1,-1,-1],[1,-1,-1],[1,1,-1],[-1,1,-1],[-1,-1,1],[1,-1,1],[1,1,1],[-1,1,1]],dtype=np.float32)*.5
faces=np.array([0,2,1,0,3,2,4,5,6,4,6,7,0,1,5,0,5,4,3,7,6,3,6,2,1,2,6,1,6,5,0,4,7,0,7,3])
for k,cs in boxes.items():
    vertices=[];indices=[]
    for c in cs:
        # Source collision yaw has the opposite convention to THREE rotations.
        yaw=c.get('yaw',0);co=math.cos(yaw);si=math.sin(yaw)
        q=cube*np.array(c['size']);x=q[:,0].copy();z=q[:,2].copy();q[:,0]=co*x-si*z;q[:,2]=si*x+co*z;q+=c['center'];q[:,2]*=-1
        indices.extend((faces.reshape(-1,3)[:,[0,2,1]].reshape(-1)+len(vertices)).tolist());vertices.extend(q)
    v=np.array(vertices);ix=np.array(indices);name='c'+str(len(chunks));write_mesh(out/'Geometry'/(name+'.bytes'),v,np.zeros_like(v),np.zeros((len(v),2)),np.full((len(v),4),255),ix,[[0,len(ix),0]])
    ch=cell('collision:'+str(k),v.min(axis=0),v.max(axis=0));ch['collider']=name;ch['lodCenter']=[0,0,0]
for ch in chunks.values():
    ch['center']=((ch.pop('min')+ch['max'])*.5).tolist()
    ch['size']=((ch.pop('max')-np.array(ch['center']))*2).tolist()
result={'chunks':list(chunks.values()),'materials':mats,'locations':data['locations'],'doors':[],'containers':[]}
(out/'manifest.bytes').write_text(json.dumps(result,separators=(',',':')))
(out/'report.json').write_text(json.dumps({'geometries':data['geometryCount'],'vertices':total_vertices,'triangles':total_triangles,'chunks':len(chunks),'textures':len(textures),'sourceTextures':data['textureCount']},indent=2))
print('PACK COMPLETE',flush=True)
